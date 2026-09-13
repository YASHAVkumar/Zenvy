import { Box, Typography } from 'react-native-paper';

export const ErrorState = ({ error }) => {
  return (
    <Box style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Typography variant="titleMedium">Error: {error}</Typography>
    </Box>
  );
};