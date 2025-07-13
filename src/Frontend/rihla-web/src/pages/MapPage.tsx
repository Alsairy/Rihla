import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Chip,
  Alert,
  Button,
  Card,
  CardContent,
} from '@mui/material';
import {
  LocationOn as LocationIcon,
  DirectionsBus as BusIcon,
  Route as RouteIcon,
  People as PeopleIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import MapComponent from '../components/MapComponent';
import { apiClient } from '../services/apiClient';
import { signalRService } from '../services/signalRService';

interface Vehicle {
  id: string;
  plateNumber: string;
  model: string;
  capacity: number;
  status: string;
  currentLatitude?: number;
  currentLongitude?: number;
  driverId?: string;
  driverName?: string;
}

interface Route {
  id: string;
  name: string;
  description: string;
  vehicleId?: string;
}

interface MapStats {
  totalVehicles: number;
  activeVehicles: number;
  totalRoutes: number;
  activeRoutes: number;
  totalStudents: number;
}

const MapPage: React.FC = () => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<string>('');
  const [selectedRouteId, setSelectedRouteId] = useState<string>('');
  const [showVehicles, setShowVehicles] = useState(true);
  const [showRoutes, setShowRoutes] = useState(true);
  const [showStudents, setShowStudents] = useState(false);
  const [realTimeEnabled, setRealTimeEnabled] = useState(true);
  const [mapStats, setMapStats] = useState<MapStats>({
    totalVehicles: 0,
    activeVehicles: 0,
    totalRoutes: 0,
    activeRoutes: 0,
    totalStudents: 0,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdate, setLastUpdate] = useState<Date>(new Date());

  const loadMapData = React.useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const [vehiclesData, routesData, dashboardData] = await Promise.all([
        apiClient.get<Vehicle[]>('/api/vehicles'),
        apiClient.get<Route[]>('/api/routes'),
        apiClient.get<{ totalStudents: number }>('/api/dashboard/stats'),
      ]);

      setVehicles(vehiclesData);
      setRoutes(routesData);

      const activeVehicles = vehiclesData.filter(
        v => v.status === 'Active' || v.status === 'InTransit'
      ).length;
      const activeRoutes = routesData.filter(r => r.vehicleId).length;

      setMapStats({
        totalVehicles: vehiclesData.length,
        activeVehicles,
        totalRoutes: routesData.length,
        activeRoutes,
        totalStudents: dashboardData.totalStudents || 0,
      });

      setLastUpdate(new Date());
    } catch {
      setError('Failed to load map data. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  const setupRealTimeUpdates = React.useCallback(async () => {
    if (!realTimeEnabled) return;

    try {
      await signalRService.startConnection();

      signalRService.onTripStatusUpdated(() => {
        loadMapData();
      });

      signalRService.onNotificationReceived(notification => {
        if (
          notification.type === 'VehicleLocationUpdate' ||
          notification.type === 'TripStatusChange'
        ) {
          loadMapData();
        }
      });

      signalRService.onEmergencyAlert(() => {
        loadMapData();
      });
    } catch {}
  }, [realTimeEnabled, loadMapData]);

  useEffect(() => {
    loadMapData();
    setupRealTimeUpdates();

    return () => {
      signalRService.stopConnection();
    };
  }, [setupRealTimeUpdates]);

  const handleRefresh = () => {
    loadMapData();
  };

  const handleRealTimeToggle = (enabled: boolean) => {
    setRealTimeEnabled(enabled);
    if (enabled) {
      setupRealTimeUpdates();
    } else {
      signalRService.stopConnection();
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'success';
      case 'InTransit':
        return 'warning';
      case 'Maintenance':
        return 'error';
      default:
        return 'default';
    }
  };

  const selectedVehicle = vehicles.find(v => v.id === selectedVehicleId);
  const selectedRoute = routes.find(r => r.id === selectedRouteId);

  return (
    <Box sx={{ p: 3 }}>
      <Typography
        variant="h4"
        gutterBottom
        sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
      >
        <LocationIcon color="primary" />
        Live Transportation Map
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Map Statistics */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <BusIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{mapStats.totalVehicles}</Typography>
              <Typography variant="body2" color="text.secondary">
                Total Vehicles
              </Typography>
              <Typography variant="caption" color="success.main">
                {mapStats.activeVehicles} Active
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <RouteIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{mapStats.totalRoutes}</Typography>
              <Typography variant="body2" color="text.secondary">
                Total Routes
              </Typography>
              <Typography variant="caption" color="success.main">
                {mapStats.activeRoutes} Active
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <PeopleIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{mapStats.totalStudents}</Typography>
              <Typography variant="body2" color="text.secondary">
                Total Students
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <LocationIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {
                  vehicles.filter(v => v.currentLatitude && v.currentLongitude)
                    .length
                }
              </Typography>
              <Typography variant="body2" color="text.secondary">
                GPS Tracked
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mb: 1,
                }}
              >
                <Box
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    backgroundColor: realTimeEnabled
                      ? 'success.main'
                      : 'error.main',
                    mr: 1,
                  }}
                />
                <Typography variant="body2">
                  {realTimeEnabled ? 'Live' : 'Static'}
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Real-time Status
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Updated: {lastUpdate.toLocaleTimeString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ visibility: 'hidden' }}>
            <CardContent>
              <Typography variant="body2">Spacer</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Map Controls */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Select Vehicle</InputLabel>
              <Select
                value={selectedVehicleId}
                label="Select Vehicle"
                onChange={e => setSelectedVehicleId(e.target.value)}
              >
                <MenuItem value="">All Vehicles</MenuItem>
                {vehicles.map(vehicle => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {vehicle.plateNumber}
                      <Chip
                        label={vehicle.status}
                        size="small"
                        color={getStatusColor(vehicle.status) as any}
                      />
                    </Box>
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Select Route</InputLabel>
              <Select
                value={selectedRouteId}
                label="Select Route"
                onChange={e => setSelectedRouteId(e.target.value)}
              >
                <MenuItem value="">All Routes</MenuItem>
                {routes.map(route => (
                  <MenuItem key={route.id} value={route.id}>
                    {route.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={showVehicles}
                  onChange={e => setShowVehicles(e.target.checked)}
                />
              }
              label="Vehicles"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={showRoutes}
                  onChange={e => setShowRoutes(e.target.checked)}
                />
              }
              label="Routes"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={showStudents}
                  onChange={e => setShowStudents(e.target.checked)}
                />
              }
              label="Students"
            />
          </Grid>
        </Grid>
        <Grid container spacing={2} alignItems="center" sx={{ mt: 1 }}>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={realTimeEnabled}
                  onChange={e => handleRealTimeToggle(e.target.checked)}
                />
              }
              label="Real-time Updates"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Button
              variant="outlined"
              startIcon={<RefreshIcon />}
              onClick={handleRefresh}
              disabled={loading}
            >
              Refresh
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Selected Vehicle/Route Info */}
      {(selectedVehicle || selectedRoute) && (
        <Paper sx={{ p: 2, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Selection Details
          </Typography>
          {selectedVehicle && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle1" gutterBottom>
                🚌 Vehicle: {selectedVehicle.plateNumber}
              </Typography>
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Typography variant="body2" color="text.secondary">
                    <strong>Model:</strong> {selectedVehicle.model}
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Typography variant="body2" color="text.secondary">
                    <strong>Capacity:</strong> {selectedVehicle.capacity}{' '}
                    students
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Typography variant="body2" color="text.secondary">
                    <strong>Driver:</strong>{' '}
                    {selectedVehicle.driverName || 'Not assigned'}
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Chip
                    label={selectedVehicle.status}
                    color={getStatusColor(selectedVehicle.status) as any}
                    size="small"
                  />
                </Grid>
              </Grid>
            </Box>
          )}
          {selectedRoute && (
            <Box>
              <Typography variant="subtitle1" gutterBottom>
                🛣️ Route: {selectedRoute.name}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedRoute.description}
              </Typography>
            </Box>
          )}
        </Paper>
      )}

      {/* Map Component */}
      <Paper sx={{ overflow: 'hidden' }}>
        <MapComponent
          center={[24.7136, 46.6753]} // Riyadh, Saudi Arabia
          zoom={11}
          height={600}
          showVehicles={showVehicles}
          showRoutes={showRoutes}
          showStudents={showStudents}
          selectedVehicleId={selectedVehicleId}
          selectedRouteId={selectedRouteId}
        />
      </Paper>
    </Box>
  );
};

export default MapPage;
