import React from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { BarChart3, Boxes, Database, GitBranch, LayoutDashboard, LogOut, Package, WalletCards } from 'lucide-react';
import { useDispatch, useSelector } from 'react-redux';
import { useSignalR } from '../signalr/signalrProvider';
import { logout } from '../features/auth/authSlice';

const navigation = [
  { to: '/dashboard', label: 'Executive', icon: LayoutDashboard },
  { to: '/operations', label: 'Operations', icon: Boxes },
  { to: '/workflow', label: 'Workflow', icon: GitBranch },
  { to: '/workspace', label: 'All data', icon: Database, roles: ['Admin', 'Manager', 'TeamLead'] },
  { to: '/finance', label: 'Finance', icon: WalletCards, roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/products', label: 'Products', icon: Package },
];

const AppLayout = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const user = useSelector((state) => state.auth.user);
  const visibleNavigation = navigation.filter((item) => !item.roles || item.roles.includes(user?.role || ''));
  const { connected, notifications, dismissNotification } = useSignalR();

  const handleLogout = () => {
    localStorage.removeItem('zenvy_token');
    localStorage.removeItem('zenvy_user');
    dispatch(logout());
    navigate('/login', { replace: true });
  };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand"><span className="brand-mark small">Z</span><span>zenvy</span></div>
        <p className="nav-caption">Workspace</p>
        <nav>{visibleNavigation.map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}><Icon size={18} /><span>{label}</span></NavLink>)}</nav>
        <div className="sidebar-footer"><div className="user-chip"><span className="avatar">{user?.fullName?.charAt(0) || 'Z'}</span><span><strong>{user?.fullName || 'Zenvy user'}</strong><small>{user?.role || 'Workspace'}</small></span></div><button className="icon-button" title="Sign out" onClick={handleLogout}><LogOut size={17} /></button></div>
      </aside>
      <main className="main-content"><header className="topbar"><div><span className="topbar-kicker">ZenVy / workspace</span><h1>Good to see you, {user?.fullName?.split(' ')[0] || 'there'}.</h1></div><div className="topbar-status"><span className="status-dot" />{connected ? 'Live workspace' : 'Offline workspace'}</div></header>{notifications.length > 0 && <div className="notification-stack">{notifications.slice(0, 3).map((notification) => <div className="live-notification" key={notification.id}><span className="status-dot" /><span><strong>{notification.title}</strong><small>{notification.message}</small></span><button onClick={() => dismissNotification(notification.id)} aria-label="Dismiss notification">×</button></div>)}</div>}<section className="page-content"><Outlet /></section></main>
    </div>
  );
};

export default AppLayout;
