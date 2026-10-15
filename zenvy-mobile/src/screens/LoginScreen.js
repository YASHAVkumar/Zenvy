import React, { useState } from 'react';
import { ActivityIndicator, Alert, KeyboardAvoidingView, Platform, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { useAuth } from '../auth/AuthContext';
import { colors } from '../theme';

export default function LoginScreen() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  
  const submit = async () => {
    if (!email.trim() || !password) {
      setError('Enter your email and password.');
      return;
    }
    setBusy(true);
    setError('');
    try {
       
      await login(email.trim(), password);
    } catch (e) {
      setError(e.message || 'Unable to sign in. Check the API address and your credentials.');
    } finally {
      setBusy(false);
    }
  };

  return <KeyboardAvoidingView style={styles.screen} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
    <View style={styles.container}>
      <View style={styles.brandMark}><Text style={styles.brandLetter}>Z</Text></View>
      <Text style={styles.eyebrow}>ZEN VY / CONTROL ROOM</Text>
      <Text style={styles.title}>Welcome back.</Text>
      <Text style={styles.subtitle}>Clarity for every order, rupee, and shelf.</Text>

      <Text style={styles.label}>Email address</Text>
      <TextInput
        style={styles.input}
        value={email}
        onChangeText={setEmail}
        placeholder="you@example.com"
        placeholderTextColor="#9A9CAF"
        keyboardType="email-address"
        autoCapitalize="none"
        autoCorrect={false}
      />
      <Text style={styles.label}>Password</Text>
      <TextInput
        style={styles.input}
        value={password}
        onChangeText={setPassword}
        placeholder="Enter your password"
        placeholderTextColor="#9A9CAF"
        secureTextEntry
        onSubmitEditing={submit}
        returnKeyType="go"
      />
      {error ? <Text style={styles.error}>{error}</Text> : null}
      <Pressable style={[styles.button, busy && styles.disabled]} onPress={submit} disabled={busy}>
        {busy ? <ActivityIndicator color="#FFFFFF" /> : <Text style={styles.buttonText}>Sign in</Text>}
      </Pressable>
      <Text style={styles.footer}>Business operations, in one calm view.</Text>
    </View>
  </KeyboardAvoidingView>;
}
const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.background, justifyContent: 'center' },
  container: { paddingHorizontal: 25, paddingVertical: 30 },
  brandMark: { width: 54, height: 54, borderRadius: 17, backgroundColor: colors.primary, justifyContent: 'center', alignItems: 'center', marginBottom: 23 },
  brandLetter: { color: '#FFFFFF', fontSize: 29, fontWeight: '900' },
  eyebrow: { color: colors.primary, fontSize: 10, fontWeight: '800', letterSpacing: 1.8, marginBottom: 11 },
  title: { fontSize: 32, fontWeight: '800', color: colors.text },
  subtitle: { fontSize: 14, lineHeight: 21, color: colors.muted, marginTop: 8, marginBottom: 28 },
  label: { fontSize: 12, color: colors.text, fontWeight: '700', marginBottom: 8, marginTop: 13 },
  input: { height: 52, borderRadius: 13, borderWidth: 1, borderColor: colors.border, backgroundColor: '#FFFFFF', paddingHorizontal: 15, fontSize: 14, color: colors.text },
  error: { color: colors.danger, fontSize: 12, marginTop: 12, lineHeight: 18 },
  button: { height: 52, borderRadius: 13, backgroundColor: colors.primary, alignItems: 'center', justifyContent: 'center', marginTop: 22 },
  disabled: { opacity: 0.7 },
  buttonText: { color: '#FFFFFF', fontSize: 14, fontWeight: '800' },
  footer: { color: colors.muted, textAlign: 'center', marginTop: 25, fontSize: 11 },
});
