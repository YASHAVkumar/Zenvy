import axios from 'axios';
import { decodeJwtPayload, getUserFromAccessToken } from '../features/auth/tokenClaims';

let refreshRequest = null;
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const refreshUrl = `${apiBaseUrl}/api/v1/auth/refresh`;

export const isAccessTokenExpired = (token = localStorage.getItem('zenvy_token')) => {
  if (!token) return true;
  const expiresAt = Number(decodeJwtPayload(token)?.exp) * 1000;
  return !Number.isFinite(expiresAt) || expiresAt <= Date.now() + 30_000;
};

export const getAccessTokenRefreshDelay = (token = localStorage.getItem('zenvy_token')) => {
  if (!token) return 0;
  const expiresAt = Number(decodeJwtPayload(token)?.exp) * 1000;
  return Number.isFinite(expiresAt) ? Math.max(0, expiresAt - Date.now() - 30_000) : 0;
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

  // One refresh request is shared by startup, scheduled renewal, and failed requests.
  // That prevents refresh-token rotation from racing within the browser.
  refreshRequest ||= axios.post(refreshUrl, { refreshToken })
    .then((response) => {
      const payload = response.data?.data ?? response.data;
      const token = payload?.accessToken || payload?.token;
      const user = getUserFromAccessToken(token);
      if (payload?.success === false || !token || !payload?.refreshToken || !user?.role) {
        throw new Error('Refresh token response is invalid');
      }
      localStorage.removeItem('zenvy_user');
      localStorage.setItem('zenvy_token', token);
      localStorage.setItem('zenvy_refresh_token', payload.refreshToken);
      window.dispatchEvent(new CustomEvent('zenvy:token-refreshed', { detail: { token } }));
      return true;
    })
    .catch(() => {
      // Another tab may have won the refresh-token rotation and already written
      // its replacement credentials. Do not erase that valid shared session.
      if (localStorage.getItem('zenvy_refresh_token') !== refreshToken && !isAccessTokenExpired()) return true;
      clearStoredSession();
      return false;
    })
    .finally(() => { refreshRequest = null; });

  return refreshRequest;
};

const api = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use(async (config) => {
  const isPublicAuthRequest = /\/auth\/(?:login|refresh|signup(?:\/|$))/.test(config.url || '');
  if (!isPublicAuthRequest && isAccessTokenExpired()) {
    const refreshed = await refreshSession();
    if (!refreshed) return Promise.reject(new Error('Your session has expired. Please sign in again.'));
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
      const isPublicAuthRequest = /\/auth\/(?:login|refresh|signup(?:\/|$))/.test(error.config?.url || '');
      if (refreshToken && !isRefreshCall && !isPublicAuthRequest && !error.config?._retry) {
        error.config._retry = true;
        return refreshSession().then((refreshed) => {
          if (!refreshed) return Promise.reject(new Error('Your session has expired. Please sign in again.'));
          error.config.headers.Authorization = `Bearer ${localStorage.getItem('zenvy_token')}`;
          return api.request(error.config);
        });
      }
      if (!isPublicAuthRequest) clearStoredSession();
    }
    const responseData = error.response?.data;
    const validationDetails = responseData?.errors && typeof responseData.errors === 'object'
      ? Object.entries(responseData.errors).map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`).join(' | ')
      : '';
    return Promise.reject(new Error([responseData?.message || responseData?.Message || error.message || 'Request failed', validationDetails].filter(Boolean).join(' ')));
  }
);

export default api;
