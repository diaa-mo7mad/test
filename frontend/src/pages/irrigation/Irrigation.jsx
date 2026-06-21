import React, { useState, useEffect } from 'react';
import { MdOutlineWaterDrop } from "react-icons/md";
import { ImConnection } from "react-icons/im";
import { LuBrain } from "react-icons/lu";
import { BsLightningChargeFill } from "react-icons/bs";
import { Droplets, Play, CircleOff, CheckCircle, Trash2, Plus, Clock } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';

import style from './Irrigation.module.css';
import Navbar from '../../component/navBar/Navbar';
import {
  getLatestReading, irrigateDome, stopIrrigation, getDome, getSchedules,
  addSchedule, deleteSchedule, getAnalytics, updateDome, suggestAiSchedule
} from '../../services/api';
import { useAuth } from '../../context/AuthContext';

function Irrigation() {
  const { selectedDomeId } = useAuth();
  const [dome, setDome] = useState(null);
  const [latest, setLatest] = useState(null);
  const [schedules, setSchedules] = useState([]);
  const [analytics, setAnalytics] = useState(null);
  const [isAiActive, setIsAiActive] = useState(false);
  const [irrigating, setIrrigating] = useState(false);
  const [showAddForm, setShowAddForm] = useState(false);
  const [newSchedule, setNewSchedule] = useState({ startTime: '', durationMinutes: 15, isRepeatDaily: true });
  const [aiScheduling, setAiScheduling] = useState(false);
  const [aiMessage, setAiMessage] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!selectedDomeId) return;
    Promise.all([
      getDome(selectedDomeId),
      getLatestReading(selectedDomeId),
      getSchedules(selectedDomeId),
      getAnalytics(selectedDomeId)
    ]).then(([domeRes, latestRes, schRes, anRes]) => {
      setDome(domeRes.data);
      setIsAiActive(domeRes.data?.isAiEnabled || false);
      setLatest(latestRes.data);
      setSchedules(schRes.data || []);
      setAnalytics(anRes.data);
    }).catch(console.error).finally(() => setLoading(false));
  }, [selectedDomeId]);

  // تحديث دوري لحالة الصمام والقراءة (كل 5 ثواني) عشان يبيّن إذا المضخة اشتغلت/توقفت تلقائياً
  useEffect(() => {
    if (!selectedDomeId) return;
    const t = setInterval(() => {
      getDome(selectedDomeId).then(r => setDome(r.data)).catch(() => {});
      getLatestReading(selectedDomeId).then(r => setLatest(r.data)).catch(() => {});
    }, 5000);
    return () => clearInterval(t);
  }, [selectedDomeId]);

  const handleIrrigate = async () => {
    if (!selectedDomeId) return;
    setIrrigating(true);
    try {
      await irrigateDome(selectedDomeId);
      const r = await getDome(selectedDomeId);
      setDome(r.data);
    } finally { setIrrigating(false); }
  };

  const handleStop = async () => {
    if (!selectedDomeId) return;
    setIrrigating(true);
    try {
      await stopIrrigation(selectedDomeId);
      const r = await getDome(selectedDomeId);
      setDome(r.data);
    } finally { setIrrigating(false); }
  };

  const handleAiToggle = async () => {
    if (!selectedDomeId || !dome) return;
    const newVal = !isAiActive;
    setIsAiActive(newVal);
    try {
      // نرسل بيانات المزرعة كاملة (الباك إند يتطلب Name و MacAddress)
      await updateDome(selectedDomeId, {
        name: dome.name,
        macAddress: dome.macAddress,
        country: dome.country,
        governorate: dome.governorate,
        neighborhood: dome.neighborhood,
        plantType: dome.plantType,
        soilType: dome.soilType,
        minTargetMoisture: dome.minTargetMoisture,
        isAiEnabled: newVal,
      });
      setDome({ ...dome, isAiEnabled: newVal });
    } catch { setIsAiActive(!newVal); }
  };

  const handleAddSchedule = async (e) => {
    e.preventDefault();
    try {
      const res = await addSchedule({ ...newSchedule, domeId: selectedDomeId });
      setSchedules(prev => [...prev, res.data]);
      setShowAddForm(false);
      setNewSchedule({ startTime: '', durationMinutes: 15, isRepeatDaily: true });
    } catch (err) { alert(err.response?.data?.message || 'Failed to add schedule'); }
  };

  const handleDeleteSchedule = async (id) => {
    await deleteSchedule(id);
    setSchedules(prev => prev.filter(s => s.id !== id));
  };

  const handleAiSchedule = async () => {
    if (!selectedDomeId) return;
    setAiScheduling(true);
    setAiMessage('');
    try {
      const res = await suggestAiSchedule(selectedDomeId);
      setAiMessage(`✅ ${res.data.message}`);
      // أعد تحميل الجداول
      const schRes = await getSchedules(selectedDomeId);
      setSchedules(schRes.data || []);
    } catch (err) {
      setAiMessage('❌ ' + (err.response?.data?.message || 'فشل الاقتراح'));
    } finally {
      setAiScheduling(false);
    }
  };

  // date already comes as "Mon","Tue" string — use directly
  const weeklyData = analytics?.dailyData?.map((d, i) => ({
    name: d.date || `Day ${i + 1}`,
    liters: Math.round((d.waterConsumption || 0) * 10) / 10
  })) || [];

  const valveOpen = dome?.isManualWateringRequested;
  const lastWatered = dome?.lastWateredAt
    ? new Date(dome.lastWateredAt).toLocaleString()
    : 'Never';

  if (loading) return <><Navbar /><div className="text-center text-white mt-5">Loading...</div></>;

  return (
    <>
      <Navbar />
      <section className="container mt-4 pb-5">
        <div className="d-flex flex-column gap-3">

          {/* Header card */}
          <div className={`${style.Controls} d-flex flex-wrap justify-content-between align-items-center gap-3`}>
            <div className="d-flex align-items-center gap-3">
              <div className={style.irgi_icon}><MdOutlineWaterDrop /></div>
              <div>
                <h2 className={style.h2}>Irrigation Control</h2>
                <p className={style.p}>Smart watering system</p>
              </div>
            </div>
            <div className={`${style.statusGroup} d-flex flex-column align-items-start align-items-sm-end`}>
              <div className="d-flex align-items-center gap-2">
                <span className={style.connec_green}><ImConnection /></span>
                <span className={style.active_badge_green}>Online</span>
              </div>
              <p className={style.p_small}>Last watered: {lastWatered}</p>
            </div>
          </div>

          {/* AI card */}
          <div className={`${style.Controls} d-flex flex-wrap justify-content-between align-items-center gap-3`}>
            <div className="d-flex align-items-center gap-3">
              <div className={style.icon}><LuBrain /></div>
              <div>
                <h2 className={style.h2}>AI-Powered Irrigation</h2>
                <p className={style.p}>Smart optimization based on needs</p>
              </div>
            </div>
            <div className="d-flex align-items-center gap-3">
              {isAiActive && (
                <div className={style.active_badge_purple}>
                  <BsLightningChargeFill />
                  <span className="ms-1">Active</span>
                </div>
              )}
              <label className={style.custom_switch}>
                <input type="checkbox" checked={isAiActive} onChange={handleAiToggle} />
                <span className={style.slider}></span>
              </label>
            </div>
          </div>

          {/* Zone info + Chart */}
          <div className="row g-3">
            <div className="col-12 col-lg-6">
              <div className={style.ZoneCard}>
                <div className="d-flex justify-content-between align-items-center mb-4">
                  <div className="d-flex align-items-center gap-3">
                    <div className={style.ZoneIcon}><Droplets size={24} /></div>
                    <div>
                      <h3 className={style.h3}>Soil and irrigation information</h3>
                    </div>
                  </div>
                </div>

                <div className="mb-4">
                  <div className="d-flex justify-content-between align-items-center mb-1">
                    <span className={style.label}>Soil Moisture</span>
                    <span className={style.value_green}>
                      {(latest?.soilMoisture ?? latest?.soilMoisturePercent)?.toFixed(1) ?? '--'}%
                    </span>
                  </div>
                  <div className={style.ProgressBg}>
                    <div className={style.ProgressBar} style={{ width: `${latest?.soilMoisture ?? latest?.soilMoisturePercent ?? 0}%` }}></div>
                  </div>
                </div>

                <div className="d-flex justify-content-between align-items-center mb-3">
                  <span className={style.label}>Valve Status</span>
                  <div className={valveOpen ? style.Badge_Open : style.Badge_Muted}>
                    {valveOpen
                      ? <><CheckCircle size={14} className="me-2" /><span>Open</span></>
                      : <><CircleOff size={14} className="me-2" /><span>Closed</span></>
                    }
                  </div>
                </div>

                <div className="d-flex justify-content-between align-items-center mb-4">
                  <span className={style.label}>Last Watered</span>
                  <span className={style.value_text}>{lastWatered}</span>
                </div>

                {valveOpen ? (
                  <button className={style.RunButton} onClick={handleStop} disabled={irrigating}
                    style={{ background: 'linear-gradient(135deg,#ef4444,#dc2626)' }}>
                    <CircleOff size={16} className="me-2" />
                    {irrigating ? '...' : 'إيقاف السقاية'}
                  </button>
                ) : (
                  <button className={style.RunButton} onClick={handleIrrigate} disabled={irrigating}>
                    <Play size={16} className="me-2" fill="currentColor" />
                    {irrigating ? '...' : 'Run Now'}
                  </button>
                )}
              </div>
            </div>

            <div className="col-12 col-lg-6">
              <div className={style.Controls}>
                <h2 className={`${style.h2} mb-4`}>Water Consumption</h2>
                <div style={{ width: '100%', height: 260 }}>
                  <ResponsiveContainer>
                    <BarChart data={weeklyData.length > 0 ? weeklyData : [{ name: 'No data', liters: 0 }]}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#3d3f47" />
                      <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#9ca3af', fontSize: 12 }} />
                      <YAxis hide />
                      <Tooltip
                        cursor={{ fill: 'rgba(255,255,255,0.05)' }}
                        contentStyle={{ backgroundColor: '#1e1f25', border: '1px solid #3d3f47', borderRadius: '10px' }}
                        itemStyle={{ color: '#a855f7', fontWeight: 'bold' }}
                      />
                      <Bar dataKey="liters" radius={[6, 6, 0, 0]} barSize={40}>
                        {(weeklyData.length > 0 ? weeklyData : [{}]).map((_, index) => (
                          <Cell key={index} fill={index === weeklyData.length - 1 ? '#a855f7' : '#38bdf8'} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </div>
          </div>

          {/* Schedules */}
          <div className={style.Controls}>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h2 className={style.h2}>Irrigation Schedule</h2>
              <div className="d-flex gap-2">
                <button className="btn btn-sm btn-outline-light" onClick={() => setShowAddForm(!showAddForm)} title="Add manually">
                  <Plus size={16} className="me-1" /> Manual
                </button>
                <button className="btn btn-sm" onClick={handleAiSchedule} disabled={aiScheduling}
                  style={{ background: 'linear-gradient(135deg,#a855f7,#6366f1)', color: '#fff', border: 'none', borderRadius: '8px', padding: '6px 14px' }}>
                  <LuBrain size={15} className="me-1" />
                  {aiScheduling ? 'Thinking...' : '🧠 AI Schedule'}
                </button>
              </div>
            </div>
            {aiMessage && (
              <div className={`alert py-2 mb-3 ${aiMessage.startsWith('✅') ? 'alert-success' : 'alert-danger'}`} style={{ fontSize: '0.85rem' }}>
                {aiMessage}
              </div>
            )}

            {showAddForm && (
              <form onSubmit={handleAddSchedule} className="mb-4 p-3" style={{ background: 'rgba(255,255,255,0.05)', borderRadius: '10px' }}>
                <div className="row g-2">
                  <div className="col-md-4">
                    <label className="text-light small">Start Time</label>
                    <input type="datetime-local" className="form-control form-control-sm"
                      style={{ background: '#1e1f25', color: '#fff', border: '1px solid #3d3f47' }}
                      value={newSchedule.startTime}
                      onChange={e => setNewSchedule({ ...newSchedule, startTime: e.target.value })} required />
                  </div>
                  <div className="col-md-3">
                    <label className="text-light small">Duration (min)</label>
                    <input type="number" min="1" max="120" className="form-control form-control-sm"
                      style={{ background: '#1e1f25', color: '#fff', border: '1px solid #3d3f47' }}
                      value={newSchedule.durationMinutes}
                      onChange={e => setNewSchedule({ ...newSchedule, durationMinutes: +e.target.value })} />
                  </div>
                  <div className="col-md-3 d-flex align-items-end">
                    <div className="form-check">
                      <input type="checkbox" className="form-check-input" id="repeatDaily"
                        checked={newSchedule.isRepeatDaily}
                        onChange={e => setNewSchedule({ ...newSchedule, isRepeatDaily: e.target.checked })} />
                      <label htmlFor="repeatDaily" className="form-check-label text-light small">Repeat Daily</label>
                    </div>
                  </div>
                  <div className="col-md-2 d-flex align-items-end">
                    <button type="submit" className="btn btn-success btn-sm w-100">Save</button>
                  </div>
                </div>
              </form>
            )}

            <div className={style.ScheduleList}>
              {schedules.length === 0
                ? (
                  <div className="text-center py-4" style={{ color: '#64748b' }}>
                    <div style={{ fontSize: '2rem', marginBottom: '8px' }}>📅</div>
                    <p style={{ color: '#94a3b8', fontSize: '0.9rem' }}>No schedules yet — use <strong>AI Schedule</strong> or add manually</p>
                  </div>
                )
                : schedules.map((item, idx) => {
                  const t = new Date(item.startTime);
                  const timeStr = isNaN(t) ? item.startTime : t.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                  const hour = isNaN(t) ? 12 : t.getHours();
                  const period = hour < 12 ? 'Morning' : hour < 17 ? 'Afternoon' : 'Evening';
                  const periodIcon = hour < 12 ? '🌅' : hour < 17 ? '☀️' : '🌙';
                  const accentColors = ['#38bdf8','#a855f7','#10b981','#f59e0b'];
                  const accent = accentColors[idx % accentColors.length];
                  const statusDone = item.isExecuted;
                  return (
                    <div key={item.id} style={{
                      background: `linear-gradient(135deg, ${accent}0a 0%, rgba(255,255,255,0.03) 100%)`,
                      border: `1px solid ${accent}25`,
                      borderLeft: `3px solid ${accent}`,
                      borderRadius: '14px', padding: '16px 20px', marginBottom: '10px',
                      display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '16px',
                      transition: 'transform 0.15s, box-shadow 0.15s'
                    }}
                      onMouseEnter={e => { e.currentTarget.style.transform='translateY(-2px)'; e.currentTarget.style.boxShadow=`0 4px 20px ${accent}20`; }}
                      onMouseLeave={e => { e.currentTarget.style.transform='translateY(0)'; e.currentTarget.style.boxShadow='none'; }}>

                      {/* الوقت + الفترة */}
                      <div style={{ display: 'flex', alignItems: 'center', gap: '12px', minWidth: '160px' }}>
                        <div style={{ width: '46px', height: '46px', borderRadius: '13px', background: `${accent}18`, border: `1px solid ${accent}40`, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.3rem', flexShrink: 0 }}>
                          {periodIcon}
                        </div>
                        <div>
                          <div style={{ color: '#fff', fontWeight: '700', fontSize: '1.15rem', letterSpacing: '0.5px' }}>{timeStr}</div>
                          <div style={{ color: '#64748b', fontSize: '0.75rem', marginTop: '1px' }}>{period}</div>
                        </div>
                      </div>

                      {/* المدة */}
                      <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Droplets size={14} color={accent} />
                        <span style={{ color: accent, fontWeight: '600', fontSize: '0.9rem' }}>{item.durationMinutes} min</span>
                      </div>

                      {/* الحالة */}
                      <div style={{ background: statusDone ? 'rgba(16,185,129,0.12)' : 'rgba(245,158,11,0.12)', border: `1px solid ${statusDone ? '#10b981' : '#f59e0b'}40`, borderRadius: '20px', padding: '4px 14px', color: statusDone ? '#10b981' : '#f59e0b', fontWeight: '500', fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '5px' }}>
                        {statusDone ? '✅' : '⏳'} {statusDone ? 'Done' : 'Pending'}
                      </div>

                      {/* التكرار */}
                      <div style={{ color: '#64748b', fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '5px', minWidth: '60px' }}>
                        {item.isRepeatDaily ? '🔁 Daily' : '1️⃣ Once'}
                      </div>

                      {/* حذف */}
                      <button onClick={() => handleDeleteSchedule(item.id)}
                        style={{ background: 'rgba(239,68,68,0.08)', border: '1px solid rgba(239,68,68,0.25)', borderRadius: '10px', padding: '8px 11px', color: '#ef4444', cursor: 'pointer', display: 'flex', alignItems: 'center', transition: 'all 0.2s', flexShrink: 0 }}
                        onMouseEnter={e => { e.currentTarget.style.background='rgba(239,68,68,0.22)'; e.currentTarget.style.borderColor='rgba(239,68,68,0.6)'; }}
                        onMouseLeave={e => { e.currentTarget.style.background='rgba(239,68,68,0.08)'; e.currentTarget.style.borderColor='rgba(239,68,68,0.25)'; }}>
                        <Trash2 size={15} />
                      </button>
                    </div>
                  );
                })
              }
            </div>
          </div>

        </div>
      </section>
    </>
  );
}

export default Irrigation;
