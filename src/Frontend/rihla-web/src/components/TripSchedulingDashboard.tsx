import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Alert,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Tooltip,
  Badge,
  Divider,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
} from '@mui/material';
import {
  Schedule as ScheduleIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Edit as EditIcon,
  Notifications as NotificationsIcon,
  Refresh as RefreshIcon,
  CalendarToday as CalendarIcon,
  DirectionsBus as BusIcon,
  Person as PersonIcon,
  Route as RouteIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface Trip {
  id: number;
  routeId: number;
  vehicleId: number;
  driverId: number;
  tripDate: string;
  type: string;
  status: string;
  scheduledStartTime: string;
  scheduledEndTime: string;
  route: {
    id: number;
    routeNumber: string;
    name: string;
  };
  vehicle: {
    id: number;
    vehicleNumber: string;
    make: string;
    model: string;
  };
  driver: {
    id: number;
    firstName: string;
    lastName: string;
    employeeNumber: string;
  };
}

interface ResourceConflict {
  conflictType: string;
  resourceId: number;
  resourceName: string;
  conflictingTripIds: number[];
  conflictTime: string;
  description: string;
  severity: string;
}

interface DailyTripSchedule {
  scheduleDate: string;
  totalRoutes: number;
  scheduledTrips: Trip[];
  unscheduledRoutes: any[];
  conflicts: ResourceConflict[];
}

interface TripSchedulingDashboardProps {
  selectedDate?: Date;
  onDateChange?: (date: Date) => void;
}

export const TripSchedulingDashboard: React.FC<
  TripSchedulingDashboardProps
> = ({ selectedDate = new Date(), onDateChange }) => {
  const [schedule, setSchedule] = useState<DailyTripSchedule | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedTrip, setSelectedTrip] = useState<Trip | null>(null);
  const [newStartTime, setNewStartTime] = useState('');
  const [newEndTime, setNewEndTime] = useState('');

  useEffect(() => {
    loadDailySchedule();
  }, [selectedDate]);

  const loadDailySchedule = async () => {
    try {
      setLoading(true);
      setError(null);

      const dateStr = selectedDate.toISOString().split('T')[0];
      const response = await apiClient.get(
        `/api/trips/daily-schedule?date=${dateStr}`
      );
      setSchedule(response as DailyTripSchedule);
    } catch (err: any) {
      setError(err.message || 'Failed to load daily schedule');
    } finally {
      setLoading(false);
    }
  };

  const handleDateChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newDate = new Date(event.target.value);
    if (!isNaN(newDate.getTime())) {
      onDateChange?.(newDate);
    }
  };

  const handleEditTrip = (trip: Trip) => {
    setSelectedTrip(trip);
    setNewStartTime(trip.scheduledStartTime.substring(11, 16));
    setNewEndTime(trip.scheduledEndTime.substring(11, 16));
    setEditDialogOpen(true);
  };

  const handleSaveTrip = async () => {
    if (!selectedTrip) return;

    try {
      const startDateTime = `${selectedDate.toISOString().split('T')[0]}T${newStartTime}:00`;
      const endDateTime = `${selectedDate.toISOString().split('T')[0]}T${newEndTime}:00`;

      await apiClient.put(`/api/trips/${selectedTrip.id}/adjust-timing`, {
        newStartTime: startDateTime,
        newEndTime: endDateTime,
      });

      setEditDialogOpen(false);
      loadDailySchedule();
    } catch (err: any) {
      setError(err.message || 'Failed to update trip timing');
    }
  };

  const handleSendNotifications = async (tripId: number) => {
    try {
      await apiClient.post(`/api/trips/${tripId}/send-notifications`);
      alert('Notifications sent successfully');
    } catch (err: any) {
      setError(err.message || 'Failed to send notifications');
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'error';
      case 'medium':
        return 'warning';
      case 'low':
        return 'info';
      default:
        return 'default';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'success';
      case 'in_progress':
        return 'info';
      case 'scheduled':
        return 'primary';
      case 'cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  const formatTime = (dateTimeString: string) => {
    return new Date(dateTimeString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: 400,
        }}
      >
        <CircularProgress />
        <Typography sx={{ ml: 2 }}>Loading daily schedule...</Typography>
      </Box>
    );
  }

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
        <Typography variant="h4" component="h1">
          <ScheduleIcon sx={{ mr: 2, verticalAlign: 'middle' }} />
          Trip Scheduling Dashboard
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            label="Select Date"
            type="date"
            value={selectedDate.toISOString().split('T')[0]}
            onChange={handleDateChange}
            InputLabelProps={{ shrink: true }}
          />
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadDailySchedule}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {schedule && (
        <>
          {/* Summary Cards */}
          <Grid container spacing={3} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12, md: 3 }}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <RouteIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4">{schedule.totalRoutes}</Typography>
                  <Typography variant="body2" color="textSecondary">
                    Total Routes
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <BusIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4">
                    {schedule.scheduledTrips.length}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Scheduled Trips
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <WarningIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4">
                    {schedule.unscheduledRoutes.length}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Unscheduled Routes
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <ErrorIcon color="error" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4">
                    {schedule.conflicts.length}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Conflicts
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {/* Conflicts Section */}
          {schedule.conflicts.length > 0 && (
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom color="error">
                  <WarningIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Resource Conflicts ({schedule.conflicts.length})
                </Typography>
                <List>
                  {schedule.conflicts.map((conflict, index) => (
                    <ListItem key={index} divider>
                      <ListItemText
                        primary={
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: 1,
                            }}
                          >
                            <Chip
                              label={conflict.conflictType}
                              color={getSeverityColor(conflict.severity) as any}
                              size="small"
                            />
                            <Typography variant="subtitle1">
                              {conflict.resourceName}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          <Box>
                            <Typography variant="body2" color="textSecondary">
                              {conflict.description}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              Conflict Time: {formatTime(conflict.conflictTime)}
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}

          {/* Scheduled Trips Table */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <CalendarIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Scheduled Trips ({schedule.scheduledTrips.length})
              </Typography>
              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Route</TableCell>
                      <TableCell>Vehicle</TableCell>
                      <TableCell>Driver</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Start Time</TableCell>
                      <TableCell>End Time</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {schedule.scheduledTrips.map(trip => (
                      <TableRow key={trip.id}>
                        <TableCell>
                          <Box>
                            <Typography variant="body2" fontWeight="bold">
                              {trip.route.routeNumber}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              {trip.route.name}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box>
                            <Typography variant="body2">
                              {trip.vehicle.vehicleNumber}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              {trip.vehicle.make} {trip.vehicle.model}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box>
                            <Typography variant="body2">
                              {trip.driver.firstName} {trip.driver.lastName}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              {trip.driver.employeeNumber}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={trip.type}
                            size="small"
                            color={
                              trip.type === 'PickUp' ? 'primary' : 'secondary'
                            }
                          />
                        </TableCell>
                        <TableCell>
                          {formatTime(trip.scheduledStartTime)}
                        </TableCell>
                        <TableCell>
                          {formatTime(trip.scheduledEndTime)}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={trip.status}
                            size="small"
                            color={getStatusColor(trip.status) as any}
                          />
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <Tooltip title="Edit Timing">
                              <IconButton
                                size="small"
                                onClick={() => handleEditTrip(trip)}
                              >
                                <EditIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Send Notifications">
                              <IconButton
                                size="small"
                                onClick={() => handleSendNotifications(trip.id)}
                              >
                                <NotificationsIcon />
                              </IconButton>
                            </Tooltip>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>

          {/* Unscheduled Routes */}
          {schedule.unscheduledRoutes.length > 0 && (
            <Card sx={{ mt: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom color="warning.main">
                  <WarningIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Unscheduled Routes ({schedule.unscheduledRoutes.length})
                </Typography>
                <List>
                  {schedule.unscheduledRoutes.map((route, index) => (
                    <ListItem key={index} divider>
                      <ListItemText
                        primary={`${route.routeNumber} - ${route.name}`}
                        secondary="No trips scheduled for this route"
                      />
                      <ListItemSecondaryAction>
                        <Button variant="outlined" size="small">
                          Schedule Trip
                        </Button>
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}
        </>
      )}

      {/* Edit Trip Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)}>
        <DialogTitle>Edit Trip Timing</DialogTitle>
        <DialogContent>
          {selectedTrip && (
            <Box sx={{ pt: 2 }}>
              <Typography variant="subtitle1" gutterBottom>
                {selectedTrip.route.routeNumber} - {selectedTrip.route.name}
              </Typography>
              <Grid container spacing={2} sx={{ mt: 1 }}>
                <Grid size={{ xs: 6 }}>
                  <TextField
                    fullWidth
                    label="Start Time"
                    type="time"
                    value={newStartTime}
                    onChange={e => setNewStartTime(e.target.value)}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid size={{ xs: 6 }}>
                  <TextField
                    fullWidth
                    label="End Time"
                    type="time"
                    value={newEndTime}
                    onChange={e => setNewEndTime(e.target.value)}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveTrip}>
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default TripSchedulingDashboard;
