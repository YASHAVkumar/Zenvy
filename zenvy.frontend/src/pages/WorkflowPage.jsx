import React, { useEffect, useMemo, useState } from 'react';
import {
  ArrowRight,
  Boxes,
  CheckCircle2,
  CircleDollarSign,
  ClipboardList,
  Factory,
  PackageCheck,
  RefreshCw,
  ShoppingCart,
  Truck,
  Warehouse,
  AlertTriangle,
} from 'lucide-react';
import api from '../lib/api';

const stages = [
  { id: 'source', label: 'Source', title: 'Purchase & receive', icon: ShoppingCart, description: 'Turn supplier commitments into stock you can trust.', color: 'lime' },
  { id: 'stock', label: 'Stock', title: 'Store & control', icon: Warehouse, description: 'Keep inventory accurate across every warehouse.', color: 'blue' },
  { id: 'sell', label: 'Sell', title: 'Order & collect', icon: ClipboardList, description: 'Move orders from channel to confirmed payment.', color: 'orange' },
  { id: 'deliver', label: 'Deliver', title: 'Pack & dispatch', icon: Truck, description: 'Close the loop with an on-time customer delivery.', color: 'green' },
];

const number = (value) => Number(value || 0).toLocaleString('en-IN');

const WorkflowPage = () => {
  const [data, setData] = useState(null);
  const [selectedStage, setSelectedStage] = useState('source');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadWorkflow = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await api.get('/api/v1/dashboards/operations');
      setData(response.data?.data || response.data || {});
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadWorkflow(); }, []);

  const cards = data?.kpiCards || {};
  const counts = useMemo(() => ({
    source: Number(cards.pendingPurchases || 0) + Number(cards.pendingGoodsReceipt || 0),
    stock: Number(cards.currentInventory || 0),
    sell: Number(cards.pendingOrders || 0),
    deliver: Number(cards.pendingDispatch || 0),
  }), [cards]);
  const selected = stages.find((stage) => stage.id === selectedStage) || stages[0];
  const SelectedIcon = selected.icon;

  const stageRows = {
    source: data?.pendingSupplierPayments || [],
    stock: data?.lowStockProducts || [],
    sell: [],
    deliver: [],
  };
  const rows = stageRows[selected.id];

  return <div className="workflow-page">
    <div className="page-heading">
      <div><p className="eyebrow">Control room / business flow</p><h2>Workflow</h2><p className="muted">See where every rupee, product, and order is moving today.</p></div>
      <button className="secondary-button" onClick={loadWorkflow} disabled={loading}><RefreshCw size={16} className={loading ? 'spin' : ''} />Refresh flow</button>
    </div>

    {error && <div className="notice error"><AlertTriangle size={16} />{error}. Showing the workflow shell until the API reconnects.</div>}
    {loading ? <div className="loading-state">Loading the operational flow...</div> : <>
      <section className="workflow-hero">
        <div><span className="panel-label">Today&apos;s operating rhythm</span><h3>One flow. Four decisions.</h3><p>Choose a stage to inspect the work waiting there. Counts are synced from the operations dashboard.</p></div>
        <div className="workflow-health"><span className="status-dot" /><strong>Live</strong><small>operations feed</small></div>
      </section>

      <section className="workflow-track" aria-label="Business workflow stages">
        {stages.map((stage, index) => {
          const Icon = stage.icon;
          return <React.Fragment key={stage.id}>
            <button className={`workflow-stage ${selectedStage === stage.id ? 'selected' : ''}`} onClick={() => setSelectedStage(stage.id)}>
              <span className={`workflow-icon ${stage.color}`}><Icon size={19} /></span>
              <span className="workflow-stage-copy"><small>{String(index + 1).padStart(2, '0')} / {stage.label}</small><strong>{stage.title}</strong><span>{stage.description}</span></span>
              <b>{number(counts[stage.id])}</b>
            </button>
            {index < stages.length - 1 && <ArrowRight className="workflow-arrow" size={18} />}
          </React.Fragment>;
        })}
      </section>

      <div className="workflow-grid">
        <section className="panel workflow-detail">
          <div className="panel-heading"><div><span className="panel-label">Selected stage</span><h3><SelectedIcon size={21} />{selected.title}</h3></div><span className="tag success">{number(counts[selected.id])} open</span></div>
          {rows.length ? <div className="simple-list">{rows.slice(0, 6).map((item, index) => <div className="list-row" key={item.supplierId || item.productId || index}><span><strong>{item.supplierName || item.productName}</strong><small>{item.dueDate ? `Due ${new Date(item.dueDate).toLocaleDateString('en-IN')}` : `Minimum threshold ${item.minimumThreshold}`}</small></span><b>{item.pendingAmount ? Number(item.pendingAmount).toLocaleString('en-IN', { style: 'currency', currency: 'INR' }) : `${number(item.currentStock)} left`}</b></div>)}</div> : <div className="workflow-empty"><CheckCircle2 size={28} /><h4>No blockers in this stage</h4><p>When work needs attention, it will appear here.</p></div>}
        </section>
        <aside className="panel workflow-next"><span className="panel-label">Next handoff</span><Factory size={24} /><h3>{selectedStage === 'deliver' ? 'Customer complete' : stages[stages.findIndex((stage) => stage.id === selectedStage) + 1]?.title || 'Store & control'}</h3><p>{selectedStage === 'deliver' ? 'Capture the delivery and close the order loop.' : 'Keep this stage clear so the next team can move.'}</p><div className="handoff-line"><span /><span /><span /><span /></div><div className="handoff-meta"><span>Owner</span><strong>{selectedStage === 'source' ? 'Procurement' : selectedStage === 'stock' ? 'Warehouse' : selectedStage === 'sell' ? 'Sales' : 'Dispatch'}</strong></div></aside>
      </div>

      <div className="workflow-summary"><div><PackageCheck size={18} /><span>Inventory signal</span><strong>{number(cards.currentInventory)} units in current stock</strong></div><div><CircleDollarSign size={18} /><span>Payment signal</span><strong>{number(cards.pendingSupplierPayments?.length || 0)} supplier obligations</strong></div><div><Boxes size={18} /><span>Exception signal</span><strong>{number((data?.lowStockProducts || []).length)} low-stock alerts</strong></div></div>
    </>}
  </div>;
};

export default WorkflowPage;
