import React, { useEffect, useMemo, useState } from 'react';
import { Check, RefreshCw, WalletCards } from 'lucide-react';
import api from '../lib/api';
import { useSignalR } from '../signalr/signalrProvider';

const PAYMENT_STATUSES = ['PENDING', 'INITIATED', 'COMPLETED', 'FAILED', 'CANCELLED'];
const money = (value) => new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 }).format(Number(value) || 0);
const unwrap = (payload) => {
  const value = payload?.data ?? payload;
  return Array.isArray(value) ? value : value?.items || value?.results || [];
};
const statusLabel = (status) => String(status || 'PENDING').replaceAll('_', ' ');

const PaymentsPage = () => {
  const [payments, setPayments] = useState([]);
  const [drafts, setDrafts] = useState({});
  const [filter, setFilter] = useState('ALL');
  const [loading, setLoading] = useState(true);
  const [savingId, setSavingId] = useState(null);
  const [notice, setNotice] = useState('');
  const [error, setError] = useState('');
  const { lastPaymentChange } = useSignalR();

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await api.get('/api/v1/payments');
      const records = unwrap(response.data);
      setPayments(records);
      setDrafts(Object.fromEntries(records.map((payment) => [payment.paymentId, {
        status: String(payment.status || 'PENDING').toUpperCase(),
        transactionRef: payment.transactionRef || '',
      }])));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);
  useEffect(() => {
    if (lastPaymentChange) load();
  }, [lastPaymentChange]);

  const pendingPayments = useMemo(
    () => payments.filter((payment) => ['PENDING', 'INITIATED'].includes(String(payment.status || '').toUpperCase())),
    [payments],
  );
  const completedPayments = useMemo(
    () => payments.filter((payment) => String(payment.status || '').toUpperCase() === 'COMPLETED'),
    [payments],
  );
  const visiblePayments = filter === 'PENDING'
    ? payments.filter((payment) => ['PENDING', 'INITIATED'].includes(String(payment.status || '').toUpperCase()))
    : filter === 'COMPLETED'
      ? completedPayments
      : payments;

  const updateDraft = (paymentId, field, value) => {
    setDrafts((current) => ({
      ...current,
      [paymentId]: { ...current[paymentId], [field]: value },
    }));
  };

  const updatePayment = async (paymentId) => {
    const draft = drafts[paymentId];
    if (!draft) return;
    setSavingId(paymentId);
    setNotice('');
    setError('');
    try {
      await api.put(`/api/v1/payments/${paymentId}/status`, {
        status: draft.status,
        transactionRef: draft.transactionRef.trim() || null,
      });
      setNotice(`Payment ${paymentId} updated. Verify transfers against your bank/UPI account before marking them completed.`);
      await load();
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setSavingId(null);
    }
  };

  return <div>
    <div className="page-heading">
      <div><p className="eyebrow">Finance / reconciliation</p><h2>Payments</h2><p className="muted">Review payment records and reconcile UPI transfers after checking your bank or UPI app.</p></div>
      <button className="secondary-button" onClick={load}><RefreshCw size={16} />Refresh</button>
    </div>
    <div className="notice payment-reconciliation-note">No gateway is connected, so payment status cannot be verified automatically. Only Admin, Manager, and Accountant roles can update statuses.</div>
    {notice && <div className="notice">{notice}</div>}
    {error && <div className="notice error">{error}</div>}
    <div className="kpi-grid">
      <article className="kpi-card"><span><WalletCards size={14} />Awaiting review</span><strong>{pendingPayments.length}</strong><small>Pending or initiated</small></article>
      <article className="kpi-card"><span><Check size={14} />Completed</span><strong>{completedPayments.length}</strong><small>Staff verified</small></article>
      <article className="kpi-card"><span>Total records</span><strong>{payments.length}</strong><small>All payment methods</small></article>
    </div>
    <section className="panel payments-panel">
      <div className="panel-heading">
        <div><span className="panel-label">Payment ledger</span><h3>Payment status and references</h3></div>
      </div>
      <div className="workspace-groups payment-filters">
        {[['ALL', 'All'], ['PENDING', 'Needs review'], ['COMPLETED', 'Completed']].map(([value, label]) => (
          <button type="button" key={value} className={filter === value ? 'active' : ''} onClick={() => setFilter(value)}>{label}</button>
        ))}
      </div>
      {loading ? <div className="loading-state">Loading payments...</div> : visiblePayments.length === 0
        ? <div className="empty-state"><WalletCards size={28} /><p>No payments match this view.</p></div>
        : <div className="table-wrap payments-table-wrap"><table className="payments-table">
          <thead><tr><th>Payment / order</th><th>Method / date</th><th>Amount</th><th>Transaction reference</th><th>Status</th><th>Reconcile</th></tr></thead>
          <tbody>{visiblePayments.map((payment) => {
            const draft = drafts[payment.paymentId] || { status: payment.status || 'PENDING', transactionRef: payment.transactionRef || '' };
            return <tr key={payment.paymentId}>
              <td><strong>Payment #{payment.paymentId}</strong><small>Order #{payment.orderId}</small></td>
              <td><strong>{payment.methodName || `Method ${payment.paymentMethodId}`}</strong><small>{payment.paymentDate ? new Date(payment.paymentDate).toLocaleString() : 'Date not recorded'}</small></td>
              <td>{money(payment.amount)}</td>
              <td><input className="payment-reference-input" value={draft.transactionRef} onChange={(event) => updateDraft(payment.paymentId, 'transactionRef', event.target.value)} aria-label={`Transaction reference for payment ${payment.paymentId}`} placeholder="Add bank / UPI reference" /></td>
              <td><span className={`tag ${draft.status === 'COMPLETED' ? 'success' : ''}`}>{statusLabel(payment.status)}</span><select className="payment-status-select" value={draft.status} onChange={(event) => updateDraft(payment.paymentId, 'status', event.target.value)} aria-label={`Status for payment ${payment.paymentId}`}>{PAYMENT_STATUSES.map((status) => <option key={status} value={status}>{statusLabel(status)}</option>)}</select></td>
              <td><button className="primary-button compact" type="button" onClick={() => updatePayment(payment.paymentId)} disabled={savingId === payment.paymentId}>{savingId === payment.paymentId ? 'Saving...' : 'Save status'}</button></td>
            </tr>;
          })}</tbody>
        </table></div>}
    </section>
  </div>;
};

export default PaymentsPage;
