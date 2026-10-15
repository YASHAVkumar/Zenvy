import React from 'react';
import { Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { useAuth } from '../auth/AuthContext';
import Screen from '../components/Screen';
import BrandHeader from '../components/BrandHeader';
import { colors } from '../theme';

export default function ProfileScreen() {
  const { logout } = useAuth();
  const settings = [
    ['01', 'Account details', 'Profile information and access'],
    ['02', 'Security', 'Your signed-in session'],
    ['03', 'Help and support', 'Help with Zenvy Mobile'],
  ];

  return <Screen>
    <BrandHeader eyebrow="ACCOUNT" title="Profile" subtitle="Your account and app preferences." />
    <View style={styles.profileCard}>
      <View style={styles.avatar}><Text style={styles.avatarText}>Z</Text></View>
      <Text style={styles.name}>Zenvy account</Text>
      <Text style={styles.description}>Signed in to Zenvy Mobile</Text>
    </View>
    <Text style={styles.heading}>Settings</Text>
    {settings.map(([num, title, detail]) => <Pressable key={num} style={styles.row} onPress={() => Alert.alert(title, 'This section can be connected to the corresponding mobile feature next.')}>
      <View style={styles.number}><Text style={styles.numberText}>{num}</Text></View>
      <View style={styles.rowContent}><Text style={styles.rowTitle}>{title}</Text><Text style={styles.rowDetail}>{detail}</Text></View>
      <Text style={styles.arrow}>›</Text>
    </Pressable>)}
    <Pressable style={styles.logout} onPress={() => Alert.alert('Sign out', 'Are you sure you want to sign out?', [{ text: 'Cancel', style: 'cancel' }, { text: 'Sign out', style: 'destructive', onPress: logout }])}>
      <Text style={styles.logoutText}>Sign out</Text>
    </Pressable>
    <Text style={styles.version}>Zenvy Mobile · JavaScript build</Text>
  </Screen>;
}
const styles = StyleSheet.create({
  profileCard: { backgroundColor: '#FFFFFF', borderWidth: 1, borderColor: colors.border, borderRadius: 19, padding: 22, alignItems: 'center', marginBottom: 26 },
  avatar: { width: 68, height: 68, borderRadius: 22, backgroundColor: colors.accent, alignItems: 'center', justifyContent: 'center' },
  avatarText: { color: colors.primary, fontSize: 30, fontWeight: '900' },
  name: { color: colors.text, fontSize: 18, fontWeight: '800', marginTop: 13 },
  description: { color: colors.muted, fontSize: 12, marginTop: 5 },
  heading: { color: colors.text, fontSize: 18, fontWeight: '800', marginBottom: 13 },
  row: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFFFFF', borderRadius: 14, borderWidth: 1, borderColor: colors.border, padding: 13, marginBottom: 10 },
  number: { width: 40, height: 40, borderRadius: 12, backgroundColor: colors.background, alignItems: 'center', justifyContent: 'center' },
  numberText: { color: colors.primary, fontWeight: '800', fontSize: 12 },
  rowContent: { flex: 1, marginLeft: 12 },
  rowTitle: { color: colors.text, fontSize: 13, fontWeight: '800' },
  rowDetail: { color: colors.muted, fontSize: 11, marginTop: 4 },
  arrow: { color: colors.muted, fontSize: 25 },
  logout: { marginTop: 16, borderWidth: 1, borderColor: '#F2C7CF', borderRadius: 13, height: 49, alignItems: 'center', justifyContent: 'center', backgroundColor: '#FFF7F8' },
  logoutText: { color: colors.danger, fontWeight: '800', fontSize: 13 },
  version: { color: colors.muted, textAlign: 'center', fontSize: 10, marginTop: 22 },
});
