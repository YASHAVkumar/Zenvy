import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors, shadow } from '../theme';

export default function InfoCard({ label, value, hint, accent = colors.primary }) {
  return <View style={styles.card}>
    <View style={[styles.dot, { backgroundColor: accent }]} />
    <Text style={styles.label}>{label}</Text>
    <Text style={styles.value}>{value}</Text>
    {hint ? <Text style={styles.hint}>{hint}</Text> : null}
  </View>;
}
const styles = StyleSheet.create({
  card: { width: '48%', minHeight: 112, backgroundColor: colors.surface, borderRadius: 16, padding: 15, marginBottom: 12, borderWidth: 1, borderColor: colors.border, ...shadow },
  dot: { width: 7, height: 7, borderRadius: 4, marginBottom: 10 },
  label: { color: colors.muted, fontSize: 11, lineHeight: 16 },
  value: { color: colors.text, fontSize: 19, fontWeight: '800', marginTop: 5 },
  hint: { color: colors.muted, fontSize: 10, marginTop: 5 },
});
