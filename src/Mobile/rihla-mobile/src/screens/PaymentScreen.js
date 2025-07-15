import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Alert,
  ScrollView,
  TextInput,
  ActivityIndicator,
  Modal,
  FlatList,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import apiClient from '../services/apiClient';

export default function PaymentScreen() {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState('card');
  const [paymentAmount, setPaymentAmount] = useState('');
  const [cardNumber, setCardNumber] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [cvv, setCvv] = useState('');
  const [cardholderName, setCardholderName] = useState('');
  const [processingPayment, setProcessingPayment] = useState(false);
  const [invoices, setInvoices] = useState([]);
  const [familyDiscounts, setFamilyDiscounts] = useState([]);
  const [paymentHistory, setPaymentHistory] = useState([]);
  const [selectedInvoice, setSelectedInvoice] = useState(null);

  useEffect(() => {
    fetchPaymentData();
  }, []);

  const fetchPaymentData = async () => {
    try {
      setLoading(true);
      
      const paymentsResponse = await apiClient.get('/api/payments');
      setPayments(paymentsResponse.data || []);
      
      const invoicesResponse = await apiClient.get('/api/payments/generate-invoices', {
        params: { studentId: 1 } // This would come from context
      });
      setInvoices(invoicesResponse.data || []);
      
      const discountsResponse = await apiClient.get('/api/payments/apply-family-discounts', {
        params: { familyId: 1 } // This would come from context
      });
      setFamilyDiscounts(discountsResponse.data || []);
      
      const historyResponse = await apiClient.get('/api/payments/payment-analytics', {
        params: { 
          startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          endDate: new Date().toISOString()
        }
      });
      setPaymentHistory(historyResponse.data?.transactions || []);
      
    } catch (error) {
      console.error('Error fetching payment data:', error);
      Alert.alert('Error', 'Failed to load payment data');
    } finally {
      setLoading(false);
    }
  };

  const handlePaymentMethodSelect = (method) => {
    setSelectedPaymentMethod(method);
  };

  const validateCardNumber = (number) => {
    const digits = number.replace(/\D/g, '');
    if (digits.length < 13 || digits.length > 19) return false;
    
    let sum = 0;
    let isEven = false;
    
    for (let i = digits.length - 1; i >= 0; i--) {
      let digit = parseInt(digits[i]);
      
      if (isEven) {
        digit *= 2;
        if (digit > 9) digit -= 9;
      }
      
      sum += digit;
      isEven = !isEven;
    }
    
    return sum % 10 === 0;
  };

  const validateExpiryDate = (expiry) => {
    const regex = /^(0[1-9]|1[0-2])\/([0-9]{2})$/;
    if (!regex.test(expiry)) return false;
    
    const [month, year] = expiry.split('/');
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear() % 100;
    const currentMonth = currentDate.getMonth() + 1;
    
    const expYear = parseInt(year);
    const expMonth = parseInt(month);
    
    if (expYear < currentYear || (expYear === currentYear && expMonth < currentMonth)) {
      return false;
    }
    
    return true;
  };

  const processSecurePayment = async () => {
    try {
      if (selectedPaymentMethod === 'card') {
        if (!validateCardNumber(cardNumber)) {
          Alert.alert('Invalid Card', 'Please enter a valid card number');
          return;
        }
        
        if (!validateExpiryDate(expiryDate)) {
          Alert.alert('Invalid Expiry', 'Please enter a valid expiry date (MM/YY)');
          return;
        }
        
        if (cvv.length < 3 || cvv.length > 4) {
          Alert.alert('Invalid CVV', 'Please enter a valid CVV');
          return;
        }
        
        if (!cardholderName.trim()) {
          Alert.alert('Missing Name', 'Please enter the cardholder name');
          return;
        }
      }
      
      if (!paymentAmount || parseFloat(paymentAmount) <= 0) {
        Alert.alert('Invalid Amount', 'Please enter a valid payment amount');
        return;
      }

      setProcessingPayment(true);

      const paymentData = {
        amount: parseFloat(paymentAmount),
        paymentMethod: selectedPaymentMethod,
        invoiceId: selectedInvoice?.id,
        studentId: 1, // This would come from context/props
        cardDetails: selectedPaymentMethod === 'card' ? {
          cardNumber: cardNumber.replace(/\s/g, ''),
          expiryDate,
          cvv,
          cardholderName
        } : null,
        timestamp: new Date().toISOString()
      };

      const response = await apiClient.post('/api/payments/secure-payment', paymentData);
      
      if (response.data && response.data.success) {
        Alert.alert('Payment Successful', 'Your payment has been processed successfully');
        setShowPaymentModal(false);
        resetPaymentForm();
        await fetchPaymentData();
      } else {
        Alert.alert('Payment Failed', response.data?.message || 'Payment processing failed');
      }
    } catch (error) {
      console.error('Error processing payment:', error);
      Alert.alert('Payment Error', 'Failed to process payment. Please try again.');
    } finally {
      setProcessingPayment(false);
    }
  };

  const resetPaymentForm = () => {
    setPaymentAmount('');
    setCardNumber('');
    setExpiryDate('');
    setCvv('');
    setCardholderName('');
    setSelectedPaymentMethod('card');
    setSelectedInvoice(null);
  };

  const formatCardNumber = (text) => {
    const cleaned = text.replace(/\s/g, '');
    const match = cleaned.match(/\d{4}/g);
    return match ? match.join(' ') : cleaned;
  };

  const formatExpiryDate = (text) => {
    const cleaned = text.replace(/\D/g, '');
    if (cleaned.length >= 2) {
      return cleaned.substring(0, 2) + '/' + cleaned.substring(2, 4);
    }
    return cleaned;
  };

  const handleRefund = async (paymentId) => {
    try {
      Alert.alert(
        'Confirm Refund',
        'Are you sure you want to request a refund for this payment?',
        [
          { text: 'Cancel', style: 'cancel' },
          {
            text: 'Request Refund',
            onPress: async () => {
              const refundData = {
                reason: 'Customer request',
                amount: null, // Full refund
                requestedBy: 1 // This would come from context
              };

              const response = await apiClient.post(`/api/payments/${paymentId}/refund`, refundData);
              
              if (response.data && response.data.success) {
                Alert.alert('Refund Requested', 'Your refund request has been submitted');
                await fetchPaymentData();
              } else {
                Alert.alert('Refund Failed', 'Failed to process refund request');
              }
            }
          }
        ]
      );
    } catch (error) {
      console.error('Error requesting refund:', error);
      Alert.alert('Error', 'Failed to request refund');
    }
  };

  const renderPaymentItem = ({ item }) => (
    <TouchableOpacity style={styles.paymentCard}>
      <View style={styles.paymentHeader}>
        <View style={styles.paymentInfo}>
          <Text style={styles.paymentAmount}>${item.amount?.toFixed(2) || '0.00'}</Text>
          <Text style={styles.paymentDate}>
            {item.createdAt ? new Date(item.createdAt).toLocaleDateString() : 'N/A'}
          </Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getPaymentStatusColor(item.status) }]}>
          <Text style={styles.statusText}>{item.status || 'Unknown'}</Text>
        </View>
      </View>
      
      <View style={styles.paymentDetails}>
        <Text style={styles.paymentMethod}>Method: {item.paymentMethod || 'N/A'}</Text>
        <Text style={styles.transactionId}>ID: {item.transactionId || 'N/A'}</Text>
        {item.status === 'completed' && (
          <TouchableOpacity
            style={styles.refundButton}
            onPress={() => handleRefund(item.id)}
          >
            <Text style={styles.refundButtonText}>Request Refund</Text>
          </TouchableOpacity>
        )}
      </View>
    </TouchableOpacity>
  );

  const renderInvoiceItem = ({ item }) => (
    <TouchableOpacity
      style={[styles.invoiceCard, selectedInvoice?.id === item.id && styles.selectedInvoice]}
      onPress={() => setSelectedInvoice(item)}
    >
      <View style={styles.invoiceHeader}>
        <Text style={styles.invoiceNumber}>Invoice #{item.invoiceNumber || 'N/A'}</Text>
        <Text style={styles.invoiceAmount}>${item.amount?.toFixed(2) || '0.00'}</Text>
      </View>
      <Text style={styles.invoiceDueDate}>
        Due: {item.dueDate ? new Date(item.dueDate).toLocaleDateString() : 'N/A'}
      </Text>
      <Text style={styles.invoiceDescription}>{item.description || 'Transportation fee'}</Text>
    </TouchableOpacity>
  );

  const getPaymentStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'completed': return '#10b981';
      case 'pending': return '#f59e0b';
      case 'failed': return '#ef4444';
      case 'refunded': return '#64748b';
      default: return '#64748b';
    }
  };

  if (loading) {
    return (
      <View style={styles.centerContainer}>
        <ActivityIndicator size="large" color="#3b82f6" />
        <Text style={styles.loadingText}>Loading payment data...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Payments</Text>
        <TouchableOpacity
          style={styles.payButton}
          onPress={() => setShowPaymentModal(true)}
        >
          <Ionicons name="card" size={20} color="#ffffff" />
          <Text style={styles.payButtonText}>Pay Now</Text>
        </TouchableOpacity>
      </View>

      <ScrollView style={styles.content}>
        {/* Payment Summary */}
        <View style={styles.summaryContainer}>
          <View style={styles.summaryCard}>
            <Text style={styles.summaryNumber}>${payments.reduce((sum, p) => sum + (p.amount || 0), 0).toFixed(2)}</Text>
            <Text style={styles.summaryLabel}>Total Paid</Text>
          </View>
          <View style={styles.summaryCard}>
            <Text style={styles.summaryNumber}>{invoices.length}</Text>
            <Text style={styles.summaryLabel}>Pending</Text>
          </View>
          <View style={styles.summaryCard}>
            <Text style={styles.summaryNumber}>{familyDiscounts.length}</Text>
            <Text style={styles.summaryLabel}>Discounts</Text>
          </View>
        </View>

        {/* Pending Invoices */}
        {invoices.length > 0 && (
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Pending Invoices</Text>
            <FlatList
              data={invoices}
              renderItem={renderInvoiceItem}
              keyExtractor={(item) => item.id?.toString() || Math.random().toString()}
              horizontal
              showsHorizontalScrollIndicator={false}
              contentContainerStyle={styles.invoicesList}
            />
          </View>
        )}

        {/* Payment History */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Payment History</Text>
          <FlatList
            data={payments}
            renderItem={renderPaymentItem}
            keyExtractor={(item) => item.id?.toString() || Math.random().toString()}
            scrollEnabled={false}
            contentContainerStyle={styles.paymentsList}
          />
        </View>
      </ScrollView>

      {/* Payment Modal */}
      <Modal
        visible={showPaymentModal}
        animationType="slide"
        transparent={true}
        onRequestClose={() => setShowPaymentModal(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Make Payment</Text>
            
            {selectedInvoice && (
              <View style={styles.selectedInvoiceInfo}>
                <Text style={styles.selectedInvoiceText}>
                  Invoice #{selectedInvoice.invoiceNumber} - ${selectedInvoice.amount?.toFixed(2)}
                </Text>
              </View>
            )}

            {/* Payment Method Selection */}
            <View style={styles.paymentMethodContainer}>
              <Text style={styles.inputLabel}>Payment Method</Text>
              <View style={styles.methodButtons}>
                <TouchableOpacity
                  style={[styles.methodButton, selectedPaymentMethod === 'card' && styles.selectedMethod]}
                  onPress={() => handlePaymentMethodSelect('card')}
                >
                  <Ionicons name="card" size={20} color={selectedPaymentMethod === 'card' ? '#ffffff' : '#64748b'} />
                  <Text style={[styles.methodText, selectedPaymentMethod === 'card' && styles.selectedMethodText]}>
                    Card
                  </Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.methodButton, selectedPaymentMethod === 'bank' && styles.selectedMethod]}
                  onPress={() => handlePaymentMethodSelect('bank')}
                >
                  <Ionicons name="business" size={20} color={selectedPaymentMethod === 'bank' ? '#ffffff' : '#64748b'} />
                  <Text style={[styles.methodText, selectedPaymentMethod === 'bank' && styles.selectedMethodText]}>
                    Bank
                  </Text>
                </TouchableOpacity>
              </View>
            </View>

            {/* Amount Input */}
            <View style={styles.inputContainer}>
              <Text style={styles.inputLabel}>Amount</Text>
              <TextInput
                style={styles.input}
                value={paymentAmount}
                onChangeText={setPaymentAmount}
                placeholder="Enter amount"
                keyboardType="numeric"
                editable={!selectedInvoice}
              />
            </View>

            {/* Card Details (if card payment selected) */}
            {selectedPaymentMethod === 'card' && (
              <>
                <View style={styles.inputContainer}>
                  <Text style={styles.inputLabel}>Card Number</Text>
                  <TextInput
                    style={styles.input}
                    value={cardNumber}
                    onChangeText={(text) => setCardNumber(formatCardNumber(text))}
                    placeholder="1234 5678 9012 3456"
                    keyboardType="numeric"
                    maxLength={19}
                  />
                </View>

                <View style={styles.rowContainer}>
                  <View style={[styles.inputContainer, styles.halfWidth]}>
                    <Text style={styles.inputLabel}>Expiry Date</Text>
                    <TextInput
                      style={styles.input}
                      value={expiryDate}
                      onChangeText={(text) => setExpiryDate(formatExpiryDate(text))}
                      placeholder="MM/YY"
                      keyboardType="numeric"
                      maxLength={5}
                    />
                  </View>

                  <View style={[styles.inputContainer, styles.halfWidth]}>
                    <Text style={styles.inputLabel}>CVV</Text>
                    <TextInput
                      style={styles.input}
                      value={cvv}
                      onChangeText={setCvv}
                      placeholder="123"
                      keyboardType="numeric"
                      maxLength={4}
                      secureTextEntry
                    />
                  </View>
                </View>

                <View style={styles.inputContainer}>
                  <Text style={styles.inputLabel}>Cardholder Name</Text>
                  <TextInput
                    style={styles.input}
                    value={cardholderName}
                    onChangeText={setCardholderName}
                    placeholder="John Doe"
                    autoCapitalize="words"
                  />
                </View>
              </>
            )}

            {processingPayment && (
              <View style={styles.processingContainer}>
                <ActivityIndicator size="large" color="#3b82f6" />
                <Text style={styles.processingText}>Processing payment...</Text>
              </View>
            )}

            <View style={styles.modalButtons}>
              <TouchableOpacity
                style={styles.cancelButton}
                onPress={() => setShowPaymentModal(false)}
              >
                <Text style={styles.cancelButtonText}>Cancel</Text>
              </TouchableOpacity>
              
              <TouchableOpacity
                style={[styles.payNowButton, processingPayment && styles.disabledButton]}
                onPress={processSecurePayment}
                disabled={processingPayment}
              >
                <Text style={styles.payNowButtonText}>
                  {processingPayment ? 'Processing...' : 'Pay Now'}
                </Text>
              </TouchableOpacity>
            </View>
          </View>
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
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  payButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#3b82f6',
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 8,
  },
  payButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '600',
    marginLeft: 8,
  },
  content: {
    flex: 1,
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
    fontSize: 12,
    color: '#64748b',
    marginTop: 4,
  },
  section: {
    marginBottom: 24,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 12,
    paddingHorizontal: 16,
  },
  invoicesList: {
    paddingHorizontal: 16,
  },
  invoiceCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    marginRight: 12,
    width: 200,
    borderWidth: 2,
    borderColor: 'transparent',
  },
  selectedInvoice: {
    borderColor: '#3b82f6',
  },
  invoiceHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  invoiceNumber: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1e293b',
  },
  invoiceAmount: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#3b82f6',
  },
  invoiceDueDate: {
    fontSize: 12,
    color: '#ef4444',
    marginBottom: 4,
  },
  invoiceDescription: {
    fontSize: 12,
    color: '#64748b',
  },
  paymentsList: {
    paddingHorizontal: 16,
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
    alignItems: 'center',
    marginBottom: 12,
  },
  paymentInfo: {
    flex: 1,
  },
  paymentAmount: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  paymentDate: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 2,
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
  paymentDetails: {
    borderTopWidth: 1,
    borderTopColor: '#e2e8f0',
    paddingTop: 12,
  },
  paymentMethod: {
    fontSize: 14,
    color: '#64748b',
    marginBottom: 4,
  },
  transactionId: {
    fontSize: 12,
    color: '#64748b',
    marginBottom: 8,
  },
  refundButton: {
    alignSelf: 'flex-start',
    backgroundColor: '#ef4444',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
  },
  refundButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
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
    maxHeight: '80%',
  },
  modalTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#1e293b',
    textAlign: 'center',
    marginBottom: 20,
  },
  selectedInvoiceInfo: {
    backgroundColor: '#f1f5f9',
    padding: 12,
    borderRadius: 8,
    marginBottom: 16,
  },
  selectedInvoiceText: {
    fontSize: 14,
    color: '#1e293b',
    textAlign: 'center',
  },
  paymentMethodContainer: {
    marginBottom: 16,
  },
  methodButtons: {
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  methodButton: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 12,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#e2e8f0',
    backgroundColor: '#ffffff',
    minWidth: 100,
    justifyContent: 'center',
  },
  selectedMethod: {
    backgroundColor: '#3b82f6',
    borderColor: '#3b82f6',
  },
  methodText: {
    fontSize: 14,
    color: '#64748b',
    marginLeft: 8,
  },
  selectedMethodText: {
    color: '#ffffff',
  },
  inputContainer: {
    marginBottom: 16,
  },
  inputLabel: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1e293b',
    marginBottom: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: '#e2e8f0',
    borderRadius: 8,
    padding: 12,
    fontSize: 16,
    backgroundColor: '#ffffff',
  },
  rowContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  halfWidth: {
    width: '48%',
  },
  processingContainer: {
    alignItems: 'center',
    marginVertical: 16,
  },
  processingText: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 8,
  },
  modalButtons: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 20,
  },
  cancelButton: {
    backgroundColor: '#e2e8f0',
    padding: 12,
    borderRadius: 8,
    flex: 1,
    marginRight: 8,
    alignItems: 'center',
  },
  cancelButtonText: {
    color: '#64748b',
    fontSize: 16,
    fontWeight: '600',
  },
  payNowButton: {
    backgroundColor: '#3b82f6',
    padding: 12,
    borderRadius: 8,
    flex: 1,
    marginLeft: 8,
    alignItems: 'center',
  },
  disabledButton: {
    backgroundColor: '#94a3b8',
  },
  payNowButtonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '600',
  },
  loadingText: {
    fontSize: 16,
    color: '#64748b',
    marginTop: 8,
  },
});
