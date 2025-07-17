import React, { useState, useEffect } from 'react';
import {
  Badge,
  IconButton,
  Menu,
  Typography,
  Box,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Button,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Notifications as NotificationsIcon,
  Info as InfoIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  CheckCircle as SuccessIcon,
  CheckCircle as CheckCircleIcon,
  Build as MaintenanceIcon,
  DirectionsCar as VehicleIcon,
  Person as DriverIcon,
} from '@mui/icons-material';
import { signalRService } from '../services/signalRService';
import { apiClient } from '../services/apiClient';

interface Notification {
  id: string;
  type: 'Info' | 'Warning' | 'Error' | 'Success';
  title: string;
  message: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  isRead: boolean;
  createdAt: string;
}

interface AlertItem {
  id: number;
  type:
    | 'driver_certification'
    | 'vehicle_maintenance'
    | 'vehicle_insurance'
    | 'vehicle_registration';
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

const NotificationCenter: React.FC = () => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [alertsCount, setAlertsCount] = useState(0);
  const [activeTab, setActiveTab] = useState(0);

  useEffect(() => {
    loadNotifications();
    loadAlerts();

    signalRService.startConnection();

    signalRService.onNotificationReceived(() => {
      loadNotifications();
    });

    signalRService.onEmergencyAlert(() => {
      loadNotifications();
    });

    signalRService.onDriverCertificationUpdated(() => {
      loadAlerts(); // Refresh alerts when driver certifications change
    });

    signalRService.onVehicleMaintenanceUpdated(() => {
      loadAlerts(); // Refresh alerts when vehicle maintenance changes
    });

    signalRService.onMaintenanceAlertCreated(() => {
      loadAlerts(); // Refresh alerts when new maintenance alerts are created
    });

    signalRService.onInsuranceExpirationAlert(() => {
      loadAlerts(); // Refresh alerts when insurance expiration alerts are created
    });

    signalRService.onRouteOptimizationUpdate((update: any) => {
      setNotifications(prev => [
        {
          id: `route-${Date.now()}`,
          type: 'Info',
          title: 'Route Optimization Update',
          message: `Route ${update.routeId} has been optimized. New efficiency: ${update.efficiency}%`,
          priority: 'Medium',
          isRead: false,
          createdAt: new Date().toISOString(),
        },
        ...prev,
      ]);
      setUnreadCount(prev => prev + 1);
    });

    signalRService.onGPSLocationUpdate((update: any) => {
      if (
        update.alertType === 'geofence_violation' ||
        update.alertType === 'emergency'
      ) {
        setNotifications(prev => [
          {
            id: `gps-${Date.now()}`,
            type: update.alertType === 'emergency' ? 'Error' : 'Warning',
            title:
              update.alertType === 'emergency'
                ? 'Emergency GPS Alert'
                : 'Geofence Violation',
            message:
              update.message || `Vehicle ${update.vehicleId} location alert`,
            priority: update.alertType === 'emergency' ? 'Critical' : 'High',
            isRead: false,
            createdAt: new Date().toISOString(),
          },
          ...prev,
        ]);
        setUnreadCount(prev => prev + 1);
      }
    });

    signalRService.onAttendanceMethodUpdate((update: any) => {
      setNotifications(prev => [
        {
          id: `attendance-${Date.now()}`,
          type: 'Info',
          title: 'Attendance Update',
          message: `Student attendance recorded via ${update.method}. Status: ${update.status}`,
          priority: 'Low',
          isRead: false,
          createdAt: new Date().toISOString(),
        },
        ...prev,
      ]);
      setUnreadCount(prev => prev + 1);
    });

    signalRService.onPaymentStatusUpdate((update: any) => {
      const isError =
        update.status === 'failed' || update.status === 'declined';
      setNotifications(prev => [
        {
          id: `payment-${Date.now()}`,
          type: isError ? 'Error' : 'Success',
          title: `Payment ${update.status}`,
          message: `Payment of $${update.amount} ${update.status}${update.reason ? `: ${update.reason}` : ''}`,
          priority: isError ? 'High' : 'Medium',
          isRead: false,
          createdAt: new Date().toISOString(),
        },
        ...prev,
      ]);
      setUnreadCount(prev => prev + 1);
    });

    if (Notification.permission === 'default') {
      Notification.requestPermission();
    }

    return () => {
      signalRService.stopConnection();
    };
  }, []);

  const loadNotifications = async () => {
    try {
      const response = await apiClient.get<{
        data: Notification[];
        total: number;
      }>('/api/notifications');
      const notifications = Array.isArray(response?.data) ? response.data : [];
      setNotifications(notifications);
      setUnreadCount(
        notifications.filter((n: Notification) => !n.isRead).length
      );
    } catch {}
  };

  const loadAlerts = async () => {
    try {
      const [
        driverCertificationResponse,
        vehicleMaintenanceResponse,
        vehicleInsuranceResponse,
        vehicleRegistrationResponse,
      ] = await Promise.all([
        apiClient.get('/api/drivers/certification-alerts') as Promise<any[]>,
        apiClient.get('/api/vehicles/maintenance-alerts') as Promise<any[]>,
        apiClient.get('/api/vehicles/insurance-alerts') as Promise<any[]>,
        apiClient.get('/api/vehicles/registration-alerts') as Promise<any[]>,
      ]);

      const driverAlerts: AlertItem[] = driverCertificationResponse.map(
        (item: any) => ({
          id: item.driverId,
          type: 'driver_certification' as const,
          severity:
            item.daysUntilExpiry <= 0
              ? 'error'
              : item.daysUntilExpiry <= 30
                ? 'warning'
                : 'info',
          title: `${item.certificationType} Expiring`,
          description: `Driver ${item.driverName}'s ${item.certificationType.toLowerCase()} ${
            item.daysUntilExpiry <= 0
              ? 'has expired'
              : `expires in ${item.daysUntilExpiry} days`
          }`,
          entityId: item.driverId,
          entityName: item.driverName,
          dueDate: item.expiryDate,
          daysUntilDue: item.daysUntilExpiry,
          category: item.certificationType,
          actionRequired: item.daysUntilExpiry <= 30,
        })
      );

      // Transform vehicle maintenance alerts
      const maintenanceAlerts: AlertItem[] = vehicleMaintenanceResponse.map(
        (item: any) => ({
          id: item.vehicleId,
          type: 'vehicle_maintenance' as const,
          severity:
            item.daysOverdue >= 0
              ? 'error'
              : item.daysUntilDue <= 30
                ? 'warning'
                : 'info',
          title: `${item.maintenanceType} ${item.daysOverdue >= 0 ? 'Overdue' : 'Due'}`,
          description: `Vehicle ${item.vehicleId} maintenance ${
            item.daysOverdue >= 0
              ? `is ${item.daysOverdue} days overdue`
              : `due in ${item.daysUntilDue} days`
          }`,
          entityId: item.vehicleId,
          entityName: `${item.vehicleMake} ${item.vehicleModel}`,
          dueDate: item.dueDate,
          daysUntilDue:
            item.daysOverdue >= 0 ? -item.daysOverdue : item.daysUntilDue,
          category: item.maintenanceType,
          actionRequired: item.daysOverdue >= 0 || item.daysUntilDue <= 7,
        })
      );

      // Transform vehicle insurance alerts
      const insuranceAlerts: AlertItem[] = vehicleInsuranceResponse.map(
        (item: any) => ({
          id: item.vehicleId,
          type: 'vehicle_insurance' as const,
          severity:
            item.daysUntilExpiry <= 0
              ? 'error'
              : item.daysUntilExpiry <= 30
                ? 'warning'
                : 'info',
          title: `Insurance ${item.daysUntilExpiry <= 0 ? 'Expired' : 'Expiring'}`,
          description: `Vehicle ${item.vehicleId} insurance ${
            item.daysUntilExpiry <= 0
              ? 'has expired'
              : `expires in ${item.daysUntilExpiry} days`
          }`,
          entityId: item.vehicleId,
          entityName: `${item.vehicleMake} ${item.vehicleModel}`,
          dueDate: item.expiryDate,
          daysUntilDue: item.daysUntilExpiry,
          category: 'Insurance',
          actionRequired: item.daysUntilExpiry <= 30,
        })
      );

      const registrationAlerts: AlertItem[] = vehicleRegistrationResponse.map(
        (item: any) => ({
          id: item.vehicleId,
          type: 'vehicle_registration' as const,
          severity:
            item.daysUntilExpiry <= 0
              ? 'error'
              : item.daysUntilExpiry <= 30
                ? 'warning'
                : 'info',
          title: `Registration ${item.daysUntilExpiry <= 0 ? 'Expired' : 'Expiring'}`,
          description: `Vehicle ${item.vehicleId} registration ${
            item.daysUntilExpiry <= 0
              ? 'has expired'
              : `expires in ${item.daysUntilExpiry} days`
          }`,
          entityId: item.vehicleId,
          entityName: `${item.vehicleMake} ${item.vehicleModel}`,
          dueDate: item.expiryDate,
          daysUntilDue: item.daysUntilExpiry,
          category: 'Registration',
          actionRequired: item.daysUntilExpiry <= 30,
        })
      );

      const allAlerts = [
        ...driverAlerts,
        ...maintenanceAlerts,
        ...insuranceAlerts,
        ...registrationAlerts,
      ].sort((a, b) => {
        const severityOrder = { error: 0, warning: 1, info: 2 };
        if (severityOrder[a.severity] !== severityOrder[b.severity]) {
          return severityOrder[a.severity] - severityOrder[b.severity];
        }
        return a.daysUntilDue - b.daysUntilDue;
      });

      setAlerts(allAlerts);
      setAlertsCount(
        allAlerts.filter(
          alert => alert.severity === 'error' || alert.actionRequired
        ).length
      );
    } catch {}
  };

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const markAsRead = async (notificationId: string) => {
    try {
      await apiClient.put(`/api/notifications/${notificationId}/read`);
      setNotifications(prev =>
        prev.map(n => (n.id === notificationId ? { ...n, isRead: true } : n))
      );
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch {}
  };

  const markAllAsRead = async () => {
    try {
      await apiClient.put('/api/notifications/mark-all-read');
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch {}
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'Warning':
        return <WarningIcon color="warning" />;
      case 'Error':
        return <ErrorIcon color="error" />;
      case 'Success':
        return <SuccessIcon color="success" />;
      default:
        return <InfoIcon color="info" />;
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Critical':
        return 'error';
      case 'High':
        return 'warning';
      case 'Medium':
        return 'info';
      default:
        return 'default';
    }
  };

  const getAlertIcon = (alert: AlertItem) => {
    switch (alert.type) {
      case 'driver_certification':
        return <DriverIcon />;
      case 'vehicle_maintenance':
        return <MaintenanceIcon />;
      case 'vehicle_insurance':
      case 'vehicle_registration':
        return <VehicleIcon />;
      default:
        return <InfoIcon />;
    }
  };

  const getAlertSeverityColor = (severity: string) => {
    switch (severity) {
      case 'error':
        return 'error';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'default';
    }
  };

  const getDaysUntilDueChip = (daysUntilDue: number) => {
    if (daysUntilDue <= 0) {
      return (
        <Chip
          label={`${Math.abs(daysUntilDue)} days overdue`}
          color="error"
          size="small"
        />
      );
    } else if (daysUntilDue <= 7) {
      return (
        <Chip label={`${daysUntilDue} days left`} color="error" size="small" />
      );
    } else if (daysUntilDue <= 30) {
      return (
        <Chip
          label={`${daysUntilDue} days left`}
          color="warning"
          size="small"
        />
      );
    } else {
      return (
        <Chip label={`${daysUntilDue} days left`} color="info" size="small" />
      );
    }
  };

  return (
    <>
      <IconButton color="inherit" onClick={handleClick} sx={{ ml: 1 }}>
        <Badge badgeContent={unreadCount + alertsCount} color="error">
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        PaperProps={{
          sx: {
            width: 450,
            maxHeight: 600,
          },
        }}
      >
        <Box
          sx={{
            p: 2,
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <Typography variant="h6">Notifications & Alerts</Typography>
          {unreadCount > 0 && (
            <Button size="small" onClick={markAllAsRead}>
              Mark all as read
            </Button>
          )}
        </Box>

        <Tabs
          value={activeTab}
          onChange={(_, newValue) => setActiveTab(newValue)}
          variant="fullWidth"
        >
          <Tab
            label={
              <Badge badgeContent={unreadCount} color="error">
                Notifications
              </Badge>
            }
          />
          <Tab
            label={
              <Badge badgeContent={alertsCount} color="warning">
                System Alerts
              </Badge>
            }
          />
        </Tabs>
        <Divider />

        {/* Notifications Tab */}
        {activeTab === 0 && (
          <>
            {notifications.length === 0 ? (
              <Box sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">
                  No notifications yet
                </Typography>
              </Box>
            ) : (
              <List sx={{ maxHeight: 400, overflow: 'auto' }}>
                {notifications.slice(0, 10).map(notification => (
                  <ListItem
                    key={notification.id}
                    onClick={() =>
                      !notification.isRead && markAsRead(notification.id)
                    }
                    sx={{
                      cursor: notification.isRead ? 'default' : 'pointer',
                      backgroundColor: notification.isRead
                        ? 'transparent'
                        : 'action.hover',
                      '&:hover': {
                        backgroundColor: 'action.selected',
                      },
                    }}
                  >
                    <ListItemIcon>
                      {getNotificationIcon(notification.type)}
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box
                          sx={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                          }}
                        >
                          <Typography
                            variant="subtitle2"
                            sx={{
                              fontWeight: notification.isRead
                                ? 'normal'
                                : 'bold',
                            }}
                          >
                            {notification.title}
                          </Typography>
                          <Chip
                            label={notification.priority}
                            size="small"
                            color={
                              getPriorityColor(notification.priority) as any
                            }
                            variant="outlined"
                          />
                        </Box>
                      }
                      secondary={
                        <Box>
                          <Typography variant="body2" color="text.secondary">
                            {notification.message}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {new Date(notification.createdAt).toLocaleString()}
                          </Typography>
                        </Box>
                      }
                    />
                  </ListItem>
                ))}
              </List>
            )}
          </>
        )}

        {/* System Alerts Tab */}
        {activeTab === 1 && (
          <>
            {alerts.length === 0 ? (
              <Box sx={{ p: 3, textAlign: 'center' }}>
                <CheckCircleIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
                <Typography color="success.main" variant="subtitle1">
                  All Systems Normal
                </Typography>
                <Typography color="text.secondary" variant="body2">
                  No certification or maintenance alerts
                </Typography>
              </Box>
            ) : (
              <List sx={{ maxHeight: 400, overflow: 'auto' }}>
                {alerts.slice(0, 15).map((alert, index) => (
                  <ListItem
                    key={`${alert.type}-${alert.id}-${index}`}
                    sx={{
                      border: '1px solid',
                      borderColor: `${getAlertSeverityColor(alert.severity)}.light`,
                      borderRadius: 1,
                      mb: 1,
                      mx: 1,
                      bgcolor: `${getAlertSeverityColor(alert.severity)}.50`,
                    }}
                  >
                    <ListItemIcon>{getAlertIcon(alert)}</ListItemIcon>
                    <ListItemText
                      primary={
                        <Box
                          sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                        >
                          <Typography variant="subtitle2" component="span">
                            {alert.title}
                          </Typography>
                          {alert.actionRequired && (
                            <Chip
                              label="Action Required"
                              color="error"
                              size="small"
                            />
                          )}
                        </Box>
                      }
                      secondary={
                        <Box>
                          <Typography variant="body2" color="text.secondary">
                            {alert.description}
                          </Typography>
                          <Box
                            sx={{
                              display: 'flex',
                              justifyContent: 'space-between',
                              alignItems: 'center',
                              mt: 0.5,
                            }}
                          >
                            <Typography
                              variant="caption"
                              color="text.secondary"
                            >
                              Due:{' '}
                              {new Date(alert.dueDate).toLocaleDateString()}
                            </Typography>
                            {getDaysUntilDueChip(alert.daysUntilDue)}
                          </Box>
                        </Box>
                      }
                    />
                  </ListItem>
                ))}
              </List>
            )}
          </>
        )}
      </Menu>
    </>
  );
};

export default NotificationCenter;
