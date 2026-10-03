import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { ProtectedRoute } from './ProtectedRoute';
import { PublicRoute } from './PublicRoute';
import { RoleRoute } from './RoleRoute';
import { useAuth } from '../hooks/useAuth';

jest.mock('../hooks/useAuth', () => ({ useAuth: jest.fn() }));
jest.mock('react-redux', () => ({
  ...jest.requireActual('react-redux'),
  useSelector: jest.fn(),
}));

const renderAt = (path, element) => render(
  <MemoryRouter initialEntries={[path]} future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
    <Routes>
      <Route path="/login" element={<p>Login page</p>} />
      <Route path="/dashboard" element={<p>Dashboard page</p>} />
      <Route path="/private" element={element} />
    </Routes>
  </MemoryRouter>,
);

describe('route guards', () => {
  it('sends unauthenticated visitors to login', () => {
    useAuth.mockReturnValue({ isAuthenticated: false });

    renderAt('/private', <ProtectedRoute><p>Private page</p></ProtectedRoute>);

    expect(screen.getByText('Login page')).toBeInTheDocument();
  });

  it('keeps authenticated visitors on protected pages', () => {
    useAuth.mockReturnValue({ isAuthenticated: true });

    renderAt('/private', <ProtectedRoute><p>Private page</p></ProtectedRoute>);

    expect(screen.getByText('Private page')).toBeInTheDocument();
  });

  it('redirects authenticated visitors away from public pages', () => {
    useAuth.mockReturnValue({ isAuthenticated: true });

    renderAt('/private', <PublicRoute><p>Login form</p></PublicRoute>);

    expect(screen.getByText('Dashboard page')).toBeInTheDocument();
  });

  it('allows only matching roles, irrespective of casing or whitespace', () => {
    useSelector.mockImplementation((selector) => selector({ auth: { user: { role: ' Admin ' } } }));

    renderAt('/private', <RoleRoute roles={['admin']}><p>Admin page</p></RoleRoute>);

    expect(screen.getByText('Admin page')).toBeInTheDocument();
  });
});
