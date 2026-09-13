import { Provider } from 'react-redux';
import { RouterProvider } from 'react-router-dom';
import { SignalRProvider } from './signalr/signalrProvider';
import store from './store/store';
import router from './routes/AppRoutes';
import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
 <Provider store={store}>
   <SignalRProvider>
    <RouterProvider router={router} />
    </SignalRProvider>
  </Provider>
);

// Hot Module Replacement (HMR) for development
// if (import.meta.hot) {
//   import.meta.hot.dispose(() => {
//     root.unmount();
//   });
// }