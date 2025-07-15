import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Divider,
  Chip,
  InputAdornment,
  IconButton
} from '@mui/material';
import {
  CreditCard as CreditCardIcon,
  Security as SecurityIcon,
  Lock as LockIcon,
  Visibility,
  VisibilityOff,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface PaymentFormData {
  amount: number;
  currency: string;
  paymentMethod: string;
  cardNumber: string;
  expiryMonth: string;
  expiryYear: string;
  cvv: string;
  cardholderName: string;
  billingAddress: string;
  city: string;
  zipCode: string;
  country: string;
}

interface SecurityValidation {
  isValid: boolean;
  riskLevel: string;
  riskScore: number;
  securityChecks: Array<{
    checkType: string;
    passed: boolean;
    message: string;
  }>;
}

interface PaymentResult {
  transactionId: string;
  status: string;
  receiptNumber?: string;
  gatewayResponse: string;
  processedAt: string;
}

const SecurePaymentForm: React.FC = () => {
  const [formData, setFormData] = useState<PaymentFormData>({
    amount: 150.00,
    currency: 'SAR',
    paymentMethod: 'CreditCard',
    cardNumber: '',
    expiryMonth: '',
    expiryYear: '',
    cvv: '',
    cardholderName: '',
    billingAddress: '',
    city: '',
    zipCode: '',
    country: 'Saudi Arabia'
  });

  const [loading, setLoading] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [showCvv, setShowCvv] = useState(false);
  const [securityValidation, setSecurityValidation] = useState<SecurityValidation | null>(null);
  const [paymentResult, setPaymentResult] = useState<PaymentResult | null>(null);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');
  const [cardType, setCardType] = useState<string>('');

  const supportedGateways = [
    { value: 'Stripe', label: 'Stripe', icon: '💳' },
    { value: 'PayPal', label: 'PayPal', icon: '🅿️' },
    { value: 'Mada', label: 'Mada (Saudi)', icon: '🇸🇦' }
  ];

  const paymentMethods = [
    { value: 'CreditCard', label: 'Credit Card' },
    { value: 'DebitCard', label: 'Debit Card' },
    { value: 'BankTransfer', label: 'Bank Transfer' }
  ];

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 20 }, (_, i) => currentYear + i);
  const months = Array.from({ length: 12 }, (_, i) => ({
    value: String(i + 1).padStart(2, '0'),
    label: String(i + 1).padStart(2, '0')
  }));

  useEffect(() => {
    if (formData.cardNumber) {
      detectCardType(formData.cardNumber);
    }
  }, [formData.cardNumber]);

  const detectCardType = (cardNumber: string) => {
    const cleanNumber = cardNumber.replace(/\s/g, '');
    if (cleanNumber.startsWith('4')) {
      setCardType('Visa');
    } else if (cleanNumber.startsWith('5') || cleanNumber.startsWith('2')) {
      setCardType('Mastercard');
    } else if (cleanNumber.startsWith('3')) {
      setCardType('American Express');
    } else {
      setCardType('');
    }
  };

  const formatCardNumber = (value: string) => {
    const cleanValue = value.replace(/\s/g, '');
    const formattedValue = cleanValue.replace(/(.{4})/g, '$1 ').trim();
    return formattedValue.substring(0, 19); // Max 16 digits + 3 spaces
  };

  const handleInputChange = (field: keyof PaymentFormData) => (
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement> | any
  ) => {
    let value = event.target.value;
    
    if (field === 'cardNumber') {
      value = formatCardNumber(value);
    } else if (field === 'cvv') {
      value = value.replace(/\D/g, '').substring(0, 4);
    } else if (field === 'amount') {
      value = parseFloat(value) || 0;
    }

    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    if (securityValidation) {
      setSecurityValidation(null);
    }
  };

  const validateSecurityAsync = async (): Promise<boolean> => {
    try {
      setLoading(true);
      setError('');

      const securityRequest = {
        paymentMethod: formData.paymentMethod,
        amount: formData.amount,
        currency: formData.currency,
        cardNumber: formData.cardNumber.replace(/\s/g, ''),
        cvv: formData.cvv,
        billingAddress: `${formData.billingAddress}, ${formData.city}, ${formData.zipCode}`,
        customerIP: '127.0.0.1' // In real app, get actual IP
      };

      const response = await apiClient.post('/api/payments/validate-security', securityRequest);
      const validation = (response as any).data;
      
      setSecurityValidation(validation);
      
      if (!validation.isValid) {
        setError(`Security validation failed: ${validation.riskLevel} risk level detected`);
        return false;
      }

      return true;
    } catch (err) {
      setError('Security validation failed. Please try again.');
      return false;
    } finally {
      setLoading(false);
    }
  };

  const processPayment = async () => {
    try {
      setProcessing(true);
      setError('');
      setSuccess('');

      const isSecure = await validateSecurityAsync();
      if (!isSecure) {
        return;
      }

      const paymentRequest = {
        amount: formData.amount,
        currency: formData.currency,
        cardNumber: formData.cardNumber.replace(/\s/g, ''),
        expiryMonth: formData.expiryMonth,
        expiryYear: formData.expiryYear,
        cvv: formData.cvv,
        cardholderName: formData.cardholderName,
        billingAddress: `${formData.billingAddress}, ${formData.city}, ${formData.zipCode}`,
        gatewayProvider: 'Stripe',
        customerIP: '127.0.0.1'
      };

      const response = await apiClient.post('/api/payments/process-credit-card', paymentRequest);
      const result = (response as any).data;
      
      setPaymentResult(result);
      
      if (result.status === 'Completed') {
        setSuccess(`Payment processed successfully! Transaction ID: ${result.transactionId}`);
        
        await generateReceipt(result.transactionId);
      } else {
        setError(`Payment failed: ${result.failureReason || 'Unknown error'}`);
      }
    } catch (err) {
      setError('Payment processing failed. Please try again.');
    } finally {
      setProcessing(false);
    }
  };

  const generateReceipt = async (transactionId: string) => {
    try {
      const response = await apiClient.post('/api/payments/generate-receipt', { transactionId });
      const receipt = (response as any).data;
      
      console.log('Receipt generated:', receipt);
    } catch (err) {
      console.error('Failed to generate receipt:', err);
    }
  };

  const isFormValid = () => {
    return (
      formData.amount > 0 &&
      formData.cardNumber.replace(/\s/g, '').length >= 13 &&
      formData.expiryMonth &&
      formData.expiryYear &&
      formData.cvv.length >= 3 &&
      formData.cardholderName.trim() &&
      formData.billingAddress.trim() &&
      formData.city.trim() &&
      formData.zipCode.trim()
    );
  };

  const getRiskLevelColor = (riskLevel: string) => {
    switch (riskLevel?.toLowerCase()) {
      case 'low': return 'success';
      case 'medium': return 'warning';
      case 'high': return 'error';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 3 }}>
      <Card elevation={3}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
            <SecurityIcon sx={{ mr: 2, color: 'primary.main' }} />
            <Typography variant="h5" component="h1">
              Secure Payment Processing
            </Typography>
            <Chip 
              label="PCI Compliant" 
              color="success" 
              size="small" 
              sx={{ ml: 2 }}
              icon={<LockIcon />}
            />
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          {paymentResult && paymentResult.status === 'Completed' && (
            <Alert severity="success" sx={{ mb: 2 }}>
              <Typography variant="h6">Payment Successful!</Typography>
              <Typography>Transaction ID: {paymentResult.transactionId}</Typography>
              <Typography>Receipt Number: {paymentResult.receiptNumber}</Typography>
              <Typography>Processed: {new Date(paymentResult.processedAt).toLocaleString()}</Typography>
            </Alert>
          )}

          <Grid container spacing={3}>
            {/* Payment Amount Section */}
            <Grid size={{ xs: 12 }}>
              <Typography variant="h6" gutterBottom>
                Payment Details
              </Typography>
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Amount"
                type="number"
                value={formData.amount}
                onChange={handleInputChange('amount')}
                InputProps={{
                  startAdornment: <InputAdornment position="start">SAR</InputAdornment>
                }}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Payment Method</InputLabel>
                <Select
                  value={formData.paymentMethod}
                  onChange={handleInputChange('paymentMethod')}
                  label="Payment Method"
                >
                  {paymentMethods.map(method => (
                    <MenuItem key={method.value} value={method.value}>
                      {method.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {/* Card Information Section */}
            <Grid size={{ xs: 12 }}>
              <Divider sx={{ my: 2 }} />
              <Typography variant="h6" gutterBottom>
                Card Information
              </Typography>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Card Number"
                value={formData.cardNumber}
                onChange={handleInputChange('cardNumber')}
                placeholder="1234 5678 9012 3456"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <CreditCardIcon />
                    </InputAdornment>
                  ),
                  endAdornment: cardType && (
                    <InputAdornment position="end">
                      <Chip label={cardType} size="small" />
                    </InputAdornment>
                  )
                }}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Cardholder Name"
                value={formData.cardholderName}
                onChange={handleInputChange('cardholderName')}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="CVV"
                type={showCvv ? 'text' : 'password'}
                value={formData.cvv}
                onChange={handleInputChange('cvv')}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowCvv(!showCvv)}
                        edge="end"
                      >
                        {showCvv ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  )
                }}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Expiry Month</InputLabel>
                <Select
                  value={formData.expiryMonth}
                  onChange={handleInputChange('expiryMonth')}
                  label="Expiry Month"
                >
                  {months.map(month => (
                    <MenuItem key={month.value} value={month.value}>
                      {month.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Expiry Year</InputLabel>
                <Select
                  value={formData.expiryYear}
                  onChange={handleInputChange('expiryYear')}
                  label="Expiry Year"
                >
                  {years.map(year => (
                    <MenuItem key={year} value={year.toString()}>
                      {year}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {/* Billing Address Section */}
            <Grid size={{ xs: 12 }}>
              <Divider sx={{ my: 2 }} />
              <Typography variant="h6" gutterBottom>
                Billing Address
              </Typography>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Address"
                value={formData.billingAddress}
                onChange={handleInputChange('billingAddress')}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="City"
                value={formData.city}
                onChange={handleInputChange('city')}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="ZIP Code"
                value={formData.zipCode}
                onChange={handleInputChange('zipCode')}
                required
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Country"
                value={formData.country}
                onChange={handleInputChange('country')}
                required
              />
            </Grid>

            {/* Security Validation Results */}
            {securityValidation && (
              <Grid size={{ xs: 12 }}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Security Validation
                </Typography>
                
                <Box sx={{ mb: 2 }}>
                  <Chip 
                    label={`Risk Level: ${securityValidation.riskLevel}`}
                    color={getRiskLevelColor(securityValidation.riskLevel) as any}
                    icon={securityValidation.isValid ? <CheckCircleIcon /> : <WarningIcon />}
                  />
                  <Chip 
                    label={`Risk Score: ${securityValidation.riskScore}`}
                    sx={{ ml: 1 }}
                  />
                </Box>

                {securityValidation.securityChecks.map((check, index) => (
                  <Alert 
                    key={index}
                    severity={check.passed ? 'success' : 'warning'}
                    sx={{ mb: 1 }}
                  >
                    <strong>{check.checkType}:</strong> {check.message}
                  </Alert>
                ))}
              </Grid>
            )}

            {/* Action Buttons */}
            <Grid size={{ xs: 12 }}>
              <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
                <Button
                  variant="outlined"
                  onClick={validateSecurityAsync}
                  disabled={!isFormValid() || loading}
                  startIcon={loading ? <CircularProgress size={20} /> : <SecurityIcon />}
                >
                  Validate Security
                </Button>
                
                <Button
                  variant="contained"
                  onClick={processPayment}
                  disabled={!isFormValid() || !securityValidation?.isValid || processing}
                  startIcon={processing ? <CircularProgress size={20} /> : <LockIcon />}
                  color="primary"
                >
                  {processing ? 'Processing...' : `Pay ${formData.amount} SAR`}
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Box>
  );
};

export default SecurePaymentForm;
