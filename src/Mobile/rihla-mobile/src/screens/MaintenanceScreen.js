import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  RefreshControl,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import apiClient from '../services/apiClient';

export default function MaintenanceScreen() {
  const [maintenance, setMaintenance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchMaintenance = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/maintenance');
      setMaintenance(response.data || []);
    } catch (error) {
      console.error('Error fetching maintenance:', error);
      Alert.alert('Error', 'Failed to load maintenance records');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = React.useCallback(async () => {
    setRefreshing(true);
    await fetchMaintenance();
    setRefreshing(false);
  }, []);

  useEffect(() => {
    fetchMaintenance();
  }, []);

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'completed': return '#10b981';
      case 'in_progress': return '#f59e0b';
      case 'scheduled': return '#2563eb';
      case 'overdue': return '#ef4444';
      default: return '#64748b';
    }
  };

  const getTypeIcon = (type) => {
    switch (type?.toLowerCase()) {
      case 'routine': return 'calendar';
      case 'repair': return 'construct';
      case 'inspection': return 'search';
      case 'emergency': return 'warning';
      default: return 'build';
    }
  };

  const handleMaintenancePress = (record) => {
    Alert.alert(
      'Maintenance Details',
      `Vehicle: ${record.vehiclePlateNumber}\nType: ${record.type}\nStatus: ${record.status}\nCost: $${record.cost || 0}`,
      [
        { text: 'Mark Complete', onPress: () => updateMaintenanceStatus(record.id, 'completed') },
        { text: 'View Details', onPress: () => console.log('View maintenance:', record.id) },
        { text: 'Cancel', style: 'cancel' }
      ]
    );
  };

  const updateMaintenanceStatus = async (maintenanceId, status) => {
    try {
      await apiClient.put(`/api/maintenance/${maintenanceId}`, { status });
      await fetchMaintenance();
      Alert.alert('Success', 'Maintenance status updated successfully');
    } catch (error) {
      console.error('Error updating maintenance:', error);
      Alert.alert('Error', 'Failed to update maintenance status');
    }
  };

  const renderMaintenanceItem = ({ item }) => (
    <TouchableOpacity style={styles.maintenanceCard} onPress={() => handleMaintenancePress(item)}>
      <View style={styles.maintenanceHeader}>
        <View style={styles.maintenanceInfo}>
          <Text style={styles.vehiclePlate}>{item.vehiclePlateNumber || 'Unknown Vehicle'}</Text>
          <Text style={styles.maintenanceType}>{item.type || 'Unknown Type'}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) }]}>
          <Text style={styles.statusText}>{item.status || 'Unknown'}</Text>
        </View>
      </View>
      
      <View style={styles.maintenanceDetails}>
        <View style={styles.detailRow}>
          <Ionicons name={getTypeIcon(item.type)} size={16} color="#64748b" />
          <Text style={styles.detailText}>{item.description || 'No description'}</Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="calendar" size={16} color="#64748b" />
          <Text style={styles.detailText}>
            Scheduled: {item.scheduledDate ? new Date(item.scheduledDate).toLocaleDateString() : 'N/A'}
          </Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="cash" size={16} color="#64748b" />
          <Text style={styles.detailText}>Cost: ${item.cost || 0}</Text>
        </View>
        {item.completedDate && (
          <View style={styles.detailRow}>
            <Ionicons name="checkmark" size={16} color="#64748b" />
            <Text style={styles.detailText}>
              Completed: {new Date(item.completedDate).toLocaleDateString()}
            </Text>
          </View>
        )}
      </View>
    </TouchableOpacity>
  );

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <Text style={styles.loadingText}>Loading maintenance records...</Text>
      </View>
    );
  }

  const totalCost = maintenance.reduce((sum, record) => sum + (record.cost || 0), 0);
  const completedCount = maintenance.filter(m => m.status === 'completed').length;
  const pendingCount = maintenance.filter(m => m.status !== 'completed').length;

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Maintenance</Text>
        <TouchableOpacity style={styles.addButton}>
          <Ionicons name="add" size={24} color="#ffffff" />
        </TouchableOpacity>
      </View>

      <View style={styles.summaryContainer}>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>{maintenance.length}</Text>
          <Text style={styles.summaryLabel}>Total Records</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={[styles.summaryNumber, { color: '#10b981' }]}>{completedCount}</Text>
          <Text style={styles.summaryLabel}>Completed</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={[styles.summaryNumber, { color: '#f59e0b' }]}>{pendingCount}</Text>
          <Text style={styles.summaryLabel}>Pending</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>${totalCost.toFixed(0)}</Text>
          <Text style={styles.summaryLabel}>Total Cost</Text>
        </View>
      </View>

      <FlatList
        data={maintenance}
        renderItem={renderMaintenanceItem}
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
  summaryContainer: {
    flexDirection: 'row',
    padding: 16,
    justifyContent: 'space-around',
  },
  summaryCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 12,
    alignItems: 'center',
    flex: 1,
    marginHorizontal: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  summaryNumber: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  summaryLabel: {
    fontSize: 12,
    color: '#64748b',
    marginTop: 4,
    textAlign: 'center',
  },
  listContainer: {
    padding: 16,
  },
  maintenanceCard: {
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
  maintenanceHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  maintenanceInfo: {
    flex: 1,
  },
  vehiclePlate: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  maintenanceType: {
    fontSize: 14,
    color: '#64748b',
    textTransform: 'capitalize',
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
  maintenanceDetails: {
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
    flex: 1,
  },
  loadingText: {
    fontSize: 16,
    color: '#64748b',
  },
});
