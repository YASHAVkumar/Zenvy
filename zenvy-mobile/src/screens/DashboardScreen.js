import React, { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import api from '../api/client';
import Screen from '../components/Screen';
import BrandHeader from '../components/BrandHeader';
import InfoCard from '../components/InfoCard';
import { colors } from '../theme';

const money = (value) => new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(Number(value) || 0);
const metricDefinitions = [
  ['todaysSales', "Today's sales", 'money'],
  ['monthlySales', 'Monthly sales', 'money'],
  ['monthlyProfit', 'Monthly profit', 'money'],
  ['inventoryValue', 'Inventory value', 'money'],
  ['availableCash', 'Available cash', 'money'],
  ['pendingOrders', 'Pending orders', 'number'],
];

export default function DashboardScreen() {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true); else setLoading(true);
    setError('');
    try {
      const { data } = await api.get('/api/v1/dashboards/executive');
      setDashboard(data?.data || data || {});
    } catch (e) {
      setError(e.message || 'Could not load dashboard.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const cards = dashboard?.kpiCards || {};

  return <Screen>
    <BrandHeader eyebrow="ZEN VY / OVERVIEW" title="Good to see you." subtitle="Here's the latest pulse of your business." />
    <View style={styles.banner}>
      <Text style={styles.bannerLabel}>EXECUTIVE OVERVIEW</Text>
      <Text style={styles.bannerTitle}>Your business, at a glance.</Text>
      <Text style={styles.bannerText}>Sales, inventory and cash flow in one place.</Text>
    </View>

    <View style={styles.sectionHeading}>
      <Text style={styles.sectionTitle}>Business metrics</Text>
      <Pressable onPress={() => load(true)}><Text style={styles.refresh}>Refresh ↻</Text></Pressable>
    </View>

    {loading ? <ActivityIndicator size="large" color={colors.primary} style={{ marginTop: 35 }} /> : null}
    {error ? <View style={styles.errorBox}>
      <Text style={styles.errorTitle}>Dashboard unavailable</Text>
      <Text style={styles.errorText}>{error}</Text>
      <Text style={styles.errorText}>Check the API URL in src/config.js and ensure the API is reachable from your phone.</Text>
      <Pressable onPress={() => load(true)} style={styles.retry}><Text style={styles.retryText}>Try again</Text></Pressable>
    </View> : null}
    {!loading && !error ? <>
      <View style={styles.grid}>
        {metricDefinitions.map(([key, label, type], i) => (
          <InfoCard key={key} label={label} value={type === 'money' ? money(cards[key]) : new Intl.NumberFormat('en-IN').format(Number(cards[key]) || 0)} hint="Current period" accent={['#5141A8', '#14866D', '#D97706', '#2563EB', '#0F766E', '#BE185D'][i]} />
        ))}
      </View>
      <View style={styles.panel}>
        <Text style={styles.panelTitle}>Business pulse</Text>
        <PulseRow label="Pending returns" value={cards.pendingReturns ?? 0} />
        <PulseRow label="Investor liability" value={money(cards.investorLiability)} />
        <PulseRow label="Reserve fund" value={money(cards.reserveFund)} />
      </View>
    </> : null}
    <Text style={styles.note}>Pull down to refresh your business data.</Text>
  </Screen>;
}

function PulseRow({ label, value }) {
  return <View style={styles.pulseRow}><Text style={styles.pulseLabel}>{label}</Text><Text style={styles.pulseValue}>{String(value)}</Text></View>;
}

const styles = StyleSheet.create({
  banner: { backgroundColor: colors.primaryDark, borderRadius: 20, padding: 20, marginBottom: 24 },
  bannerLabel: { color: '#C9C0FF', fontSize: 10, letterSpacing: 1.4, fontWeight: '800' },
  bannerTitle: { color: '#FFFFFF', fontSize: 22, lineHeight: 29, fontWeight: '800', marginTop: 10 },
  bannerText: { color: '#E4E0FF', fontSize: 12, marginTop: 8, lineHeight: 18 },
  sectionHeading: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 13 },
  sectionTitle: { fontSize: 18, fontWeight: '800', color: colors.text },
  refresh: { color: colors.primary, fontWeight: '700', fontSize: 12 },
  grid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  panel: { backgroundColor: '#FFFFFF', borderRadius: 16, borderWidth: 1, borderColor: colors.border, padding: 17, marginTop: 3 },
  panelTitle: { color: colors.text, fontSize: 16, fontWeight: '800', marginBottom: 12 },
  pulseRow: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 12, borderTopWidth: 1, borderTopColor: '#F0F1F6' },
  pulseLabel: { color: colors.muted, fontSize: 12 },
  pulseValue: { color: colors.text, fontSize: 12, fontWeight: '700' },
  errorBox: { backgroundColor: '#FFF1F2', borderRadius: 15, padding: 16, marginVertical: 10 },
  errorTitle: { color: colors.danger, fontSize: 15, fontWeight: '800', marginBottom: 7 },
  errorText: { color: '#8D3445', fontSize: 12, lineHeight: 18, marginBottom: 5 },
  retry: { backgroundColor: colors.danger, alignSelf: 'flex-start', borderRadius: 9, paddingVertical: 9, paddingHorizontal: 14, marginTop: 8 },
  retryText: { color: '#FFFFFF', fontWeight: '800', fontSize: 12 },
  note: { color: colors.muted, fontSize: 11, textAlign: 'center', marginTop: 20 },
});
