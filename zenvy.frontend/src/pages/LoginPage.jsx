import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import api from '../lib/api';
import { setAuth } from '../features/auth/authSlice';

const LoginPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error } = useSelector((state) => state.auth);
  const [form, setForm] = useState({ email: '', password: '' });
  const [requestError, setRequestError] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();
    setRequestError('');
    dispatch({ type: 'auth/setLoading', payload: true });
    try {
      const { data } = await api.post('/api/v1/auth/login', form);
      if (!data?.token) throw new Error(data?.message || 'Login failed');
      const user = {
        userId: data.userId,
        fullName: data.fullName,
        email: data.email,
        role: data.role,
      };
      localStorage.setItem('zenvy_token', data.token);
      localStorage.setItem('zenvy_user', JSON.stringify(user));
      dispatch(setAuth({ token: data.token, user }));
      navigate('/dashboard', { replace: true });
    } catch (loginError) {
      setRequestError(loginError.message);
      dispatch({ type: 'auth/setError', payload: loginError.message });
    } finally {
      dispatch({ type: 'auth/setLoading', payload: false });
    }
  };

  return (
    <main className="login-page">
      <section className="login-panel">
        <div className="brand-mark">Z</div>
        <p className="eyebrow">Business operations</p>
        <h1>Welcome back.</h1>
        <p className="muted">Sign in to see what is moving across your business.</p>
        <form onSubmit={handleSubmit} className="login-form">
          <label>Email<input type="email" autoComplete="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required /></label>
          <label>Password<input type="password" autoComplete="current-password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} required /></label>
          {(requestError || error) && <p className="form-error">{requestError || error}</p>}
          <button className="primary-button" type="submit" disabled={loading}>{loading ? 'Signing in...' : 'Sign in'}</button>
        </form>
      </section>
      <aside className="login-aside">
        <span className="aside-label">ZEN VY / CONTROL ROOM</span>
        <h2>Clarity for every order, rupee, and shelf.</h2>
        <p>One calm view of sales, cash, inventory, and the work that keeps it all moving.</p>
        <div className="aside-stat"><strong>01</strong><span>Executive overview</span></div>
        <div className="aside-stat"><strong>02</strong><span>Operations pulse</span></div>
      </aside>
    </main>
  );
};

export default LoginPage;
