import { Button } from 'react-native-paper';

export const Button = ({ children, onPress, mode = 'contained', loading = false }) => {
  return (
    <Button
      mode={mode}
      onPress={onPress}
      loading={loading}
      style={{ marginVertical: 10 }}
    >
      {children}
    </Button>
  );
};