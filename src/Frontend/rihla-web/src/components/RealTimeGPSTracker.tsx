import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Alert,
  CircularProgress,
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
  Divider
} from '@mui/material';
import {
  LocationOn,
  DirectionsCar,
  Speed,
  Warning,
  CheckCircle,
  Error,
  Navigation,
  Timeline,
  Refresh,
  PlayArrow,
  Stop,
  Map as MapIcon,
  Schedule,
  Route
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface Vehicle {
  id: number;
  licensePlate: string;
  model: string;
  currentLatitude?: number;
  currentLongitude?: number;
  lastLocationUpdate?: string;
}

interface Trip {
  id: number;
  routeName: string;
  vehicleId: number;
  driverName: string;
  status: string;
  scheduledStartTime: string;
}

interface VehicleLocation {
  id: number;
  vehicleId: number;
  tripId?: number;
  latitude: number;
  longitude: number;
  speed: number;
  heading: number;
  timestamp: string;
  isActive: boolean;
  vehicleName?: string;
}

interface GeofenceViolation {
  vehicleId: number;
  tripId: number;
  violationType: string;
  description: string;
  latitude: number;
  longitude: number;
  timestamp: string;
  severity: 'Low' | 'Medium' | 'High';
  actionRequired: string;
}

interface EstimatedArrival {
  tripId: number;
  stopId: number;
  stopName: string;
  estimatedArrivalTime: string;
  distanceKm: number;
  averageSpeedKmh: number;
  confidenceLevel: 'Low' | 'Medium' | 'High';
  lastUpdated: string;
  delayMinutes: number;
}

const RealTimeGPSTracker: React.FC = () => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [activeTrips, setActiveTrips] = useState<Trip[]>([]);
  const [selectedVehicle, setSelectedVehicle] = useState<number | null>(null);
  const [vehicleLocations, setVehicleLocations] = useState<VehicleLocation[]>([]);
  const [geofenceViolations, setGeofenceViolations] = useState<GeofenceViolation[]>([]);
  const [estimatedArrivals, setEstimatedArrivals] = useState<EstimatedArrival[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [trackingActive, setTrackingActive] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [violationDialog, setViolationDialog] = useState(false);
  const [selectedViolation, setSelectedViolation] = useState<GeofenceViolation | null>(null);

  const intervalRef = useRef<number | null>(null);
  const mapRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    loadInitialData();
    
    return () => {
      if (intervalRef.current) {
        window.clearInterval(intervalRef.current);
      }
    };
  }, []);

  useEffect(() => {
    if (autoRefresh && trackingActive) {
      intervalRef.current = window.setInterval(() => {
        updateVehicleLocations();
        checkGeofenceViolations();
      }, 5000); // Update every 5 seconds
    } else if (intervalRef.current) {
      window.clearInterval(intervalRef.current);
      intervalRef.current = null;
    }

    return () => {
      if (intervalRef.current) {
        window.clearInterval(intervalRef.current);
      }
    };
  }, [autoRefresh, trackingActive]);

  const loadInitialData = async () => {
    setLoading(true);
    try {
      const [vehiclesResponse, tripsResponse] = await Promise.all([
        apiClient.get('/api/vehicles'),
        apiClient.get('/api/trips/active')
      ]);
      
      setVehicles((vehiclesResponse as any).data?.items || []);
      setActiveTrips((tripsResponse as any).data?.items || []);
      
      if ((vehiclesResponse as any).data?.items?.length > 0) {
        setSelectedVehicle((vehiclesResponse as any).data.items[0].id);
      }
      
      await loadActiveVehicleLocations();
    } catch (err) {
      setError('Failed to load initial data');
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadActiveVehicleLocations = async () => {
    try {
      const response = await apiClient.get('/api/gps/active-locations');
      setVehicleLocations((response as any).data || []);
    } catch (err) {
      console.error('Error loading active locations:', err);
    }
  };

  const startTracking = async (vehicleId: number) => {
    const trip = activeTrips.find(t => t.vehicleId === vehicleId);
    if (!trip) {
      setError('No active trip found for this vehicle');
      return;
    }

    setLoading(true);
    try {
      const response = await apiClient.post('/api/gps/start-tracking', {
        vehicleId,
        tripId: trip.id
      });
      
      if ((response as any).data?.success) {
        setTrackingActive(true);
        setSuccess('Real-time tracking started');
        updateVehicleLocations();
      } else {
        setError((response as any).data?.message || 'Failed to start tracking');
      }
    } catch (err) {
      setError('Failed to start tracking');
      console.error('Start tracking error:', err);
    } finally {
      setLoading(false);
    }
  };

  const stopTracking = async (vehicleId: number) => {
    setLoading(true);
    try {
      const response = await apiClient.post('/api/gps/stop-tracking', {
        vehicleId
      });
      
      if ((response as any).data?.success) {
        setTrackingActive(false);
        setSuccess('Real-time tracking stopped');
        if (intervalRef.current) {
          window.clearInterval(intervalRef.current);
          intervalRef.current = null;
        }
      } else {
        setError((response as any).data?.message || 'Failed to stop tracking');
      }
    } catch (err) {
      setError('Failed to stop tracking');
      console.error('Stop tracking error:', err);
    } finally {
      setLoading(false);
    }
  };

  const updateVehicleLocations = async () => {
    try {
      await loadActiveVehicleLocations();
    } catch (err) {
      console.error('Error updating locations:', err);
    }
  };

  const checkGeofenceViolations = async () => {
    if (!selectedVehicle) return;
    
    const location = vehicleLocations.find(l => l.vehicleId === selectedVehicle);
    if (!location) return;

    try {
      const response = await apiClient.post('/api/gps/check-violations', {
        vehicleId: selectedVehicle,
        latitude: location.latitude,
        longitude: location.longitude
      });
      
      if ((response as any).data && (response as any).data.length > 0) {
        setGeofenceViolations((response as any).data);
        
        const highSeverityViolations = (response as any).data.filter((v: GeofenceViolation) => v.severity === 'High');
        if (highSeverityViolations.length > 0) {
          setSelectedViolation(highSeverityViolations[0]);
          setViolationDialog(true);
        }
      }
    } catch (err) {
      console.error('Error checking violations:', err);
    }
  };

  const getEstimatedArrival = async (tripId: number, stopId: number) => {
    try {
      const response = await apiClient.get(`/api/gps/eta/${tripId}/${stopId}`);
      
      if ((response as any).data) {
        setEstimatedArrivals(prev => {
          const filtered = prev.filter(eta => !(eta.tripId === tripId && eta.stopId === stopId));
          return [...filtered, (response as any).data];
        });
      }
    } catch (err) {
      console.error('Error getting ETA:', err);
    }
  };

  const getVehicleLocationHistory = async (vehicleId: number, hours: number = 24) => {
    const endTime = new Date();
    const startTime = new Date(endTime.getTime() - (hours * 60 * 60 * 1000));
    
    try {
      const response = await apiClient.get(`/api/gps/history/${vehicleId}?startTime=${startTime.toISOString()}&endTime=${endTime.toISOString()}`);
      
      return (response as any).data || [];
    } catch (err) {
      console.error('Error getting location history:', err);
      return [];
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High': return 'error';
      case 'Medium': return 'warning';
      case 'Low': return 'info';
      default: return 'default';
    }
  };

  const getViolationIcon = (type: string) => {
    switch (type) {
      case 'Speed Violation': return <Speed />;
      case 'Route Deviation': return <Route />;
      case 'Restricted Area': return <Warning />;
      default: return <Error />;
    }
  };

  const formatSpeed = (speed: number) => {
    return `${speed.toFixed(1)} km/h`;
  };

  const formatDistance = (distance: number) => {
    return distance < 1 ? `${(distance * 1000).toFixed(0)}m` : `${distance.toFixed(1)}km`;
  };

  const getConfidenceColor = (confidence: string) => {
    switch (confidence) {
      case 'High': return 'success';
      case 'Medium': return 'warning';
      case 'Low': return 'error';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Real-Time GPS Tracker
      </Typography>

      {/* Control Panel */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 4 }}>
              <FormControl fullWidth>
                <InputLabel>Select Vehicle</InputLabel>
                <Select
                  value={selectedVehicle || ''}
                  onChange={(e) => setSelectedVehicle(Number(e.target.value))}
                >
                  {vehicles.map((vehicle) => (
                    <MenuItem key={vehicle.id} value={vehicle.id}>
                      {vehicle.licensePlate} - {vehicle.model}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                {!trackingActive ? (
                  <Button
                    variant="contained"
                    startIcon={<PlayArrow />}
                    onClick={() => selectedVehicle && startTracking(selectedVehicle)}
                    disabled={!selectedVehicle || loading}
                  >
                    Start Tracking
                  </Button>
                ) : (
                  <Button
                    variant="contained"
                    color="error"
                    startIcon={<Stop />}
                    onClick={() => selectedVehicle && stopTracking(selectedVehicle)}
                    disabled={!selectedVehicle || loading}
                  >
                    Stop Tracking
                  </Button>
                )}
                <Button
                  variant="outlined"
                  startIcon={<Refresh />}
                  onClick={updateVehicleLocations}
                  disabled={loading}
                >
                  Refresh
                </Button>
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={autoRefresh}
                    onChange={(e) => setAutoRefresh(e.target.checked)}
                  />
                }
                label="Auto Refresh"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Vehicle Status */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <DirectionsCar sx={{ mr: 1, verticalAlign: 'middle' }} />
                Vehicle Status
              </Typography>
              {selectedVehicle && (
                <>
                  {vehicleLocations
                    .filter(loc => loc.vehicleId === selectedVehicle)
                    .map((location) => (
                      <Box key={location.id} sx={{ mb: 2 }}>
                        <Grid container spacing={2}>
                          <Grid size={{ xs: 6 }}>
                            <Typography variant="body2" color="text.secondary">
                              Location
                            </Typography>
                            <Typography variant="body1">
                              {location.latitude.toFixed(6)}, {location.longitude.toFixed(6)}
                            </Typography>
                          </Grid>
                          <Grid size={{ xs: 6 }}>
                            <Typography variant="body2" color="text.secondary">
                              Speed
                            </Typography>
                            <Typography variant="body1">
                              {formatSpeed(location.speed)}
                            </Typography>
                          </Grid>
                          <Grid size={{ xs: 6 }}>
                            <Typography variant="body2" color="text.secondary">
                              Heading
                            </Typography>
                            <Typography variant="body1">
                              {location.heading.toFixed(0)}°
                            </Typography>
                          </Grid>
                          <Grid size={{ xs: 6 }}>
                            <Typography variant="body2" color="text.secondary">
                              Last Update
                            </Typography>
                            <Typography variant="body1">
                              {new Date(location.timestamp).toLocaleTimeString()}
                            </Typography>
                          </Grid>
                        </Grid>
                        <Chip
                          label={location.isActive ? 'Active' : 'Inactive'}
                          color={location.isActive ? 'success' : 'default'}
                          size="small"
                          sx={{ mt: 1 }}
                        />
                      </Box>
                    ))}
                  {vehicleLocations.filter(loc => loc.vehicleId === selectedVehicle).length === 0 && (
                    <Typography color="text.secondary">
                      No location data available
                    </Typography>
                  )}
                </>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Geofence Violations */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Warning sx={{ mr: 1, verticalAlign: 'middle' }} />
                Geofence Violations
              </Typography>
              {geofenceViolations.length === 0 ? (
                <Box sx={{ display: 'flex', alignItems: 'center', color: 'success.main' }}>
                  <CheckCircle sx={{ mr: 1 }} />
                  <Typography>No violations detected</Typography>
                </Box>
              ) : (
                <List dense>
                  {geofenceViolations.slice(0, 5).map((violation, index) => (
                    <ListItem key={index} divider>
                      <ListItemIcon>
                        {getViolationIcon(violation.violationType)}
                      </ListItemIcon>
                      <ListItemText
                        primary={violation.violationType}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              {violation.description}
                            </Typography>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                              <Chip
                                label={violation.severity}
                                color={getSeverityColor(violation.severity) as any}
                                size="small"
                              />
                              <Typography variant="caption">
                                {new Date(violation.timestamp).toLocaleTimeString()}
                              </Typography>
                            </Box>
                          </Box>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Active Vehicle Locations */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <LocationOn sx={{ mr: 1, verticalAlign: 'middle' }} />
                All Active Vehicles
              </Typography>
              {vehicleLocations.length === 0 ? (
                <Typography color="text.secondary">
                  No active vehicles being tracked
                </Typography>
              ) : (
                <List>
                  {vehicleLocations.map((location) => (
                    <ListItem key={location.id} divider>
                      <ListItemIcon>
                        <DirectionsCar />
                      </ListItemIcon>
                      <ListItemText
                        primary={location.vehicleName || `Vehicle ${location.vehicleId}`}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              {location.latitude.toFixed(4)}, {location.longitude.toFixed(4)}
                            </Typography>
                            <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                              <Typography variant="caption">
                                Speed: {formatSpeed(location.speed)}
                              </Typography>
                              <Typography variant="caption">
                                Updated: {new Date(location.timestamp).toLocaleTimeString()}
                              </Typography>
                            </Box>
                          </Box>
                        }
                      />
                      <Chip
                        label={location.isActive ? 'Tracking' : 'Stopped'}
                        color={location.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Estimated Arrivals */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Schedule sx={{ mr: 1, verticalAlign: 'middle' }} />
                Estimated Arrivals
              </Typography>
              {estimatedArrivals.length === 0 ? (
                <Typography color="text.secondary">
                  No ETA data available
                </Typography>
              ) : (
                <List dense>
                  {estimatedArrivals.map((eta) => (
                    <ListItem key={`${eta.tripId}-${eta.stopId}`} divider>
                      <ListItemText
                        primary={eta.stopName}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              ETA: {new Date(eta.estimatedArrivalTime).toLocaleTimeString()}
                            </Typography>
                            <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                              <Typography variant="caption">
                                Distance: {formatDistance(eta.distanceKm)}
                              </Typography>
                              <Typography variant="caption">
                                Speed: {formatSpeed(eta.averageSpeedKmh)}
                              </Typography>
                              <Chip
                                label={eta.confidenceLevel}
                                color={getConfidenceColor(eta.confidenceLevel) as any}
                                size="small"
                              />
                            </Box>
                          </Box>
                        }
                      />
                      {eta.delayMinutes > 0 && (
                        <Chip
                          label={`+${eta.delayMinutes}min`}
                          color="warning"
                          size="small"
                        />
                      )}
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Violation Details Dialog */}
      <Dialog open={violationDialog} onClose={() => setViolationDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Warning color="error" />
            <Typography variant="h6">
              Geofence Violation Alert
            </Typography>
            {selectedViolation && (
              <Chip
                label={selectedViolation.severity}
                color={getSeverityColor(selectedViolation.severity) as any}
                size="small"
              />
            )}
          </Box>
        </DialogTitle>
        <DialogContent>
          {selectedViolation && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Alert severity="error">
                <Typography variant="h6">
                  {selectedViolation.violationType}
                </Typography>
                <Typography variant="body1">
                  {selectedViolation.description}
                </Typography>
              </Alert>
              
              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Violation Details
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Vehicle ID
                    </Typography>
                    <Typography variant="body1">
                      {selectedViolation.vehicleId}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Trip ID
                    </Typography>
                    <Typography variant="body1">
                      {selectedViolation.tripId}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Location
                    </Typography>
                    <Typography variant="body1">
                      {selectedViolation.latitude.toFixed(6)}, {selectedViolation.longitude.toFixed(6)}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Time
                    </Typography>
                    <Typography variant="body1">
                      {new Date(selectedViolation.timestamp).toLocaleString()}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Recommended Action
                </Typography>
                <Typography variant="body1">
                  {selectedViolation.actionRequired}
                </Typography>
              </Paper>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViolationDialog(false)}>
            Close
          </Button>
          <Button 
            variant="contained" 
            color="primary"
            onClick={() => {
              setViolationDialog(false);
              setSuccess('Driver has been notified of the violation');
            }}
          >
            Contact Driver
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
        <Alert severity="success" sx={{ mt: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}
    </Box>
  );
};

export default RealTimeGPSTracker;
