import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  TouchableOpacity,
  RefreshControl,
  Alert,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';

export default function TripsScreen() {
  const [trips, setTrips] = useState([]);
  const [refreshing, setRefreshing] = useState(false);
  const [selectedFilter, setSelectedFilter] = useState('all');

  useEffect(() => {
    fetchTrips();
  }, []);

  const fetchTrips = async () => {
    try {
      // Mock data for trips
      const mockTrips = [
        {
          id: 1,
          routeName: 'Route A - Morning',
          driverName: 'Ahmed Al-Rashid',
          vehicleNumber: 'BUS-001',
          startTime: '07:00 AM',
          endTime: '08:30 AM',
          status: 'In Progress',
          studentsCount: 25,
          currentLocation: 'Al-Noor School',
        },
        {
          id: 2,
          routeName: 'Route B - Morning',
          driverName: 'Omar Al-Mansouri',
          vehicleNumber: 'BUS-002',
          startTime: '07:15 AM',
          endTime: '08:45 AM',
          status: 'Completed',
          studentsCount: 30,
          currentLocation: 'King Abdulaziz School',
        },
        {
          id: 3,
          routeName: 'Route C - Afternoon',
          driverName: 'Khalid Al-Zahra',
          vehicleNumber: 'BUS-003',
          startTime: '02:00 PM',
          endTime: '03:30 PM',
          status: 'Scheduled',
          studentsCount: 22,
          currentLocation: 'Depot',
        },
      ];
      setTrips(mockTrips);
    } catch (error) {
      console.error('Error fetching trips:', error);
    } finally {
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    fetchTrips();
  };

  const handleTripPress = (trip) => {
    Alert.alert(
      'Trip Details',
      `Route: ${trip.routeName}\nDriver: ${trip.driverName}\nVehicle: ${trip.vehicleNumber}\nTime: ${trip.startTime} - ${trip.endTime}\nStudents: ${trip.studentsCount}\nStatus: ${trip.status}`,
      [
        { text: 'Track Vehicle', onPress: () => console.log('Track vehicle') },
        { text: 'Contact Driver', onPress: () => console.log('Contact driver') },
        { text: 'Close', style: 'cancel' },
      ]
    );
  };

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return '#10b981';
      case 'in progress':
        return '#2563eb';
      case 'scheduled':
        return '#f59e0b';
      case 'cancelled':
        return '#ef4444';
      default:
        return '#64748b';
    }
  };

  const getStatusIcon = (status) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return 'checkmark-circle';
      case 'in progress':
        return 'play-circle';
      case 'scheduled':
        return 'time';
      case 'cancelled':
        return 'close-circle';
      default:
        return 'help-circle';
    }
  };

  const renderTrip = ({ item }) => (
    <TouchableOpacity style={styles.tripCard} onPress={() => handleTripPress(item)}>
      <View style={styles.tripHeader}>
        <View style={styles.tripInfo}>
          <Text style={styles.routeName}>{item.routeName}</Text>
          <Text style={styles.timeRange}>{item.startTime} - {item.endTime}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) }]}>
          <Ionicons name={getStatusIcon(item.status)} size={16} color="#ffffff" />
          <Text style={styles.statusText}>{item.status}</Text>
        </View>
      </View>

      <View style={styles.tripDetails}>
        <View style={styles.detailRow}>
          <Ionicons name="person" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.driverName}</Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="bus" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.vehicleNumber}</Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="people" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.studentsCount} students</Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="location" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.currentLocation}</Text>
        </View>
      </View>

      <View style={styles.tripActions}>
        <TouchableOpacity style={styles.actionButton}>
          <Ionicons name="location" size={16} color="#2563eb" />
          <Text style={styles.actionText}>Track</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.actionButton}>
          <Ionicons name="call" size={16} color="#2563eb" />
          <Text style={styles.actionText}>Call Driver</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.actionButton}>
          <Ionicons name="list" size={16} color="#2563eb" />
          <Text style={styles.actionText}>Students</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );

  const filterButtons = [
    { key: 'all', label: 'All', count: trips.length },
    { key: 'in progress', label: 'Active', count: trips.filter(t => t.status === 'In Progress').length },
    { key: 'scheduled', label: 'Scheduled', count: trips.filter(t => t.status === 'Scheduled').length },
    { key: 'completed', label: 'Completed', count: trips.filter(t => t.status === 'Completed').length },
  ];

  const filteredTrips = selectedFilter === 'all' 
    ? trips 
    : trips.filter(trip => trip.status.toLowerCase() === selectedFilter);

  return (
    <View style={styles.container}>
      {/* Filter Tabs */}
      <View style={styles.filterContainer}>
        {filterButtons.map((filter) => (
          <TouchableOpacity
            key={filter.key}
            style={[
              styles.filterButton,
              selectedFilter === filter.key && styles.filterButtonActive
            ]}
            onPress={() => setSelectedFilter(filter.key)}
          >
            <Text style={[
              styles.filterText,
              selectedFilter === filter.key && styles.filterTextActive
            ]}>
              {filter.label}
            </Text>
            <View style={[
              styles.filterBadge,
              selectedFilter === filter.key && styles.filterBadgeActive
            ]}>
              <Text style={[
                styles.filterBadgeText,
                selectedFilter === filter.key && styles.filterBadgeTextActive
              ]}>
                {filter.count}
              </Text>
            </View>
          </TouchableOpacity>
        ))}
      </View>

      {/* Trips List */}
      <FlatList
        data={filteredTrips}
        renderItem={renderTrip}
        keyExtractor={(item) => item.id.toString()}
        style={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons name="bus-outline" size={64} color="#94a3b8" />
            <Text style={styles.emptyText}>No trips found</Text>
            <Text style={styles.emptySubtext}>
              {selectedFilter === 'all' 
                ? 'No trips scheduled for today'
                : `No ${selectedFilter} trips found`
              }
            </Text>
          </View>
        }
      />

      {/* Floating Action Button */}
      <TouchableOpacity style={styles.fab}>
        <Ionicons name="add" size={24} color="#ffffff" />
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  filterContainer: {
    flexDirection: 'row',
    backgroundColor: '#ffffff',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  filterButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 8,
    paddingHorizontal: 12,
    marginHorizontal: 4,
    borderRadius: 20,
    backgroundColor: '#f1f5f9',
  },
  filterButtonActive: {
    backgroundColor: '#2563eb',
  },
  filterText: {
    fontSize: 12,
    fontWeight: '600',
    color: '#64748b',
    marginRight: 6,
  },
  filterTextActive: {
    color: '#ffffff',
  },
  filterBadge: {
    backgroundColor: '#e2e8f0',
    borderRadius: 10,
    paddingHorizontal: 6,
    paddingVertical: 2,
    minWidth: 20,
    alignItems: 'center',
  },
  filterBadgeActive: {
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
  },
  filterBadgeText: {
    fontSize: 10,
    fontWeight: 'bold',
    color: '#64748b',
  },
  filterBadgeTextActive: {
    color: '#ffffff',
  },
  list: {
    flex: 1,
  },
  tripCard: {
    backgroundColor: '#ffffff',
    marginHorizontal: 16,
    marginVertical: 6,
    borderRadius: 12,
    padding: 16,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  tripHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  tripInfo: {
    flex: 1,
  },
  routeName: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  timeRange: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 2,
  },
  statusBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  statusText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
    marginLeft: 4,
  },
  tripDetails: {
    marginBottom: 12,
  },
  detailRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 6,
  },
  detailText: {
    fontSize: 14,
    color: '#64748b',
    marginLeft: 8,
  },
  tripActions: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    borderTopWidth: 1,
    borderTopColor: '#f1f5f9',
    paddingTop: 12,
  },
  actionButton: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 6,
  },
  actionText: {
    fontSize: 12,
    color: '#2563eb',
    marginLeft: 4,
    fontWeight: '500',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingVertical: 64,
  },
  emptyText: {
    fontSize: 18,
    color: '#64748b',
    marginTop: 16,
    textAlign: 'center',
  },
  emptySubtext: {
    fontSize: 14,
    color: '#94a3b8',
    marginTop: 8,
    textAlign: 'center',
  },
  fab: {
    position: 'absolute',
    bottom: 20,
    right: 20,
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: '#2563eb',
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 8,
  },
});

