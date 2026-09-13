import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('zenvy_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => {
    const payload = response.data;
    if (payload && Object.prototype.hasOwnProperty.call(payload, 'success')) {
      if (!payload.success) {
        return Promise.reject(new Error(payload.message || 'Request failed'));
      }
      return { ...response, data: payload.data };
    }
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('zenvy_token');
      localStorage.removeItem('zenvy_user');
      window.dispatchEvent(new Event('zenvy:unauthorized'));
    }
    return Promise.reject(new Error(
      error.response?.data?.message || error.response?.data?.Message || error.message || 'Request failed'
    ));
  }
);

export default api;
