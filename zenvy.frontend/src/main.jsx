import { Provider } from 'react-redux';
import { RouterProvider } from 'react-router-dom';
import { SignalRProvider } from './signalr/signalrProvider';
import store from './store/store';
import { logout, setAuth } from './features/auth/authSlice';
import router from './routes/AppRoutes';
import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { clearStoredSession, isAccessTokenExpired, refreshSession } from './lib/api';

const root = ReactDOM.createRoot(document.getElementById('root'));

const renderApplication = () => root.render(
  <Provider store={store}>
    <SignalRProvider>
      <RouterProvider router={router} />
    </SignalRProvider>
  </Provider>
);

const restore = isAccessTokenExpired()
  ? (localStorage.getItem('zenvy_refresh_token') ? refreshSession() : Promise.resolve(false))
  : Promise.resolve(true);

restore.finally(() => {
  const token = localStorage.getItem('zenvy_token');
  if (token && !isAccessTokenExpired(token)) store.dispatch(setAuth({ token }));
  else {
    clearStoredSession();
    store.dispatch(logout());
  }
  renderApplication();
});

// Hot Module Replacement (HMR) for development
// if (import.meta.hot) {
//   import.meta.hot.dispose(() => {
//     root.unmount();
//   });
// }
