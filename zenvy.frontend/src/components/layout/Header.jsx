import { Box, Text, View, Button } from 'react-native-paper';

export const Header = () => {
  return (
    <View style={{ backgroundColor: 'lightgray', padding: 10, flexDirection: 'row', justifyContent: 'space-between' }}>
      <Text style={{ fontWeight: 'bold' }}>ZenVy</Text>
      <Button mode="contained" onPress={() => console.log('Logout clicked')}>Logout</Button>
    </View>
  );
};