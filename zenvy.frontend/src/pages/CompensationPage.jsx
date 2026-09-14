import React, { useEffect, useState } from 'react';
import { RefreshCw, Users } from 'lucide-react';
import api from '../lib/api';

const money = (value) => new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(value || 0);
const CompensationPage = () => {
  const now = new Date();
  const [fromDate, setFromDate] = useState(new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10));
  const [toDate, setToDate] = useState(now.toISOString().slice(0, 10));
  const [rows, setRows] = useState([]); const [error, setError] = useState(''); const [loading, setLoading] = useState(false);
  const load = async () => { setLoading(true); try { const response = await api.get('/api/v1/employee-reports', { params: { fromDate, toDate } }); setRows(response.data?.data || response.data || []); setError(''); } catch (requestError) { setError(requestError.message); } finally { setLoading(false); } };
  useEffect(() => { load(); }, []);
  return <div><div className="page-heading"><div><p className="eyebrow">People / compensation</p><h2>Employee compensation</h2><p className="muted">Review salary, commissions, and total compensation for a selected period.</p></div><button className="secondary-button" onClick={load}><RefreshCw size={16} />Refresh</button></div><section className="panel"><div className="form-grid"><label>From<input type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} /></label><label>To<input type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} /></label><button className="primary-button" onClick={load}>Run report</button></div></section>{error && <div className="notice error">{error}</div>}{loading ? <div className="loading-state">Loading compensation...</div> : <section className="panel"><div className="panel-heading"><div><span className="panel-label">Period report</span><h3>{rows.length} employees</h3></div></div><div className="table-wrap"><table><thead><tr><th>Employee</th><th>Base salary</th><th>Commission</th><th>Orders</th><th>Total</th></tr></thead><tbody>{rows.map((row) => <tr key={row.employeeId}><td><Users size={15} /> {row.employeeName}</td><td>{money(row.baseSalary)}</td><td>{money(row.commissionAmount)}</td><td>{row.commissionCount}</td><td><strong>{money(row.totalCompensation)}</strong></td></tr>)}</tbody></table>{rows.length === 0 && <div className="empty-state">No employee compensation records for this period.</div>}</div></section>}</div>;
};
export default CompensationPage;
