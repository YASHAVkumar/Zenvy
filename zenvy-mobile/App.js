import React, { useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { StatusBar } from 'expo-status-bar';
import { AuthProvider, useAuth } from './src/auth/AuthContext';
import LoginScreen from './src/screens/LoginScreen';
import DashboardScreen from './src/screens/DashboardScreen';
import ProductsScreen from './src/screens/ProductsScreen';
import OperationsScreen from './src/screens/OperationsScreen';
import ProfileScreen from './src/screens/ProfileScreen';
import { colors } from './src/theme';

const Tab = createBottomTabNavigator();

function AppContent() {
  const { token, loading } = useAuth();

  if (loading) {
    return <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.background }}>
      <ActivityIndicator size="large" color={colors.primary} />
    </View>;
  }

  if (!token) return <LoginScreen />;

  return (
    <NavigationContainer>
      <Tab.Navigator screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        tabBarStyle: { height: 64, paddingTop: 7, paddingBottom: 7, borderTopColor: colors.border, backgroundColor: '#FFFFFF' },
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
      }}>
        <Tab.Screen name="Dashboard" component={DashboardScreen} options={{ tabBarLabel: 'Home', tabBarIcon: ({ color }) => <TabIcon symbol="⌂" color={color} /> }} />
        <Tab.Screen name="Products" component={ProductsScreen} options={{ tabBarIcon: ({ color }) => <TabIcon symbol="▦" color={color} /> }} />
        <Tab.Screen name="Operations" component={OperationsScreen} options={{ tabBarIcon: ({ color }) => <TabIcon symbol="↗" color={color} /> }} />
        <Tab.Screen name="Profile" component={ProfileScreen} options={{ tabBarIcon: ({ color }) => <TabIcon symbol="●" color={color} /> }} />
      </Tab.Navigator>
    </NavigationContainer>
  );
}

function TabIcon({ symbol, color }) {
  const { Text } = require('react-native');
  return <Text style={{ color, fontSize: 22, fontWeight: '700' }}>{symbol}</Text>;
}

export default function App() {
  return <AuthProvider><StatusBar style="dark" /><AppContent /></AuthProvider>;
}
