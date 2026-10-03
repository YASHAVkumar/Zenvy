import React, { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { setAuth } from './features/auth/authSlice';

const App = () => {
	const dispatch = useDispatch();

	useEffect(() => {
		const handleTokenRefresh = (event) => {
			const user = localStorage.getItem('zenvy_user');
			dispatch(setAuth({ token: event.detail.token, user: user ? JSON.parse(user) : null }));
		};
		window.addEventListener('zenvy:token-refreshed', handleTokenRefresh);
		return () => window.removeEventListener('zenvy:token-refreshed', handleTokenRefresh);
	}, [dispatch]);

	return <Outlet />;
};

export default App;
