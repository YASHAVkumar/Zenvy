import React from 'react';
import { Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { hasAnyRole } from '../features/auth/tokenClaims';

export const RoleRoute = ({ roles, children }) => {
  const userRoles = useSelector((state) => state.auth.user?.roles || state.auth.user?.role);
  const allowed = hasAnyRole(userRoles, roles);
  return allowed ? children : <Navigate to="/dashboard" replace />;
};
