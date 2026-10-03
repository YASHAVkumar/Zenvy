import axios from 'axios';

let refreshRequest = null;

export const isAccessTokenExpired = (token = localStorage.getItem('zenvy_token')) => {
  if (!token) return true;
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return !payload.exp || payload.exp * 1000 <= Date.now() + 30_000;
  } catch {
    return true;
  }
};

export const clearStoredSession = () => {
  localStorage.removeItem('zenvy_token');
  localStorage.removeItem('zenvy_refresh_token');
  localStorage.removeItem('zenvy_user');
  window.dispatchEvent(new Event('zenvy:unauthorized'));
};

export const refreshSession = async () => {
  const refreshToken = localStorage.getItem('zenvy_refresh_token');
  if (!refreshToken) return false;

  try {
    const response = await axios.post(
      `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/api/v1/auth/refresh`,
      { refreshToken },
    );
    const payload = response.data?.data || response.data;
    if (!payload?.token || !payload?.refreshToken) throw new Error('Refresh token response is invalid');
    localStorage.setItem('zenvy_token', payload.token);
    localStorage.setItem('zenvy_refresh_token', payload.refreshToken);
    window.dispatchEvent(new CustomEvent('zenvy:token-refreshed', { detail: { token: payload.token } }));
    return true;
  } catch {
    clearStoredSession();
    return false;
  }
};

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use(async (config) => {
  if (!config.url?.includes('/auth/') && isAccessTokenExpired() && localStorage.getItem('zenvy_refresh_token')) {
    await refreshSession();
  }
  const token = localStorage.getItem('zenvy_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => {
    const payload = response.data;
    if (payload && Object.prototype.hasOwnProperty.call(payload, 'success')) {
      if (!payload.success) {
        const validationDetails = payload.errors && typeof payload.errors === 'object'
          ? Object.entries(payload.errors).map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`).join(' | ')
          : '';
        return Promise.reject(new Error([payload.message, validationDetails].filter(Boolean).join(' ')));
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
          window.dispatchEvent(new CustomEvent('zenvy:token-refreshed', { detail: { token: payload.token } }));
          error.config.headers.Authorization = `Bearer ${payload.token}`;
          return api.request(error.config);
        }).catch((refreshError) => {
          clearStoredSession();
          return Promise.reject(refreshError);
        }).finally(() => { refreshRequest = null; });
      }
      clearStoredSession();
    }
    const responseData = error.response?.data;
    const validationDetails = responseData?.errors && typeof responseData.errors === 'object'
      ? Object.entries(responseData.errors).map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`).join(' | ')
      : '';
    return Promise.reject(new Error([responseData?.message || responseData?.Message || error.message || 'Request failed', validationDetails].filter(Boolean).join(' ')));
  }
);

export default api;
