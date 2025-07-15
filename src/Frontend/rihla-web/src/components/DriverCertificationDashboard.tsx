import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Avatar,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  IconButton,
  Alert,
  LinearProgress,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Badge,
} from '@mui/material';
import {
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Person as PersonIcon,
  Description as DescriptionIcon,
  Download as DownloadIcon,
  Visibility as VisibilityIcon,
  Refresh as RefreshIcon,
  Assignment as AssignmentIcon,
  LocalHospital as MedicalIcon,
  DriveEta as LicenseIcon,
  School as TrainingIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';
import { signalRService } from '../services/signalRService';

interface Driver {
  id: number;
  firstName: string;
  lastName: string;
  employeeId: string;
  email: string;
  phone: string;
  licenseNumber: string;
  licenseClass: string;
  licenseExpiry: string;
  medicalCertExpiry?: string;
  isActive: boolean;
  hireDate: string;
  profileImageUrl?: string;
}

interface DriverDocument {
  id: number;
  driverId: number;
  documentType: string;
  fileName: string;
  fileUrl: string;
  uploadDate: string;
  expiryDate?: string;
  isValid: boolean;
}

interface CertificationStatus {
  type: 'license' | 'medical' | 'training' | 'background';
  label: string;
  expiryDate?: string;
  daysUntilExpiry: number;
  status: 'valid' | 'warning' | 'expired' | 'missing';
  document?: DriverDocument;
}

const DriverCertificationDashboard: React.FC = () => {
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [documents, setDocuments] = useState<{
    [driverId: number]: DriverDocument[];
  }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedDriver, setSelectedDriver] = useState<Driver | null>(null);
  const [documentDialogOpen, setDocumentDialogOpen] = useState(false);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    fetchDriversAndCertifications();

    signalRService.startConnection();

    signalRService.onDriverCertificationUpdated(() => {
      fetchDriversAndCertifications(); // Refresh driver data
    });

    signalRService.onDriverStatusChanged(() => {
      fetchDriversAndCertifications(); // Refresh driver data
    });

    return () => {
      signalRService.removeAllListeners();
    };
  }, []);

  const fetchDriversAndCertifications = async () => {
    setLoading(true);
    setError(null);

    try {
      const driversResponse = (await apiClient.get('/api/drivers')) as any;
      const driversData = Array.isArray(driversResponse)
        ? driversResponse
        : driversResponse.items || [];
      setDrivers(driversData);

      const documentsData: { [driverId: number]: DriverDocument[] } = {};

      for (const driver of driversData) {
        try {
          const docsResponse = (await apiClient.get(
            `/api/files/driver/${driver.id}/documents`
          )) as any;
          documentsData[driver.id] = Array.isArray(docsResponse)
            ? docsResponse
            : [];
        } catch {
          documentsData[driver.id] = [];
        }
      }

      setDocuments(documentsData);
    } catch {
      setError('Failed to load driver certification data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    setRefreshing(true);
    await fetchDriversAndCertifications();
    setRefreshing(false);
  };

  const calculateDaysUntilExpiry = (expiryDate: string): number => {
    const today = new Date();
    const expiry = new Date(expiryDate);
    const diffTime = expiry.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  };

  const getCertificationStatus = (driver: Driver): CertificationStatus[] => {
    const driverDocs = documents[driver.id] || [];
    const statuses: CertificationStatus[] = [];

    const licenseExpiry = driver.licenseExpiry;
    const licenseDays = licenseExpiry
      ? calculateDaysUntilExpiry(licenseExpiry)
      : -999;
    const licenseDoc = driverDocs.find(doc => doc.documentType === 'license');

    statuses.push({
      type: 'license',
      label: 'Driver License',
      expiryDate: licenseExpiry,
      daysUntilExpiry: licenseDays,
      status: !licenseDoc
        ? 'missing'
        : licenseDays < 0
          ? 'expired'
          : licenseDays <= 30
            ? 'warning'
            : 'valid',
      document: licenseDoc,
    });

    const medicalExpiry = driver.medicalCertExpiry;
    const medicalDays = medicalExpiry
      ? calculateDaysUntilExpiry(medicalExpiry)
      : -999;
    const medicalDoc = driverDocs.find(doc => doc.documentType === 'medical');

    statuses.push({
      type: 'medical',
      label: 'Medical Certificate',
      expiryDate: medicalExpiry,
      daysUntilExpiry: medicalDays,
      status: !medicalDoc
        ? 'missing'
        : medicalDays < 0
          ? 'expired'
          : medicalDays <= 30
            ? 'warning'
            : 'valid',
      document: medicalDoc,
    });

    const trainingDoc = driverDocs.find(doc => doc.documentType === 'training');
    const trainingExpiry = trainingDoc?.expiryDate;
    const trainingDays = trainingExpiry
      ? calculateDaysUntilExpiry(trainingExpiry)
      : -999;

    statuses.push({
      type: 'training',
      label: 'Training Certificate',
      expiryDate: trainingExpiry,
      daysUntilExpiry: trainingDays,
      status: !trainingDoc
        ? 'missing'
        : trainingDays < 0
          ? 'expired'
          : trainingDays <= 90
            ? 'warning'
            : 'valid',
      document: trainingDoc,
    });

    const backgroundDoc = driverDocs.find(
      doc => doc.documentType === 'background'
    );
    const backgroundExpiry = backgroundDoc?.expiryDate;
    const backgroundDays = backgroundExpiry
      ? calculateDaysUntilExpiry(backgroundExpiry)
      : -999;

    statuses.push({
      type: 'background',
      label: 'Background Check',
      expiryDate: backgroundExpiry,
      daysUntilExpiry: backgroundDays,
      status: !backgroundDoc
        ? 'missing'
        : backgroundDays < 0
          ? 'expired'
          : backgroundDays <= 365
            ? 'warning'
            : 'valid',
      document: backgroundDoc,
    });

    return statuses;
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'valid':
        return 'success';
      case 'warning':
        return 'warning';
      case 'expired':
        return 'error';
      case 'missing':
        return 'default';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'valid':
        return <CheckCircleIcon />;
      case 'warning':
        return <WarningIcon />;
      case 'expired':
        return <ErrorIcon />;
      case 'missing':
        return <ScheduleIcon />;
      default:
        return <ScheduleIcon />;
    }
  };

  const getCertificationTypeIcon = (type: string) => {
    switch (type) {
      case 'license':
        return <LicenseIcon />;
      case 'medical':
        return <MedicalIcon />;
      case 'training':
        return <TrainingIcon />;
      case 'background':
        return <AssignmentIcon />;
      default:
        return <DescriptionIcon />;
    }
  };

  const getOverallDriverStatus = (certifications: CertificationStatus[]) => {
    const hasExpired = certifications.some(cert => cert.status === 'expired');
    const hasMissing = certifications.some(cert => cert.status === 'missing');
    const hasWarning = certifications.some(cert => cert.status === 'warning');

    if (hasExpired || hasMissing) return 'critical';
    if (hasWarning) return 'warning';
    return 'good';
  };

  const handleViewDocument = async (document: DriverDocument) => {
    try {
      window.open(document.fileUrl, '_blank');
      // eslint-disable-next-line no-empty
    } catch {}
  };

  const handleDownloadDocument = async (document: DriverDocument) => {
    try {
      const response = await fetch(document.fileUrl);
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = window.document.createElement('a');
      a.href = url;
      a.download = document.fileName;
      window.document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      window.document.body.removeChild(a);
      // eslint-disable-next-line no-empty
    } catch {}
  };

  const handleDriverClick = (driver: Driver) => {
    setSelectedDriver(driver);
    setDocumentDialogOpen(true);
  };

  const getExpiringCertificationsCount = () => {
    let count = 0;
    drivers.forEach(driver => {
      const certifications = getCertificationStatus(driver);
      certifications.forEach(cert => {
        if (
          cert.status === 'warning' ||
          cert.status === 'expired' ||
          cert.status === 'missing'
        ) {
          count++;
        }
      });
    });
    return count;
  };

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <LinearProgress />
        <Typography variant="h6" sx={{ mt: 2, textAlign: 'center' }}>
          Loading driver certifications...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
        <Button
          variant="contained"
          onClick={handleRefresh}
          startIcon={<RefreshIcon />}
        >
          Retry
        </Button>
      </Box>
    );
  }

  const expiringCount = getExpiringCertificationsCount();

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <PersonIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
          <Typography variant="h4" sx={{ fontWeight: 600 }}>
            Driver Certification Dashboard
          </Typography>
        </Box>
        <Button
          variant="outlined"
          onClick={handleRefresh}
          disabled={refreshing}
          startIcon={<RefreshIcon />}
        >
          {refreshing ? 'Refreshing...' : 'Refresh'}
        </Button>
      </Box>

      {/* Summary Cards */}
      <Box sx={{ display: 'flex', gap: 3, mb: 4, flexWrap: 'wrap' }}>
        <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
          <Card sx={{ bgcolor: 'primary.main', color: 'white' }}>
            <CardContent>
              <Typography variant="h6">Total Drivers</Typography>
              <Typography variant="h3" sx={{ fontWeight: 700 }}>
                {drivers.length}
              </Typography>
            </CardContent>
          </Card>
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
          <Card
            sx={{
              bgcolor: expiringCount > 0 ? 'warning.main' : 'success.main',
              color: 'white',
            }}
          >
            <CardContent>
              <Typography variant="h6">Expiring/Missing</Typography>
              <Typography variant="h3" sx={{ fontWeight: 700 }}>
                {expiringCount}
              </Typography>
            </CardContent>
          </Card>
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
          <Card sx={{ bgcolor: 'info.main', color: 'white' }}>
            <CardContent>
              <Typography variant="h6">Active Drivers</Typography>
              <Typography variant="h3" sx={{ fontWeight: 700 }}>
                {drivers.filter(d => d.isActive).length}
              </Typography>
            </CardContent>
          </Card>
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
          <Card sx={{ bgcolor: 'secondary.main', color: 'white' }}>
            <CardContent>
              <Typography variant="h6">Valid Certifications</Typography>
              <Typography variant="h3" sx={{ fontWeight: 700 }}>
                {drivers.length * 4 - expiringCount}
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Drivers Table */}
      <Paper sx={{ mb: 3 }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Driver</TableCell>
                <TableCell>Employee ID</TableCell>
                <TableCell>License</TableCell>
                <TableCell>Medical</TableCell>
                <TableCell>Training</TableCell>
                <TableCell>Background</TableCell>
                <TableCell>Overall Status</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {drivers.map(driver => {
                const certifications = getCertificationStatus(driver);
                const overallStatus = getOverallDriverStatus(certifications);

                return (
                  <TableRow
                    key={driver.id}
                    sx={{
                      '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' },
                      cursor: 'pointer',
                    }}
                    onClick={() => handleDriverClick(driver)}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Avatar
                          src={driver.profileImageUrl}
                          sx={{ width: 40, height: 40, mr: 2 }}
                        >
                          {driver.firstName.charAt(0)}
                        </Avatar>
                        <Box>
                          <Typography
                            variant="subtitle2"
                            sx={{ fontWeight: 600 }}
                          >
                            {driver.firstName} {driver.lastName}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {driver.email}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>{driver.employeeId}</TableCell>
                    {certifications.map(cert => (
                      <TableCell key={cert.type}>
                        <Tooltip
                          title={
                            cert.expiryDate
                              ? `Expires: ${new Date(cert.expiryDate).toLocaleDateString()} (${cert.daysUntilExpiry} days)`
                              : 'No expiry date set'
                          }
                        >
                          <Chip
                            icon={getStatusIcon(cert.status)}
                            label={
                              cert.daysUntilExpiry > 0
                                ? `${cert.daysUntilExpiry}d`
                                : cert.status
                            }
                            color={getStatusColor(cert.status) as any}
                            size="small"
                            variant={
                              cert.status === 'missing' ? 'outlined' : 'filled'
                            }
                          />
                        </Tooltip>
                      </TableCell>
                    ))}
                    <TableCell>
                      <Chip
                        label={overallStatus}
                        color={
                          overallStatus === 'good'
                            ? 'success'
                            : overallStatus === 'warning'
                              ? 'warning'
                              : 'error'
                        }
                        variant="filled"
                      />
                    </TableCell>
                    <TableCell>
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={e => {
                          e.stopPropagation();
                          handleDriverClick(driver);
                        }}
                      >
                        View Details
                      </Button>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Driver Details Dialog */}
      <Dialog
        open={documentDialogOpen}
        onClose={() => setDocumentDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Avatar
              src={selectedDriver?.profileImageUrl}
              sx={{ width: 48, height: 48, mr: 2 }}
            >
              {selectedDriver?.firstName.charAt(0)}
            </Avatar>
            <Box>
              <Typography variant="h6">
                {selectedDriver?.firstName} {selectedDriver?.lastName}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Employee ID: {selectedDriver?.employeeId}
              </Typography>
            </Box>
          </Box>
        </DialogTitle>
        <DialogContent>
          {selectedDriver && (
            <Box>
              {/* Driver Info */}
              <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Email
                  </Typography>
                  <Typography variant="body1">
                    {selectedDriver.email}
                  </Typography>
                </Box>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Phone
                  </Typography>
                  <Typography variant="body1">
                    {selectedDriver.phone}
                  </Typography>
                </Box>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    License Number
                  </Typography>
                  <Typography variant="body1">
                    {selectedDriver.licenseNumber}
                  </Typography>
                </Box>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    License Class
                  </Typography>
                  <Typography variant="body1">
                    {selectedDriver.licenseClass}
                  </Typography>
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              {/* Certifications */}
              <Typography variant="h6" sx={{ mb: 2 }}>
                Certifications & Documents
              </Typography>
              <List>
                {getCertificationStatus(selectedDriver).map(cert => (
                  <ListItem key={cert.type} sx={{ px: 0 }}>
                    <ListItemIcon>
                      <Badge
                        badgeContent={getStatusIcon(cert.status)}
                        color={getStatusColor(cert.status) as any}
                        overlap="circular"
                      >
                        {getCertificationTypeIcon(cert.type)}
                      </Badge>
                    </ListItemIcon>
                    <ListItemText
                      primary={cert.label}
                      secondary={
                        cert.expiryDate
                          ? `Expires: ${new Date(cert.expiryDate).toLocaleDateString()} (${cert.daysUntilExpiry} days)`
                          : 'No expiry date set'
                      }
                    />
                    {cert.document && (
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <IconButton
                          size="small"
                          onClick={() => handleViewDocument(cert.document!)}
                          title="View Document"
                        >
                          <VisibilityIcon />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDownloadDocument(cert.document!)}
                          title="Download Document"
                        >
                          <DownloadIcon />
                        </IconButton>
                      </Box>
                    )}
                  </ListItem>
                ))}
              </List>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDocumentDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DriverCertificationDashboard;
