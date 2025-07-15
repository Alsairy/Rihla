import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  IconButton,
  Tooltip,
  Switch,
  FormControlLabel,
  LinearProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  Receipt as ReceiptIcon,
  Send as SendIcon,
  Download as DownloadIcon,
  Edit as EditIcon,
  Add as AddIcon,
  Refresh as RefreshIcon,
  Schedule as ScheduleIcon,
  Payment as PaymentIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Settings as SettingsIcon,
  ExpandMore as ExpandMoreIcon,
  AttachMoney as MoneyIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface Invoice {
  id: number;
  invoiceNumber: string;
  studentId: number;
  studentName: string;
  parentName: string;
  amount: number;
  dueDate: string;
  issueDate: string;
  status: 'Draft' | 'Sent' | 'Paid' | 'Overdue' | 'Cancelled';
  paymentMethod?: string;
  description: string;
  items: InvoiceItem[];
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
}

interface InvoiceItem {
  id: number;
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  serviceType: string;
}

interface BillingSettings {
  autoGenerateInvoices: boolean;
  invoiceFrequency: 'Monthly' | 'Quarterly' | 'Annually';
  dueDays: number;
  lateFeesEnabled: boolean;
  lateFeeAmount: number;
  reminderDays: number[];
  emailTemplate: string;
  taxRate: number;
  companyInfo: {
    name: string;
    address: string;
    phone: string;
    email: string;
    taxId: string;
  };
}

interface BillingStats {
  totalInvoices: number;
  totalRevenue: number;
  paidInvoices: number;
  overdueInvoices: number;
  pendingAmount: number;
  collectionRate: number;
}

const AutomatedBillingDashboard: React.FC = () => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [billingSettings, setBillingSettings] = useState<BillingSettings>({
    autoGenerateInvoices: true,
    invoiceFrequency: 'Monthly',
    dueDays: 30,
    lateFeesEnabled: false,
    lateFeeAmount: 25,
    reminderDays: [7, 3, 1],
    emailTemplate: 'Default',
    taxRate: 0.15,
    companyInfo: {
      name: 'Rihla Transportation Services',
      address: '123 School District Ave, City, State 12345',
      phone: '+1 (555) 123-4567',
      email: 'billing@rihla.com',
      taxId: 'TAX123456789',
    },
  });
  const [billingStats, setBillingStats] = useState<BillingStats>({
    totalInvoices: 0,
    totalRevenue: 0,
    paidInvoices: 0,
    overdueInvoices: 0,
    pendingAmount: 0,
    collectionRate: 0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [settingsDialogOpen, setSettingsDialogOpen] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [generatingInvoices, setGeneratingInvoices] = useState(false);

  useEffect(() => {
    loadInvoices();
    loadBillingSettings();
    loadBillingStats();
  }, []);

  const loadInvoices = async () => {
    try {
      setLoading(true);
      const response = (await apiClient.get('/api/billing/invoices')) as {
        data: Invoice[];
      };
      setInvoices(response.data || []);
    } catch {
      setError('Failed to load invoices');
    } finally {
      setLoading(false);
    }
  };

  const loadBillingSettings = async () => {
    try {
      const response = (await apiClient.get('/api/billing/settings')) as {
        data: BillingSettings;
      };
      if (response.data) {
        setBillingSettings(response.data);
      }
    } catch {
      setError('Failed to load billing settings');
    }
  };

  const loadBillingStats = async () => {
    try {
      const response = (await apiClient.get('/api/billing/stats')) as {
        data: BillingStats;
      };
      if (response.data) {
        setBillingStats(response.data);
      }
    } catch {
      setError('Failed to load billing stats');
    }
  };

  const generateAutomatedInvoices = async () => {
    try {
      setGeneratingInvoices(true);
      setError(null);

      const response = (await apiClient.post(
        '/api/billing/generate-automated-invoices',
        {
          frequency: billingSettings.invoiceFrequency,
          dueDays: billingSettings.dueDays,
          taxRate: billingSettings.taxRate,
        }
      )) as { data: { success: boolean; invoicesCreated: number } };

      if (response.data.success) {
        setSuccess(
          `Generated ${response.data.invoicesCreated} automated invoices successfully`
        );
        loadInvoices();
        loadBillingStats();
      }
    } catch {
      setError('Failed to generate automated invoices');
    } finally {
      setGeneratingInvoices(false);
    }
  };

  const sendInvoice = async (invoiceId: number) => {
    try {
      setLoading(true);
      const response = (await apiClient.post(
        `/api/billing/invoices/${invoiceId}/send`
      )) as { data: { success: boolean } };

      if (response.data.success) {
        setSuccess('Invoice sent successfully');
        loadInvoices();
      }
    } catch {
      setError('Failed to send invoice');
    } finally {
      setLoading(false);
    }
  };

  const downloadInvoice = async (invoiceId: number) => {
    try {
      const response = (await apiClient.get(
        `/api/billing/invoices/${invoiceId}/download`
      )) as { data: Blob };

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `invoice-${invoiceId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch {
      setError('Failed to download invoice');
    }
  };

  const saveBillingSettings = async () => {
    try {
      setLoading(true);
      const response = (await apiClient.put(
        '/api/billing/settings',
        billingSettings
      )) as { data: { success: boolean } };

      if (response.data.success) {
        setSuccess('Billing settings saved successfully');
        setSettingsDialogOpen(false);
      }
    } catch {
      setError('Failed to save billing settings');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'success';
      case 'Sent':
        return 'info';
      case 'Draft':
        return 'default';
      case 'Overdue':
        return 'error';
      case 'Cancelled':
        return 'warning';
      default:
        return 'default';
    }
  };

  const filteredInvoices = invoices.filter(invoice => {
    const matchesStatus =
      filterStatus === 'All' || invoice.status === filterStatus;
    const matchesSearch =
      invoice.studentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      invoice.parentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      invoice.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesStatus && matchesSearch;
  });

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

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
          <ReceiptIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Automated Billing Dashboard
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => {}}
          >
            Create Invoice
          </Button>
          <Button
            variant="outlined"
            startIcon={
              generatingInvoices ? (
                <CircularProgress size={20} />
              ) : (
                <ScheduleIcon />
              )
            }
            onClick={generateAutomatedInvoices}
            disabled={generatingInvoices}
          >
            Generate Automated
          </Button>
          <Button
            variant="outlined"
            startIcon={<SettingsIcon />}
            onClick={() => setSettingsDialogOpen(true)}
          >
            Settings
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

      {/* Billing Statistics */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <ReceiptIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{billingStats.totalInvoices}</Typography>
              <Typography variant="body2" color="textSecondary">
                Total Invoices
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <MoneyIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {formatCurrency(billingStats.totalRevenue)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Total Revenue
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <CheckCircleIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{billingStats.paidInvoices}</Typography>
              <Typography variant="body2" color="textSecondary">
                Paid Invoices
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <WarningIcon color="error" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {billingStats.overdueInvoices}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Overdue
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <PaymentIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {formatCurrency(billingStats.pendingAmount)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Pending Amount
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h6" color="primary">
                {(billingStats.collectionRate * 100).toFixed(1)}%
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Collection Rate
              </Typography>
              <LinearProgress
                variant="determinate"
                value={billingStats.collectionRate * 100}
                sx={{ mt: 1 }}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters and Search */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                label="Search invoices..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                size="small"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Status Filter</InputLabel>
                <Select
                  value={filterStatus}
                  onChange={e => setFilterStatus(e.target.value)}
                  label="Status Filter"
                >
                  <MenuItem value="All">All Statuses</MenuItem>
                  <MenuItem value="Draft">Draft</MenuItem>
                  <MenuItem value="Sent">Sent</MenuItem>
                  <MenuItem value="Paid">Paid</MenuItem>
                  <MenuItem value="Overdue">Overdue</MenuItem>
                  <MenuItem value="Cancelled">Cancelled</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <Button
                variant="outlined"
                startIcon={<RefreshIcon />}
                onClick={loadInvoices}
                fullWidth
              >
                Refresh
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Invoices Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Invoices ({filteredInvoices.length})
          </Typography>

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Invoice #</TableCell>
                    <TableCell>Student</TableCell>
                    <TableCell>Parent</TableCell>
                    <TableCell>Amount</TableCell>
                    <TableCell>Issue Date</TableCell>
                    <TableCell>Due Date</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredInvoices.map(invoice => (
                    <TableRow key={invoice.id}>
                      <TableCell>{invoice.invoiceNumber}</TableCell>
                      <TableCell>{invoice.studentName}</TableCell>
                      <TableCell>{invoice.parentName}</TableCell>
                      <TableCell>
                        {formatCurrency(invoice.totalAmount)}
                      </TableCell>
                      <TableCell>{formatDate(invoice.issueDate)}</TableCell>
                      <TableCell>{formatDate(invoice.dueDate)}</TableCell>
                      <TableCell>
                        <Chip
                          label={invoice.status}
                          color={getStatusColor(invoice.status) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Tooltip title="View/Edit">
                            <IconButton size="small" onClick={() => {}}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Send Invoice">
                            <IconButton
                              size="small"
                              onClick={() => sendInvoice(invoice.id)}
                              disabled={
                                invoice.status === 'Paid' ||
                                invoice.status === 'Cancelled'
                              }
                            >
                              <SendIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Download PDF">
                            <IconButton
                              size="small"
                              onClick={() => downloadInvoice(invoice.id)}
                            >
                              <DownloadIcon />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Billing Settings Dialog */}
      <Dialog
        open={settingsDialogOpen}
        onClose={() => setSettingsDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Billing Settings</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <Accordion defaultExpanded>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="h6">Automation Settings</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={3}>
                  <Grid size={{ xs: 12 }}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={billingSettings.autoGenerateInvoices}
                          onChange={e =>
                            setBillingSettings(prev => ({
                              ...prev,
                              autoGenerateInvoices: e.target.checked,
                            }))
                          }
                        />
                      }
                      label="Auto-generate invoices"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControl fullWidth>
                      <InputLabel>Invoice Frequency</InputLabel>
                      <Select
                        value={billingSettings.invoiceFrequency}
                        onChange={e =>
                          setBillingSettings(prev => ({
                            ...prev,
                            invoiceFrequency: e.target.value as any,
                          }))
                        }
                        label="Invoice Frequency"
                      >
                        <MenuItem value="Monthly">Monthly</MenuItem>
                        <MenuItem value="Quarterly">Quarterly</MenuItem>
                        <MenuItem value="Annually">Annually</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Due Days"
                      type="number"
                      value={billingSettings.dueDays}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          dueDays: parseInt(e.target.value),
                        }))
                      }
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="h6">Company Information</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Company Name"
                      value={billingSettings.companyInfo.name}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          companyInfo: {
                            ...prev.companyInfo,
                            name: e.target.value,
                          },
                        }))
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Tax ID"
                      value={billingSettings.companyInfo.taxId}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          companyInfo: {
                            ...prev.companyInfo,
                            taxId: e.target.value,
                          },
                        }))
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <TextField
                      fullWidth
                      label="Address"
                      multiline
                      rows={2}
                      value={billingSettings.companyInfo.address}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          companyInfo: {
                            ...prev.companyInfo,
                            address: e.target.value,
                          },
                        }))
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Phone"
                      value={billingSettings.companyInfo.phone}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          companyInfo: {
                            ...prev.companyInfo,
                            phone: e.target.value,
                          },
                        }))
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Email"
                      type="email"
                      value={billingSettings.companyInfo.email}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          companyInfo: {
                            ...prev.companyInfo,
                            email: e.target.value,
                          },
                        }))
                      }
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="h6">Tax & Fees</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      label="Tax Rate (%)"
                      type="number"
                      value={billingSettings.taxRate * 100}
                      onChange={e =>
                        setBillingSettings(prev => ({
                          ...prev,
                          taxRate: parseFloat(e.target.value) / 100,
                        }))
                      }
                      inputProps={{ step: 0.1, min: 0, max: 100 }}
                    />
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={billingSettings.lateFeesEnabled}
                          onChange={e =>
                            setBillingSettings(prev => ({
                              ...prev,
                              lateFeesEnabled: e.target.checked,
                            }))
                          }
                        />
                      }
                      label="Enable late fees"
                    />
                  </Grid>
                  {billingSettings.lateFeesEnabled && (
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField
                        fullWidth
                        label="Late Fee Amount"
                        type="number"
                        value={billingSettings.lateFeeAmount}
                        onChange={e =>
                          setBillingSettings(prev => ({
                            ...prev,
                            lateFeeAmount: parseFloat(e.target.value),
                          }))
                        }
                        inputProps={{ step: 0.01, min: 0 }}
                      />
                    </Grid>
                  )}
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSettingsDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveBillingSettings}>
            Save Settings
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AutomatedBillingDashboard;
