import React, { createContext, useContext, useEffect, useState } from 'react';
import { useSignalRConnection } from './signalrConnection';
import { signalrEvents } from './signalrEvents';

export const SignalRContext = createContext();

export const SignalRProvider = ({ children }) => {
  const connection = useSignalRConnection();
  const [messages, setMessages] = useState([]);
  const [connected, setConnected] = useState(false);
  const [lastResourceChange, setLastResourceChange] = useState(null);
  const [lastInventoryChange, setLastInventoryChange] = useState(null);
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    if (!connection) return undefined;
    const startConnection = async () => {
      try {
        await connection.start();
        setConnected(true);
      } catch (err) {
        console.error('SignalR Connection Error:', err);
        setConnected(false);
      }
    };

    startConnection();

    return () => {
      setConnected(false);
      connection.stop();
    };
  }, [connection]);

  useEffect(() => {
    if (!connection) return undefined;
    const onReceiveMessage = (user, message) => {
      setMessages(prev => [...prev, { user, message }]);
    };

    connection.on(signalrEvents.ON_RECEIVE_MESSAGE, onReceiveMessage);
    connection.on(signalrEvents.ON_RESOURCE_CHANGED, setLastResourceChange);
    connection.on(signalrEvents.ON_INVENTORY_CHANGED, (event) => {
      setLastInventoryChange(event);
      setNotifications((previous) => [{ id: `${Date.now()}-${Math.random()}`, title: 'Inventory changed', message: `${event.actor || 'A team member'} updated stock.`, at: new Date() }, ...previous].slice(0, 20));
    });

    return () => {
      connection.off(signalrEvents.ON_RECEIVE_MESSAGE, onReceiveMessage);
      connection.off(signalrEvents.ON_RESOURCE_CHANGED, setLastResourceChange);
      connection.off(signalrEvents.ON_INVENTORY_CHANGED);
    };
  }, [connection]);

  return (
    <SignalRContext.Provider value={{ messages, connection, connected, lastResourceChange, lastInventoryChange, notifications, dismissNotification: (id) => setNotifications((previous) => previous.filter((item) => item.id !== id)) }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalR must be used within a SignalRProvider');
  }
  return context;
};