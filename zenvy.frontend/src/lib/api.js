import axios from 'axios';

let refreshRequest = null;

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
      const refreshToken = localStorage.getItem('zenvy_refresh_token');
      const isRefreshCall = error.config?.url?.includes('/auth/refresh');
      if (refreshToken && !isRefreshCall && !error.config?._retry) {
        error.config._retry = true;
        refreshRequest ||= axios.post(`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/api/v1/auth/refresh`, { refreshToken });
        return refreshRequest.then((response) => {
          const payload = response.data?.data || response.data;
          localStorage.setItem('zenvy_token', payload.token);
          localStorage.setItem('zenvy_refresh_token', payload.refreshToken);
          error.config.headers.Authorization = `Bearer ${payload.token}`;
          return api.request(error.config);
        }).catch((refreshError) => {
          localStorage.removeItem('zenvy_token');
          localStorage.removeItem('zenvy_refresh_token');
          localStorage.removeItem('zenvy_user');
          window.dispatchEvent(new Event('zenvy:unauthorized'));
          return Promise.reject(refreshError);
        }).finally(() => { refreshRequest = null; });
      }
      localStorage.removeItem('zenvy_token');
      localStorage.removeItem('zenvy_refresh_token');
      localStorage.removeItem('zenvy_user');
      window.dispatchEvent(new Event('zenvy:unauthorized'));
    }
    return Promise.reject(new Error(
      error.response?.data?.message || error.response?.data?.Message || error.message || 'Request failed'
    ));
  }
);

export default api;
