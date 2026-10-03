import axios from 'axios';

let refreshRequest = null;
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const refreshUrl = `${apiBaseUrl}/api/v1/auth/refresh`;

export const getStoredUser = () => {
  try {
    const value = localStorage.getItem('zenvy_user');
    return value ? JSON.parse(value) : null;
  } catch {
    localStorage.removeItem('zenvy_user');
    return null;
  }
};

export const isAccessTokenExpired = (token = localStorage.getItem('zenvy_token')) => {
  if (!token) return true;
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return !payload.exp || payload.exp * 1000 <= Date.now() + 30_000;
  } catch {
    return true;
  }
};

export const getAccessTokenRefreshDelay = (token = localStorage.getItem('zenvy_token')) => {
  if (!token) return 0;
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return Math.max(0, (Number(payload.exp) * 1000) - Date.now() - 30_000);
  } catch {
    return 0;
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

  // One refresh request is shared by startup, scheduled renewal, and failed requests.
  // That prevents refresh-token rotation from racing within the browser.
  refreshRequest ||= axios.post(refreshUrl, { refreshToken })
    .then((response) => {
      const payload = response.data?.data ?? response.data;
      if (payload?.success === false || !payload?.token || !payload?.refreshToken) {
        throw new Error('Refresh token response is invalid');
      }
      localStorage.setItem('zenvy_token', payload.token);
      localStorage.setItem('zenvy_refresh_token', payload.refreshToken);
      const user = payload.userId ? {
        userId: payload.userId, fullName: payload.fullName, email: payload.email, role: payload.role,
      } : getStoredUser();
      if (user) localStorage.setItem('zenvy_user', JSON.stringify(user));
      window.dispatchEvent(new CustomEvent('zenvy:token-refreshed', { detail: { token: payload.token, user } }));
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
  const isAuthRequest = config.url?.includes('/auth/');
  if (!isAuthRequest && isAccessTokenExpired()) {
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
      if (refreshToken && !isRefreshCall && !error.config?._retry) {
        error.config._retry = true;
        return refreshSession().then((refreshed) => {
          if (!refreshed) return Promise.reject(new Error('Your session has expired. Please sign in again.'));
          error.config.headers.Authorization = `Bearer ${localStorage.getItem('zenvy_token')}`;
          return api.request(error.config);
        });
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
