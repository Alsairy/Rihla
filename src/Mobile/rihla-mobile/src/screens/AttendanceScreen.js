import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  RefreshControl,
  TouchableOpacity,
  Alert,
  Modal,
  Image,
  ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Camera } from 'expo-camera';
import * as ImagePicker from 'expo-image-picker';
import apiClient from '../services/apiClient';
import biometricService from '../services/biometricService';

export default function AttendanceScreen() {
  const [attendance, setAttendance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [attendanceMethods, setAttendanceMethods] = useState([]);
  const [showMethodModal, setShowMethodModal] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [cameraVisible, setCameraVisible] = useState(false);
  const [photoUri, setPhotoUri] = useState(null);
  const [processingAttendance, setProcessingAttendance] = useState(false);
  const [offlineQueue, setOfflineQueue] = useState([]);

  const fetchAttendance = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get(`/api/attendance?date=${selectedDate}`);
      setAttendance(response.data || []);
      
      const methodsResponse = await apiClient.get('/api/attendance/methods');
      setAttendanceMethods(methodsResponse.data || []);
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
    setSelectedStudent(record);
    setShowMethodModal(true);
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

  const handleRFIDAttendance = async (studentId) => {
    try {
      setProcessingAttendance(true);
      const rfidData = {
        studentId,
        rfidTag: `RFID_${studentId}_${Date.now()}`,
        timestamp: new Date().toISOString(),
        location: 'Bus Stop',
        tripId: selectedStudent?.tripId
      };

      const response = await apiClient.post('/api/attendance/rfid', rfidData);
      
      if (response.data) {
        Alert.alert('Success', 'RFID attendance recorded successfully');
        await fetchAttendance();
      }
    } catch (error) {
      console.error('Error recording RFID attendance:', error);
      if (!navigator.onLine) {
        const offlineRecord = {
          type: 'rfid',
          studentId,
          timestamp: new Date().toISOString(),
          data: rfidData
        };
        setOfflineQueue(prev => [...prev, offlineRecord]);
        Alert.alert('Offline', 'Attendance saved offline. Will sync when online.');
      } else {
        Alert.alert('Error', 'Failed to record RFID attendance');
      }
    } finally {
      setProcessingAttendance(false);
      setShowMethodModal(false);
    }
  };

  const handlePhotoAttendance = async () => {
    try {
      const { status } = await Camera.requestCameraPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission needed', 'Camera permission is required for photo attendance');
        return;
      }

      setCameraVisible(true);
    } catch (error) {
      console.error('Error requesting camera permission:', error);
      Alert.alert('Error', 'Failed to access camera');
    }
  };

  const takePicture = async (camera) => {
    try {
      const photo = await camera.takePictureAsync({
        quality: 0.8,
        base64: true,
        skipProcessing: false
      });
      
      setPhotoUri(photo.uri);
      setCameraVisible(false);
      await processPhotoAttendance(photo);
    } catch (error) {
      console.error('Error taking picture:', error);
      Alert.alert('Error', 'Failed to take picture');
    }
  };

  const processPhotoAttendance = async (photo) => {
    try {
      setProcessingAttendance(true);
      
      const photoData = {
        studentId: selectedStudent.studentId,
        photoBase64: photo.base64,
        timestamp: new Date().toISOString(),
        location: 'Bus Stop',
        tripId: selectedStudent?.tripId,
        confidence: 0.95
      };

      const response = await apiClient.post('/api/attendance/photo', photoData);
      
      if (response.data) {
        Alert.alert('Success', 'Photo attendance recorded successfully');
        await fetchAttendance();
      }
    } catch (error) {
      console.error('Error processing photo attendance:', error);
      if (!navigator.onLine) {
        const offlineRecord = {
          type: 'photo',
          studentId: selectedStudent.studentId,
          timestamp: new Date().toISOString(),
          data: photoData
        };
        setOfflineQueue(prev => [...prev, offlineRecord]);
        Alert.alert('Offline', 'Photo attendance saved offline. Will sync when online.');
      } else {
        Alert.alert('Error', 'Failed to process photo attendance');
      }
    } finally {
      setProcessingAttendance(false);
      setShowMethodModal(false);
    }
  };

  const handleBiometricAttendance = async (studentId) => {
    try {
      setProcessingAttendance(true);
      
      const biometricResult = await biometricService.authenticate();
      
      if (biometricResult.success) {
        const biometricData = {
          studentId,
          biometricHash: biometricResult.hash,
          biometricType: biometricResult.type,
          timestamp: new Date().toISOString(),
          location: 'Bus Stop',
          tripId: selectedStudent?.tripId,
          confidence: biometricResult.confidence
        };

        const response = await apiClient.post('/api/attendance/biometric', biometricData);
        
        if (response.data) {
          Alert.alert('Success', 'Biometric attendance recorded successfully');
          await fetchAttendance();
        }
      } else {
        Alert.alert('Authentication Failed', 'Biometric authentication was not successful');
      }
    } catch (error) {
      console.error('Error recording biometric attendance:', error);
      Alert.alert('Error', 'Failed to record biometric attendance');
    } finally {
      setProcessingAttendance(false);
      setShowMethodModal(false);
    }
  };

  const syncOfflineAttendance = async () => {
    if (offlineQueue.length === 0) return;

    try {
      const syncData = {
        offlineRecords: offlineQueue,
        deviceId: 'mobile_device_001',
        syncTimestamp: new Date().toISOString()
      };

      const response = await apiClient.post('/api/attendance/sync-offline', syncData);
      
      if (response.data && response.data.success) {
        setOfflineQueue([]);
        Alert.alert('Sync Complete', `${offlineQueue.length} offline records synced successfully`);
        await fetchAttendance();
      }
    } catch (error) {
      console.error('Error syncing offline attendance:', error);
      Alert.alert('Sync Error', 'Failed to sync offline attendance records');
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
        <View style={styles.headerActions}>
          <Text style={styles.dateText}>{selectedDate}</Text>
          {offlineQueue.length > 0 && (
            <TouchableOpacity style={styles.syncButton} onPress={syncOfflineAttendance}>
              <Ionicons name="sync" size={16} color="#ffffff" />
              <Text style={styles.syncButtonText}>{offlineQueue.length}</Text>
            </TouchableOpacity>
          )}
        </View>
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
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>{attendanceMethods.length}</Text>
          <Text style={styles.summaryLabel}>Methods</Text>
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

      {/* Attendance Method Selection Modal */}
      <Modal
        visible={showMethodModal}
        animationType="slide"
        transparent={true}
        onRequestClose={() => setShowMethodModal(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Select Attendance Method</Text>
            <Text style={styles.modalSubtitle}>
              Student: {selectedStudent?.studentName}
            </Text>

            <View style={styles.methodButtons}>
              <TouchableOpacity
                style={[styles.methodButton, styles.rfidButton]}
                onPress={() => handleRFIDAttendance(selectedStudent?.studentId)}
                disabled={processingAttendance}
              >
                <Ionicons name="card" size={24} color="#ffffff" />
                <Text style={styles.methodButtonText}>RFID Scan</Text>
              </TouchableOpacity>

              <TouchableOpacity
                style={[styles.methodButton, styles.photoButton]}
                onPress={handlePhotoAttendance}
                disabled={processingAttendance}
              >
                <Ionicons name="camera" size={24} color="#ffffff" />
                <Text style={styles.methodButtonText}>Photo</Text>
              </TouchableOpacity>

              <TouchableOpacity
                style={[styles.methodButton, styles.biometricButton]}
                onPress={() => handleBiometricAttendance(selectedStudent?.studentId)}
                disabled={processingAttendance}
              >
                <Ionicons name="finger-print" size={24} color="#ffffff" />
                <Text style={styles.methodButtonText}>Biometric</Text>
              </TouchableOpacity>
            </View>

            {processingAttendance && (
              <View style={styles.processingContainer}>
                <ActivityIndicator size="large" color="#3b82f6" />
                <Text style={styles.processingText}>Processing attendance...</Text>
              </View>
            )}

            <TouchableOpacity
              style={styles.cancelButton}
              onPress={() => setShowMethodModal(false)}
            >
              <Text style={styles.cancelButtonText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      {/* Camera Modal */}
      <Modal
        visible={cameraVisible}
        animationType="slide"
        onRequestClose={() => setCameraVisible(false)}
      >
        <View style={styles.cameraContainer}>
          <Camera
            style={styles.camera}
            type={Camera.Constants.Type.back}
            ref={(ref) => {
              if (ref) {
                setTimeout(() => takePicture(ref), 1000);
              }
            }}
          />
          <TouchableOpacity
            style={styles.closeCameraButton}
            onPress={() => setCameraVisible(false)}
          >
            <Ionicons name="close" size={30} color="#ffffff" />
          </TouchableOpacity>
        </View>
      </Modal>
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
  headerActions: {
    flexDirection: 'row',
    alignItems: 'center',
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
  syncButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f59e0b',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
    marginLeft: 12,
  },
  syncButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
    marginLeft: 4,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalContent: {
    backgroundColor: '#ffffff',
    borderRadius: 16,
    padding: 24,
    width: '90%',
    maxWidth: 400,
  },
  modalTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#1e293b',
    textAlign: 'center',
    marginBottom: 8,
  },
  modalSubtitle: {
    fontSize: 16,
    color: '#64748b',
    textAlign: 'center',
    marginBottom: 24,
  },
  methodButtons: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginBottom: 24,
  },
  methodButton: {
    alignItems: 'center',
    padding: 16,
    borderRadius: 12,
    minWidth: 80,
  },
  rfidButton: {
    backgroundColor: '#3b82f6',
  },
  photoButton: {
    backgroundColor: '#10b981',
  },
  biometricButton: {
    backgroundColor: '#8b5cf6',
  },
  methodButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
    marginTop: 8,
  },
  processingContainer: {
    alignItems: 'center',
    marginBottom: 16,
  },
  processingText: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 8,
  },
  cancelButton: {
    backgroundColor: '#e2e8f0',
    padding: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  cancelButtonText: {
    color: '#64748b',
    fontSize: 16,
    fontWeight: '600',
  },
  cameraContainer: {
    flex: 1,
    backgroundColor: '#000000',
  },
  camera: {
    flex: 1,
  },
  closeCameraButton: {
    position: 'absolute',
    top: 50,
    right: 20,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    borderRadius: 20,
    padding: 8,
  },
});
