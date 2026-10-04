import React from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import App from '../App';
import { ProtectedRoute } from './ProtectedRoute';
import { PublicRoute } from './PublicRoute';
import { NotFoundPage } from './NotFoundPage';
import AppLayout from '../components/AppLayout';
import LoginPage from '../pages/LoginPage';
import DashboardPage from '../pages/DashboardPage';
import ProductsPage from '../pages/ProductsPage';
import FinancePage from '../pages/FinancePage';
import OperationsPage from '../pages/OperationsPage';
import WorkflowPage from '../pages/WorkflowPage';
import WorkspacePage from '../pages/WorkspacePage';
import AdminPage from '../pages/AdminPage';
import ManualPage from '../pages/ManualPage';
import CompensationPage from '../pages/CompensationPage';
import ProfitDistributionPage from '../pages/ProfitDistributionPage';
import SalesDeskPage from '../pages/SalesDeskPage';
import PaymentsPage from '../pages/PaymentsPage';
import { RoleRoute } from './RoleRoute';

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />, 
    children: [
      {
        index: true,
        element: <Navigate to="/login" replace />,
      },
      {
        path: 'login',
        element: <PublicRoute><LoginPage /></PublicRoute>,
      },
      {
        element: <ProtectedRoute><AppLayout /></ProtectedRoute>,
        children: [
          { path: 'dashboard', element: <DashboardPage /> },
          { path: 'operations', element: <OperationsPage /> },
          { path: 'workflow', element: <WorkflowPage /> },
          { path: 'workspace', element: <RoleRoute roles={['Admin', 'Manager', 'TeamLead']}><WorkspacePage /></RoleRoute> },
          { path: 'finance', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><FinancePage /></RoleRoute> },
          { path: 'compensation', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><CompensationPage /></RoleRoute> },
          { path: 'profit-distribution', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><ProfitDistributionPage /></RoleRoute> },
          { path: 'products', element: <ProductsPage /> },
          { path: 'sales-desk', element: <RoleRoute roles={['Admin', 'Manager', 'SalesPerson', 'TeamLead']}><SalesDeskPage /></RoleRoute> },
          { path: 'payments', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><PaymentsPage /></RoleRoute> },
          { path: 'admin', element: <RoleRoute roles={['Admin']}><AdminPage /></RoleRoute> },
          { path: 'manual', element: <ManualPage /> },
        ],
      },
      {
        path: '*',
        element: <NotFoundPage />
      }
    ]
  }
]);

export default router;