import React, { useState, useEffect, useCallback } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import styles from './Dashboard.module.css';
import {
  Chart as ChartJS, CategoryScale, LinearScale, PointElement,
  LineElement, Title, Tooltip, Legend, Filler
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import Navbar from '../../component/navBar/Navbar';
import * as signalR from '@microsoft/signalr';
import {
  getLatestReading, getReadings, getNotifications,
  markNotificationRead, irrigateDome, getDomes,
  BASE_URL, TUNNEL_HEADERS
} from '../../services/api';
import { useAuth } from '../../context/AuthContext';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const chartOptions = {
  responsive: true, maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#25262c', titleColor: '#fff', bodyColor: '#94a3b8',
      borderColor: 'rgba(255,255,255,0.1)', borderWidth: 1, padding: 10, displayColors: false,
    }
  },
  scales: {
    y: { border: { display: false }, grid: { color: 'rgba(255,255,255,0.05)' }, ticks: { color: '#64748b', font: { size: 11 } } },
    x: { border: { display: false }, grid: { display: false }, ticks: { color: '#64748b', font: { size: 11 } } }
  },
  elements: { line: { tension: 0.4 } }
};

const Dashboard = () => {
  const { selectedDomeId, selectDome } = useAuth();
  const [latest, setLatest] = useState(null);
  const [history, setHistory] = useState([]);
  const [notifications, setNotifications] = useState([]);
  const [domes, setDomes] = useState([]);
  const [irrigating, setIrrigating] = useState(false);
  const [lastUpdate, setLastUpdate] = useState('--');

  const loadData = useCallback(async () => {
    if (!selectedDomeId) return;
    try {
      const [latestRes, histRes, notifRes] = await Promise.all([
        getLatestReading(selectedDomeId),
        getReadings(selectedDomeId),
        getNotifications(selectedDomeId)
      ]);
      setLatest(latestRes.data);
      setHistory(Array.isArray(histRes.data) ? histRes.data : histRes.data?.items || []);
      setNotifications(notifRes.data || []);
      setLastUpdate(new Date().toLocaleTimeString());
    } catch (e) { console.error(e); }
  }, [selectedDomeId]);

  // Load domes list
  useEffect(() => {
    getDomes().then(r => {
      const list = Array.isArray(r.data) ? r.data : (r.data?.value || []);
      setDomes(list);
      if (!selectedDomeId && list.length > 0) selectDome(list[0].id);
    }).catch(() => {});
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  // SignalR
  useEffect(() => {
    if (!selectedDomeId) return;
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/dome`, {
        accessTokenFactory: () => localStorage.getItem('token'),
        headers: { ...TUNNEL_HEADERS }
      })
      .withAutomaticReconnect()
      .build();

    conn.start().then(() => {
      conn.invoke('JoinDome', String(selectedDomeId));
    }).catch(() => {});

    conn.on('SensorUpdate', (reading) => {
      setLatest(reading);
      setLastUpdate(new Date().toLocaleTimeString());
      setHistory(prev => [...prev.slice(-23), { ...reading, timestamp: reading.timestamp || new Date().toISOString() }]);
    });

    conn.on('NewNotification', (notif) => {
      setNotifications(prev => [notif, ...prev]);
    });

    return () => { conn.stop(); };
  }, [selectedDomeId]);

  const handleIrrigate = async () => {
    if (!selectedDomeId) return;
    setIrrigating(true);
    try {
      await irrigateDome(selectedDomeId);
    } finally {
      setIrrigating(false);
      loadData();
    }
  };

  const handleMarkRead = async (id) => {
    await markNotificationRead(id);
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
  };

  const moistureChartData = {
    labels: history.map(r => new Date(r.timestamp || r.readingTime || r.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
    datasets: [{
      label: 'Moisture %',
      data: history.map(r => r.soilMoisture ?? r.soilMoisturePercent),
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59,130,246,0.1)',
      fill: true, pointRadius: 0, pointHoverRadius: 6,
    }],
  };

  const cardsData = latest ? [
    { title: "Soil Moisture", value: (latest.soilMoisture ?? latest.soilMoisturePercent)?.toFixed(1) ?? '--', unit: "%", sub: "Target: 60-75%", icon: "💧", theme: styles.soilIcon },
    { title: "Temp & Humidity", value: latest.temperature?.toFixed(1) ?? '--', unit: "°C", sub: `Humidity: ${latest.humidity?.toFixed(0) ?? '--'}%`, icon: "🌡️", theme: styles.tempIcon },
    { title: "Light Intensity", value: latest.lightIntensity?.toFixed(0) ?? '--', unit: "lux", sub: "Target: 7k-10k", icon: "☀️", theme: styles.sunIcon },
    { title: "Rain State", value: (latest.rainState === 1 || latest.rainState === true) ? 'Raining' : 'Clear', unit: "", sub: "Current state", icon: "🌧️", theme: styles.waterIcon },
  ] : [];

  const notifIconMap = { Warning: "⚠️", Info: "ℹ️", Success: "✅", Error: "❌" };
  const notifThemeMap = { Warning: styles.insightYellow, Info: styles.insightBlue, Success: styles.insightGreen, Error: styles.insightYellow };

  return (
    <>
      <Navbar />
      <div className={styles.dashboardContainer}>
        <div className="container py-4">

          {/* Header */}
          <div className="row align-items-center mb-4">
            <div className="col">
              <div className="d-flex align-items-center justify-content-between">
                <div className="d-flex align-items-center">
                  <div className={styles.mainIconBox}>📊</div>
                  <div className="ms-3">
                    <h2 className={styles.h2}>Smart Plant Monitor</h2>
                    <p className={styles.p}>Real-time plant health monitoring system</p>
                  </div>
                </div>
                {domes.length > 1 && (
                  <select className="form-select form-select-sm w-auto" style={{ background: '#1e1f25', color: '#fff', border: '1px solid #3d3f47' }}
                    value={selectedDomeId || ''} onChange={e => selectDome(e.target.value)}>
                    {domes.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                  </select>
                )}
              </div>
            </div>
          </div>

          {/* Stats Cards */}
          <div className="row g-4 mb-4">
            {(cardsData.length > 0 ? cardsData : [
              { title: "Soil Moisture", value: "--", unit: "%", sub: "No data yet", icon: "💧", theme: styles.soilIcon },
              { title: "Temp & Humidity", value: "--", unit: "°C", sub: "No data yet", icon: "🌡️", theme: styles.tempIcon },
              { title: "Light Intensity", value: "--", unit: "lux", sub: "No data yet", icon: "☀️", theme: styles.sunIcon },
              { title: "Rain State", value: "--", unit: "", sub: "No data yet", icon: "🌧️", theme: styles.waterIcon },
            ]).map((card, index) => (
              <div key={index} className="col-12 col-md-6 col-lg-3">
                <div className={styles.mainCard}>
                  <div className="d-flex justify-content-between align-items-start h-100">
                    <div>
                      <span className={styles.label}>{card.title}</span>
                      <div className="mt-2">
                        <span className={styles.valueLarge}>{card.value}</span>
                        <span className="ms-1 text-muted" style={{ fontSize: '0.9rem' }}>{card.unit}</span>
                      </div>
                      <p className={`${styles.pSmall} mt-1 mb-0`}>{card.sub}</p>
                    </div>
                    <div className={`${styles.iconCircle} ${card.theme}`}>{card.icon}</div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Main Content */}
          <div className="row g-4 mb-4">
            <div className="col-12 col-lg-8">
              <div className={styles.mainCard}>
                <h3 className={styles.h3}>Soil Moisture History (24h)</h3>
                <div className={styles.chartWrapper}>
                  {history.length > 1
                    ? <Line data={moistureChartData} options={chartOptions} />
                    : <div className="d-flex align-items-center justify-content-center h-100 text-muted">No history data yet</div>
                  }
                </div>
              </div>
            </div>

            <div className="col-12 col-lg-4">
              <div className="d-flex flex-column gap-3 h-100">
                <div className={styles.mainCard}>
                  <h3 className={styles.h3}>System Status <span style={{ float: 'right' }}>ℹ️</span></h3>
                  <div className="d-flex justify-content-between align-items-center mb-3 p-2" style={{ background: 'rgba(255,255,255,0.05)', borderRadius: '10px' }}>
                    <span className="text-light">📡 Connection</span>
                    <span className="badge bg-success bg-opacity-25 text-success">Online</span>
                  </div>
                  <div className="d-flex justify-content-between align-items-center mb-2 p-2" style={{ background: 'rgba(255,255,255,0.05)', borderRadius: '10px' }}>
                    <span className="text-light">🧠 AI Monitor</span>
                    <span className="badge bg-primary bg-opacity-25 text-primary">Running</span>
                  </div>
                  <div className="text-end mt-2">
                    <small className={styles.pSmall}>Last update: {lastUpdate}</small>
                  </div>
                </div>

                <div className={`${styles.mainCard} flex-grow-1`}>
                  <h3 className={styles.h3}>Manual Controls</h3>
                  <div className="d-grid gap-3">
                    <button onClick={handleIrrigate} disabled={irrigating}
                      className={`btn btn-outline-light d-flex justify-content-between align-items-center ${styles.controlBtn}`}>
                      <span>{irrigating ? 'Irrigating...' : 'Start Irrigation'}</span> <span>💧</span>
                    </button>
                    <button onClick={loadData}
                      className={`btn btn-outline-light d-flex justify-content-between align-items-center ${styles.controlBtn}`}>
                      <span>Refresh Sensors</span> <span>🔄</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Notifications */}
          <div className="row">
            <div className="col-12">
              <div className={styles.mainCard}>
                <h3 className={styles.h3}>Notifications</h3>
                <div className={styles.notificationList}>
                  {notifications.length === 0
                    ? (
                      <div className="text-center py-4">
                        <div style={{ fontSize: '2rem', marginBottom: '8px' }}>🔔</div>
                        <p style={{ color: '#64748b', fontSize: '0.9rem' }}>No notifications yet</p>
                      </div>
                    )
                    : notifications.map((note, i) => {
                      const isAi = note.message?.includes('ذكاء');
                      const isWarn = note.message?.includes('رطوبة') || note.message?.includes('منخفض');
                      const borderColor = isAi ? '#a855f7' : isWarn ? '#f59e0b' : '#3b82f6';
                      const bgColor    = isAi ? 'rgba(168,85,247,0.08)' : isWarn ? 'rgba(245,158,11,0.08)' : 'rgba(59,130,246,0.08)';
                      const icon       = isAi ? '🧠' : isWarn ? '⚠️' : '🔔';
                      return (
                        <div key={note.id || i}
                          onClick={() => !note.isRead && handleMarkRead(note.id)}
                          style={{ background: bgColor, border: `1px solid ${borderColor}30`, borderLeft: `3px solid ${borderColor}`, borderRadius: '12px', padding: '14px 16px', marginBottom: '10px', opacity: note.isRead ? 0.55 : 1, cursor: note.isRead ? 'default' : 'pointer', transition: 'opacity 0.2s' }}>
                          <div className="d-flex align-items-start gap-3">
                            <div style={{ width: '36px', height: '36px', borderRadius: '10px', background: `${borderColor}18`, border: `1px solid ${borderColor}35`, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.1rem', flexShrink: 0 }}>
                              {icon}
                            </div>
                            <div className="flex-grow-1" style={{ minWidth: 0 }}>
                              <p style={{ color: '#e2e8f0', fontSize: '0.88rem', lineHeight: '1.5', marginBottom: '5px', wordBreak: 'break-word' }}>
                                {note.message}
                              </p>
                              <span style={{ color: '#64748b', fontSize: '0.75rem' }}>
                                {new Date(note.createdAt).toLocaleString()}
                              </span>
                            </div>
                            {!note.isRead && (
                              <span style={{ background: borderColor, color: '#fff', borderRadius: '20px', padding: '2px 10px', fontSize: '0.7rem', fontWeight: '600', flexShrink: 0 }}>New</span>
                            )}
                          </div>
                        </div>
                      );
                    })
                  }
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </>
  );
};

export default Dashboard;
