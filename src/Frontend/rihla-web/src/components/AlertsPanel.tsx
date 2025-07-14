import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Alert,
  AlertTitle,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Badge,
  Tabs,
  Tab,
  Grid,
  Card,
  CardContent,
  Button,
  Tooltip,
  Collapse,
  Divider,
} from '@mui/material';
import {
  Warning as WarningIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
  CheckCircle as CheckCircleIcon,
  DirectionsCar as VehicleIcon,
  Person as DriverIcon,
  Build as MaintenanceIcon,
  Security as InsuranceIcon,
  Assignment as LicenseIcon,
  LocalHospital as MedicalIcon,
  School as TrainingIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Refresh as RefreshIcon,
  Notifications as NotificationsIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';
import signalRService from '../services/signalRService';

interface AlertItem {
  id: number;
  type: 'driver_certification' | 'vehicle_maintenance' | 'vehicle_insurance' | 'vehicle_registration';
  severity: 'error' | 'warning' | 'info';
  title: string;
  description: string;
  entityId: number;
  entityName: string;
  dueDate: string;
  daysUntilDue: number;
  category: string;
  actionRequired: boolean;
}

interface AlertsPanelProps {
  onAlertClick?: (alert: AlertItem) => void;
  maxItems?: number;
  showCategories?: boolean;
  autoRefresh?: boolean;
  refreshInterval?: number;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <div role="tabpanel" hidden={value !== index}>
    {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
  </div>
);

const AlertsPanel: React.FC<AlertsPanelProps> = ({
  onAlertClick,
  maxItems = 50,
  showCategories = true,
  autoRefresh = true,
  refreshInterval = 300000, // 5 minutes
}) => {
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set(['critical']));
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());

  const alertCategories = {
    critical: {
      label: 'Critical Alerts',
      icon: <ErrorIcon />,
      color: 'error' as const,
      filter: (alert: AlertItem) => alert.severity === 'error' || alert.daysUntilDue <= 0,
    },
    warning: {
      label: 'Warnings',
      icon: <WarningIcon />,
      color: 'warning' as const,
      filter: (alert: AlertItem) => alert.severity === 'warning' && alert.daysUntilDue > 0 && alert.daysUntilDue <= 30,
    },
    upcoming: {
      label: 'Upcoming',
      icon: <InfoIcon />,
      color: 'info' as const,
      filter: (alert: AlertItem) => alert.severity === 'info' && alert.daysUntilDue > 30 && alert.daysUntilDue <= 90,
    },
  };

  const fetchAlerts = async () => {
    try {
      setLoading(true);
      setError(null);

      const driverCertificationResponse = await apiClient.get('/api/drivers/certification-alerts') as any[];
      const driverAlerts: AlertItem[] = driverCertificationResponse.map((item: any) => ({
        id: item.driverId,
        type: 'driver_certification' as const,
        severity: item.daysUntilExpiry <= 0 ? 'error' : item.daysUntilExpiry <= 30 ? 'warning' : 'info',
        title: `${item.certificationType} Expiring`,
        description: `Driver ${item.driverName}'s ${item.certificationType.toLowerCase()} ${
          item.daysUntilExpiry <= 0 ? 'has expired' : `expires in ${item.daysUntilExpiry} days`
        }`,
        entityId: item.driverId,
        entityName: item.driverName,
        dueDate: item.expiryDate,
        daysUntilDue: item.daysUntilExpiry,
        category: item.certificationType,
        actionRequired: item.daysUntilExpiry <= 30,
      }));

      const vehicleMaintenanceResponse = await apiClient.get('/api/vehicles/maintenance-alerts') as any[];
      const maintenanceAlerts: AlertItem[] = vehicleMaintenanceResponse.map((item: any) => ({
        id: item.vehicleId,
        type: 'vehicle_maintenance' as const,
        severity: item.daysOverdue >= 0 ? 'error' : item.daysUntilDue <= 30 ? 'warning' : 'info',
        title: `${item.maintenanceType} ${item.daysOverdue >= 0 ? 'Overdue' : 'Due'}`,
        description: `Vehicle ${item.vehicleId} (${item.vehicleMake} ${item.vehicleModel}) ${
          item.daysOverdue >= 0 
            ? `maintenance is ${item.daysOverdue} days overdue`
            : `maintenance due in ${item.daysUntilDue} days`
        }`,
        entityId: item.vehicleId,
        entityName: `${item.vehicleMake} ${item.vehicleModel}`,
        dueDate: item.dueDate,
        daysUntilDue: item.daysOverdue >= 0 ? -item.daysOverdue : item.daysUntilDue,
        category: item.maintenanceType,
        actionRequired: item.daysOverdue >= 0 || item.daysUntilDue <= 7,
      }));

      const vehicleInsuranceResponse = await apiClient.get('/api/vehicles/insurance-alerts') as any[];
      const insuranceAlerts: AlertItem[] = vehicleInsuranceResponse.map((item: any) => ({
        id: item.vehicleId,
        type: 'vehicle_insurance' as const,
        severity: item.daysUntilExpiry <= 0 ? 'error' : item.daysUntilExpiry <= 30 ? 'warning' : 'info',
        title: `Insurance ${item.daysUntilExpiry <= 0 ? 'Expired' : 'Expiring'}`,
        description: `Vehicle ${item.vehicleId} insurance ${
          item.daysUntilExpiry <= 0 ? 'has expired' : `expires in ${item.daysUntilExpiry} days`
        }`,
        entityId: item.vehicleId,
        entityName: `${item.vehicleMake} ${item.vehicleModel}`,
        dueDate: item.expiryDate,
        daysUntilDue: item.daysUntilExpiry,
        category: 'Insurance',
        actionRequired: item.daysUntilExpiry <= 30,
      }));

      const vehicleRegistrationResponse = await apiClient.get('/api/vehicles/registration-alerts') as any[];
      const registrationAlerts: AlertItem[] = vehicleRegistrationResponse.map((item: any) => ({
        id: item.vehicleId,
        type: 'vehicle_registration' as const,
        severity: item.daysUntilExpiry <= 0 ? 'error' : item.daysUntilExpiry <= 30 ? 'warning' : 'info',
        title: `Registration ${item.daysUntilExpiry <= 0 ? 'Expired' : 'Expiring'}`,
        description: `Vehicle ${item.vehicleId} registration ${
          item.daysUntilExpiry <= 0 ? 'has expired' : `expires in ${item.daysUntilExpiry} days`
        }`,
        entityId: item.vehicleId,
        entityName: `${item.vehicleMake} ${item.vehicleModel}`,
        dueDate: item.expiryDate,
        daysUntilDue: item.daysUntilExpiry,
        category: 'Registration',
        actionRequired: item.daysUntilExpiry <= 30,
      }));

      const allAlerts = [...driverAlerts, ...maintenanceAlerts, ...insuranceAlerts, ...registrationAlerts]
        .sort((a, b) => {
          const severityOrder = { error: 0, warning: 1, info: 2 };
          if (severityOrder[a.severity] !== severityOrder[b.severity]) {
            return severityOrder[a.severity] - severityOrder[b.severity];
          }
          return a.daysUntilDue - b.daysUntilDue;
        })
        .slice(0, maxItems);

      setAlerts(allAlerts);
      setLastRefresh(new Date());
    } catch (error: any) {
      setError('Failed to load alerts. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAlerts();

    if (autoRefresh) {
      const interval = setInterval(fetchAlerts, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [autoRefresh, refreshInterval, maxItems, fetchAlerts]);

  const getAlertIcon = (alert: AlertItem) => {
    switch (alert.type) {
      case 'driver_certification':
        if (alert.category.toLowerCase().includes('license')) return <LicenseIcon />;
        if (alert.category.toLowerCase().includes('medical')) return <MedicalIcon />;
        if (alert.category.toLowerCase().includes('training')) return <TrainingIcon />;
        return <DriverIcon />;
      case 'vehicle_maintenance':
        return <MaintenanceIcon />;
      case 'vehicle_insurance':
        return <InsuranceIcon />;
      case 'vehicle_registration':
        return <VehicleIcon />;
      default:
        return <InfoIcon />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'error': return 'error';
      case 'warning': return 'warning';
      case 'info': return 'info';
      default: return 'default';
    }
  };

  const getDaysUntilDueChip = (daysUntilDue: number) => {
    if (daysUntilDue <= 0) {
      return <Chip label={`${Math.abs(daysUntilDue)} days overdue`} color="error" size="small" />;
    } else if (daysUntilDue <= 7) {
      return <Chip label={`${daysUntilDue} days left`} color="error" size="small" />;
    } else if (daysUntilDue <= 30) {
      return <Chip label={`${daysUntilDue} days left`} color="warning" size="small" />;
    } else {
      return <Chip label={`${daysUntilDue} days left`} color="info" size="small" />;
    }
  };

  const toggleCategoryExpansion = (category: string) => {
    const newExpanded = new Set(expandedCategories);
    if (newExpanded.has(category)) {
      newExpanded.delete(category);
    } else {
      newExpanded.add(category);
    }
    setExpandedCategories(newExpanded);
  };

  const getAlertsByCategory = (categoryKey: string) => {
    const category = alertCategories[categoryKey as keyof typeof alertCategories];
    return alerts.filter(category.filter);
  };

  const getAlertsByType = (type: string) => {
    return alerts.filter(alert => alert.type === type);
  };

  const alertCounts = {
    critical: getAlertsByCategory('critical').length,
    warning: getAlertsByCategory('warning').length,
    upcoming: getAlertsByCategory('upcoming').length,
    drivers: getAlertsByType('driver_certification').length,
    vehicles: alerts.filter(alert => alert.type.startsWith('vehicle_')).length,
  };

  const renderAlertsList = (alertsToShow: AlertItem[]) => (
    <List>
      {alertsToShow.map((alert, index) => (
        <ListItem
          key={`${alert.type}-${alert.id}-${index}`}
          sx={{
            border: '1px solid',
            borderColor: `${getSeverityColor(alert.severity)}.light`,
            borderRadius: 1,
            mb: 1,
            bgcolor: `${getSeverityColor(alert.severity)}.50`,
            cursor: onAlertClick ? 'pointer' : 'default',
            '&:hover': onAlertClick ? {
              bgcolor: `${getSeverityColor(alert.severity)}.100`,
            } : {},
          }}
          onClick={() => onAlertClick?.(alert)}
        >
          <ListItemIcon>
            {getAlertIcon(alert)}
          </ListItemIcon>
          <ListItemText
            primary={
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography variant="subtitle2" component="span">
                  {alert.title}
                </Typography>
                {alert.actionRequired && (
                  <Chip label="Action Required" color="error" size="small" />
                )}
              </Box>
            }
            secondary={
              <Box>
                <Typography variant="body2" color="text.secondary">
                  {alert.description}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Due: {new Date(alert.dueDate).toLocaleDateString()}
                </Typography>
              </Box>
            }
          />
          <ListItemSecondaryAction>
            {getDaysUntilDueChip(alert.daysUntilDue)}
          </ListItemSecondaryAction>
        </ListItem>
      ))}
    </List>
  );

  if (loading) {
    return (
      <Paper sx={{ p: 3 }}>
        <Typography>Loading alerts...</Typography>
      </Paper>
    );
  }

  if (error) {
    return (
      <Paper sx={{ p: 3 }}>
        <Alert severity="error">
          <AlertTitle>Error Loading Alerts</AlertTitle>
          {error}
          <Button onClick={fetchAlerts} sx={{ mt: 1 }}>
            Retry
          </Button>
        </Alert>
      </Paper>
    );
  }

  return (
    <Paper sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <NotificationsIcon color="primary" />
          <Typography variant="h6">System Alerts</Typography>
          <Badge badgeContent={alerts.length} color="error" />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Last updated: {lastRefresh.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh Alerts">
            <IconButton onClick={fetchAlerts} size="small">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Summary Cards */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Box sx={{ flex: '1 1 300px', minWidth: '200px' }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <ErrorIcon color="error" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4" color="error">
                {alertCounts.critical}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Critical Alerts
              </Typography>
            </CardContent>
          </Card>
        </Box>
        <Box sx={{ flex: '1 1 300px', minWidth: '200px' }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <WarningIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4" color="warning.main">
                {alertCounts.warning}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Warnings
              </Typography>
            </CardContent>
          </Card>
        </Box>
        <Box sx={{ flex: '1 1 300px', minWidth: '200px' }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <InfoIcon color="info" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4" color="info.main">
                {alertCounts.upcoming}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Upcoming
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Tabs for different views */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
          <Tab 
            label={
              <Badge badgeContent={alertCounts.critical + alertCounts.warning} color="error">
                By Priority
              </Badge>
            } 
          />
          <Tab 
            label={
              <Badge badgeContent={alertCounts.drivers} color="primary">
                Driver Certifications
              </Badge>
            } 
          />
          <Tab 
            label={
              <Badge badgeContent={alertCounts.vehicles} color="primary">
                Vehicle Alerts
              </Badge>
            } 
          />
        </Tabs>
      </Box>

      {/* Tab Panels */}
      <TabPanel value={activeTab} index={0}>
        {showCategories ? (
          Object.entries(alertCategories).map(([categoryKey, category]) => {
            const categoryAlerts = getAlertsByCategory(categoryKey);
            if (categoryAlerts.length === 0) return null;

            return (
              <Box key={categoryKey} sx={{ mb: 2 }}>
                <Button
                  onClick={() => toggleCategoryExpansion(categoryKey)}
                  startIcon={category.icon}
                  endIcon={expandedCategories.has(categoryKey) ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  color={category.color}
                  sx={{ mb: 1 }}
                >
                  {category.label} ({categoryAlerts.length})
                </Button>
                <Collapse in={expandedCategories.has(categoryKey)}>
                  {renderAlertsList(categoryAlerts)}
                </Collapse>
                <Divider sx={{ mt: 2 }} />
              </Box>
            );
          })
        ) : (
          renderAlertsList(alerts)
        )}
      </TabPanel>

      <TabPanel value={activeTab} index={1}>
        {renderAlertsList(getAlertsByType('driver_certification'))}
      </TabPanel>

      <TabPanel value={activeTab} index={2}>
        {renderAlertsList(alerts.filter(alert => alert.type.startsWith('vehicle_')))}
      </TabPanel>

      {alerts.length === 0 && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <CheckCircleIcon color="success" sx={{ fontSize: 60, mb: 2 }} />
          <Typography variant="h6" color="success.main">
            No Active Alerts
          </Typography>
          <Typography variant="body2" color="text.secondary">
            All certifications and maintenance are up to date.
          </Typography>
        </Box>
      )}
    </Paper>
  );
};

export default AlertsPanel;
