import React, { useEffect } from 'react';
import { View, Text, TouchableOpacity } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { StatusBar } from 'expo-status-bar';
import OfflineIndicator from './src/components/OfflineIndicator';
import syncService from './src/services/syncService';
import notificationService from './src/services/notificationService';
import localizationService from './src/localization';
import { ThemeProvider } from './src/contexts/ThemeContext';

// Import screens
import LoginScreen from './src/screens/LoginScreen';
import DashboardScreen from './src/screens/DashboardScreen';
import StudentsScreen from './src/screens/StudentsScreen';
import DriversScreen from './src/screens/DriversScreen';
import VehiclesScreen from './src/screens/VehiclesScreen';
import RoutesScreen from './src/screens/RoutesScreen';
import TripsScreen from './src/screens/TripsScreen';
import AttendanceScreen from './src/screens/AttendanceScreen';
import PaymentsScreen from './src/screens/PaymentsScreen';
import MaintenanceScreen from './src/screens/MaintenanceScreen';
import ProfileScreen from './src/screens/ProfileScreen';
import { AuthProvider, useAuth } from './src/contexts/AuthContext';

const Stack = createStackNavigator();
const Tab = createBottomTabNavigator();

function MoreMenuScreen({ navigation }) {
  const menuItems = [
    { name: 'Routes', icon: 'map', screen: 'Routes' },
    { name: 'Trips', icon: 'car', screen: 'Trips' },
    { name: 'Attendance', icon: 'checkmark-circle', screen: 'Attendance' },
    { name: 'Payments', icon: 'card', screen: 'Payments' },
    { name: 'Maintenance', icon: 'construct', screen: 'Maintenance' },
    { name: 'Profile', icon: 'person-circle', screen: 'Profile' },
  ];

  return (
    <View style={{ flex: 1, backgroundColor: '#f8fafc' }}>
      {menuItems.map((item, index) => (
        <TouchableOpacity
          key={index}
          style={{
            flexDirection: 'row',
            alignItems: 'center',
            padding: 20,
            backgroundColor: '#ffffff',
            marginVertical: 1,
            borderBottomWidth: 1,
            borderBottomColor: '#e2e8f0',
          }}
          onPress={() => navigation.navigate(item.screen)}
        >
          <Ionicons name={item.icon} size={24} color="#2563eb" />
          <Text style={{ marginLeft: 16, fontSize: 16, color: '#1e293b' }}>{item.name}</Text>
          <Ionicons name="chevron-forward" size={20} color="#64748b" style={{ marginLeft: 'auto' }} />
        </TouchableOpacity>
      ))}
    </View>
  );
}

function MoreTabScreen() {
  return (
    <Stack.Navigator screenOptions={{ headerShown: true }}>
      <Stack.Screen name="MoreMenu" component={MoreMenuScreen} options={{ title: 'More' }} />
      <Stack.Screen name="Routes" component={RoutesScreen} />
      <Stack.Screen name="Trips" component={TripsScreen} />
      <Stack.Screen name="Attendance" component={AttendanceScreen} />
      <Stack.Screen name="Payments" component={PaymentsScreen} />
      <Stack.Screen name="Maintenance" component={MaintenanceScreen} />
      <Stack.Screen name="Profile" component={ProfileScreen} />
    </Stack.Navigator>
  );
}

function TabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        tabBarIcon: ({ focused, color, size }) => {
          let iconName;

          if (route.name === 'Dashboard') {
            iconName = focused ? 'home' : 'home-outline';
          } else if (route.name === 'Students') {
            iconName = focused ? 'people' : 'people-outline';
          } else if (route.name === 'Drivers') {
            iconName = focused ? 'person' : 'person-outline';
          } else if (route.name === 'Vehicles') {
            iconName = focused ? 'bus' : 'bus-outline';
          } else if (route.name === 'More') {
            iconName = focused ? 'grid' : 'grid-outline';
          }

          return <Ionicons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: '#2563eb',
        tabBarInactiveTintColor: 'gray',
        headerStyle: {
          backgroundColor: '#2563eb',
        },
        headerTintColor: '#fff',
        headerTitleStyle: {
          fontWeight: 'bold',
        },
      })}
    >
      <Tab.Screen name="Dashboard" component={DashboardScreen} />
      <Tab.Screen name="Students" component={StudentsScreen} />
      <Tab.Screen name="Drivers" component={DriversScreen} />
      <Tab.Screen name="Vehicles" component={VehiclesScreen} />
      <Tab.Screen name="More" component={MoreTabScreen} />
    </Tab.Navigator>
  );
}

function AppNavigator() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return null; // You can add a loading screen here
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {isAuthenticated ? (
          <Stack.Screen name="Main" component={TabNavigator} />
        ) : (
          <Stack.Screen name="Login" component={LoginScreen} />
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}

export default function App() {
  useEffect(() => {
    const initializeServices = async () => {
      try {
        const localizationInitialized = await localizationService.initialize();
        if (localizationInitialized) {
          console.log('Localization service initialized successfully');
        }

        const notificationInitialized = await notificationService.initialize();
        if (notificationInitialized) {
          console.log('Notification service initialized successfully');
        }

      } catch (error) {
        console.error('Failed to initialize services:', error);
      }
    };

    initializeServices();

    return () => {
      syncService.cleanup();
      notificationService.cleanup();
    };
  }, []);

  return (
    <ThemeProvider>
      <AuthProvider>
        <StatusBar style="light" />
        <View style={{ flex: 1 }}>
          <OfflineIndicator />
          <AppNavigator />
        </View>
      </AuthProvider>
    </ThemeProvider>
  );
}

