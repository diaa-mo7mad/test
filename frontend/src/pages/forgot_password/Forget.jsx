import { useState } from "react";
import { useNavigate } from "react-router-dom";
import style from "./Forget.module.css";
import { sendResetCode, resetPassword } from "../../services/api";

function Forget() {
  const navigate = useNavigate();
  const [step, setStep] = useState(1); // 1=email, 2=code+newpass
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSendCode = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await sendResetCode({ email });
      setStep(2);
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Failed to send code');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = async (e) => {
    e.preventDefault();
    setError('');
    if (newPassword !== confirmPassword) { setError('Passwords do not match'); return; }
    setLoading(true);
    try {
      await resetPassword({ email, code, newPassword, confirmPassword });
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Reset failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="d-flex justify-content-center align-items-center vh-100">
      <div className={`rounded-4 ${style.back}`}>
        <h1 className="text-center my-3">Argi-Dome</h1>
        {error && <div className="alert alert-danger py-2 mx-3">{error}</div>}

        {step === 1 ? (
          <form onSubmit={handleSendCode}>
            <div className="form-floating mb-3">
              <input type="email" className={`form-control ${style.darkInput}`}
                id="floatingInput" placeholder="name@example.com"
                value={email} onChange={(e) => setEmail(e.target.value)} required />
              <label htmlFor="floatingInput" className={style.darkLabel}>Email address</label>
            </div>
            <button type="submit" className={style.Formbtn} disabled={loading}>
              {loading ? 'Sending...' : 'Send Reset Code'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleReset}>
            <div className="form-floating mb-3">
              <input type="text" className={`form-control ${style.darkInput}`}
                id="code" placeholder="Reset Code"
                value={code} onChange={(e) => setCode(e.target.value)} required />
              <label htmlFor="code" className={style.darkLabel}>Reset Code</label>
            </div>
            <div className="form-floating mb-3">
              <input type="password" className={`form-control ${style.darkInput}`}
                id="newPass" placeholder="New Password"
                value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required />
              <label htmlFor="newPass" className={style.darkLabel}>New Password</label>
            </div>
            <div className="form-floating mb-3">
              <input type="password" className={`form-control ${style.darkInput}`}
                id="confirmPass" placeholder="Confirm Password"
                value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required />
              <label htmlFor="confirmPass" className={style.darkLabel}>Confirm Password</label>
            </div>
            <button type="submit" className={style.Formbtn} disabled={loading}>
              {loading ? 'Resetting...' : 'Change Password'}
            </button>
          </form>
        )}
      </div>
    </section>
  );
}

export default Forget;
