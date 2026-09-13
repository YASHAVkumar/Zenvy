import React, { useEffect, useMemo, useState } from 'react';
import { Activity, Database, Eye, Plus, RefreshCw, Send, Wifi, WifiOff } from 'lucide-react';
import api from '../lib/api';
import { useSignalR } from '../signalr/signalrProvider';

const resources = [
  { id: 'products', label: 'Products', path: '/products', group: 'Catalog' },
  { id: 'brands', label: 'Brands', path: '/brands', group: 'Catalog' },
  { id: 'categories', label: 'Categories', path: '/categories', group: 'Catalog' },
  { id: 'warehouses', label: 'Warehouses', path: '/warehouses', group: 'Operations' },
  { id: 'inventory', label: 'Inventory', path: '/inventory', group: 'Operations' },
  { id: 'inventoryTransactions', label: 'Inventory transactions', path: '/inventory/transactions', group: 'Operations' },
  { id: 'suppliers', label: 'Suppliers', path: '/suppliers', group: 'Operations' },
  { id: 'customers', label: 'Customers', path: '/customers', group: 'Sales' },
  { id: 'purchaseOrders', label: 'Purchase orders', path: '/purchase-orders', group: 'Sales' },
  { id: 'salesOrders', label: 'Sales orders', path: '/sales-orders', group: 'Sales' },
  { id: 'shipments', label: 'Shipments', path: '/shipments', group: 'Sales' },
  { id: 'returns', label: 'Returns', path: '/returns', group: 'Sales' },
  { id: 'salesChannels', label: 'Sales channels', path: '/sales-channels', group: 'Sales' },
  { id: 'expenses', label: 'Expenses', path: '/expenses', group: 'Finance' },
  { id: 'expenseTypes', label: 'Expense types', path: '/expense-types', group: 'Finance' },
  { id: 'payments', label: 'Payments', path: '/payments', group: 'Finance' },
  { id: 'investors', label: 'Investors', path: '/investors', group: 'Finance' },
  { id: 'profitDistributions', label: 'Profit distributions', path: '/investors/profit-distributions', group: 'Finance' },
  { id: 'employeeCommissions', label: 'Employee commissions', path: '/employee-commissions', group: 'Finance' },
  { id: 'marketplaceSettlements', label: 'Marketplace settlements', path: '/marketplace-settlements', group: 'Finance' },
  { id: 'users', label: 'Users', path: '/users', group: 'Administration' },
];

const groups = ['All', ...new Set(resources.map((resource) => resource.group))];
const unwrap = (payload) => {
  const value = payload?.data ?? payload;
  if (Array.isArray(value)) return value;
  return value?.items || value?.results || value?.data || (value && typeof value === 'object' ? [value] : []);
};
const pretty = (value) => typeof value === 'object' ? JSON.stringify(value) : String(value ?? '');
const readOnlyResources = new Set(['inventoryTransactions']);
const sampleFor = (resource) => {
  const samples = {
    products: [{ productName: 'Sample product', description: 'Replace with catalog data', categoryId: 1, brandId: 1 }],
    brands: [{ name: 'Sample brand' }], categories: [{ name: 'Sample category' }],
    warehouses: [{ warehouseName: 'Main warehouse', address: 'Replace with address' }],
    suppliers: [{ name: 'Sample supplier', email: 'supplier@example.com', phone: '0000000000' }],
    customers: [{ fullName: 'Sample customer', email: 'customer@example.com', phone: '0000000000' }],
    salesChannels: [{ name: 'Direct sales', channelType: 'Direct' }],
    investors: [{ name: 'Sample investor', email: 'investor@example.com', investmentAmount: 10000, ownershipPercent: 10, lossPercent: 0, joinDate: new Date().toISOString() }],
    expenseTypes: [{ name: 'Operations' }],
  };
  return JSON.stringify(samples[resource.id] || [{}], null, 2);
};

const WorkspacePage = () => {
  const { connection, connected, lastResourceChange } = useSignalR();
  const [activeGroup, setActiveGroup] = useState('All');
  const [activeId, setActiveId] = useState('products');
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [showBulk, setShowBulk] = useState(false);
  const [payload, setPayload] = useState('{\n  \n}');
  const [notice, setNotice] = useState('');

  const visibleResources = useMemo(() => activeGroup === 'All' ? resources : resources.filter((resource) => resource.group === activeGroup), [activeGroup]);
  const active = resources.find((resource) => resource.id === activeId) || resources[0];
  const filteredRows = rows.filter((row) => !search || JSON.stringify(row).toLowerCase().includes(search.toLowerCase()));
  const columns = [...new Set(filteredRows.flatMap((row) => Object.keys(row || {})))].slice(0, 7);

  const load = async () => {
    setLoading(true); setError('');
    try { const response = await api.get(`/api/v1${active.path}`); setRows(unwrap(response.data)); }
    catch (requestError) { setRows([]); setError(requestError.message); }
    finally { setLoading(false); }
  };
  useEffect(() => { load(); }, [activeId]);
  useEffect(() => {
    if (!lastResourceChange || !lastResourceChange.path?.includes(active.path)) return;
    load();
  }, [lastResourceChange]);

  const create = async (event) => {
    event.preventDefault(); setNotice('');
    try { await api.post(`/api/v1${active.path}`, JSON.parse(payload)); setShowCreate(false); setPayload('{\n  \n}'); setNotice(`${active.label} created.`); await load(); }
    catch (requestError) { setNotice(requestError.message); }
  };
  const createBulk = async (event) => {
    event.preventDefault(); setNotice('');
    try { const value = JSON.parse(payload); if (!Array.isArray(value) || value.length === 0) throw new Error('Bulk payload must be a non-empty JSON array.'); if (activeId === 'products') await api.post('/api/v1/products/bulk', value); else for (const item of value) await api.post(`/api/v1${active.path}`, item); setShowBulk(false); setPayload(sampleFor(active)); setNotice(`${value.length} ${active.label.toLowerCase()} submitted.`); await load(); }
    catch (requestError) { setNotice(requestError.message); }
  };
  const downloadSample = () => { const blob = new Blob([sampleFor(active)], { type: 'application/json' }); const url = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = url; anchor.download = `zenvy-${active.id}-sample.json`; anchor.click(); URL.revokeObjectURL(url); };

  return <div className="workspace-page">
    <div className="page-heading"><div><p className="eyebrow">Workspace / API control</p><h2>All business data</h2><p className="muted">Browse and create records across the Zenvy API from one operating surface.</p></div><div className="workspace-status">{connected ? <><Wifi size={15} />Live updates</> : <><WifiOff size={15} />Offline updates</>}</div></div>
    <div className="workspace-groups">{groups.map((group) => <button key={group} className={activeGroup === group ? 'active' : ''} onClick={() => setActiveGroup(group)}>{group}</button>)}</div>
    <div className="workspace-layout">
      <aside className="workspace-nav">{visibleResources.map((resource) => <button key={resource.id} className={activeId === resource.id ? 'active' : ''} onClick={() => { setActiveId(resource.id); setSearch(''); setNotice(''); }}><Database size={15} /><span>{resource.label}</span></button>)}</aside>
      <section className="workspace-main">
        <div className="workspace-toolbar"><div><span className="panel-label">{active.group}</span><h3>{active.label}</h3><small>/api/v1{active.path}</small></div><div className="workspace-actions"><label className="workspace-search"><Eye size={15} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Filter loaded rows" /></label><button className="secondary-button" onClick={load} disabled={loading}><RefreshCw size={15} className={loading ? 'spin' : ''} />Refresh</button>{!readOnlyResources.has(activeId) && <><button className="secondary-button" onClick={downloadSample}>Sample JSON</button><button className="secondary-button" onClick={() => { setPayload(sampleFor(active)); setShowBulk(true); }}><Plus size={15} />Bulk upload</button><button className="primary-button compact" onClick={() => setShowCreate(true)}><Plus size={15} />Create</button></>}</div></div>
        {notice && <div className="notice">{notice}</div>}{error && <div className="notice error">{error}</div>}
        {loading ? <div className="loading-state">Loading {active.label.toLowerCase()}...</div> : filteredRows.length ? <div className="table-wrap workspace-table"><table><thead><tr>{columns.map((column) => <th key={column}>{column}</th>)}</tr></thead><tbody>{filteredRows.map((row, index) => <tr key={row.id || row.productMasterId || row.productId || row.warehouseId || index}>{columns.map((column) => <td key={column}>{pretty(row[column])}</td>)}</tr>)}</tbody></table></div> : <div className="empty-state large"><Activity size={30} /><h3>No {active.label.toLowerCase()} loaded</h3><p>{error ? 'Check the endpoint and your permissions.' : 'Create the first record or refresh the collection.'}</p></div>}
      </section>
    </div>
    {showCreate && <div className="modal-backdrop" role="presentation" onMouseDown={(event) => event.target === event.currentTarget && setShowCreate(false)}><form className="create-modal" onSubmit={create}><div className="panel-heading"><div><span className="panel-label">POST /api/v1{active.path}</span><h3>Create {active.label}</h3></div><button type="button" className="icon-button" onClick={() => setShowCreate(false)}>×</button></div><p className="muted">Paste the request body expected by the selected API endpoint.</p><textarea value={payload} onChange={(event) => setPayload(event.target.value)} spellCheck="false" /><button className="primary-button" type="submit"><Send size={15} />Send request</button></form></div>}
    {showBulk && <div className="modal-backdrop" role="presentation" onMouseDown={(event) => event.target === event.currentTarget && setShowBulk(false)}><form className="create-modal" onSubmit={createBulk}><div className="panel-heading"><div><span className="panel-label">Bulk upload /api/v1{active.path}</span><h3>{active.label} batch</h3></div><button type="button" className="icon-button" onClick={() => setShowBulk(false)}>×</button></div><p className="muted">Upload a JSON array. Products use the atomic bulk API; other resources are submitted one record at a time.</p><textarea value={payload} onChange={(event) => setPayload(event.target.value)} spellCheck="false" /><button className="primary-button" type="submit"><Send size={15} />Upload batch</button></form></div>}
  </div>;
};

export default WorkspacePage;
