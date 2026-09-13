import { Box, Text, View, Button } from 'react-native-paper';

export const Sidebar = () => {
  return (
    <View style={{ width: 200, backgroundColor: 'lightgray', padding: 10, height: '100%' }}>
      <Text style={{ fontWeight: 'bold', marginBottom: 10 }}>Menu</Text>
      <Button mode="contained" onPress={() => console.log('Dashboard clicked')}>Dashboard</Button>
      <Button mode="contained" onPress={() => console.log('Settings clicked')}>Settings</Button>
    </View>
  );
};