import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  RefreshControl,
  TouchableOpacity,
  TextInput,
  Alert,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import apiClient from '../services/apiClient';

export default function DriversScreen() {
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/drivers');
      setDrivers(response.data || []);
    } catch (error) {
      console.error('Error fetching drivers:', error);
      Alert.alert('Error', 'Failed to load drivers');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = React.useCallback(async () => {
    setRefreshing(true);
    await fetchDrivers();
    setRefreshing(false);
  }, []);

  useEffect(() => {
    fetchDrivers();
  }, []);

  const filteredDrivers = drivers.filter(driver =>
    driver.fullName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    driver.licenseNumber?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    driver.phoneNumber?.includes(searchQuery)
  );

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'active': return '#10b981';
      case 'inactive': return '#ef4444';
      case 'on_leave': return '#f59e0b';
      default: return '#64748b';
    }
  };

  const handleDriverPress = (driver) => {
    Alert.alert(
      'Driver Details',
      `Name: ${driver.fullName}\nLicense: ${driver.licenseNumber}\nPhone: ${driver.phoneNumber}\nStatus: ${driver.status}`,
      [
        { text: 'Call Driver', onPress: () => console.log('Call driver:', driver.phoneNumber) },
        { text: 'View Details', onPress: () => console.log('View driver details:', driver.id) },
        { text: 'Cancel', style: 'cancel' }
      ]
    );
  };

  const renderDriverItem = ({ item }) => (
    <TouchableOpacity style={styles.driverCard} onPress={() => handleDriverPress(item)}>
      <View style={styles.driverHeader}>
        <View style={styles.driverInfo}>
          <Text style={styles.driverName}>{item.fullName || 'Unknown Driver'}</Text>
          <Text style={styles.driverLicense}>License: {item.licenseNumber || 'N/A'}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) }]}>
          <Text style={styles.statusText}>{item.status || 'Unknown'}</Text>
        </View>
      </View>
      
      <View style={styles.driverDetails}>
        <View style={styles.detailRow}>
          <Ionicons name="call" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.phoneNumber || 'No phone'}</Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="mail" size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.email || 'No email'}</Text>
        </View>
      </View>
    </TouchableOpacity>
  );

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <Text style={styles.loadingText}>Loading drivers...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Drivers ({drivers.length})</Text>
        <TouchableOpacity style={styles.addButton}>
          <Ionicons name="add" size={24} color="#ffffff" />
        </TouchableOpacity>
      </View>

      <View style={styles.searchContainer}>
        <Ionicons name="search" size={20} color="#64748b" style={styles.searchIcon} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search drivers..."
          value={searchQuery}
          onChangeText={setSearchQuery}
        />
      </View>

      <FlatList
        data={filteredDrivers}
        renderItem={renderDriverItem}
        keyExtractor={(item) => item.id?.toString() || Math.random().toString()}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
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
  addButton: {
    backgroundColor: '#2563eb',
    borderRadius: 20,
    width: 40,
    height: 40,
    justifyContent: 'center',
    alignItems: 'center',
  },
  searchContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#ffffff',
    margin: 16,
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  searchIcon: {
    marginRight: 12,
  },
  searchInput: {
    flex: 1,
    fontSize: 16,
    color: '#1e293b',
  },
  listContainer: {
    padding: 16,
  },
  driverCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  driverHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  driverInfo: {
    flex: 1,
  },
  driverName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  driverLicense: {
    fontSize: 14,
    color: '#64748b',
  },
  statusBadge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  statusText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
    textTransform: 'capitalize',
  },
  driverDetails: {
    borderTopWidth: 1,
    borderTopColor: '#e2e8f0',
    paddingTop: 12,
  },
  detailRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  detailText: {
    fontSize: 14,
    color: '#64748b',
    marginLeft: 8,
  },
  loadingText: {
    fontSize: 16,
    color: '#64748b',
  },
});
