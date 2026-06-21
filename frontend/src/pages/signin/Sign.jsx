import { Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import Btn from "../../component/btn/Btn";
import style from "./Sign.module.css";
import { login, getDomes } from "../../services/api";
import { useAuth } from "../../context/AuthContext";

function Sign() {
  const navigate = useNavigate();
  const { saveLogin, selectDome } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await login({ email, password });
      const { accessToken, token, ...userData } = res.data;
      saveLogin(accessToken || token, userData);

      // جلب أول dome للمستخدم
      const domesRes = await getDomes();
      if (domesRes.data?.length > 0) {
        selectDome(domesRes.data[0].id);
      }

      navigate('/Domes');
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="d-flex justify-content-center align-items-center vh-100">
      <div className={`rounded-4 ${style.back}`}>
        <h1 className="text-center my-3">Argi-Dome</h1>
        <Btn regester="0" />
        <form onSubmit={handleSubmit}>
          {error && <div className="alert alert-danger py-2">{error}</div>}

          <div className="form-floating mb-3">
            <input
              type="email"
              className={`form-control ${style.darkInput}`}
              id="floatingInput"
              placeholder="name@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <label htmlFor="floatingInput" className={style.darkLabel}>Email address</label>
          </div>

          <div className="form-floating mb-3">
            <input
              type="password"
              className={`form-control ${style.darkInput}`}
              id="floatingPassword"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
            <label htmlFor="floatingPassword" className={style.darkLabel}>Password</label>
          </div>

          <Link to="/Forget" className={`mx-2 my-3 d-block ${style.linkStyle}`}>Forget Password</Link>

          <button type="submit" className={style.Formbtn} disabled={loading}>
            {loading ? 'Signing in...' : 'Submit'}
          </button>
        </form>
      </div>
    </section>
  );
}

export default Sign;
