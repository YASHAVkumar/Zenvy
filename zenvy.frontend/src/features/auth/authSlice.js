import { createSlice } from '@reduxjs/toolkit';
import { getUserFromAccessToken } from './tokenClaims';

const persistedToken = localStorage.getItem('zenvy_token');
localStorage.removeItem('zenvy_user');
const persistedUser = getUserFromAccessToken(persistedToken);

const authSlice = createSlice({
  name: 'auth',
  initialState: {
    token: persistedToken,
    user: persistedUser,
    isAuthenticated: Boolean(persistedToken && persistedUser),
    loading: false,
    error: null,
  },
  reducers: {
    setAuth: (state, action) => {
      state.token = action.payload.token;
      state.user = getUserFromAccessToken(action.payload.token);
      state.isAuthenticated = Boolean(state.token && state.user);
      state.loading = false;
      state.error = null;
    },
    logout: (state) => {
      state.token = null;
      state.user = null;
      state.isAuthenticated = false;
      state.loading = false;
      state.error = null;
    },
    setLoading: (state, action) => {
      state.loading = action.payload;
    },
    setError: (state, action) => {
      state.error = action.payload;
    },
  },
});

export const { setAuth, logout, setLoading, setError } = authSlice.actions;
export default authSlice;
