import React, { useState, useEffect } from 'react';
import { Settings as SettingsIcon, User, Mail, Lock, Upload } from 'lucide-react';
import styles from './Settings.module.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import Navbar from '../../component/navBar/Navbar';
import { getProfile, updateProfile, changePassword } from '../../services/api';
import { useAuth } from '../../context/AuthContext';

const Settings = () => {
  const { user, saveLogin, token } = useAuth();
  const [profile, setProfile] = useState({ firstName: '', lastName: '', email: '', country: '', city: '', phoneNumber: '' });
  const [passwords, setPasswords] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [profileMsg, setProfileMsg] = useState('');
  const [passMsg, setPassMsg] = useState('');
  const [saving, setSaving] = useState(false);
  const [changingPass, setChangingPass] = useState(false);

  useEffect(() => {
    getProfile().then(r => {
      const d = r.data;
      setProfile({
        firstName: d.firstName || '',
        lastName: d.lastName || '',
        email: d.email || '',
        country: d.country || '',
        city: d.city || '',
        phoneNumber: d.phoneNumber || ''
      });
    }).catch(console.error);
  }, []);

  const setP = (field) => (e) => setProfile({ ...profile, [field]: e.target.value });
  const setPw = (field) => (e) => setPasswords({ ...passwords, [field]: e.target.value });

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setSaving(true);
    setProfileMsg('');
    try {
      const res = await updateProfile(profile);
      saveLogin(token, { ...user, ...res.data });
      setProfileMsg('Profile updated successfully!');
    } catch (err) {
      setProfileMsg(err.response?.data?.message || 'Failed to update profile');
    } finally { setSaving(false); }
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (passwords.newPassword !== passwords.confirmPassword) {
      setPassMsg('Passwords do not match'); return;
    }
    setChangingPass(true);
    setPassMsg('');
    try {
      await changePassword(passwords);
      setPassMsg('Password changed successfully!');
      setPasswords({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch (err) {
      setPassMsg(err.response?.data?.message || err.response?.data || 'Failed to change password');
    } finally { setChangingPass(false); }
  };

  const initials = `${profile.firstName?.[0] || ''}${profile.lastName?.[0] || ''}`.toUpperCase() || 'U';

  return (
    <>
      <Navbar />
      <div className={styles.pageWrapper}>
        <div className="container py-5">

          {/* Header */}
          <div className={`card border-0 mb-4 ${styles.darkCard}`}>
            <div className="card-body p-4 d-flex align-items-center">
              <div className={styles.headerIconBox}><SettingsIcon size={28} /></div>
              <div className="ms-3">
                <h3 className="card-title text-white mb-1 fw-bold">System Settings</h3>
                <p className="card-text text-white-50 mb-0">Configure your Agridome system preferences</p>
              </div>
            </div>
          </div>

          {/* Profile Picture */}
          <div className={`card border-0 mb-4 ${styles.darkCard}`}>
            <div className="card-body p-4">
              <h5 className="text-white fw-bold mb-2">Profile Picture</h5>
              <p className="text-white-50 small mb-4">Update your profile photo</p>
              <div className="d-flex align-items-center gap-3">
                <div className={styles.avatarCircle}>{initials}</div>
                <div>
                  <button className={`btn btn-outline-light mb-2 d-flex align-items-center gap-2 ${styles.btnDark}`}>
                    <Upload size={16} /> Upload Photo
                  </button>
                  <div className="text-white-50 small">JPG, PNG or GIF. Max 2MB</div>
                </div>
              </div>
            </div>
          </div>

          {/* Personal Information */}
          <div className={`card border-0 mb-4 ${styles.darkCard}`}>
            <div className="card-body p-4">
              <h5 className="text-white fw-bold mb-2">Personal Information</h5>
              <p className="text-white-50 small mb-4">Update your personal details</p>

              {profileMsg && (
                <div className={`alert py-2 ${profileMsg.includes('success') ? 'alert-success' : 'alert-danger'}`}>
                  {profileMsg}
                </div>
              )}

              <form onSubmit={handleSaveProfile}>
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label text-light small">First Name</label>
                    <div className="input-group">
                      <span className={`input-group-text ${styles.inputGroupText}`}><User size={18} /></span>
                      <input type="text" className={`form-control ${styles.darkInput}`}
                        value={profile.firstName} onChange={setP('firstName')} />
                    </div>
                  </div>
                  <div className="col-md-6">
                    <label className="form-label text-light small">Last Name</label>
                    <div className="input-group">
                      <span className={`input-group-text ${styles.inputGroupText}`}><User size={18} /></span>
                      <input type="text" className={`form-control ${styles.darkInput}`}
                        value={profile.lastName} onChange={setP('lastName')} />
                    </div>
                  </div>
                  <div className="col-12">
                    <label className="form-label text-light small">Email Address</label>
                    <div className="input-group">
                      <span className={`input-group-text ${styles.inputGroupText}`}><Mail size={18} /></span>
                      <input type="email" className={`form-control ${styles.darkInput}`}
                        value={profile.email} onChange={setP('email')} />
                    </div>
                  </div>
                  <div className="col-md-6">
                    <label className="form-label text-light small">Country</label>
                    <input type="text" className={`form-control ${styles.darkInput}`}
                      value={profile.country} onChange={setP('country')} />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label text-light small">City</label>
                    <input type="text" className={`form-control ${styles.darkInput}`}
                      value={profile.city} onChange={setP('city')} />
                  </div>
                  <div className="col-12">
                    <label className="form-label text-light small">Phone Number</label>
                    <input type="tel" className={`form-control ${styles.darkInput}`}
                      value={profile.phoneNumber} onChange={setP('phoneNumber')} />
                  </div>
                </div>
                <div className="mt-4 text-end">
                  <button type="submit" className="btn btn-success px-4" disabled={saving}>
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </form>
            </div>
          </div>

          {/* Password */}
          <div className={`card border-0 mb-4 ${styles.darkCard}`}>
            <div className="card-body p-4">
              <h5 className="text-white fw-bold mb-2">Password</h5>
              <p className="text-white-50 small mb-4">Change your password to keep your account secure</p>

              {passMsg && (
                <div className={`alert py-2 ${passMsg.includes('success') ? 'alert-success' : 'alert-danger'}`}>
                  {passMsg}
                </div>
              )}

              <form onSubmit={handleChangePassword}>
                <div className="mb-3">
                  <label className="form-label text-light small">Current Password</label>
                  <div className="input-group">
                    <span className={`input-group-text ${styles.inputGroupText}`}><Lock size={18} /></span>
                    <input type="password" className={`form-control ${styles.darkInput}`}
                      placeholder="Enter current password"
                      value={passwords.currentPassword} onChange={setPw('currentPassword')} required />
                  </div>
                </div>
                <div className="mb-3">
                  <label className="form-label text-light small">New Password</label>
                  <div className="input-group">
                    <span className={`input-group-text ${styles.inputGroupText}`}><Lock size={18} /></span>
                    <input type="password" className={`form-control ${styles.darkInput}`}
                      placeholder="Enter new password"
                      value={passwords.newPassword} onChange={setPw('newPassword')} required />
                  </div>
                </div>
                <div className="mb-4">
                  <label className="form-label text-light small">Confirm Password</label>
                  <div className="input-group">
                    <span className={`input-group-text ${styles.inputGroupText}`}><Lock size={18} /></span>
                    <input type="password" className={`form-control ${styles.darkInput}`}
                      placeholder="Confirm new password"
                      value={passwords.confirmPassword} onChange={setPw('confirmPassword')} required />
                  </div>
                </div>
                <div className="text-end">
                  <button type="submit" className="btn btn-success px-4" disabled={changingPass}>
                    {changingPass ? 'Updating...' : 'Update Password'}
                  </button>
                </div>
              </form>
            </div>
          </div>

        </div>
      </div>
    </>
  );
};

export default Settings;
