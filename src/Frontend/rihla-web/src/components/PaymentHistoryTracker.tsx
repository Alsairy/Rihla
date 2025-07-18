import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  Divider,
  Avatar,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  LinearProgress,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Payment as PaymentIcon,
  Download as DownloadIcon,
  Refresh as RefreshIcon,
  Search as SearchIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  AccountBalance as BankIcon,
  CreditCard as CardIcon,
  Money as CashIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Person as PersonIcon,
  Visibility as ViewIcon,
  AttachMoney as MoneyIcon,
  Analytics as AnalyticsIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface PaymentTransaction {
  id: number;
  transactionId: string;
  studentId: number;
  studentName: string;
  parentName: string;
  amount: number;
  paymentMethod: 'CreditCard' | 'DebitCard' | 'BankTransfer' | 'Cash' | 'Check';
  status: 'Pending' | 'Completed' | 'Failed' | 'Refunded' | 'Cancelled';
  transactionDate: string;
  description: string;
  invoiceNumber?: string;
  gatewayResponse?: string;
  fees: number;
  netAmount: number;
  currency: string;
  receiptUrl?: string;
  refundReason?: string;
  refundDate?: string;
  refundAmount?: number;
}

interface PaymentSummary {
  totalTransactions: number;
  totalAmount: number;
  successfulTransactions: number;
  failedTransactions: number;
  refundedTransactions: number;
  averageTransactionAmount: number;
  totalFees: number;
  netRevenue: number;
}

interface PaymentAnalytics {
  monthlyTrends: MonthlyTrend[];
  paymentMethodBreakdown: PaymentMethodStats[];
  statusDistribution: StatusStats[];
  topPayingFamilies: TopPayerStats[];
}

interface MonthlyTrend {
  month: string;
  totalAmount: number;
  transactionCount: number;
  successRate: number;
}

interface PaymentMethodStats {
  method: string;
  count: number;
  totalAmount: number;
  percentage: number;
}

interface StatusStats {
  status: string;
  count: number;
  percentage: number;
}

interface TopPayerStats {
  parentName: string;
  studentName: string;
  totalPaid: number;
  transactionCount: number;
}

const PaymentHistoryTracker: React.FC = () => {
  const [transactions, setTransactions] = useState<PaymentTransaction[]>([]);
  const [paymentSummary, setPaymentSummary] = useState<PaymentSummary>({
    totalTransactions: 0,
    totalAmount: 0,
    successfulTransactions: 0,
    failedTransactions: 0,
    refundedTransactions: 0,
    averageTransactionAmount: 0,
    totalFees: 0,
    netRevenue: 0,
  });
  const [paymentAnalytics, setPaymentAnalytics] = useState<PaymentAnalytics>({
    monthlyTrends: [],
    paymentMethodBreakdown: [],
    statusDistribution: [],
    topPayingFamilies: [],
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selectedTransaction, setSelectedTransaction] =
    useState<PaymentTransaction | null>(null);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>('All');
  const [filterMethod, setFilterMethod] = useState<string>('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [startDate] = useState<Date | null>(null);
  const [endDate] = useState<Date | null>(null);
  const [currentTab, setCurrentTab] = useState(0);
  const [page] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  useEffect(() => {
    loadPaymentHistory();
    loadPaymentSummary();
    loadPaymentAnalytics();
  }, []);

  useEffect(() => {
    loadPaymentHistory();
  }, [filterStatus, filterMethod, searchTerm, startDate, endDate]);

  const loadPaymentHistory = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();

      if (filterStatus !== 'All') params.append('status', filterStatus);
      if (filterMethod !== 'All') params.append('method', filterMethod);
      if (searchTerm) params.append('search', searchTerm);
      if (startDate) params.append('startDate', startDate.toISOString());
      if (endDate) params.append('endDate', endDate.toISOString());

      const response = (await apiClient.get(
        `/api/payments/history?${params.toString()}`
      )) as { data: PaymentTransaction[] };
      setTransactions(response.data || []);
    } catch (err) {
      setError('Failed to load payment history');
      console.error('Error loading payment history:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadPaymentSummary = async () => {
    try {
      const response = (await apiClient.get('/api/payments/summary')) as {
        data: PaymentSummary;
      };
      if (response.data) {
        setPaymentSummary(response.data);
      }
    } catch (err) {
      console.error('Error loading payment summary:', err);
    }
  };

  const loadPaymentAnalytics = async () => {
    try {
      const response = (await apiClient.get('/api/payments/analytics')) as {
        data: PaymentAnalytics;
      };
      if (response.data) {
        setPaymentAnalytics(response.data);
      }
    } catch (err) {
      console.error('Error loading payment analytics:', err);
    }
  };

  const downloadReceipt = async (transactionId: number) => {
    try {
      const response = (await apiClient.get(
        `/api/payments/${transactionId}/receipt`
      )) as { data: Blob };

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `receipt-${transactionId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      setError('Failed to download receipt');
      console.error('Error downloading receipt:', err);
    }
  };

  const processRefund = async (
    transactionId: number,
    refundAmount: number,
    reason: string
  ) => {
    try {
      setLoading(true);
      const response = (await apiClient.post(
        `/api/payments/${transactionId}/refund`,
        {
          amount: refundAmount,
          reason,
        }
      )) as { data: { success: boolean } };

      if (response.data.success) {
        setSuccess('Refund processed successfully');
        loadPaymentHistory();
        loadPaymentSummary();
        setTransactionDialogOpen(false);
      }
    } catch (err) {
      setError('Failed to process refund');
      console.error('Error processing refund:', err);
    } finally {
      setLoading(false);
    }
  };

  const exportPaymentHistory = async () => {
    try {
      const params = new URLSearchParams();
      if (filterStatus !== 'All') params.append('status', filterStatus);
      if (filterMethod !== 'All') params.append('method', filterMethod);
      if (searchTerm) params.append('search', searchTerm);
      if (startDate) params.append('startDate', startDate.toISOString());
      if (endDate) params.append('endDate', endDate.toISOString());

      const response = (await apiClient.get(
        `/api/payments/export?${params.toString()}`
      )) as { data: Blob };

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute(
        'download',
        `payment-history-${new Date().toISOString().split('T')[0]}.xlsx`
      );
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      setError('Failed to export payment history');
      console.error('Error exporting payment history:', err);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'success';
      case 'Pending':
        return 'warning';
      case 'Failed':
        return 'error';
      case 'Refunded':
        return 'info';
      case 'Cancelled':
        return 'default';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return <CheckCircleIcon />;
      case 'Pending':
        return <ScheduleIcon />;
      case 'Failed':
        return <ErrorIcon />;
      case 'Refunded':
        return <WarningIcon />;
      case 'Cancelled':
        return <ErrorIcon />;
      default:
        return <PaymentIcon />;
    }
  };

  const getPaymentMethodIcon = (method: string) => {
    switch (method) {
      case 'CreditCard':
      case 'DebitCard':
        return <CardIcon />;
      case 'BankTransfer':
        return <BankIcon />;
      case 'Cash':
        return <CashIcon />;
      default:
        return <PaymentIcon />;
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const filteredTransactions = transactions.filter(transaction => {
    const matchesStatus =
      filterStatus === 'All' || transaction.status === filterStatus;
    const matchesMethod =
      filterMethod === 'All' || transaction.paymentMethod === filterMethod;
    const matchesSearch =
      transaction.studentName
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      transaction.parentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      transaction.transactionId
        .toLowerCase()
        .includes(searchTerm.toLowerCase());
    return matchesStatus && matchesMethod && matchesSearch;
  });

  const TabPanel = ({
    children,
    value,
    index,
  }: {
    children: React.ReactNode;
    value: number;
    index: number;
  }) => (
    <div hidden={value !== index}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" gutterBottom>
          <PaymentIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Payment History Tracker
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={<DownloadIcon />}
            onClick={exportPaymentHistory}
          >
            Export
          </Button>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadPaymentHistory}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert
          severity="success"
          sx={{ mb: 2 }}
          onClose={() => setSuccess(null)}
        >
          {success}
        </Alert>
      )}

      {/* Payment Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <PaymentIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {paymentSummary.totalTransactions}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Total Transactions
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <MoneyIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {formatCurrency(paymentSummary.totalAmount)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Total Amount
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <CheckCircleIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {paymentSummary.successfulTransactions}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Successful
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h6" color="primary">
                {(
                  (paymentSummary.successfulTransactions /
                    paymentSummary.totalTransactions) *
                    100 || 0
                ).toFixed(1)}
                %
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Success Rate
              </Typography>
              <LinearProgress
                variant="determinate"
                value={
                  (paymentSummary.successfulTransactions /
                    paymentSummary.totalTransactions) *
                    100 || 0
                }
                sx={{ mt: 1 }}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Tabs for different views */}
      <Card sx={{ mb: 3 }}>
        <Tabs
          value={currentTab}
          onChange={(e, newValue) => setCurrentTab(newValue)}
        >
          <Tab label="Transaction History" />
          <Tab label="Analytics" />
          <Tab label="Payment Methods" />
        </Tabs>

        <TabPanel value={currentTab} index={0}>
          {/* Filters */}
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                label="Search transactions..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />
                  ),
                }}
                size="small"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  value={filterStatus}
                  onChange={e => setFilterStatus(e.target.value)}
                  label="Status"
                >
                  <MenuItem value="All">All Statuses</MenuItem>
                  <MenuItem value="Completed">Completed</MenuItem>
                  <MenuItem value="Pending">Pending</MenuItem>
                  <MenuItem value="Failed">Failed</MenuItem>
                  <MenuItem value="Refunded">Refunded</MenuItem>
                  <MenuItem value="Cancelled">Cancelled</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Payment Method</InputLabel>
                <Select
                  value={filterMethod}
                  onChange={e => setFilterMethod(e.target.value)}
                  label="Payment Method"
                >
                  <MenuItem value="All">All Methods</MenuItem>
                  <MenuItem value="CreditCard">Credit Card</MenuItem>
                  <MenuItem value="DebitCard">Debit Card</MenuItem>
                  <MenuItem value="BankTransfer">Bank Transfer</MenuItem>
                  <MenuItem value="Cash">Cash</MenuItem>
                  <MenuItem value="Check">Check</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            {/* Date pickers removed due to missing dependency */}
          </Grid>

          {/* Transactions Table */}
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Transaction ID</TableCell>
                    <TableCell>Student</TableCell>
                    <TableCell>Parent</TableCell>
                    <TableCell>Amount</TableCell>
                    <TableCell>Method</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredTransactions
                    .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                    .map(transaction => (
                      <TableRow key={transaction.id}>
                        <TableCell>{transaction.transactionId}</TableCell>
                        <TableCell>{transaction.studentName}</TableCell>
                        <TableCell>{transaction.parentName}</TableCell>
                        <TableCell>
                          {formatCurrency(transaction.amount)}
                        </TableCell>
                        <TableCell>
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: 1,
                            }}
                          >
                            {getPaymentMethodIcon(transaction.paymentMethod)}
                            {transaction.paymentMethod}
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={transaction.status}
                            color={getStatusColor(transaction.status) as any}
                            icon={getStatusIcon(transaction.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          {formatDateTime(transaction.transactionDate)}
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <Tooltip title="View Details">
                              <IconButton
                                size="small"
                                onClick={() => {
                                  setSelectedTransaction(transaction);
                                  setTransactionDialogOpen(true);
                                }}
                              >
                                <ViewIcon />
                              </IconButton>
                            </Tooltip>
                            {transaction.receiptUrl && (
                              <Tooltip title="Download Receipt">
                                <IconButton
                                  size="small"
                                  onClick={() =>
                                    downloadReceipt(transaction.id)
                                  }
                                >
                                  <DownloadIcon />
                                </IconButton>
                              </Tooltip>
                            )}
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>

        <TabPanel value={currentTab} index={1}>
          {/* Analytics Dashboard */}
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    <AnalyticsIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Payment Method Breakdown
                  </Typography>
                  <List>
                    {paymentAnalytics.paymentMethodBreakdown.map(
                      (method, index) => (
                        <ListItem key={index}>
                          <ListItemAvatar>
                            <Avatar>
                              {getPaymentMethodIcon(method.method)}
                            </Avatar>
                          </ListItemAvatar>
                          <ListItemText
                            primary={method.method}
                            secondary={`${method.count} transactions • ${method.percentage.toFixed(1)}%`}
                          />
                          <ListItemSecondaryAction>
                            <Typography variant="body2" color="primary">
                              {formatCurrency(method.totalAmount)}
                            </Typography>
                          </ListItemSecondaryAction>
                        </ListItem>
                      )
                    )}
                  </List>
                </CardContent>
              </Card>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    <TrendingUpIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Top Paying Families
                  </Typography>
                  <List>
                    {paymentAnalytics.topPayingFamilies.map((family, index) => (
                      <ListItem key={index}>
                        <ListItemAvatar>
                          <Avatar>
                            <PersonIcon />
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={family.parentName}
                          secondary={`${family.studentName} • ${family.transactionCount} transactions`}
                        />
                        <ListItemSecondaryAction>
                          <Typography variant="body2" color="primary">
                            {formatCurrency(family.totalPaid)}
                          </Typography>
                        </ListItemSecondaryAction>
                      </ListItem>
                    ))}
                  </List>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={currentTab} index={2}>
          {/* Payment Methods Analysis */}
          <Grid container spacing={3}>
            {paymentAnalytics.paymentMethodBreakdown.map((method, index) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={index}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    {getPaymentMethodIcon(method.method)}
                    <Typography variant="h6" sx={{ mt: 1 }}>
                      {method.method}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      {method.count} transactions
                    </Typography>
                    <Typography variant="h5" color="primary" sx={{ mt: 1 }}>
                      {formatCurrency(method.totalAmount)}
                    </Typography>
                    <LinearProgress
                      variant="determinate"
                      value={method.percentage}
                      sx={{ mt: 2 }}
                    />
                    <Typography
                      variant="body2"
                      color="textSecondary"
                      sx={{ mt: 1 }}
                    >
                      {method.percentage.toFixed(1)}% of total
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </TabPanel>
      </Card>

      {/* Transaction Details Dialog */}
      <Dialog
        open={transactionDialogOpen}
        onClose={() => setTransactionDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Transaction Details
          {selectedTransaction && (
            <Chip
              label={selectedTransaction.status}
              color={getStatusColor(selectedTransaction.status) as any}
              sx={{ ml: 2 }}
            />
          )}
        </DialogTitle>
        <DialogContent>
          {selectedTransaction && (
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Transaction ID
                </Typography>
                <Typography variant="body1">
                  {selectedTransaction.transactionId}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Amount
                </Typography>
                <Typography variant="body1">
                  {formatCurrency(selectedTransaction.amount)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Student
                </Typography>
                <Typography variant="body1">
                  {selectedTransaction.studentName}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Parent
                </Typography>
                <Typography variant="body1">
                  {selectedTransaction.parentName}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Payment Method
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {getPaymentMethodIcon(selectedTransaction.paymentMethod)}
                  <Typography variant="body1">
                    {selectedTransaction.paymentMethod}
                  </Typography>
                </Box>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Transaction Date
                </Typography>
                <Typography variant="body1">
                  {formatDateTime(selectedTransaction.transactionDate)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Description
                </Typography>
                <Typography variant="body1">
                  {selectedTransaction.description}
                </Typography>
              </Grid>
              {selectedTransaction.invoiceNumber && (
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Invoice Number
                  </Typography>
                  <Typography variant="body1">
                    {selectedTransaction.invoiceNumber}
                  </Typography>
                </Grid>
              )}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Fees
                </Typography>
                <Typography variant="body1">
                  {formatCurrency(selectedTransaction.fees)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="textSecondary">
                  Net Amount
                </Typography>
                <Typography variant="body1">
                  {formatCurrency(selectedTransaction.netAmount)}
                </Typography>
              </Grid>
              {selectedTransaction.refundReason && (
                <>
                  <Grid size={{ xs: 12 }}>
                    <Divider sx={{ my: 2 }} />
                    <Typography variant="h6" color="warning.main">
                      Refund Information
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Refund Amount
                    </Typography>
                    <Typography variant="body1">
                      {formatCurrency(selectedTransaction.refundAmount || 0)}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Refund Date
                    </Typography>
                    <Typography variant="body1">
                      {selectedTransaction.refundDate
                        ? formatDateTime(selectedTransaction.refundDate)
                        : 'N/A'}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Refund Reason
                    </Typography>
                    <Typography variant="body1">
                      {selectedTransaction.refundReason}
                    </Typography>
                  </Grid>
                </>
              )}
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTransactionDialogOpen(false)}>Close</Button>
          {selectedTransaction?.receiptUrl && (
            <Button
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={() => downloadReceipt(selectedTransaction.id)}
            >
              Download Receipt
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PaymentHistoryTracker;
