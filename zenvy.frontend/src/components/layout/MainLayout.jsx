import { Box, Text, View } from 'react-native-paper';

export const MainLayout = ({ children }) => {
  return (
    <View style={{ flex: 1, backgroundColor: 'white' }}>
      <Header />
      <Sidebar />
      <View style={{ flex: 1, padding: 20 }}>
        {children}
      </View>
    </View>
  );
};