import { HubConnectionBuilder } from '@microsoft/signalr';
import { useMemo } from 'react';
import { useAuth } from '../hooks/useAuth';

export const useSignalRConnection = () => {
  const { token } = useAuth();
  const connection = useMemo(() => {
    const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/hubs/notifications`;
    if (!hubUrl || !token) return null;
    return new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();
  }, [token]);

  return connection;
};