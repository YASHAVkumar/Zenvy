import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
//import { API_BASE_URL } from '../config';

export const TOKEN_KEY = 'zenvy_token';
export const REFRESH_TOKEN_KEY = 'zenvy_refresh_token';
const API_BASE_URL='http://localhost:5108';
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 20000,
});

api.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem(TOKEN_KEY);
  const isPublicAuth = /\/auth\/(login|signup|refresh)/.test(config.url || '');
  if (token && !isPublicAuth) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use((response) => {
  const payload = response.data;
  if (payload && Object.prototype.hasOwnProperty.call(payload, 'success')) {
    if (!payload.success) throw new Error(payload.message || 'Request failed');
    return { ...response, data: payload.data };
  }
  return response;
}, (error) => {
  const message = error.response?.data?.message || error.response?.data?.Message ||
    (error.code === 'ECONNABORTED' ? 'The API request timed out.' : error.message) ||
    'Request failed';
  return Promise.reject(new Error(message));
});

export default api;
