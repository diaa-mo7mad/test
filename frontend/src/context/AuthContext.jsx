import React, { createContext, useContext, useState } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('token'));
  const [user, setUser] = useState(() => {
    try { return JSON.parse(localStorage.getItem('user')); } catch { return null; }
  });
  const [selectedDomeId, setSelectedDomeId] = useState(() =>
    localStorage.getItem('selectedDomeId')
  );

  const saveLogin = (tokenValue, userData) => {
    localStorage.setItem('token', tokenValue);
    localStorage.setItem('user', JSON.stringify(userData));
    setToken(tokenValue);
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
  };

  const selectDome = (id) => {
    localStorage.setItem('selectedDomeId', id);
    setSelectedDomeId(id);
  };

  return (
    <AuthContext.Provider value={{ token, user, selectedDomeId, saveLogin, logout, selectDome }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
