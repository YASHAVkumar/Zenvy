import React, { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import api from '../api/client';
import Screen from '../components/Screen';
import BrandHeader from '../components/BrandHeader';
import { colors } from '../theme';

export default function ProductsScreen() {
  const [products, setProducts] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const { data } = await api.get('/api/v1/products');
      const rows = Array.isArray(data) ? data : data?.items || data?.products || [];
      setProducts(rows);
    } catch (e) {
      setError(e.message || 'Could not load products.');
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = products.filter((p) =>
    String(p.name || p.productName || '').toLowerCase().includes(search.toLowerCase())
  );

  return <Screen>
    <BrandHeader eyebrow="CATALOGUE" title="Products" subtitle="Browse your product catalogue." />
    <TextInput value={search} onChangeText={setSearch} style={styles.search} placeholder="Search products..." placeholderTextColor="#9295A9" />
    {loading ? <ActivityIndicator size="large" color={colors.primary} style={{ marginTop: 30 }} /> : null}
    {error ? <View style={styles.message}><Text style={styles.errorTitle}>Could not load products</Text><Text style={styles.messageText}>{error}</Text><Text style={styles.messageText}>Confirm the API endpoint and your API configuration.</Text><Pressable style={styles.retry} onPress={load}><Text style={styles.retryText}>Try again</Text></Pressable></View> : null}
    {!loading && !error && filtered.length === 0 ? <View style={styles.empty}><Text style={styles.emptyEmoji}>▦</Text><Text style={styles.emptyTitle}>No products to show</Text><Text style={styles.messageText}>Products will appear here when the API returns catalogue data.</Text></View> : null}
    {filtered.map((p, index) => (
      <View key={String(p.id ?? p.productId ?? index)} style={styles.card}>
        <View style={styles.productIcon}><Text style={styles.productIconText}>Z</Text></View>
        <View style={styles.productInfo}>
          <Text style={styles.productName}>{p.name || p.productName || 'Unnamed product'}</Text>
          <Text style={styles.sku}>{p.sku || p.code || 'Product'}</Text>
        </View>
        <Text style={styles.price}>{new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(Number(p.price ?? p.sellingPrice) || 0)}</Text>
      </View>
    ))}
  </Screen>;
}
const styles = StyleSheet.create({
  search: { height: 49, backgroundColor: '#FFFFFF', borderWidth: 1, borderColor: colors.border, borderRadius: 13, paddingHorizontal: 14, color: colors.text, marginBottom: 17 },
  card: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFFFFF', borderRadius: 15, padding: 13, marginBottom: 11, borderWidth: 1, borderColor: colors.border },
  productIcon: { width: 45, height: 45, borderRadius: 13, backgroundColor: colors.accent, alignItems: 'center', justifyContent: 'center' },
  productIconText: { color: colors.primary, fontSize: 22, fontWeight: '900' },
  productInfo: { flex: 1, marginLeft: 12 },
  productName: { color: colors.text, fontSize: 13, fontWeight: '800' },
  sku: { color: colors.muted, fontSize: 11, marginTop: 5 },
  price: { color: colors.text, fontSize: 12, fontWeight: '800' },
  message: { backgroundColor: '#FFF1F2', borderRadius: 15, padding: 16 },
  errorTitle: { color: colors.danger, fontWeight: '800', marginBottom: 7 },
  messageText: { color: colors.muted, fontSize: 12, lineHeight: 18, marginTop: 4 },
  retry: { backgroundColor: colors.primary, alignSelf: 'flex-start', paddingVertical: 9, paddingHorizontal: 14, borderRadius: 9, marginTop: 12 },
  retryText: { color: '#FFFFFF', fontSize: 12, fontWeight: '800' },
  empty: { backgroundColor: '#FFFFFF', borderRadius: 16, padding: 25, alignItems: 'center' },
  emptyEmoji: { color: colors.primary, fontSize: 35, marginBottom: 10 },
  emptyTitle: { color: colors.text, fontSize: 15, fontWeight: '800' },
});
