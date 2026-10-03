import { configureStore } from '@reduxjs/toolkit';
import { Provider } from 'react-redux';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import authSlice from '../features/auth/authSlice';
import api from '../lib/api';
import LoginPage from './LoginPage';

jest.mock('../lib/api', () => ({
  __esModule: true,
  default: { post: jest.fn() },
}));

const renderPage = () => {
  const store = configureStore({ reducer: { auth: authSlice.reducer } });
  return render(
    <Provider store={store}>
      <MemoryRouter initialEntries={['/login']} future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/dashboard" element={<p>Dashboard</p>} />
        </Routes>
      </MemoryRouter>
    </Provider>,
  );
};

describe('LoginPage', () => {
  beforeEach(() => localStorage.clear());

  it('signs in, persists the session, and navigates to the dashboard', async () => {
    const user = userEvent.setup();
    api.post.mockResolvedValue({ data: {
      token: 'access-token', refreshToken: 'refresh-token', userId: 'u-1', fullName: 'Asha', email: 'asha@example.com', role: 'Admin',
    } });
    renderPage();

    await user.type(screen.getByLabelText('Email'), 'asha@example.com');
    await user.type(screen.getByLabelText('Password'), 'password');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => expect(screen.getByText('Dashboard')).toBeInTheDocument());
    expect(api.post).toHaveBeenCalledWith('/api/v1/auth/login', { email: 'asha@example.com', password: 'password' });
    expect(localStorage.getItem('zenvy_token')).toBe('access-token');
    expect(JSON.parse(localStorage.getItem('zenvy_user'))).toMatchObject({ userId: 'u-1', role: 'Admin' });
  });

  it('shows a request error when login fails', async () => {
    const user = userEvent.setup();
    api.post.mockRejectedValue(new Error('Invalid credentials'));
    renderPage();

    await user.type(screen.getByLabelText('Email'), 'asha@example.com');
    await user.type(screen.getByLabelText('Password'), 'password');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByText('Invalid credentials')).toBeInTheDocument();
    expect(localStorage.getItem('zenvy_token')).toBeNull();
  });

  it('submits a manager-signup request with the extended form', async () => {
    const user = userEvent.setup();
    api.post.mockResolvedValue({ data: { message: 'Request received' } });
    renderPage();

    await user.click(screen.getByRole('button', { name: 'Manager signup' }));
    await user.type(screen.getByLabelText('Full name'), 'Asha Rao');
    await user.type(screen.getByLabelText('Email'), 'asha@example.com');
    await user.type(screen.getByLabelText('Phone'), '9999999999');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Request Manager access' }));

    expect(await screen.findByText('Request received')).toBeInTheDocument();
    expect(api.post).toHaveBeenCalledWith('/api/v1/auth/signup/manager', {
      fullName: 'Asha Rao', email: 'asha@example.com', phone: '9999999999', password: 'password123',
    });
  });
});
