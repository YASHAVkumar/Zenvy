import { Snackbar } from 'react-native-paper';

export const Notification = ({ visible, message, onDismiss }) => {
  return (
    <Snackbar
      visible={visible}
      onDismiss={onDismiss}
      style={{ margin: 20 }}
    >
      {message}
    </Snackbar>
  );
};