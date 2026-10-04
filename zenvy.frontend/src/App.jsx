import React, { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { getAccessTokenRefreshDelay, refreshSession } from './lib/api';
import { logout, setAuth } from './features/auth/authSlice';

const App = () => {
	const dispatch = useDispatch();
	const token = useSelector((state) => state.auth.token);

	useEffect(() => {
		const handleTokenRefresh = (event) => {
			dispatch(setAuth({ token: event.detail.token }));
		};
		const handleUnauthorized = () => dispatch(logout());
		const handleStorage = (event) => {
			if (event.key && !['zenvy_token', 'zenvy_refresh_token'].includes(event.key)) return;
			const latestToken = localStorage.getItem('zenvy_token');
			if (latestToken) dispatch(setAuth({ token: latestToken }));
			else dispatch(logout());
		};
		window.addEventListener('zenvy:token-refreshed', handleTokenRefresh);
		window.addEventListener('zenvy:unauthorized', handleUnauthorized);
		window.addEventListener('storage', handleStorage);
		return () => {
			window.removeEventListener('zenvy:token-refreshed', handleTokenRefresh);
			window.removeEventListener('zenvy:unauthorized', handleUnauthorized);
			window.removeEventListener('storage', handleStorage);
		};
	}, [dispatch]);

	useEffect(() => {
		if (!token) return undefined;
		const delay = getAccessTokenRefreshDelay(token);
		const timer = window.setTimeout(() => { refreshSession(); }, delay);
		return () => window.clearTimeout(timer);
	}, [token]);

	return <Outlet />;
};

export default App;
