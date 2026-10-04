import React, { useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Banknote, Boxes, Coins, CreditCard, Database, FileText, GitBranch, LayoutDashboard, LogOut, Menu, Package, ShieldCheck, Users, WalletCards, X } from 'lucide-react';
import { useDispatch, useSelector } from 'react-redux';
import { useSignalR } from '../signalr/signalrProvider';
import { logout } from '../features/auth/authSlice';
import api, { clearStoredSession, isAccessTokenExpired, refreshSession } from '../lib/api';
import { hasAnyRole } from '../features/auth/tokenClaims';

const navigation = [
  { to: '/dashboard', label: 'Executive', icon: LayoutDashboard }, { to: '/operations', label: 'Operations', icon: Boxes }, { to: '/workflow', label: 'Workflow', icon: GitBranch },
  { to: '/workspace', label: 'All data', icon: Database, roles: ['Admin', 'Manager', 'TeamLead'] }, { to: '/finance', label: 'Finance', icon: WalletCards, roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/compensation', label: 'Compensation', icon: Users, roles: ['Admin', 'Manager', 'Accountant'] }, { to: '/profit-distribution', label: 'Profit distribution', icon: Coins, roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/products', label: 'Products', icon: Package }, { to: '/sales-desk', label: 'Sales desk', icon: Banknote, roles: ['Admin', 'Manager', 'SalesPerson', 'TeamLead'] },
  { to: '/payments', label: 'Payments', icon: CreditCard, roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/admin', label: 'People & access', icon: ShieldCheck, roles: ['Admin'] }, { to: '/manual', label: 'Role manual', icon: FileText },
];
const pageNames = Object.fromEntries(navigation.map((item) => [item.to, item.label]));
const hasRole = (userRoles, allowedRoles) => !allowedRoles || hasAnyRole(userRoles, allowedRoles);

const AppLayout = () => {
  const dispatch = useDispatch(); const navigate = useNavigate(); const location = useLocation();
  const user = useSelector((state) => state.auth.user);
  const { connected, notifications, dismissNotification } = useSignalR();
  const [navOpen, setNavOpen] = useState(false);
  const visibleNavigation = navigation.filter((item) => hasRole(user?.roles || user?.role, item.roles));
  const handleLogout = async () => {
    try {
      if (localStorage.getItem('zenvy_refresh_token') && isAccessTokenExpired()) await refreshSession();
      const refreshToken = localStorage.getItem('zenvy_refresh_token');
      if (refreshToken) await api.post('/api/v1/auth/revoke', { refreshToken });
    } catch {
      // Local sign-out must succeed offline.
    } finally {
      clearStoredSession(); dispatch(logout()); navigate('/login', { replace: true });
    }
  };

  return <div className="app-shell">
    {navOpen && <button className="sidebar-scrim" aria-label="Close navigation" onClick={() => setNavOpen(false)} />}
    <aside className={`sidebar ${navOpen ? 'mobile-open' : ''}`} aria-label="Main navigation">
      <div className="sidebar-brand"><span className="brand-mark small">Z</span><span>zenvy</span></div><button className="mobile-nav-close icon-button" aria-label="Close navigation" onClick={() => setNavOpen(false)}><X size={20} /></button>
      <p className="nav-caption">Workspace</p><nav>{visibleNavigation.map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} onClick={() => setNavOpen(false)} className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}><Icon size={18} /><span>{label}</span></NavLink>)}</nav>
      <div className="sidebar-footer"><div className="user-chip"><span className="avatar">{user?.fullName?.charAt(0) || 'Z'}</span><span><strong>{user?.fullName || 'Zenvy user'}</strong><small>{user?.role || 'Workspace'}</small></span></div><button className="icon-button" title="Sign out" aria-label="Sign out" onClick={handleLogout}><LogOut size={17} /></button></div>
    </aside>
    <main className="main-content">
      <header className="topbar">
        <div className="topbar-title">
          <button className="mobile-nav-toggle icon-button" aria-label="Open navigation" onClick={() => setNavOpen(true)}><Menu size={21} /></button>
        <div>
          <span className="topbar-kicker">ZenVy / {pageNames[location.pathname] || 'workspace'}</span><h1>Good to see you, {user?.fullName?.split(' ')[0] || 'there'}.</h1></div></div>
          <div className="topbar-status"><span className="status-dot" />{connected ? 'Live workspace' : 'Offline workspace'}</div>
          </header>{notifications.length > 0 && <div className="notification-stack" aria-live="polite">{notifications.slice(0, 3).map((notification) => <div className="live-notification" key={notification.id}><span className="status-dot" /><span>
            <strong>{notification.title}</strong><small>{notification.message}</small></span>
          <button onClick={() => dismissNotification(notification.id)} aria-label="Dismiss notification">×</button></div>)}</div>}
          <section className="page-content"><Outlet /></section>
    </main>
  </div>;
};
export default AppLayout;
