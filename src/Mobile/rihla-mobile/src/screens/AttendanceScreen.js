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

export default function AttendanceScreen() {
  const [attendance, setAttendance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);

  const fetchAttendance = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get(`/api/attendance?date=${selectedDate}`);
      setAttendance(response.data || []);
    } catch (error) {
      console.error('Error fetching attendance:', error);
      Alert.alert('Error', 'Failed to load attendance data');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = React.useCallback(async () => {
    setRefreshing(true);
    await fetchAttendance();
    setRefreshing(false);
  }, [selectedDate]);

  useEffect(() => {
    fetchAttendance();
  }, [selectedDate]);

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'present': return '#10b981';
      case 'absent': return '#ef4444';
      case 'late': return '#f59e0b';
      default: return '#64748b';
    }
  };

  const getStatusIcon = (status) => {
    switch (status?.toLowerCase()) {
      case 'present': return 'checkmark-circle';
      case 'absent': return 'close-circle';
      case 'late': return 'time';
      default: return 'help-circle';
    }
  };

  const handleAttendancePress = (record) => {
    Alert.alert(
      'Attendance Details',
      `Student: ${record.studentName}\nStatus: ${record.status}\nTime: ${record.checkInTime || 'N/A'}\nTrip: ${record.tripName || 'N/A'}`,
      [
        { text: 'Mark Present', onPress: () => updateAttendance(record.id, 'present') },
        { text: 'Mark Absent', onPress: () => updateAttendance(record.id, 'absent') },
        { text: 'Cancel', style: 'cancel' }
      ]
    );
  };

  const updateAttendance = async (attendanceId, status) => {
    try {
      await apiClient.put(`/api/attendance/${attendanceId}`, { status });
      await fetchAttendance();
      Alert.alert('Success', 'Attendance updated successfully');
    } catch (error) {
      console.error('Error updating attendance:', error);
      Alert.alert('Error', 'Failed to update attendance');
    }
  };

  const renderAttendanceItem = ({ item }) => (
    <TouchableOpacity style={styles.attendanceCard} onPress={() => handleAttendancePress(item)}>
      <View style={styles.attendanceHeader}>
        <View style={styles.studentInfo}>
          <Text style={styles.studentName}>{item.studentName || 'Unknown Student'}</Text>
          <Text style={styles.tripInfo}>Trip: {item.tripName || 'N/A'}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) }]}>
          <Ionicons 
            name={getStatusIcon(item.status)} 
            size={16} 
            color="#ffffff" 
          />
          <Text style={styles.statusText}>{item.status || 'Unknown'}</Text>
        </View>
      </View>
      
      <View style={styles.attendanceDetails}>
        <View style={styles.detailRow}>
          <Ionicons name="time" size={16} color="#64748b" />
          <Text style={styles.detailText}>
            Check-in: {item.checkInTime ? new Date(item.checkInTime).toLocaleTimeString() : 'N/A'}
          </Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="location" size={16} color="#64748b" />
          <Text style={styles.detailText}>Location: {item.location || 'N/A'}</Text>
        </View>
        {item.notes && (
          <View style={styles.detailRow}>
            <Ionicons name="document-text" size={16} color="#64748b" />
            <Text style={styles.detailText}>Notes: {item.notes}</Text>
          </View>
        )}
      </View>
    </TouchableOpacity>
  );

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <Text style={styles.loadingText}>Loading attendance...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Attendance</Text>
        <Text style={styles.dateText}>{selectedDate}</Text>
      </View>

      <View style={styles.summaryContainer}>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>{attendance.filter(a => a.status === 'present').length}</Text>
          <Text style={styles.summaryLabel}>Present</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>{attendance.filter(a => a.status === 'absent').length}</Text>
          <Text style={styles.summaryLabel}>Absent</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>{attendance.filter(a => a.status === 'late').length}</Text>
          <Text style={styles.summaryLabel}>Late</Text>
        </View>
      </View>

      <FlatList
        data={attendance}
        renderItem={renderAttendanceItem}
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
  dateText: {
    fontSize: 16,
    color: '#64748b',
  },
  summaryContainer: {
    flexDirection: 'row',
    padding: 16,
    justifyContent: 'space-around',
  },
  summaryCard: {
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
  summaryNumber: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  summaryLabel: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 4,
  },
  listContainer: {
    padding: 16,
  },
  attendanceCard: {
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
  attendanceHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  studentInfo: {
    flex: 1,
  },
  studentName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  tripInfo: {
    fontSize: 14,
    color: '#64748b',
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
    textTransform: 'capitalize',
  },
  attendanceDetails: {
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
