import React, { useEffect, useMemo, useState } from 'react';
import { Banknote, CloudOff, CreditCard, RefreshCw, Send, ShoppingBag, Smartphone } from 'lucide-react';
import QRCode from 'qrcode';
import { useSelector } from 'react-redux';
import api from '../lib/api';

const QUEUE_KEY = 'zenvy_offline_sales';
const money = (value) => new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(Number(value) || 0);
const unwrap = (payload) => {
  const value = payload?.data ?? payload;
  return Array.isArray(value) ? value : value?.items || value?.results || [];
};
const readQueue = () => {
  try { return JSON.parse(localStorage.getItem(QUEUE_KEY) || '[]'); } catch { return []; }
};
const writeQueue = (queue) => localStorage.setItem(QUEUE_KEY, JSON.stringify(queue));

const SalesDeskPage = () => {
  const user = useSelector((state) => state.auth.user);
  const [products, setProducts] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [channels, setChannels] = useState([]);
  const [payments, setPayments] = useState([]);
  const [queue, setQueue] = useState(readQueue);
  const [form, setForm] = useState({ variantId: '', customerId: '', channelId: '', qty: 1, unitPrice: '', additionalFee: '', shippingFee: '', paymentMethodId: 1, referenceId: '' });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [notice, setNotice] = useState('');
  const [error, setError] = useState('');
  const [paymentLink, setPaymentLink] = useState('');
  const [paymentQr, setPaymentQr] = useState('');
  const [linkCopied, setLinkCopied] = useState(false);

  const variants = useMemo(() => products.flatMap((product) => (product.productVariants || []).map((variant) => ({ ...variant, productName: product.productName }))), [products]);
  const selectedVariant = variants.find((variant) => String(variant.variantId) === String(form.variantId));
  const itemTotal = Number(form.qty || 0) * Number(form.unitPrice || 0);
  const total = itemTotal + Number(form.additionalFee || 0) + Number(form.shippingFee || 0);
  const todaysPayments = payments.filter((payment) => new Date(payment.paymentDate).toDateString() === new Date().toDateString());
  const settledPayments = todaysPayments.filter((payment) => ['COMPLETED', 'CAPTURED', 'AUTHORIZED'].includes(String(payment.status || '').toUpperCase()));
  const collectedToday = settledPayments.reduce((sum, payment) => sum + Number(payment.amount || 0), 0);

  const load = async () => {
    setLoading(true); setError('');
    try {
      const [productResponse, customerResponse, channelResponse, paymentResponse] = await Promise.all([
        api.get('/api/v1/products?pageNumber=1&pageSize=200'),
        api.get('/api/v1/customers'),
        api.get('/api/v1/sales-channels'),
        api.get('/api/v1/payments'),
      ]);
      setProducts(unwrap(productResponse.data));
      setCustomers(unwrap(customerResponse.data));
      setChannels(unwrap(channelResponse.data));
      setPayments(unwrap(paymentResponse.data));
      setForm((current) => ({ ...current, channelId: current.channelId || unwrap(channelResponse.data)[0]?.channelId || '' }));
    } catch (requestError) { setError(requestError.message); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);
  useEffect(() => { if (selectedVariant && !form.unitPrice) setForm((current) => ({ ...current, unitPrice: selectedVariant.productVariantPrice?.salePrice || '' })); }, [selectedVariant, form.unitPrice]);
  useEffect(() => {
    let cancelled = false;
    if (!paymentLink) {
      setPaymentQr('');
      return undefined;
    }
    QRCode.toDataURL(paymentLink, { errorCorrectionLevel: 'M', margin: 2, width: 240 })
      .then((dataUrl) => { if (!cancelled) setPaymentQr(dataUrl); })
      .catch((qrError) => { if (!cancelled) setError(`Could not generate the payment QR code: ${qrError.message}`); });
    return () => { cancelled = true; };
  }, [paymentLink]);

  const submitOrder = async (payload) => {
    return api.post('/api/v1/sales-orders', payload);
  };

  const copyPaymentLink = async () => {
    if (!paymentLink) return;
    try {
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(paymentLink);
      } else {
        const input = document.createElement('textarea');
        input.value = paymentLink;
        input.setAttribute('readonly', '');
        input.style.position = 'fixed';
        input.style.opacity = '0';
        document.body.appendChild(input);
        input.select();
        document.execCommand('copy');
        document.body.removeChild(input);
      }
      setLinkCopied(true);
      window.setTimeout(() => setLinkCopied(false), 2000);
    } catch {
      setError('Copy was blocked by the browser. Select the payment link and copy it manually.');
    }
  };

  const syncQueue = async () => {
    if (!queue.length) return;
    const pending = [...queue]; const failed = [];
    for (const item of pending) { try { await submitOrder(item); } catch { failed.push(item); } }
    setQueue(failed); writeQueue(failed);
    setNotice(failed.length ? `${pending.length - failed.length} queued sale(s) synced. ${failed.length} remain pending.` : `${pending.length} queued sale(s) synced.`);
    await load();
  };

  const collectPayment = async (event) => {
    event.preventDefault(); setSaving(true); setNotice(''); setError('');
    const payload = {
      customerId: form.customerId ? Number(form.customerId) : null,
      channelId: Number(form.channelId),
      createdBy: user?.userId,
      orderDate: new Date().toISOString(),
      status: 1,
      shippingFee: Number(form.shippingFee || 0),
      additionalFee: Number(form.additionalFee || 0),
      lines: [{ variantId: Number(form.variantId), qty: Number(form.qty), unitPrice: Number(form.unitPrice), discount: 0, tax: 0 }],
      paymentMethodId: Number(form.paymentMethodId),
      referenceId: form.referenceId || null,
      payStatus: form.paymentMethodId === 2 ? 1 : 5,
    };
    try {
      setPaymentLink('');
      setLinkCopied(false);
      const response = await submitOrder(payload);
      const orderId = response?.data?.orderId || response?.data?.data?.orderId;
      setNotice(form.paymentMethodId === 2 ? `Sale recorded. UPI payment is pending staff verification.` : `Sale recorded and ${money(total)} collected.`);
      if (form.paymentMethodId === 2 && import.meta.env.VITE_UPI_ID && orderId) {
        const query = new URLSearchParams({ pa: import.meta.env.VITE_UPI_ID, pn: 'Zenvy', am: total.toFixed(2), cu: 'INR', tn: `Zenvy order ${orderId}` });
        setPaymentLink(`upi://pay?${query.toString()}`);
      } else if (form.paymentMethodId === 2 && !import.meta.env.VITE_UPI_ID) {
        setNotice(`Sale recorded, but no UPI payment link was generated. Configure VITE_UPI_ID for this store.`);
      }
      setForm((current) => ({ ...current, variantId: '', unitPrice: '', qty: 1, additionalFee: '', shippingFee: '', referenceId: '' }));
      await load();
    } catch (requestError) {
      if (requestError.response || /validation|required|invalid|must be|greater than/i.test(requestError.message)) {
        setError(requestError.message);
      } else {
        const nextQueue = [...queue, payload]; setQueue(nextQueue); writeQueue(nextQueue);
        setNotice('The sale is saved on this device and will sync when the API is reachable.');
      }
    } finally { setSaving(false); }
  };

  return <div>
    <div className="page-heading"><div><p className="eyebrow">Sales / counter</p><h2>Sales desk</h2><p className="muted">Sell clothing offline, collect payment, and sync safely when the connection returns.</p></div><div className="workspace-actions"><button className="secondary-button" onClick={syncQueue} disabled={!queue.length}><CloudOff size={16} />Sync {queue.length} pending</button><button className="secondary-button" onClick={load}><RefreshCw size={16} />Refresh</button></div></div>
    {notice && <div className="notice">{notice}</div>}{paymentLink && <div className="panel payment-share-panel"><div><span className="panel-label">UPI payment request</span><h3>Share link or QR</h3><p className="muted">Customer payment is not verified automatically. Check your bank/UPI app and update the status in Payments.</p><a className="secondary-button" href={paymentLink}><Smartphone size={16} />Open payment app</a><div className="payment-link-controls"><input className="payment-link-value" value={paymentLink} readOnly onFocus={(event) => event.target.select()} aria-label="UPI payment link" /><button type="button" className="secondary-button compact" onClick={copyPaymentLink}>{linkCopied ? 'Copied' : 'Copy link'}</button></div></div>{paymentQr && <div className="payment-qr"><img src={paymentQr} alt="QR code for this UPI payment request" /><a className="secondary-button compact" href={paymentQr} download="zenvy-payment-qr.png">Download QR</a></div>}</div>}{error && <div className="notice error">{error}</div>}
    <div className="kpi-grid sales-kpis"><article className="kpi-card"><span><Banknote size={14} />Collected today</span><strong>{money(collectedToday)}</strong><small>{todaysPayments.length} payment records</small></article><article className="kpi-card"><span><ShoppingBag size={14} />Pending sync</span><strong>{queue.length}</strong><small>Stored locally until delivered</small></article><article className="kpi-card"><span><CreditCard size={14} />Catalog variants</span><strong>{variants.length}</strong><small>{customers.length} customers available</small></article></div>
    <section className="panel sales-extra-charges"><div className="panel-heading"><div><span className="panel-label">Order charges</span><h3>Additional charges</h3><p className="muted">These amounts are saved with the order and added to the payment request.</p></div></div><div className="form-grid"><label>Additional fee<input type="number" min="0" step="0.01" value={form.additionalFee} onChange={(event) => setForm({ ...form, additionalFee: event.target.value })} placeholder="0.00" /></label><label>Transport fee<input type="number" min="0" step="0.01" value={form.shippingFee} onChange={(event) => setForm({ ...form, shippingFee: event.target.value })} placeholder="0.00" /></label></div><div className="sale-total"><span>Product {money(itemTotal)} + fees {money(Number(form.additionalFee || 0) + Number(form.shippingFee || 0))}</span><strong>Total {money(total)}</strong></div></section>
    <div className="dashboard-grid sales-desk-grid"><section className="panel"><div className="panel-heading"><div><span className="panel-label">Counter checkout</span><h3>Collect from customer</h3></div><ShoppingBag size={20} /></div>{loading ? <div className="loading-state">Loading catalog...</div> : <form className="sales-form" onSubmit={collectPayment}><label>Clothing item<select value={form.variantId} onChange={(event) => setForm({ ...form, variantId: event.target.value, unitPrice: '' })} required><option value="">Choose a variant</option>{variants.map((variant) => <option key={variant.variantId} value={variant.variantId}>{variant.productName} / {variant.size || 'one size'} / {variant.color || 'standard'} / {variant.sku}</option>)}</select></label><div className="form-grid"><label>Customer<select value={form.customerId} onChange={(event) => setForm({ ...form, customerId: event.target.value })}><option value="">Walk-in customer</option>{customers.map((customer) => <option key={customer.customerId} value={customer.customerId}>{customer.name} {customer.phone ? `(${customer.phone})` : ''}</option>)}</select></label><label>Sales channel<select value={form.channelId} onChange={(event) => setForm({ ...form, channelId: event.target.value })} required><option value="">Choose channel</option>{channels.map((channel) => <option key={channel.channelId} value={channel.channelId}>{channel.channelName}</option>)}</select></label></div><div className="form-grid"><label>Quantity<input type="number" min="1" value={form.qty} onChange={(event) => setForm({ ...form, qty: event.target.value })} required /></label><label>Unit price<input type="number" min="0" step="0.01" value={form.unitPrice} onChange={(event) => setForm({ ...form, unitPrice: event.target.value })} required /></label></div><div className="payment-methods"><label className={form.paymentMethodId === 1 ? 'selected' : ''}><input type="radio" checked={form.paymentMethodId === 1} onChange={() => setForm({ ...form, paymentMethodId: 1 })} /><Banknote size={16} />Cash</label><label className={form.paymentMethodId === 2 ? 'selected' : ''}><input type="radio" checked={form.paymentMethodId === 2} onChange={() => setForm({ ...form, paymentMethodId: 2 })} /><Smartphone size={16} />UPI</label><label className={form.paymentMethodId === 3 ? 'selected' : ''}><input type="radio" checked={form.paymentMethodId === 3} onChange={() => setForm({ ...form, paymentMethodId: 3 })} /><CreditCard size={16} />Card</label></div>{form.paymentMethodId !== 1 && <label>Transaction reference<input value={form.referenceId} onChange={(event) => setForm({ ...form, referenceId: event.target.value })} placeholder="Optional UPI or card reference" /></label>}<div className="sale-total"><span>Total to collect</span><strong>{money(total)}</strong></div><button className="primary-button" type="submit" disabled={saving || !form.channelId || !form.variantId}><Send size={16} />{saving ? 'Recording...' : 'Record sale and payment'}</button></form>}</section><section className="panel"><div className="panel-heading"><div><span className="panel-label">Cash desk</span><h3>Recent payments</h3></div></div>{todaysPayments.length ? <div className="simple-list">{todaysPayments.slice(-8).reverse().map((payment) => <div className="list-row" key={payment.paymentId}><span><strong>{payment.methodName || `Method ${payment.paymentMethodId}`} · {String(payment.status || 'PENDING').replaceAll('_', ' ')}</strong><small>{payment.transactionRef || 'No reference'}</small></span><b>{money(payment.amount)}</b></div>)}</div> : <div className="empty-state"><Banknote size={28} /><p>No payments recorded today.</p></div>}</section></div>
  </div>;
};

export default SalesDeskPage;
