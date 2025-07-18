import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Alert,
  ScrollView,
  ActivityIndicator,
  Switch,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import * as Location from 'expo-location';
import MapView, { Marker, Polyline } from 'react-native-maps';
import apiClient from '../services/apiClient';

export default function GPSTrackingScreen() {
  const [location, setLocation] = useState(null);
  const [tracking, setTracking] = useState(false);
  const [route, setRoute] = useState([]);
  const [geofences, setGeofences] = useState([]);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tripId, setTripId] = useState(null);
  const [trackingHistory, setTrackingHistory] = useState([]);
  const [speed, setSpeed] = useState(0);
  const [accuracy, setAccuracy] = useState(0);

  useEffect(() => {
    initializeGPS();
    fetchGeofences();
    return () => {
      if (tracking) {
        stopTracking();
      }
    };
  }, []);

  const initializeGPS = async () => {
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Denied', 'Location permission is required for GPS tracking');
        return;
      }

      const currentLocation = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.High,
      });

      setLocation({
        latitude: currentLocation.coords.latitude,
        longitude: currentLocation.coords.longitude,
        latitudeDelta: 0.01,
        longitudeDelta: 0.01,
      });

      setSpeed(currentLocation.coords.speed || 0);
      setAccuracy(currentLocation.coords.accuracy || 0);
    } catch (error) {
      console.error('Error initializing GPS:', error);
      Alert.alert('GPS Error', 'Failed to initialize GPS tracking');
    } finally {
      setLoading(false);
    }
  };

  const fetchGeofences = async () => {
    try {
      const response = await apiClient.get('/api/gps/geofences');
      setGeofences(response.data || []);
    } catch (error) {
      console.error('Error fetching geofences:', error);
    }
  };

  const startTracking = async () => {
    try {
      if (!location) {
        Alert.alert('GPS Error', 'Location not available');
        return;
      }

      const trackingData = {
        vehicleId: 1, // This would come from context/props
        driverId: 1,  // This would come from context/props
        startLocation: {
          latitude: location.latitude,
          longitude: location.longitude
        },
        timestamp: new Date().toISOString()
      };

      const response = await apiClient.post('/api/gps/start-tracking', trackingData);
      
      if (response.data && response.data.tripId) {
        setTripId(response.data.tripId);
        setTracking(true);
        
        const locationSubscription = await Location.watchPositionAsync(
          {
            accuracy: Location.Accuracy.High,
            timeInterval: 5000, // Update every 5 seconds
            distanceInterval: 10, // Update every 10 meters
          },
          (newLocation) => {
            updateLocation(newLocation);
          }
        );

        Alert.alert('Tracking Started', 'GPS tracking is now active');
      }
    } catch (error) {
      console.error('Error starting tracking:', error);
      Alert.alert('Error', 'Failed to start GPS tracking');
    }
  };

  const stopTracking = async () => {
    try {
      if (tripId) {
        const stopData = {
          tripId,
          endLocation: {
            latitude: location.latitude,
            longitude: location.longitude
          },
          timestamp: new Date().toISOString(),
          totalDistance: calculateTotalDistance(),
          averageSpeed: calculateAverageSpeed()
        };

        await apiClient.post('/api/gps/stop-tracking', stopData);
      }

      setTracking(false);
      setTripId(null);
      Alert.alert('Tracking Stopped', 'GPS tracking has been stopped');
    } catch (error) {
      console.error('Error stopping tracking:', error);
      Alert.alert('Error', 'Failed to stop GPS tracking');
    }
  };

  const updateLocation = async (newLocation) => {
    const newCoords = {
      latitude: newLocation.coords.latitude,
      longitude: newLocation.coords.longitude,
    };

    setLocation(prev => ({
      ...prev,
      ...newCoords
    }));

    setSpeed(newLocation.coords.speed || 0);
    setAccuracy(newLocation.coords.accuracy || 0);

    setRoute(prev => [...prev, newCoords]);
    setTrackingHistory(prev => [...prev, {
      ...newCoords,
      timestamp: new Date().toISOString(),
      speed: newLocation.coords.speed || 0
    }]);

    checkGeofenceViolations(newCoords);

    if (tracking && tripId) {
      try {
        const locationData = {
          tripId,
          latitude: newCoords.latitude,
          longitude: newCoords.longitude,
          speed: newLocation.coords.speed || 0,
          accuracy: newLocation.coords.accuracy || 0,
          timestamp: new Date().toISOString()
        };

        await apiClient.post('/api/gps/update-location', locationData);
      } catch (error) {
        console.error('Error updating location:', error);
      }
    }
  };

  const checkGeofenceViolations = (coords) => {
    geofences.forEach(geofence => {
      const distance = calculateDistance(coords, geofence.center);
      
      if (geofence.type === 'restricted' && distance < geofence.radius) {
        const alert = {
          id: Date.now(),
          type: 'geofence_violation',
          message: `Entered restricted area: ${geofence.name}`,
          timestamp: new Date().toISOString(),
          location: coords
        };
        
        setAlerts(prev => [alert, ...prev.slice(0, 9)]); // Keep last 10 alerts
        
        sendGeofenceAlert(alert);
      }
    });
  };

  const sendGeofenceAlert = async (alert) => {
    try {
      await apiClient.post('/api/gps/geofence-alert', {
        tripId,
        alertType: alert.type,
        message: alert.message,
        location: alert.location,
        timestamp: alert.timestamp
      });
    } catch (error) {
      console.error('Error sending geofence alert:', error);
    }
  };

  const calculateDistance = (point1, point2) => {
    const R = 6371e3; // Earth's radius in meters
    const φ1 = point1.latitude * Math.PI/180;
    const φ2 = point2.latitude * Math.PI/180;
    const Δφ = (point2.latitude-point1.latitude) * Math.PI/180;
    const Δλ = (point2.longitude-point1.longitude) * Math.PI/180;

    const a = Math.sin(Δφ/2) * Math.sin(Δφ/2) +
              Math.cos(φ1) * Math.cos(φ2) *
              Math.sin(Δλ/2) * Math.sin(Δλ/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));

    return R * c;
  };

  const calculateTotalDistance = () => {
    if (route.length < 2) return 0;
    
    let totalDistance = 0;
    for (let i = 1; i < route.length; i++) {
      totalDistance += calculateDistance(route[i-1], route[i]);
    }
    return totalDistance;
  };

  const calculateAverageSpeed = () => {
    if (trackingHistory.length === 0) return 0;
    
    const totalSpeed = trackingHistory.reduce((sum, point) => sum + point.speed, 0);
    return totalSpeed / trackingHistory.length;
  };

  const formatSpeed = (speedMs) => {
    return (speedMs * 3.6).toFixed(1); // Convert m/s to km/h
  };

  const formatDistance = (meters) => {
    if (meters < 1000) {
      return `${meters.toFixed(0)}m`;
    }
    return `${(meters / 1000).toFixed(2)}km`;
  };

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <ActivityIndicator size="large" color="#3b82f6" />
        <Text style={styles.loadingText}>Initializing GPS...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>GPS Tracking</Text>
        <View style={styles.trackingToggle}>
          <Text style={styles.toggleLabel}>Tracking</Text>
          <Switch
            value={tracking}
            onValueChange={tracking ? stopTracking : startTracking}
            trackColor={{ false: '#e2e8f0', true: '#10b981' }}
            thumbColor={tracking ? '#ffffff' : '#64748b'}
          />
        </View>
      </View>

      <View style={styles.statsContainer}>
        <View style={styles.statCard}>
          <Ionicons name="speedometer" size={20} color="#3b82f6" />
          <Text style={styles.statValue}>{formatSpeed(speed)} km/h</Text>
          <Text style={styles.statLabel}>Speed</Text>
        </View>
        <View style={styles.statCard}>
          <Ionicons name="location" size={20} color="#10b981" />
          <Text style={styles.statValue}>{accuracy.toFixed(0)}m</Text>
          <Text style={styles.statLabel}>Accuracy</Text>
        </View>
        <View style={styles.statCard}>
          <Ionicons name="navigate" size={20} color="#f59e0b" />
          <Text style={styles.statValue}>{formatDistance(calculateTotalDistance())}</Text>
          <Text style={styles.statLabel}>Distance</Text>
        </View>
      </View>

      {location && (
        <MapView
          style={styles.map}
          region={location}
          showsUserLocation={true}
          showsMyLocationButton={true}
          followsUserLocation={tracking}
        >
          {/* Current location marker */}
          <Marker
            coordinate={{
              latitude: location.latitude,
              longitude: location.longitude,
            }}
            title="Current Location"
            pinColor={tracking ? "#10b981" : "#3b82f6"}
          />

          {/* Route polyline */}
          {route.length > 1 && (
            <Polyline
              coordinates={route}
              strokeColor="#3b82f6"
              strokeWidth={3}
            />
          )}

          {/* Geofence markers */}
          {geofences.map(geofence => (
            <Marker
              key={geofence.id}
              coordinate={geofence.center}
              title={geofence.name}
              description={geofence.type}
              pinColor={geofence.type === 'restricted' ? "#ef4444" : "#10b981"}
            />
          ))}
        </MapView>
      )}

      {alerts.length > 0 && (
        <ScrollView style={styles.alertsContainer}>
          <Text style={styles.alertsTitle}>Recent Alerts</Text>
          {alerts.map(alert => (
            <View key={alert.id} style={styles.alertCard}>
              <Ionicons name="warning" size={16} color="#ef4444" />
              <View style={styles.alertContent}>
                <Text style={styles.alertMessage}>{alert.message}</Text>
                <Text style={styles.alertTime}>
                  {new Date(alert.timestamp).toLocaleTimeString()}
                </Text>
              </View>
            </View>
          ))}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  centerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 20,
    backgroundColor: '#ffffff',
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  trackingToggle: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  toggleLabel: {
    fontSize: 16,
    color: '#64748b',
    marginRight: 8,
  },
  statsContainer: {
    flexDirection: 'row',
    padding: 16,
    justifyContent: 'space-around',
  },
  statCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    alignItems: 'center',
    flex: 1,
    marginHorizontal: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  statValue: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginTop: 4,
  },
  statLabel: {
    fontSize: 12,
    color: '#64748b',
    marginTop: 2,
  },
  map: {
    flex: 1,
    margin: 16,
    borderRadius: 12,
  },
  alertsContainer: {
    maxHeight: 150,
    backgroundColor: '#ffffff',
    margin: 16,
    borderRadius: 12,
    padding: 16,
  },
  alertsTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 12,
  },
  alertCard: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  alertContent: {
    flex: 1,
    marginLeft: 8,
  },
  alertMessage: {
    fontSize: 14,
    color: '#1e293b',
  },
  alertTime: {
    fontSize: 12,
    color: '#64748b',
    marginTop: 2,
  },
  loadingText: {
    fontSize: 16,
    color: '#64748b',
    marginTop: 8,
  },
});
