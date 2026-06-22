import axios from 'axios';

export const BASE_URL = 'https://3nfmpd2b-5163.euw.devtunnels.ms';

export const TUNNEL_HEADERS = {
  'X-Tunnel-Skip-AntiPhishing-Page': 'true',
};

const api = axios.create({
  baseURL: BASE_URL,
  headers: { ...TUNNEL_HEADERS },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/';
    }
    return Promise.reject(err);
  }
);

export const login = (data) => api.post('/api/Account/Login', data);
export const register = (data) => api.post('/api/Account/Register', data);
export const sendResetCode = (data) => api.post('/api/Account/SendCode', data);
export const resetPassword = (data) => api.post('/api/Account/PasswordReset', data);
export const getProfile = () => api.get('/api/Account/Profile');
export const updateProfile = (data) => api.put('/api/Account/Profile', data);
export const changePassword = (data) => api.put('/api/Account/ChangePassword', data);

export const getDomes = () => api.get('/api/domes');
export const getDome = (id) => api.get(`/api/domes/${id}`);
export const createDome = (data) => api.post('/api/domes', data);
export const updateDome = (id, data) => api.put(`/api/domes/${id}`, data);
export const deleteDome = (id) => api.delete(`/api/domes/${id}`);
export const irrigateDome = (id) => api.post(`/api/domes/${id}/irrigate`);
export const stopIrrigation = (id) => api.post(`/api/domes/${id}/stop-irrigation`);
export const toggleAi = (id, enabled) => api.put(`/api/domes/${id}`, { isAiEnabled: enabled });

export const getLatestReading = (domeId) => api.get(`/api/sensorreadings/latest/${domeId}`);
export const getReadings = (domeId) =>
  api.get(`/api/sensorreadings/history/${domeId}`);

export const getSchedules = (domeId) => api.get(`/api/irrigationschedules/${domeId}`);
export const addSchedule = (data) => api.post('/api/irrigationschedules', data);
export const deleteSchedule = (id) => api.delete(`/api/irrigationschedules/${id}`);

export const getNotifications = (domeId) => api.get(`/api/notifications/${domeId}`);
export const markNotificationRead = (id) => api.put(`/api/notifications/${id}/read`);

export const getAnalytics = (domeId, period = '7d') =>
  api.get(`/api/analytics/${domeId}?period=${period}`);

export const getAiInsights = (domeId) => api.post(`/api/aiinsights/${domeId}`);
export const calibrateProfile = (domeId) => api.post(`/api/aiinsights/${domeId}/calibrate`);
export const suggestAiSchedule = (domeId) => api.post(`/api/aiinsights/${domeId}/suggest-schedule`);

export default api;
