import { Box, Typography } from 'react-native-paper';

export const EmptyState = ({ message }) => {
  return (
    <Box style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Typography variant="titleMedium">{message}</Typography>
    </Box>
  );
};