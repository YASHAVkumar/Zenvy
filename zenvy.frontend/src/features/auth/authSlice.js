import { createSlice } from '@reduxjs/toolkit';

const persistedUser = localStorage.getItem('zenvy_user');
const persistedToken = localStorage.getItem('zenvy_token');

const authSlice = createSlice({
  name: 'auth',
  initialState: {
    token: persistedToken,
    user: persistedUser ? JSON.parse(persistedUser) : null,
    isAuthenticated: Boolean(persistedToken),
    loading: false,
    error: null,
  },
  reducers: {
    setAuth: (state, action) => {
      state.token = action.payload.token;
      state.user = action.payload.user;
      state.isAuthenticated = true;
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