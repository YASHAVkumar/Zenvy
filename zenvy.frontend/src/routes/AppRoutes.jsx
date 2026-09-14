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
          { path: 'workspace', element: <WorkspacePage /> },
          { path: 'finance', element: <FinancePage /> },
          { path: 'compensation', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><CompensationPage /></RoleRoute> },
          { path: 'profit-distribution', element: <RoleRoute roles={['Admin', 'Manager', 'Accountant']}><ProfitDistributionPage /></RoleRoute> },
          { path: 'products', element: <ProductsPage /> },
          { path: 'admin', element: <AdminPage /> },
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