import { Badge } from 'react-native-paper';

export const Badge = ({ children, color = 'primary' }) => {
  return (
    <Badge style={{ backgroundColor: color }}>{children}</Badge>
  );
};