import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import style from './Navbar.module.css';
import { useAuth } from '../../context/AuthContext';

function Navbar() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };



  
  return (
    <nav className={`navbar navbar-expand-lg ${style.back}`}>
      <div className="container"> 
        
        {/* اللوجو */}
        <NavLink className={style.navbarLogo} to="/dashboard">
          Argidome
        </NavLink>

        {/* زر القائمة للموبايل */}
        <button 
          className="navbar-toggler" 
          type="button" 
          data-bs-toggle="collapse" 
          data-bs-target="#navbarSupportedContent" 
          aria-controls="navbarSupportedContent" 
          aria-expanded="false" 
          aria-label="Toggle navigation"
          style={{ borderColor: 'rgba(255,255,255,0.1)' }}
        >
          <span className="navbar-toggler-icon" style={{ filter: 'invert(1)' }} />
        </button>

        {/* روابط الصفحات */}
        <div className="collapse navbar-collapse" id="navbarSupportedContent">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">

            <li className="nav-item">
              <NavLink
                to="/Domes"
                className={({ isActive }) => `${style.navLink} ${isActive ? style.active : ''}`}
              >
                ← مزارعي
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink
                to="/Dashboard"
                className={({ isActive }) => `${style.navLink} ${isActive ? style.active : ''}`}
              >
                Dashboard
              </NavLink>
            </li>



             <li className="nav-item">
              <NavLink 
                to="/Irrigation" 
                className={({ isActive }) => `${style.navLink} ${isActive ? style.active : ''}`}
              >
                Irrigation
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink 
                to="/Analytics" 
                className={({ isActive }) => `${style.navLink} ${isActive ? style.active : ''}`}
              >
                Analytics
              </NavLink>
            </li>

           

            <li className="nav-item">
              <NavLink 
                to="/Settings" 
                className={({ isActive }) => `${style.navLink} ${isActive ? style.active : ''}`}
              >
                Settings
              </NavLink>
            </li>

          </ul>
          
          
          <div className="d-flex">
             <button onClick={handleLogout} className={`btn btn-link ${style.navLink} text-danger p-0`} style={{textDecoration:'none'}}>Logout</button>
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;