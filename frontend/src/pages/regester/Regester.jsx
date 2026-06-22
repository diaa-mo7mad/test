import { useState } from "react";
import { useNavigate } from "react-router-dom";
import style from "./Regester.module.css";
import Btn from "../../component/btn/Btn";
import { register, login } from "../../services/api";
import { useAuth } from "../../context/AuthContext";

function Register() {
  const navigate = useNavigate();
  const { saveLogin } = useAuth();
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '', confirmPassword: '' });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  const set = (field) => (e) => {
    setForm({ ...form, [field]: e.target.value });
    setErrors(prev => ({ ...prev, [field]: '', general: '' }));
  };

  const validate = () => {
    const e = {};
    if (!form.firstName.trim())       e.firstName = 'الاسم الأول مطلوب';
    if (!form.lastName.trim())        e.lastName  = 'اسم العائلة مطلوب';
    if (!form.email.trim())           e.email     = 'البريد الإلكتروني مطلوب';
    if (form.password.length < 6)     e.password  = 'كلمة المرور 6 أحرف على الأقل';
    if (form.password !== form.confirmPassword) e.confirmPassword = 'كلمتا المرور غير متطابقتين';
    return e;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const fieldErrors = validate();
    if (Object.keys(fieldErrors).length > 0) { setErrors(fieldErrors); return; }

    setLoading(true);
    try {
      await register(form);
      // login تلقائي بعد التسجيل
      const loginRes = await login({ email: form.email, password: form.password });
      const { accessToken, token, ...userData } = loginRes.data;
      saveLogin(accessToken || token, userData);
      navigate('/Domes');
    } catch (err) {
      const data = err.response?.data;
      const msg = Array.isArray(data)
        ? data.map(e => e.description).join(', ')
        : (data?.message || 'فشل التسجيل، حاول مجدداً');
      setErrors({ general: msg });
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="d-flex justify-content-center align-items-center vh-100">
      <div className={`rounded-4 ${style.back}`}>
        <h1 className="text-center my-3">Agridome</h1>
        <Btn regester="1" />
        <form onSubmit={handleSubmit}>
          {errors.general && <div className="alert alert-danger py-2">{errors.general}</div>}

          <div className="form-floating mb-1">
            <input type="text"
              className={`form-control ${style.darkInput} ${errors.firstName ? 'is-invalid' : ''}`}
              id="firstName" placeholder="First Name"
              value={form.firstName} onChange={set('firstName')} />
            <label htmlFor="firstName" className={style.darkLabel}>First Name</label>
          </div>
          {errors.firstName && <div className="text-danger small mb-2 px-1">{errors.firstName}</div>}

          <div className="form-floating mb-1">
            <input type="text"
              className={`form-control ${style.darkInput} ${errors.lastName ? 'is-invalid' : ''}`}
              id="lastName" placeholder="Last Name"
              value={form.lastName} onChange={set('lastName')} />
            <label htmlFor="lastName" className={style.darkLabel}>Last Name</label>
          </div>
          {errors.lastName && <div className="text-danger small mb-2 px-1">{errors.lastName}</div>}

          <div className="form-floating mb-1">
            <input type="email"
              className={`form-control ${style.darkInput} ${errors.email ? 'is-invalid' : ''}`}
              id="floatingEmail" placeholder="name@example.com"
              value={form.email} onChange={set('email')} />
            <label htmlFor="floatingEmail" className={style.darkLabel}>Email</label>
          </div>
          {errors.email && <div className="text-danger small mb-2 px-1">{errors.email}</div>}

          <div className="form-floating mb-1">
            <input type="password"
              className={`form-control ${style.darkInput} ${errors.password ? 'is-invalid' : ''}`}
              id="floatingPassword" placeholder="Password"
              value={form.password} onChange={set('password')} />
            <label htmlFor="floatingPassword" className={style.darkLabel}>Password</label>
          </div>
          {errors.password && <div className="text-danger small mb-2 px-1">{errors.password}</div>}

          <div className="form-floating mb-1">
            <input type="password"
              className={`form-control ${style.darkInput} ${errors.confirmPassword ? 'is-invalid' : ''}`}
              id="confirmPassword" placeholder="Confirm Password"
              value={form.confirmPassword} onChange={set('confirmPassword')} />
            <label htmlFor="confirmPassword" className={style.darkLabel}>Confirm Password</label>
          </div>
          {errors.confirmPassword && <div className="text-danger small mb-2 px-1">{errors.confirmPassword}</div>}

          <button type="submit" className={style.Formbtn} disabled={loading}>
            {loading ? 'جارٍ التسجيل...' : 'REGISTER'}
          </button>
        </form>
      </div>
    </section>
  );
}

export default Register;
