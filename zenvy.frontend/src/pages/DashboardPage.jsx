import React, { useEffect, useState } from 'react';
import { ArrowUpRight, BarChart3, RefreshCw, TrendingUp } from 'lucide-react';
import api from '../lib/api';

const kpis = [
  ['todaysSales', "Today's sales", 'currency'], ['monthlySales', 'Monthly sales', 'currency'], ['monthlyProfit', 'Monthly profit', 'currency'],
  ['inventoryValue', 'Inventory value', 'currency'], ['availableCash', 'Available cash', 'currency'], ['pendingOrders', 'Pending orders', 'number'],
];
const formatValue = (value, type) => type === 'currency' ? new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(value || 0) : new Intl.NumberFormat('en-IN').format(value || 0);

const DashboardPage = () => {
  const [dashboard, setDashboard] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const loadDashboard = async () => { setLoading(true); setError(''); try { const { data } = await api.get('/api/v1/dashboards/executive'); setDashboard(data?.data || data || {}); } catch (requestError) { setError(requestError.message); } finally { setLoading(false); } };
  useEffect(() => { loadDashboard(); }, []);
  const cards = dashboard?.kpiCards || {};
  const trend = dashboard?.charts?.salesTrend || [];
  const maxTrend = Math.max(...trend.map((item) => Number(item.value) || 0), 1);

  return <div className="dashboard-page">
    <div className="page-heading"><div><p className="eyebrow">Monday, 14 September 2026</p><h2>Executive overview</h2><p className="muted">The numbers that tell you how the business is breathing.</p></div><button className="secondary-button" onClick={loadDashboard} disabled={loading}><RefreshCw size={16} className={loading ? 'spin' : ''} />Refresh</button></div>
    {error && <div className="notice error">{error}. Check that the API is running and reachable.</div>}
    {loading ? <div className="loading-state">Loading your business overview...</div> : <>
      <div className="kpi-grid">{kpis.map(([key, label, type]) => <article className="kpi-card" key={key}><span>{label}</span><strong>{formatValue(cards[key], type)}</strong><small><TrendingUp size={13} /> Current period</small></article>)}</div>
      <div className="dashboard-grid"><section className="panel chart-panel"><div className="panel-heading"><div><span className="panel-label">Sales movement</span><h3>Recent sales trend</h3></div><ArrowUpRight size={20} /></div>{trend.length ? <div className="bar-chart">{trend.map((item) => <div className="bar-column" key={item.label}><div className="bar-value">{formatValue(item.value, 'currency')}</div><div className="bar" style={{ height: `${Math.max((item.value / maxTrend) * 100, 4)}%` }} /><span>{item.label}</span></div>)}</div> : <div className="empty-state"><BarChart3 size={30} /><p>Sales trend data will appear here when the API has chart rows.</p></div>}</section><section className="panel pulse-panel"><div className="panel-heading"><div><span className="panel-label">At a glance</span><h3>Business pulse</h3></div></div><div className="pulse-row"><span>Pending returns</span><strong>{cards.pendingReturns || 0}</strong></div><div className="pulse-row"><span>Investor liability</span><strong>{formatValue(cards.investorLiability, 'currency')}</strong></div><div className="pulse-row"><span>Reserve fund</span><strong>{formatValue(cards.reserveFund, 'currency')}</strong></div><div className="pulse-callout"><span className="status-dot" /><span>Data synced from executive dashboard</span></div></section></div>
    </>}
  </div>;
};
export default DashboardPage;
