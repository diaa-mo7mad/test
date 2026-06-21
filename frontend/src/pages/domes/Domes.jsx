import React, { useState, useEffect, useCallback } from 'react';
import styles from './Domes.module.css';
import { getDomes, createDome, updateDome, deleteDome } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const emptyForm = {
  name: '', macAddress: '', country: '', governorate: '', neighborhood: '',
  plantType: '', soilType: '', isAiEnabled: true, minTargetMoisture: 30,
};

const Domes = () => {
  const { selectedDomeId, selectDome, logout } = useAuth();
  const navigate = useNavigate();

  const [domes, setDomes]         = useState([]);
  const [loading, setLoading]     = useState(true);
  const [modal, setModal]         = useState(null); // null | 'add' | 'edit' | 'delete'
  const [editing, setEditing]     = useState(null); // dome object being edited/deleted
  const [form, setForm]           = useState(emptyForm);
  const [saving, setSaving]       = useState(false);
  const [error, setError]         = useState('');

  const loadDomes = useCallback(async () => {
    try {
      const r = await getDomes();
      const list = Array.isArray(r.data) ? r.data : (r.data?.value || []);
      setDomes(list);
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { loadDomes(); }, [loadDomes]);

  const openAdd = () => {
    setForm(emptyForm);
    setEditing(null);
    setError('');
    setModal('add');
  };

  const openEdit = (dome, e) => {
    e.stopPropagation();
    setForm({
      name: dome.name || '',
      macAddress: dome.macAddress || '',
      country: dome.country || '',
      governorate: dome.governorate || '',
      neighborhood: dome.neighborhood || '',
      plantType: dome.plantType || 'Tomato',
      soilType: dome.soilType || 'Loamy',
      isAiEnabled: dome.isAiEnabled ?? true,
      minTargetMoisture: dome.minTargetMoisture ?? 30,
    });
    setEditing(dome);
    setError('');
    setModal('edit');
  };

  const openDelete = (dome, e) => {
    e.stopPropagation();
    setEditing(dome);
    setError('');
    setModal('delete');
  };

  const closeModal = () => { setModal(null); setEditing(null); setError(''); };

  const handleField = (key, value) => setForm(f => ({ ...f, [key]: value }));

  const handleSave = async () => {
    if (!form.name.trim())       { setError('اسم المزرعة مطلوب.'); return; }
    if (!form.macAddress.trim()) { setError('عنوان MAC مطلوب.'); return; }

    setSaving(true); setError('');
    try {
      if (modal === 'add') {
        const r = await createDome(form);
        if (r.data?.success === false) { setError(r.data.message || 'Failed to create.'); return; }
        // auto-select the new dome
        const newDomes = await getDomes();
        const list = Array.isArray(newDomes.data) ? newDomes.data : (newDomes.data?.value || []);
        setDomes(list);
        const created = list.find(d => d.macAddress === form.macAddress);
        if (created) selectDome(created.id);
      } else {
        const r = await updateDome(editing.id, form);
        if (r.data?.success === false) { setError(r.data.message || 'Failed to update.'); return; }
        await loadDomes();
      }
      closeModal();
    } catch (e) {
      setError(e.response?.data?.message || e.response?.data?.errors?.[0] || 'حدث خطأ، حاول مجدداً.');
    } finally { setSaving(false); }
  };

  const handleDelete = async () => {
    setSaving(true); setError('');
    try {
      const r = await deleteDome(editing.id);
      if (r.data?.success === false) { setError(r.data.message || 'Failed to delete.'); return; }
      if (selectedDomeId == editing.id) {
        const remaining = domes.filter(d => d.id !== editing.id);
        selectDome(remaining.length > 0 ? remaining[0].id : null);
      }
      await loadDomes();
      closeModal();
    } catch (e) {
      setError(e.response?.data?.message || 'حدث خطأ، حاول مجدداً.');
    } finally { setSaving(false); }
  };

  const handleSelectAndGo = (dome) => {
    selectDome(dome.id);
    navigate('/dashboard');
  };

  const handleLogout = () => { logout(); navigate('/'); };

  return (
    <>
      <div className={styles.pageContainer}>
        <div className="container py-4">

          {/* Header */}
          <div className="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-3">
            <div className="d-flex align-items-center gap-3">
              <div className={styles.mainIconBox}>🏡</div>
              <div>
                <h2 className={styles.h2}>مزارعي</h2>
                <p className={styles.p}>{domes.length} مزرعة مسجّلة</p>
              </div>
            </div>
            <div className="d-flex gap-2 align-items-center">
              <button className={styles.addBtn} onClick={openAdd}>
                <span style={{ fontSize: '1.1rem' }}>+</span> إضافة مزرعة
              </button>
              <button className={styles.logoutBtn} onClick={handleLogout}>
                تسجيل الخروج
              </button>
            </div>
          </div>

          {/* Content */}
          {loading ? (
            <div className="text-center py-5" style={{ color: '#64748b' }}>
              <div className="spinner-border spinner-border-sm me-2" /> جارٍ تحميل المزارع...
            </div>
          ) : domes.length === 0 ? (
            <div className={styles.emptyState}>
              <div className={styles.emptyIcon}>🌱</div>
              <p className={styles.emptyTitle}>لا توجد مزارع بعد</p>
              <p className={styles.emptyDesc}>أضف مزرعتك الذكية الأولى لبدء المراقبة</p>
              <button className={styles.addBtn} style={{ margin: '1rem auto' }} onClick={openAdd}>
                + إضافة أول مزرعة
              </button>
            </div>
          ) : (
            <div className="row g-4">
              {domes.map(dome => {
                const isActive = dome.id == selectedDomeId;
                return (
                  <div key={dome.id} className="col-12 col-md-6 col-xl-4">
                    <div
                      className={`${styles.domeCard} ${isActive ? styles.domeCardActive : ''}`}
                      onClick={() => handleSelectAndGo(dome)}
                    >
                      {isActive && <div className={styles.activeGlow} />}

                      {/* Top row */}
                      <div className="d-flex align-items-start justify-content-between mb-3">
                        <div className="d-flex align-items-center gap-3">
                          <div className={styles.domeIcon}>🏡</div>
                          <div>
                            <div className={styles.domeName}>{dome.name}</div>
                            <div className={styles.domeMac}>{dome.macAddress}</div>
                          </div>
                        </div>
                        <div className="d-flex gap-1 flex-wrap justify-content-end">
                          {isActive && <span className={`${styles.badge} ${styles.badgeActive}`}>نشطة</span>}
                          <span className={`${styles.badge} ${dome.isAiEnabled ? styles.badgeAi : styles.badgeNoAi}`}>
                            {dome.isAiEnabled ? '🤖 ذكاء اصطناعي' : 'بدون ذكاء'}
                          </span>
                        </div>
                      </div>

                      {/* Meta */}
                      <div className="d-flex flex-wrap gap-3 mb-3">
                        {(dome.country || dome.governorate) && (
                          <div className={styles.metaItem}>
                            <span className={styles.metaIcon}>📍</span>
                            {[dome.neighborhood, dome.governorate, dome.country].filter(Boolean).join(', ')}
                          </div>
                        )}
                        {dome.plantType && (
                          <div className={styles.metaItem}>
                            <span className={styles.metaIcon}>🌿</span> {dome.plantType}
                          </div>
                        )}
                        {dome.soilType && (
                          <div className={styles.metaItem}>
                            <span className={styles.metaIcon}>🪨</span> {dome.soilType}
                          </div>
                        )}
                      </div>

                      <hr className={styles.divider} />

                      {/* Actions */}
                      <div className="d-flex align-items-center justify-content-between">
                        <div style={{ fontSize: '0.78rem', color: '#64748b' }}>
                          الرطوبة الدنيا: <strong style={{ color: '#94a3b8' }}>{dome.minTargetMoisture ?? 30}%</strong>
                        </div>
                        <div className="d-flex gap-2">
                          <button
                            className={`${styles.actionBtn} ${styles.editBtn}`}
                            title="Edit farm"
                            onClick={(e) => openEdit(dome, e)}
                          >✏️</button>
                          <button
                            className={`${styles.actionBtn} ${styles.deleteBtn}`}
                            title="Delete farm"
                            onClick={(e) => openDelete(dome, e)}
                          >🗑️</button>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {/* Add / Edit Modal */}
      {(modal === 'add' || modal === 'edit') && (
        <div className={styles.modalOverlay} onClick={closeModal}>
          <div className={styles.modalBox} onClick={e => e.stopPropagation()}>
            <div className={styles.modalTitle}>
              {modal === 'add' ? '➕ إضافة مزرعة جديدة' : '✏️ تعديل المزرعة'}
            </div>

            <div className="row g-3">
              <div className="col-12">
                <label className={styles.formLabel}>اسم المزرعة *</label>
                <input className={styles.formInput} placeholder="مثال: البيت المحمي الشمالي"
                  value={form.name} onChange={e => handleField('name', e.target.value)} />
              </div>
              <div className="col-12">
                <label className={styles.formLabel}>عنوان MAC *</label>
                <input className={styles.formInput} placeholder="مثال: AA:BB:CC:DD:EE:FF"
                  value={form.macAddress} onChange={e => handleField('macAddress', e.target.value)} />
              </div>
              <div className="col-md-4">
                <label className={styles.formLabel}>الدولة</label>
                <input className={styles.formInput} placeholder="مثال: الأردن"
                  value={form.country} onChange={e => handleField('country', e.target.value)} />
              </div>
              <div className="col-md-4">
                <label className={styles.formLabel}>المحافظة</label>
                <input className={styles.formInput} placeholder="مثال: عمّان"
                  value={form.governorate} onChange={e => handleField('governorate', e.target.value)} />
              </div>
              <div className="col-md-4">
                <label className={styles.formLabel}>الحي / المنطقة</label>
                <input className={styles.formInput} placeholder="مثال: الجبيهة"
                  value={form.neighborhood} onChange={e => handleField('neighborhood', e.target.value)} />
              </div>
              <div className="col-md-6">
                <label className={styles.formLabel}>نوع النبات</label>
                <input className={styles.formInput} placeholder="مثال: طماطم، خيار، فراولة..."
                  value={form.plantType} onChange={e => handleField('plantType', e.target.value)} />
              </div>
              <div className="col-md-6">
                <label className={styles.formLabel}>نوع التربة</label>
                <input className={styles.formInput} placeholder="مثال: رملية، طينية، طمية..."
                  value={form.soilType} onChange={e => handleField('soilType', e.target.value)} />
              </div>
              <div className="col-12">
                <label className={styles.formLabel}>الحد الأدنى للرطوبة المستهدفة (%)</label>
                <input className={styles.formInput} type="number" min="0" max="100"
                  placeholder="مثال: 30"
                  value={form.minTargetMoisture}
                  onChange={e => handleField('minTargetMoisture', parseFloat(e.target.value))} />
              </div>
              <div className="col-12">
                <div className={styles.toggleRow}>
                  <div>
                    <div className={styles.toggleLabel}>🤖 الري التلقائي بالذكاء الاصطناعي</div>
                    <div className={styles.toggleSub}>يراقب الذكاء الاصطناعي الرطوبة ويُشغّل الري تلقائياً</div>
                  </div>
                  <label className={styles.toggle}>
                    <input type="checkbox" checked={form.isAiEnabled}
                      onChange={e => handleField('isAiEnabled', e.target.checked)} />
                    <span className={styles.toggleSlider} />
                  </label>
                </div>
              </div>
            </div>

            {error && <div className={styles.errorMsg}>⚠️ {error}</div>}

            <div className={styles.modalActions}>
              <button className={styles.btnCancel} onClick={closeModal} disabled={saving}>إلغاء</button>
              <button className={styles.btnSave} onClick={handleSave} disabled={saving}>
                {saving ? 'جارٍ الحفظ...' : (modal === 'add' ? 'إنشاء المزرعة' : 'حفظ التعديلات')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirm Modal */}
      {modal === 'delete' && editing && (
        <div className={styles.modalOverlay} onClick={closeModal}>
          <div className={styles.modalBox} style={{ maxWidth: '420px' }} onClick={e => e.stopPropagation()}>
            <div style={{ textAlign: 'center', padding: '1rem 0' }}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🗑️</div>
              <div className={styles.modalTitle} style={{ justifyContent: 'center' }}>حذف المزرعة</div>
              <p style={{ color: '#94a3b8', fontSize: '0.9rem', lineHeight: '1.6' }}>
                هل أنت متأكد من حذف <strong style={{ color: '#f1f5f9' }}>{editing.name}</strong>؟
                <br />
                <span style={{ color: '#f87171', fontSize: '0.82rem' }}>
                  سيتم حذف جميع القراءات والجداول والإشعارات المرتبطة بها بشكل دائم.
                </span>
              </p>
            </div>
            {error && <div className={styles.errorMsg} style={{ textAlign: 'center' }}>⚠️ {error}</div>}
            <div className={styles.modalActions} style={{ justifyContent: 'center' }}>
              <button className={styles.btnCancel} onClick={closeModal} disabled={saving}>إلغاء</button>
              <button className={styles.btnDelete} onClick={handleDelete} disabled={saving}>
                {saving ? 'جارٍ الحذف...' : 'نعم، احذف'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default Domes;
