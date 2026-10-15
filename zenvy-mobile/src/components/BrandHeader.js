import React from 'react';
import { Text, View, StyleSheet } from 'react-native';
import { colors } from '../theme';

export default function BrandHeader({ eyebrow, title, subtitle }) {
  return <View style={styles.wrap}>
    {eyebrow ? <Text style={styles.eyebrow}>{eyebrow}</Text> : null}
    <Text style={styles.title}>{title}</Text>
    {subtitle ? <Text style={styles.subtitle}>{subtitle}</Text> : null}
  </View>;
}
const styles = StyleSheet.create({
  wrap: { marginBottom: 22 },
  eyebrow: { color: colors.primary, fontSize: 10, fontWeight: '800', letterSpacing: 1.8, marginBottom: 8 },
  title: { color: colors.text, fontSize: 28, fontWeight: '800', letterSpacing: -0.5 },
  subtitle: { color: colors.muted, fontSize: 13, lineHeight: 19, marginTop: 7 },
});
