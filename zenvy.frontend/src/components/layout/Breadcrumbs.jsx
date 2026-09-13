import { Box, Text } from 'react-native-paper';

export const Breadcrumbs = ({ items }) => {
  return (
    <Box style={{ padding: 10, backgroundColor: 'lightgray' }}>
      {items.map((item, index) => (
        <Text key={index} style={{ marginRight: 10 }}>{item}</Text>
      ))}
    </Box>
  );
};