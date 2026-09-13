import { Modal } from 'react-native-paper';

export const Modal = ({ visible, onDismiss, children }) => {
  return (
    <Modal
      visible={visible}
      onDismiss={onDismiss}
      style={{ margin: 20 }}
    >
      {children}
    </Modal>
  );
};