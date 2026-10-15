import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import Screen from '../components/Screen';
import BrandHeader from '../components/BrandHeader';
import { colors } from '../theme';

const tasks = [
  { number: '01', title: 'Inventory control', detail: 'Track stock and product movement.' },
  { number: '02', title: 'Sales workflow', detail: 'Keep daily selling activities organized.' },
  { number: '03', title: 'Payments follow-up', detail: 'Review pending collections and payments.' },
];

export default function OperationsScreen() {
  return <Screen>
    <BrandHeader eyebrow="DAILY WORK" title="Operations" subtitle="A quick entry point for day-to-day business tasks." />
    {tasks.map((task) => <View key={task.number} style={styles.card}>
      <View style={styles.number}><Text style={styles.numberText}>{task.number}</Text></View>
      <View style={styles.content}><Text style={styles.title}>{task.title}</Text><Text style={styles.detail}>{task.detail}</Text></View>
    </View>)}
    <View style={styles.notice}><Text style={styles.noticeTitle}>Mobile workflow starter</Text><Text style={styles.noticeText}>These are navigation placeholders. The web project's individual operations and workflow pages still need their forms and API actions ported to native components.</Text></View>
  </Screen>;
}
const styles = StyleSheet.create({
  card: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFFFFF', borderRadius: 16, borderWidth: 1, borderColor: colors.border, padding: 16, marginBottom: 12 },
  number: { width: 43, height: 43, borderRadius: 13, backgroundColor: colors.accent, alignItems: 'center', justifyContent: 'center' },
  numberText: { color: colors.primary, fontWeight: '900', fontSize: 13 },
  content: { flex: 1, marginLeft: 13 },
  title: { color: colors.text, fontWeight: '800', fontSize: 14 },
  detail: { color: colors.muted, fontSize: 12, lineHeight: 18, marginTop: 5 },
  notice: { backgroundColor: '#F0EDFF', borderRadius: 15, padding: 16, marginTop: 8 },
  noticeTitle: { color: colors.primaryDark, fontSize: 13, fontWeight: '800' },
  noticeText: { color: '#5F5A83', fontSize: 12, lineHeight: 18, marginTop: 6 },
});
