import React, { useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import styles from './Analytics.module.css';
import {
  Chart as ChartJS, CategoryScale, LinearScale, PointElement,
  LineElement, Title, Tooltip, Legend, Filler
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import Navbar from '../../component/navBar/Navbar';
import { getAnalytics, getAiInsights } from '../../services/api';
import { useAuth } from '../../context/AuthContext';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const commonOptions = {
  responsive: true, maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom', labels: { color: '#9ca3af', usePointStyle: true, pointStyle: 'circle', padding: 25, font: { size: 12 } } },
    tooltip: { backgroundColor: '#1c1c2b', padding: 12, cornerRadius: 10 }
  },
  scales: {
    y: { border: { display: false }, grid: { color: 'rgba(255,255,255,0.05)', drawTicks: false }, ticks: { color: '#9ca3af' } },
    x: { border: { display: false }, grid: { display: false }, ticks: { color: '#9ca3af' } }
  }
};

const periodMap = { 'Last 24 Hours': '24h', 'Last 7 Days': '7d', 'Last 30 Days': '30d', 'Last 90 Days': '90d' };

const insightTheme   = { Irrigation: styles.insightYellow, Optimization: styles.insightGreen, Growth: styles.insightBlue, Maintenance: styles.insightPurple };
const insightIcon    = { Irrigation: "💧", Optimization: "⚡", Growth: "🌱", Maintenance: "🔧" };
const insightBgColor = { Irrigation: 'rgba(251,191,36,0.08)', Optimization: 'rgba(16,185,129,0.08)', Growth: 'rgba(59,130,246,0.08)', Maintenance: 'rgba(168,85,247,0.08)' };
const insightBorder  = { Irrigation: '#fbbf24', Optimization: '#10b981', Growth: '#3b82f6', Maintenance: '#a855f7' };

const Analytics = () => {
  const { selectedDomeId } = useAuth();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [selectedRange, setSelectedRange] = useState('Last 7 Days');
  const [analytics, setAnalytics] = useState(null);
  const [insights, setInsights] = useState([]);
  const [loadingInsights, setLoadingInsights] = useState(false);

  useEffect(() => {
    if (!selectedDomeId) return;
    const period = periodMap[selectedRange] || '7d';
    getAnalytics(selectedDomeId, period).then(r => setAnalytics(r.data)).catch(console.error);
  }, [selectedDomeId, selectedRange]);

  const handleRangeSelect = (range) => { setSelectedRange(range); setIsDropdownOpen(false); };

  const handleGetInsights = async () => {
    if (!selectedDomeId) return;
    setLoadingInsights(true);
    try {
      const r = await getAiInsights(selectedDomeId);
      setInsights(r.data?.insights || r.data || []);
    } catch (e) { console.error(e); }
    finally { setLoadingInsights(false); }
  };

  const daily = analytics?.dailyData || [];

  const labels = daily.map(d => d.date || '');

  const moistureData = {
    labels, datasets: [{
      label: 'Soil Moisture', data: daily.map(d => d.avgSoilMoisture),
      borderColor: '#10b981', tension: 0.4, pointRadius: 4
    }]
  };
  const lightData = {
    labels, datasets: [{
      label: 'Lux Intensity', data: daily.map(d => d.avgLightIntensity),
      borderColor: '#f59e0b', backgroundColor: 'rgba(245,158,11,0.1)', fill: true, tension: 0.4, pointRadius: 6
    }]
  };
  const healthData = {
    labels, datasets: [{
      label: 'Overall Health %',
      data: daily.map(d => d.healthScore ?? analytics?.healthScore ?? 0),
      borderColor: '#10b981', backgroundColor: 'rgba(16,185,129,0.1)', fill: true, tension: 0.4, pointRadius: 5
    }]
  };

  const thermoIcon = (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#fb7185"
      strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M14 14.76V5a2 2 0 0 0-4 0v9.76a4 4 0 1 0 4 0z" />
    </svg>
  );

  const statCards = analytics ? [
    { title: "Avg. Soil Moisture", value: analytics.avgSoilMoisture?.toFixed(1) ?? '--', unit: "%", icon: "💧", iconColor: styles.soilIcon },
    { title: "Avg. Temperature", value: analytics.avgTemperature?.toFixed(1) ?? '--', unit: "°C", icon: thermoIcon, iconColor: styles.tempIcon },
    { title: "Avg. Light Intensity", value: analytics.avgLightIntensity ? (analytics.avgLightIntensity / 1000).toFixed(1) : '--', unit: "klux", icon: "☀️", iconColor: styles.sunIcon },
    { title: "Water Consumption", value: (analytics.totalWaterConsumption ?? analytics.waterConsumption)?.toFixed(0) ?? '--', unit: "L", icon: "⏲️", iconColor: styles.waterIcon },
  ] : [];

  const timeRanges = ['Last 24 Hours', 'Last 7 Days', 'Last 30 Days', 'Last 90 Days'];

  return (
    <>
      <Navbar />
      <div className={styles.dashboardContainer}>
        <div className="container py-4">

          {}
          <div className={`card ${styles.mainCard} mb-4`}>
            <div className="card-body d-flex justify-content-between align-items-center flex-wrap gap-3">
              <div className="d-flex align-items-center">
                <div className={styles.mainIconBox}>📊</div>
                <div className="ms-3">
                  <h2 className={styles.h2}>Analytics & Reports</h2>
                  <p className={styles.p}>Data-driven insights for smart farming</p>
                </div>
              </div>
              <div className={styles.dropdownContainer}>
                <button className={`${styles.dropdownTrigger} ${isDropdownOpen ? styles.active : ''}`}
                  onClick={() => setIsDropdownOpen(!isDropdownOpen)}>
                  <div className="d-flex align-items-center">
                    <span className="me-2 text-muted" style={{ fontSize: '1.1rem' }}>📅</span>
                    {selectedRange}
                  </div>
                  <span className={`ms-3 ${styles.chevron} ${isDropdownOpen ? styles.rotate : ''}`}>▼</span>
                </button>
                {isDropdownOpen && (
                  <div className={styles.dropdownMenu}>
                    {timeRanges.map((range) => (
                      <div key={range} className={`${styles.dropdownItem} ${selectedRange === range ? styles.selected : ''}`}
                        onClick={() => handleRangeSelect(range)}>
                        {range}
                        {selectedRange === range && <span className={styles.checkIcon}>✓</span>}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>

          {}
          <div className="row g-3 mb-4">
            {(statCards.length > 0 ? statCards : [
              { title: "Avg. Soil Moisture", value: "--", unit: "%", icon: "💧", iconColor: styles.soilIcon },
              { title: "Avg. Temperature", value: "--", unit: "°C", icon: thermoIcon, iconColor: styles.tempIcon },
              { title: "Sunlight Exposure", value: "--", unit: "hrs/day", icon: "☀️", iconColor: styles.sunIcon },
              { title: "Water Consumption", value: "--", unit: "L", icon: "⏲️", iconColor: styles.waterIcon },
            ]).map((card, i) => (
              <div key={i} className="col-12 col-md-6 col-lg-3">
                <div className={`card ${styles.mainCard}`}>
                  <div className="card-body">
                    <div className="d-flex justify-content-between mb-3">
                      <span className={styles.label}>{card.title}</span>
                      <div className={`${styles.iconCircle} ${card.iconColor}`}>{card.icon}</div>
                    </div>
                    <div className="d-flex align-items-baseline">
                      <span className={styles.valueLarge}>{card.value}</span>
                      <span className="ms-2 text-muted">{card.unit}</span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {}
          <div className="row g-4 mb-5">
            <div className="col-12 col-lg-6">
              <div className={`card ${styles.mainCard}`}>
                <div className="card-body">
                  <h3 className={styles.h3}>Soil Moisture Trends</h3>
                  <div className={styles.chartWrapper}>
                    {daily.length > 0 ? <Line data={moistureData} options={commonOptions} />
                      : <div className="d-flex align-items-center justify-content-center h-100 text-muted">No data</div>}
                  </div>
                </div>
              </div>
            </div>
            <div className="col-12 col-lg-6">
              <div className={`card ${styles.mainCard}`}>
                <div className="card-body">
                  <h3 className={styles.h3}>Light Intensity Trends</h3>
                  <div className={styles.chartWrapper}>
                    {daily.length > 0 ? <Line data={lightData} options={commonOptions} />
                      : <div className="d-flex align-items-center justify-content-center h-100 text-muted">No data</div>}
                  </div>
                </div>
              </div>
            </div>
            <div className="col-12">
              <div className={`card ${styles.mainCard}`}>
                <div className="card-body">
                  <h3 className={styles.h3}>Plant Health Performance</h3>
                  <div className={styles.chartWrapper}>
                    {daily.length > 0 ? <Line data={healthData} options={commonOptions} />
                      : <div className="d-flex align-items-center justify-content-center h-100 text-muted">No data</div>}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {}
          <div className="mt-5">
            <div className="d-flex align-items-center justify-content-between mb-4">
              <div className="d-flex align-items-center">
                <div className={styles.insightHeaderIcon}>💡</div>
                <h2 className={`${styles.h2} ms-2`}>AI-Generated Insights</h2>
              </div>
              <button className="btn btn-outline-light btn-sm" onClick={handleGetInsights} disabled={loadingInsights}>
                {loadingInsights ? 'Analyzing...' : '🧠 Generate Insights'}
              </button>
            </div>
            <div className="d-flex flex-column gap-3">
              {insights.length === 0
                ? (
                  <div className="text-center py-5" style={{ color: '#64748b' }}>
                    <div style={{ fontSize: '2.5rem', marginBottom: '12px' }}>🧠</div>
                    <p className="mb-1" style={{ fontSize: '1rem', color: '#94a3b8' }}>No AI analysis yet</p>
                    <p style={{ fontSize: '0.85rem' }}>Click "Generate Insights" to get personalized recommendations</p>
                  </div>
                )
                : insights.map((item, i) => {
                  const cat   = item.category || item.type || 'Growth';
                  const icon  = insightIcon[cat]  || '💡';
                  const bg    = insightBgColor[cat] || 'rgba(99,102,241,0.08)';
                  const border= insightBorder[cat]  || '#6366f1';
                  const theme = insightTheme[cat]   || styles.insightBlue;
                  return (
                    <div key={i} dir="rtl" style={{ background: bg, border: `1px solid ${border}30`, borderRadius: '14px', padding: '18px 20px', transition: 'transform 0.2s' }}
                      onMouseEnter={e => e.currentTarget.style.transform='translateY(-2px)'}
                      onMouseLeave={e => e.currentTarget.style.transform='translateY(0)'}>
                      <div className="d-flex justify-content-between align-items-start gap-3 flex-wrap">
                        <div className="d-flex align-items-start gap-3 flex-grow-1">
                          <div style={{ width: '44px', height: '44px', borderRadius: '12px', background: `${border}20`, border: `1px solid ${border}40`, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.4rem', flexShrink: 0 }}>
                            {icon}
                          </div>
                          <div className="flex-grow-1">
                            <h4 style={{ color: '#f1f5f9', fontWeight: '600', fontSize: '0.95rem', marginBottom: '6px' }}>{item.title}</h4>
                            <p style={{ color: '#94a3b8', fontSize: '0.85rem', lineHeight: '1.7', marginBottom: '0' }}>{item.description || item.content || item.message}</p>
                          </div>
                        </div>
                        <span style={{ background: `${border}20`, color: border, border: `1px solid ${border}50`, borderRadius: '20px', padding: '3px 12px', fontSize: '0.75rem', fontWeight: '600', whiteSpace: 'nowrap', flexShrink: 0 }}>
                          {cat}
                        </span>
                      </div>
                    </div>
                  );
                })
              }
            </div>
          </div>

        </div>
      </div>
    </>
  );
};

export default Analytics;
