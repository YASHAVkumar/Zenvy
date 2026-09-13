import { Select } from 'react-native-paper';

export const Select = ({ label, value, onChange, items }) => {
  return (
    <Select
      label={label}
      value={value}
      onChange={onChange}
      items={items}
      style={{ marginBottom: 10 }}
    />
  );
};