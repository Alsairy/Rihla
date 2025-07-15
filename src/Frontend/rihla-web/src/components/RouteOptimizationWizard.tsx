import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stepper,
  Step,
  StepLabel,
  TextField,
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Chip,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Route as RouteIcon,
  Speed as SpeedIcon,
  LocalGasStation as FuelIcon,
  AccessTime as TimeIcon,
  LocationOn as LocationIcon,
  Settings as OptimizeIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface RouteStop {
  id?: number;
  name: string;
  address: string;
  latitude: number;
  longitude: number;
  sequenceNumber: number;
  estimatedArrivalTime: string;
}

interface OptimalRouteRequest {
  routeName: string;
  startTime: string;
  vehicleCapacity: number;
  stops: RouteStop[];
  type: string;
  notes?: string;
}

interface RouteOptimizationResult {
  routeId: number;
  routeNumber: string;
  routeName: string;
  originalDistance: number;
  optimizedDistance: number;
  distanceSavings: number;
  originalDuration: string;
  optimizedDuration: string;
  timeSavings: string;
  fuelSavings: number;
  costSavings: number;
  optimizedStops: RouteStop[];
  optimizedAt: string;
}

interface RouteOptimizationWizardProps {
  open: boolean;
  onClose: () => void;
  onRouteCreated: (route: any) => void;
  existingRouteId?: number;
}

const steps = [
  'Route Details',
  'Add Stops',
  'Optimization Settings',
  'Review & Optimize',
];

export const RouteOptimizationWizard: React.FC<
  RouteOptimizationWizardProps
> = ({ open, onClose, onRouteCreated, existingRouteId }) => {
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [optimizationResult, setOptimizationResult] =
    useState<RouteOptimizationResult | null>(null);

  const [routeData, setRouteData] = useState<OptimalRouteRequest>({
    routeName: '',
    startTime: '07:00',
    vehicleCapacity: 30,
    stops: [],
    type: 'Regular',
    notes: '',
  });

  const [newStop, setNewStop] = useState<Partial<RouteStop>>({
    name: '',
    address: '',
    latitude: 0,
    longitude: 0,
  });

  const [vehicles, setVehicles] = useState<any[]>([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(
    null
  );

  useEffect(() => {
    if (open) {
      loadVehicles();
      if (existingRouteId) {
        loadExistingRoute();
      }
    }
  }, [open, existingRouteId]);

  const loadVehicles = async () => {
    try {
      const response = (await apiClient.get('/api/vehicles')) as any;
      setVehicles(response.items || []);
    } catch (err) {
      console.error('Failed to load vehicles:', err);
    }
  };

  const loadExistingRoute = async () => {
    if (!existingRouteId) return;

    try {
      setLoading(true);
      const response = await apiClient.get(`/api/routes/${existingRouteId}`);
      const route = response as any;

      setRouteData({
        routeName: route.name || '',
        startTime: route.startTime || '08:00',
        vehicleCapacity: route.assignedVehicle?.capacity || 30,
        stops: route.routeStops || [],
        type: route.type || 'Regular',
        notes: route.notes || '',
      });
    } catch (err) {
      setError('Failed to load existing route');
    } finally {
      setLoading(false);
    }
  };

  const handleNext = () => {
    if (activeStep === steps.length - 1) {
      handleOptimizeRoute();
    } else {
      setActiveStep(prevActiveStep => prevActiveStep + 1);
    }
  };

  const handleBack = () => {
    setActiveStep(prevActiveStep => prevActiveStep - 1);
  };

  const handleAddStop = () => {
    if (newStop.name && newStop.address) {
      const stop: RouteStop = {
        name: newStop.name,
        address: newStop.address,
        latitude: newStop.latitude || 0,
        longitude: newStop.longitude || 0,
        sequenceNumber: routeData.stops.length + 1,
        estimatedArrivalTime: '07:00',
      };

      setRouteData(prev => ({
        ...prev,
        stops: [...prev.stops, stop],
      }));

      setNewStop({
        name: '',
        address: '',
        latitude: 0,
        longitude: 0,
      });
    }
  };

  const handleRemoveStop = (index: number) => {
    setRouteData(prev => ({
      ...prev,
      stops: prev.stops.filter((_, i) => i !== index),
    }));
  };

  const handleOptimizeRoute = async () => {
    try {
      setLoading(true);
      setError(null);

      let result;
      if (existingRouteId) {
        result = await apiClient.post(
          `/api/routes/${existingRouteId}/optimize`
        );
      } else {
        result = await apiClient.post(
          '/api/routes/generate-optimal',
          routeData
        );
      }

      setOptimizationResult(result as RouteOptimizationResult);
      onRouteCreated(result as RouteOptimizationResult);
    } catch (err: any) {
      setError(err.message || 'Failed to optimize route');
    } finally {
      setLoading(false);
    }
  };

  const handleGeocodeAddress = async (address: string) => {
    try {
      const response = await fetch(
        `https://api.opencagedata.com/geocode/v1/json?q=${encodeURIComponent(address)}&key=YOUR_API_KEY`
      );
      const data = await response.json();

      if (data.results && data.results.length > 0) {
        const { lat, lng } = data.results[0].geometry;
        setNewStop(prev => ({
          ...prev,
          latitude: lat,
          longitude: lng,
        }));
      }
    } catch (err) {
      console.error('Geocoding failed:', err);
    }
  };

  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box sx={{ mt: 2 }}>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  label="Route Name"
                  value={routeData.routeName}
                  onChange={e =>
                    setRouteData(prev => ({
                      ...prev,
                      routeName: e.target.value,
                    }))
                  }
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  label="Start Time"
                  type="time"
                  value={routeData.startTime}
                  onChange={e =>
                    setRouteData(prev => ({
                      ...prev,
                      startTime: e.target.value,
                    }))
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  label="Vehicle Capacity"
                  type="number"
                  value={routeData.vehicleCapacity}
                  onChange={e =>
                    setRouteData(prev => ({
                      ...prev,
                      vehicleCapacity: parseInt(e.target.value),
                    }))
                  }
                  inputProps={{ min: 1, max: 100 }}
                />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Route Type</InputLabel>
                  <Select
                    value={routeData.type}
                    onChange={e =>
                      setRouteData(prev => ({ ...prev, type: e.target.value }))
                    }
                  >
                    <MenuItem value="Regular">Regular</MenuItem>
                    <MenuItem value="Express">Express</MenuItem>
                    <MenuItem value="Special">Special Needs</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Notes"
                  multiline
                  rows={3}
                  value={routeData.notes}
                  onChange={e =>
                    setRouteData(prev => ({ ...prev, notes: e.target.value }))
                  }
                />
              </Grid>
            </Grid>
          </Box>
        );

      case 1:
        return (
          <Box sx={{ mt: 2 }}>
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Add New Stop
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <TextField
                      fullWidth
                      label="Stop Name"
                      value={newStop.name}
                      onChange={e =>
                        setNewStop(prev => ({ ...prev, name: e.target.value }))
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <TextField
                      fullWidth
                      label="Address"
                      value={newStop.address}
                      onChange={e =>
                        setNewStop(prev => ({
                          ...prev,
                          address: e.target.value,
                        }))
                      }
                      onBlur={() =>
                        newStop.address && handleGeocodeAddress(newStop.address)
                      }
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 2 }}>
                    <Button
                      fullWidth
                      variant="contained"
                      onClick={handleAddStop}
                      startIcon={<AddIcon />}
                      disabled={!newStop.name || !newStop.address}
                    >
                      Add
                    </Button>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>

            <Typography variant="h6" gutterBottom>
              Route Stops ({routeData.stops.length})
            </Typography>
            <List>
              {routeData.stops.map((stop, index) => (
                <ListItem key={index} divider>
                  <ListItemText
                    primary={stop.name}
                    secondary={
                      <Box>
                        <Typography variant="body2" color="textSecondary">
                          {stop.address}
                        </Typography>
                        <Chip
                          size="small"
                          label={`Stop ${stop.sequenceNumber}`}
                          color="primary"
                          sx={{ mt: 1 }}
                        />
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <IconButton
                      edge="end"
                      onClick={() => handleRemoveStop(index)}
                      color="error"
                    >
                      <DeleteIcon />
                    </IconButton>
                  </ListItemSecondaryAction>
                </ListItem>
              ))}
            </List>
            {routeData.stops.length === 0 && (
              <Alert severity="info">
                Add at least one stop to continue with route optimization.
              </Alert>
            )}
          </Box>
        );

      case 2:
        return (
          <Box sx={{ mt: 2 }}>
            <Typography variant="h6" gutterBottom>
              Optimization Settings
            </Typography>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Select Vehicle</InputLabel>
                  <Select
                    value={selectedVehicleId || ''}
                    onChange={e =>
                      setSelectedVehicleId(e.target.value as number)
                    }
                  >
                    {vehicles.map(vehicle => (
                      <MenuItem key={vehicle.id} value={vehicle.id}>
                        {vehicle.vehicleNumber} - {vehicle.make} {vehicle.model}{' '}
                        (Capacity: {vehicle.capacity})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12 }}>
                <Alert severity="info">
                  The optimization algorithm will:
                  <ul>
                    <li>Minimize total travel distance</li>
                    <li>Optimize stop sequence for efficiency</li>
                    <li>Consider vehicle capacity constraints</li>
                    <li>Calculate estimated arrival times</li>
                    <li>Provide fuel and cost savings estimates</li>
                  </ul>
                </Alert>
              </Grid>
            </Grid>
          </Box>
        );

      case 3:
        return (
          <Box sx={{ mt: 2 }}>
            <Typography variant="h6" gutterBottom>
              Route Summary
            </Typography>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Card>
                  <CardContent>
                    <Typography variant="subtitle1" gutterBottom>
                      <RouteIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                      Route Details
                    </Typography>
                    <Typography variant="body2">
                      Name: {routeData.routeName}
                    </Typography>
                    <Typography variant="body2">
                      Start Time: {routeData.startTime}
                    </Typography>
                    <Typography variant="body2">
                      Capacity: {routeData.vehicleCapacity} students
                    </Typography>
                    <Typography variant="body2">
                      Type: {routeData.type}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <Card>
                  <CardContent>
                    <Typography variant="subtitle1" gutterBottom>
                      <LocationIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                      Stops Summary
                    </Typography>
                    <Typography variant="body2">
                      Total Stops: {routeData.stops.length}
                    </Typography>
                    <Typography variant="body2">
                      Stops: {routeData.stops.map(s => s.name).join(', ')}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>

            {optimizationResult && (
              <Box sx={{ mt: 3 }}>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="h6" gutterBottom color="success.main">
                  <OptimizeIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Optimization Results
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Card>
                      <CardContent sx={{ textAlign: 'center' }}>
                        <SpeedIcon
                          color="primary"
                          sx={{ fontSize: 40, mb: 1 }}
                        />
                        <Typography variant="h6">
                          {optimizationResult.distanceSavings.toFixed(1)} km
                        </Typography>
                        <Typography variant="body2" color="textSecondary">
                          Distance Saved
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Card>
                      <CardContent sx={{ textAlign: 'center' }}>
                        <TimeIcon
                          color="primary"
                          sx={{ fontSize: 40, mb: 1 }}
                        />
                        <Typography variant="h6">
                          {optimizationResult.timeSavings}
                        </Typography>
                        <Typography variant="body2" color="textSecondary">
                          Time Saved
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Card>
                      <CardContent sx={{ textAlign: 'center' }}>
                        <FuelIcon
                          color="primary"
                          sx={{ fontSize: 40, mb: 1 }}
                        />
                        <Typography variant="h6">
                          {optimizationResult.fuelSavings.toFixed(1)}L
                        </Typography>
                        <Typography variant="body2" color="textSecondary">
                          Fuel Saved
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Card>
                      <CardContent sx={{ textAlign: 'center' }}>
                        <Typography variant="h6" color="success.main">
                          ${optimizationResult.costSavings.toFixed(2)}
                        </Typography>
                        <Typography variant="body2" color="textSecondary">
                          Cost Saved
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              </Box>
            )}
          </Box>
        );

      default:
        return null;
    }
  };

  const isStepValid = (step: number) => {
    switch (step) {
      case 0:
        return routeData.routeName.trim() !== '';
      case 1:
        return routeData.stops.length > 0;
      case 2:
        return true;
      case 3:
        return true;
      default:
        return false;
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        {existingRouteId
          ? 'Optimize Existing Route'
          : 'Route Optimization Wizard'}
      </DialogTitle>
      <DialogContent>
        <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
          {steps.map(label => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
            <Typography sx={{ ml: 2 }}>
              {activeStep === steps.length - 1
                ? 'Optimizing route...'
                : 'Loading...'}
            </Typography>
          </Box>
        ) : (
          renderStepContent(activeStep)
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button disabled={activeStep === 0} onClick={handleBack}>
          Back
        </Button>
        <Button
          variant="contained"
          onClick={handleNext}
          disabled={!isStepValid(activeStep) || loading}
        >
          {activeStep === steps.length - 1 ? 'Optimize Route' : 'Next'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default RouteOptimizationWizard;
