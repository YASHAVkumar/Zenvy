import React, { useEffect, useState } from 'react';
import { RefreshCw, WalletCards } from 'lucide-react';
import api from '../lib/api';

const money = (value) => new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(value || 0);
const FinancePage = () => {
  const [data, setData] = useState(null); const [error, setError] = useState(''); const [loading, setLoading] = useState(true);
  const load = async () => { setLoading(true); try { const response = await api.get('/api/v1/dashboards/finance'); setData(response.data?.data || response.data || {}); setError(''); } catch (requestError) { setError(requestError.message); } finally { setLoading(false); } };
  useEffect(() => { load(); }, []);
  const cards = data?.kpiCards || {};
  const items = [['cashAvailable', 'Cash available'], ['bankBalance', 'Bank balance'], ['workingCapital', 'Working capital'], ['reserveFund', 'Reserve fund'], ['receivables', 'Receivables'], ['payables', 'Payables']];
  return <div><div className="page-heading"><div><p className="eyebrow">Finance / cash position</p><h2>Finance room</h2><p className="muted">A clean read on liquidity, obligations, and working capital.</p></div><button className="secondary-button" onClick={load}><RefreshCw size={16} />Refresh</button></div>{error && <div className="notice error">{error}</div>}{loading ? <div className="loading-state">Loading finance data...</div> : <><div className="kpi-grid">{items.map(([key, label]) => <article className="kpi-card" key={key}><span>{label}</span><strong>{money(cards[key])}</strong><small><WalletCards size={13} /> From finance dashboard</small></article>)}</div><section className="panel report-panel"><div className="panel-heading"><div><span className="panel-label">Profit and loss</span><h3>Current report</h3></div></div><div className="report-grid">{Object.entries(data?.reports?.profitLoss || {}).map(([key, value]) => <div key={key}><span>{key.replace(/[A-Z]/g, (letter) => ` ${letter}`).replace(/^./, (letter) => letter.toUpperCase())}</span><strong>{key.toLowerCase().includes('margin') ? `${value || 0}%` : money(value)}</strong></div>)}</div></section></>}</div>;
};
export default FinancePage;
