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

export default function PaymentsScreen() {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchPayments = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/payments');
      setPayments(response.data || []);
    } catch (error) {
      console.error('Error fetching payments:', error);
      Alert.alert('Error', 'Failed to load payments');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = React.useCallback(async () => {
    setRefreshing(true);
    await fetchPayments();
    setRefreshing(false);
  }, []);

  useEffect(() => {
    fetchPayments();
  }, []);

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'paid': return '#10b981';
      case 'pending': return '#f59e0b';
      case 'overdue': return '#ef4444';
      case 'cancelled': return '#64748b';
      default: return '#64748b';
    }
  };

  const getStatusIcon = (status) => {
    switch (status?.toLowerCase()) {
      case 'paid': return 'checkmark-circle';
      case 'pending': return 'time';
      case 'overdue': return 'warning';
      case 'cancelled': return 'close-circle';
      default: return 'help-circle';
    }
  };

  const handlePaymentPress = (payment) => {
    Alert.alert(
      'Payment Details',
      `Student: ${payment.studentName}\nAmount: $${payment.amount}\nStatus: ${payment.status}\nDue Date: ${payment.dueDate}`,
      [
        { text: 'Mark as Paid', onPress: () => updatePaymentStatus(payment.id, 'paid') },
        { text: 'View Invoice', onPress: () => console.log('View invoice:', payment.id) },
        { text: 'Cancel', style: 'cancel' }
      ]
    );
  };

  const updatePaymentStatus = async (paymentId, status) => {
    try {
      await apiClient.put(`/api/payments/${paymentId}`, { status });
      await fetchPayments();
      Alert.alert('Success', 'Payment status updated successfully');
    } catch (error) {
      console.error('Error updating payment:', error);
      Alert.alert('Error', 'Failed to update payment status');
    }
  };

  const renderPaymentItem = ({ item }) => (
    <TouchableOpacity style={styles.paymentCard} onPress={() => handlePaymentPress(item)}>
      <View style={styles.paymentHeader}>
        <View style={styles.paymentInfo}>
          <Text style={styles.studentName}>{item.studentName || 'Unknown Student'}</Text>
          <Text style={styles.paymentAmount}>${item.amount || 0}</Text>
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
      
      <View style={styles.paymentDetails}>
        <View style={styles.detailRow}>
          <Ionicons name="calendar" size={16} color="#64748b" />
          <Text style={styles.detailText}>
            Due: {item.dueDate ? new Date(item.dueDate).toLocaleDateString() : 'N/A'}
          </Text>
        </View>
        <View style={styles.detailRow}>
          <Ionicons name="document-text" size={16} color="#64748b" />
          <Text style={styles.detailText}>Invoice: {item.invoiceNumber || 'N/A'}</Text>
        </View>
        {item.paidDate && (
          <View style={styles.detailRow}>
            <Ionicons name="checkmark" size={16} color="#64748b" />
            <Text style={styles.detailText}>
              Paid: {new Date(item.paidDate).toLocaleDateString()}
            </Text>
          </View>
        )}
      </View>
    </TouchableOpacity>
  );

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <Text style={styles.loadingText}>Loading payments...</Text>
      </View>
    );
  }

  const totalAmount = payments.reduce((sum, payment) => sum + (payment.amount || 0), 0);
  const paidAmount = payments.filter(p => p.status === 'paid').reduce((sum, payment) => sum + (payment.amount || 0), 0);
  const pendingAmount = totalAmount - paidAmount;

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Payments</Text>
        <TouchableOpacity style={styles.addButton}>
          <Ionicons name="add" size={24} color="#ffffff" />
        </TouchableOpacity>
      </View>

      <View style={styles.summaryContainer}>
        <View style={styles.summaryCard}>
          <Text style={styles.summaryNumber}>${totalAmount.toFixed(2)}</Text>
          <Text style={styles.summaryLabel}>Total</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={[styles.summaryNumber, { color: '#10b981' }]}>${paidAmount.toFixed(2)}</Text>
          <Text style={styles.summaryLabel}>Paid</Text>
        </View>
        <View style={styles.summaryCard}>
          <Text style={[styles.summaryNumber, { color: '#ef4444' }]}>${pendingAmount.toFixed(2)}</Text>
          <Text style={styles.summaryLabel}>Pending</Text>
        </View>
      </View>

      <FlatList
        data={payments}
        renderItem={renderPaymentItem}
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
    fontSize: 20,
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
  paymentCard: {
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
  paymentHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  paymentInfo: {
    flex: 1,
  },
  studentName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  paymentAmount: {
    fontSize: 16,
    fontWeight: '600',
    color: '#2563eb',
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
  paymentDetails: {
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
