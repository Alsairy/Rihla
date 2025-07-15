import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Alert,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Paper,
  Divider,
  IconButton,
  Tooltip,
  Badge,
} from '@mui/material';
import {
  LocationOn,
  Warning,
  Error,
  CheckCircle,
  Notifications,
  NotificationsOff,
  DirectionsCar,
  Person,
  Schedule,
  Refresh,
  Settings,
  Phone,
  Clear,
  FilterList,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface GeofenceAlert {
  id: string;
  studentId: number;
  studentName: string;
  tripId: number;
  stopId: number;
  stopName: string;
  alertType: string;
  message: string;
  latitude: number;
  longitude: number;
  distance: number;
  timestamp: string;
  severity: 'Low' | 'Medium' | 'High';
  acknowledged: boolean;
  actionTaken?: string;
}

interface Trip {
  id: number;
  routeName: string;
  vehicleName: string;
  driverName: string;
  status: string;
}

interface AlertSettings {
  enableNotifications: boolean;
  alertRadius: number;
  autoAcknowledge: boolean;
  soundEnabled: boolean;
  severityFilter: 'All' | 'High' | 'Medium' | 'Low';
}

const GeofenceAlertPanel: React.FC = () => {
  const [alerts, setAlerts] = useState<GeofenceAlert[]>([]);
  const [trips, setTrips] = useState<Trip[]>([]);
  const [selectedTrip, setSelectedTrip] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selectedAlert, setSelectedAlert] = useState<GeofenceAlert | null>(
    null
  );
  const [alertDialog, setAlertDialog] = useState(false);
  const [settingsDialog, setSettingsDialog] = useState(false);
  const [alertSettings, setAlertSettings] = useState<AlertSettings>({
    enableNotifications: true,
    alertRadius: 500,
    autoAcknowledge: false,
    soundEnabled: true,
    severityFilter: 'All',
  });

  useEffect(() => {
    loadInitialData();
    loadAlertSettings();

    const interval = window.setInterval(() => {
      if (selectedTrip && alertSettings.enableNotifications) {
        checkGeofenceAlerts();
      }
    }, 10000); // Check every 10 seconds

    return () => window.clearInterval(interval);
  }, [selectedTrip, alertSettings.enableNotifications]);

  const loadInitialData = async () => {
    setLoading(true);
    try {
      const tripsResponse = await apiClient.get('/api/trips/active');
      setTrips((tripsResponse as any).data?.items || []);

      if ((tripsResponse as any).data?.items?.length > 0) {
        setSelectedTrip((tripsResponse as any).data.items[0].id);
        await loadAlertsForTrip((tripsResponse as any).data.items[0].id);
      }
    } catch {
      setError('Failed to load initial data');
    } finally {
      setLoading(false);
    }
  };

  const loadAlertsForTrip = async (tripId: number) => {
    try {
      const response = await apiClient.get(`/api/geofence/alerts/${tripId}`);
      setAlerts((response as any).data || []);
    } catch {
      setError('Failed to load alerts');
    }
  };

  const checkGeofenceAlerts = async () => {
    if (!selectedTrip) return;

    try {
      const locationResponse = await apiClient.get(
        `/api/trips/${selectedTrip}/current-location`
      );
      const location = (locationResponse as any).data;

      if (location) {
        const response = await apiClient.post(
          '/api/attendance/geofence-alerts',
          {
            tripId: selectedTrip,
            latitude: location.latitude,
            longitude: location.longitude,
          }
        );

        const newAlerts = (response as any).data || [];
        if (newAlerts.length > 0) {
          setAlerts(prev => {
            const existingIds = prev.map(a => a.id);
            const uniqueNewAlerts = newAlerts.filter(
              (alert: GeofenceAlert) => !existingIds.includes(alert.id)
            );
            return [...prev, ...uniqueNewAlerts];
          });

          if (alertSettings.soundEnabled) {
            const highSeverityAlerts = newAlerts.filter(
              (alert: GeofenceAlert) => alert.severity === 'High'
            );
            if (highSeverityAlerts.length > 0) {
              playAlertSound();
            }
          }

          if (alertSettings.autoAcknowledge) {
            newAlerts.forEach((alert: GeofenceAlert) => {
              acknowledgeAlert(alert.id);
            });
          }
        }
      }
    } catch {
      setError('Failed to check geofence alerts');
    }
  };

  const acknowledgeAlert = async (alertId: string) => {
    try {
      await apiClient.post(`/api/geofence/alerts/${alertId}/acknowledge`);

      setAlerts(prev =>
        prev.map(alert =>
          alert.id === alertId
            ? {
                ...alert,
                acknowledged: true,
                actionTaken: 'Acknowledged by operator',
              }
            : alert
        )
      );

      setSuccess('Alert acknowledged successfully');
    } catch {
      setError('Failed to acknowledge alert');
    }
  };

  const dismissAlert = (alertId: string) => {
    setAlerts(prev => prev.filter(alert => alert.id !== alertId));
    setSuccess('Alert dismissed');
  };

  const contactParent = async (studentId: number) => {
    try {
      await apiClient.post(`/api/notifications/contact-parent`, {
        studentId,
        message:
          'Your child has not boarded the school bus at the designated stop. Please contact the school immediately.',
        urgency: 'High',
      });

      setSuccess('Parent has been notified');
    } catch {
      setError('Failed to contact parent');
    }
  };


  const loadAlertSettings = () => {
    const saved = localStorage.getItem('geofenceAlertSettings');
    if (saved) {
      setAlertSettings(JSON.parse(saved));
    }
  };

  const saveAlertSettings = (settings: AlertSettings) => {
    setAlertSettings(settings);
    localStorage.setItem('geofenceAlertSettings', JSON.stringify(settings));
    setSuccess('Settings saved successfully');
  };

  const playAlertSound = () => {
    const audio = new Audio('/alert-sound.mp3');
    audio.play().catch(() => {
    });
  };

  const getFilteredAlerts = () => {
    let filtered = alerts;

    if (alertSettings.severityFilter !== 'All') {
      filtered = filtered.filter(
        alert => alert.severity === alertSettings.severityFilter
      );
    }

    return filtered.sort((a, b) => {
      const severityOrder = { High: 3, Medium: 2, Low: 1 };
      const severityDiff =
        severityOrder[b.severity] - severityOrder[a.severity];
      if (severityDiff !== 0) return severityDiff;

      return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime();
    });
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High':
        return 'error';
      case 'Medium':
        return 'warning';
      case 'Low':
        return 'info';
      default:
        return 'default';
    }
  };

  const getAlertIcon = (alertType: string) => {
    switch (alertType) {
      case 'Student Not Boarded':
        return <Person />;
      case 'Vehicle Delayed':
        return <Schedule />;
      case 'Route Deviation':
        return <DirectionsCar />;
      default:
        return <Warning />;
    }
  };

  const getUnacknowledgedCount = () => {
    return alerts.filter(alert => !alert.acknowledged).length;
  };

  const getHighSeverityCount = () => {
    return alerts.filter(
      alert => alert.severity === 'High' && !alert.acknowledged
    ).length;
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
        <Typography variant="h4">Geofence Alert Panel</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Badge badgeContent={getUnacknowledgedCount()} color="error">
            <Button
              variant={
                alertSettings.enableNotifications ? 'contained' : 'outlined'
              }
              startIcon={
                alertSettings.enableNotifications ? (
                  <Notifications />
                ) : (
                  <NotificationsOff />
                )
              }
              onClick={() =>
                saveAlertSettings({
                  ...alertSettings,
                  enableNotifications: !alertSettings.enableNotifications,
                })
              }
            >
              {alertSettings.enableNotifications ? 'Enabled' : 'Disabled'}
            </Button>
          </Badge>
          <Button
            variant="outlined"
            startIcon={<Settings />}
            onClick={() => setSettingsDialog(true)}
          >
            Settings
          </Button>
        </Box>
      </Box>

      {/* Alert Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Warning color="primary" />
                <Box>
                  <Typography variant="h6">{alerts.length}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Alerts
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Error color="error" />
                <Box>
                  <Typography variant="h6">{getHighSeverityCount()}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    High Priority
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Schedule color="warning" />
                <Box>
                  <Typography variant="h6">
                    {getUnacknowledgedCount()}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Unacknowledged
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <CheckCircle color="success" />
                <Box>
                  <Typography variant="h6">
                    {alerts.filter(a => a.acknowledged).length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Resolved
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Trip Selection and Controls */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Select Trip</InputLabel>
                <Select
                  value={selectedTrip || ''}
                  onChange={e => {
                    const tripId = Number(e.target.value);
                    setSelectedTrip(tripId);
                    loadAlertsForTrip(tripId);
                  }}
                >
                  {trips.map(trip => (
                    <MenuItem key={trip.id} value={trip.id}>
                      {trip.routeName} - {trip.vehicleName} ({trip.driverName})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="outlined"
                  startIcon={<Refresh />}
                  onClick={() =>
                    selectedTrip && loadAlertsForTrip(selectedTrip)
                  }
                  disabled={loading}
                >
                  Refresh
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<FilterList />}
                  onClick={() => setSettingsDialog(true)}
                >
                  Filter
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<Clear />}
                  onClick={() => setAlerts([])}
                  disabled={alerts.length === 0}
                >
                  Clear All
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Alerts List */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Active Alerts ({getFilteredAlerts().length})
          </Typography>
          {getFilteredAlerts().length === 0 ? (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                py: 4,
              }}
            >
              <CheckCircle color="success" sx={{ mr: 1 }} />
              <Typography color="text.secondary">
                No active alerts for selected trip
              </Typography>
            </Box>
          ) : (
            <List>
              {getFilteredAlerts().map((alert, index) => (
                <React.Fragment key={alert.id}>
                  <ListItem>
                    <ListItemIcon>{getAlertIcon(alert.alertType)}</ListItemIcon>
                    <ListItemText
                      primary={
                        <Box
                          sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                        >
                          <Typography variant="subtitle1">
                            {alert.alertType}
                          </Typography>
                          <Chip
                            label={alert.severity}
                            color={getSeverityColor(alert.severity) as any}
                            size="small"
                          />
                          {alert.acknowledged && (
                            <Chip
                              label="Acknowledged"
                              color="success"
                              size="small"
                            />
                          )}
                        </Box>
                      }
                      secondary={
                        <Box>
                          <Typography variant="body2">
                            {alert.message}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            Student: {alert.studentName} • Stop:{' '}
                            {alert.stopName} • Distance:{' '}
                            {alert.distance.toFixed(0)}m •
                            {new Date(alert.timestamp).toLocaleString()}
                          </Typography>
                        </Box>
                      }
                    />
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="View Details">
                        <IconButton
                          size="small"
                          onClick={() => {
                            setSelectedAlert(alert);
                            setAlertDialog(true);
                          }}
                        >
                          <LocationOn />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Contact Parent">
                        <IconButton
                          size="small"
                          onClick={() => contactParent(alert.studentId)}
                          color="primary"
                        >
                          <Phone />
                        </IconButton>
                      </Tooltip>
                      {!alert.acknowledged && (
                        <Tooltip title="Acknowledge">
                          <IconButton
                            size="small"
                            onClick={() => acknowledgeAlert(alert.id)}
                            color="success"
                          >
                            <CheckCircle />
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title="Dismiss">
                        <IconButton
                          size="small"
                          onClick={() => dismissAlert(alert.id)}
                          color="error"
                        >
                          <Clear />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </ListItem>
                  {index < getFilteredAlerts().length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Alert Details Dialog */}
      <Dialog
        open={alertDialog}
        onClose={() => setAlertDialog(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Alert Details</DialogTitle>
        <DialogContent>
          {selectedAlert && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Alert severity={getSeverityColor(selectedAlert.severity) as any}>
                <Typography variant="h6">{selectedAlert.alertType}</Typography>
                <Typography variant="body1">{selectedAlert.message}</Typography>
              </Alert>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Student Information
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Student Name
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.studentName}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Student ID
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.studentId}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Location Details
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Stop Name
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.stopName}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Distance from Stop
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.distance.toFixed(0)}m
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Coordinates
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.latitude.toFixed(6)},{' '}
                      {selectedAlert.longitude.toFixed(6)}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Time
                    </Typography>
                    <Typography variant="body1">
                      {new Date(selectedAlert.timestamp).toLocaleString()}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Alert Status
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Chip
                    label={selectedAlert.severity}
                    color={getSeverityColor(selectedAlert.severity) as any}
                    size="small"
                  />
                  <Chip
                    label={
                      selectedAlert.acknowledged ? 'Acknowledged' : 'Pending'
                    }
                    color={selectedAlert.acknowledged ? 'success' : 'warning'}
                    size="small"
                  />
                </Box>
                {selectedAlert.actionTaken && (
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                      Action Taken
                    </Typography>
                    <Typography variant="body1">
                      {selectedAlert.actionTaken}
                    </Typography>
                  </Box>
                )}
              </Paper>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAlertDialog(false)}>Close</Button>
          {selectedAlert && !selectedAlert.acknowledged && (
            <Button
              variant="contained"
              color="primary"
              onClick={() => {
                acknowledgeAlert(selectedAlert.id);
                setAlertDialog(false);
              }}
            >
              Acknowledge
            </Button>
          )}
          {selectedAlert && (
            <Button
              variant="contained"
              color="secondary"
              startIcon={<Phone />}
              onClick={() => {
                contactParent(selectedAlert.studentId);
                setAlertDialog(false);
              }}
            >
              Contact Parent
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Settings Dialog */}
      <Dialog
        open={settingsDialog}
        onClose={() => setSettingsDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Alert Settings</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, pt: 1 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={alertSettings.enableNotifications}
                  onChange={e =>
                    setAlertSettings(prev => ({
                      ...prev,
                      enableNotifications: e.target.checked,
                    }))
                  }
                />
              }
              label="Enable Notifications"
            />

            <FormControlLabel
              control={
                <Switch
                  checked={alertSettings.soundEnabled}
                  onChange={e =>
                    setAlertSettings(prev => ({
                      ...prev,
                      soundEnabled: e.target.checked,
                    }))
                  }
                />
              }
              label="Sound Alerts"
            />

            <FormControlLabel
              control={
                <Switch
                  checked={alertSettings.autoAcknowledge}
                  onChange={e =>
                    setAlertSettings(prev => ({
                      ...prev,
                      autoAcknowledge: e.target.checked,
                    }))
                  }
                />
              }
              label="Auto-acknowledge Low Priority Alerts"
            />

            <FormControl fullWidth>
              <InputLabel>Alert Radius (meters)</InputLabel>
              <Select
                value={alertSettings.alertRadius}
                onChange={e =>
                  setAlertSettings(prev => ({
                    ...prev,
                    alertRadius: Number(e.target.value),
                  }))
                }
              >
                <MenuItem value={200}>200m</MenuItem>
                <MenuItem value={500}>500m</MenuItem>
                <MenuItem value={1000}>1000m</MenuItem>
                <MenuItem value={2000}>2000m</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel>Severity Filter</InputLabel>
              <Select
                value={alertSettings.severityFilter}
                onChange={e =>
                  setAlertSettings(prev => ({
                    ...prev,
                    severityFilter: e.target.value as any,
                  }))
                }
              >
                <MenuItem value="All">All Alerts</MenuItem>
                <MenuItem value="High">High Priority Only</MenuItem>
                <MenuItem value="Medium">Medium &amp; High</MenuItem>
                <MenuItem value="Low">Low Priority Only</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSettingsDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => {
              saveAlertSettings(alertSettings);
              setSettingsDialog(false);
            }}
          >
            Save Settings
          </Button>
        </DialogActions>
      </Dialog>

      {/* Status Messages */}
      {error && (
        <Alert severity="error" sx={{ mt: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert
          severity="success"
          sx={{ mt: 2 }}
          onClose={() => setSuccess(null)}
        >
          {success}
        </Alert>
      )}
    </Box>
  );
};

export default GeofenceAlertPanel;
