import { TextInput } from 'react-native-paper';

export const Input = ({ label, value, onChangeText, keyboardType = 'default', secureTextEntry = false }) => {
  return (
    <TextInput
      label={label}
      value={value}
      onChangeText={onChangeText}
      keyboardType={keyboardType}
      secureTextEntry={secureTextEntry}
      style={{ marginBottom: 10 }}
    />
  );
};