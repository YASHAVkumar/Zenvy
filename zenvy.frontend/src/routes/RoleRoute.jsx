import React from 'react';
import { Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';

export const RoleRoute = ({ roles, children }) => {
  const role = String(useSelector((state) => state.auth.user?.role) || '').trim().toLowerCase();
  const allowed = roles.some((item) => item.toLowerCase() === role);
  return allowed ? children : <Navigate to="/dashboard" replace />;
};
