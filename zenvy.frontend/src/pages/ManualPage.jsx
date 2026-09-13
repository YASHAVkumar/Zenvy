import React from 'react';
import { Download, FileText } from 'lucide-react';

const manual = `ZENVY WORKFLOW AND ROLE MANUAL
================================

PURPOSE
Zenvy connects purchasing, inventory, sales, finance, investors, and administration in one controlled workflow.

HIERARCHY
Admin -> Manager / Team Lead -> Functional owners -> Sales and operations users

ADMIN
- Create, approve, suspend, and manage users.
- Assign roles and control all modules.
- Review audit activity, inventory changes, and financial decisions.

MANAGER
- Coordinate daily business operations.
- Review orders, stock, purchasing, and team work.
- Escalate finance or access decisions to Admin.

TEAM LEAD
- Supervise a team and monitor operational queues.
- Review workflow exceptions and handoffs.
- Cannot approve users or change protected finance settings.

INVENTORY MANAGER
- Receive, adjust, transfer, and damage stock.
- Keep warehouse quantities accurate.
- Respond to live inventory change notifications and low-stock alerts.

ACCOUNTANT
- Review expenses, payments, profit, investors, and monthly distributions.
- Confirm period figures before profit distribution.

SALES PERSON
- Manage customers, sales orders, channels, and customer handoffs.
- Escalate stock or payment exceptions to the Team Lead or Manager.

APPROVAL FLOW
1. A manager signs up from the login page.
2. The account remains pending and cannot sign in.
3. Admin opens People & access and approves or suspends the request.
4. The manager signs in and receives only the access allowed by role.

LIVE NOTIFICATIONS
Inventory changes are broadcast to connected authorized users through SignalR. Email delivery can be enabled by configuring the deployment mail provider.

MONTHLY INVESTMENT CYCLE
Accountant reviews the profit period, confirms positive net profit, and runs one distribution for the month. The existing API prevents duplicate month/year distributions.
`;

const ManualPage = () => {
  const download = () => { const blob = new Blob([manual], { type: 'text/plain;charset=utf-8' }); const url = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = url; anchor.download = 'zenvy-role-and-workflow-manual.txt'; anchor.click(); URL.revokeObjectURL(url); };
  return <div className="manual-page"><div className="page-heading"><div><p className="eyebrow">Workspace / operating guide</p><h2>Role manual</h2><p className="muted">A quick, downloadable guide to responsibilities and approvals.</p></div><button className="primary-button" onClick={download}><Download size={16} />Download manual</button></div><section className="manual-sheet"><FileText size={24} /><pre>{manual}</pre></section></div>;
};
export default ManualPage;
