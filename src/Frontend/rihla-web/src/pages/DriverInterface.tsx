import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  AppBar,
  Toolbar,
  Button,
  Container,
  Paper,
  List,
  ListItem,
  ListItemText,
  Chip,
  IconButton,
} from '@mui/material';
import { PlayArrow, CheckCircle } from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';
import { Trip, Vehicle } from '../types';

const DriverInterface: React.FC = () => {
  const { user, logout } = useAuth();
  const [trips, setTrips] = useState<Trip[]>([]);
  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDriverData = async () => {
      try {
        const [tripsResponse, vehicleResponse] = await Promise.all([
          apiClient.get<Trip[]>('/api/trips/my-trips'),
          apiClient.get<Vehicle>('/api/vehicles/my-vehicle'),
        ]);

        setTrips(tripsResponse);
        setVehicle(vehicleResponse);
      } catch (error) {
        console.error('Error fetching driver data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchDriverData();
  }, []);

  const handleLogout = async () => {
    await logout();
  };

  const handleStartTrip = async (tripId: number) => {
    try {
      await apiClient.post(`/api/trips/${tripId}/start`);
      const tripsResponse = await apiClient.get<Trip[]>('/api/trips/my-trips');
      setTrips(tripsResponse);
    } catch (error) {
      console.error('Error starting trip:', error);
    }
  };

  const handleCompleteTrip = async (tripId: number) => {
    try {
      await apiClient.post(`/api/trips/${tripId}/complete`);
      const tripsResponse = await apiClient.get<Trip[]>('/api/trips/my-trips');
      setTrips(tripsResponse);
    } catch (error) {
      console.error('Error completing trip:', error);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'success';
      case 'in_progress':
        return 'primary';
      case 'scheduled':
        return 'default';
      case 'cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  const getTodaysTrips = () => {
    const today = new Date().toDateString();
    return trips.filter(trip => 
      new Date(trip.scheduledStartTime).toDateString() === today
    );
  };

  if (loading) {
    return <Typography>Loading...</Typography>;
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Rihla Driver Interface
          </Typography>
          <Typography variant="body1" sx={{ mr: 2 }}>
            Welcome, {user?.username}
          </Typography>
          <Button color="inherit" onClick={handleLogout}>
            Logout
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Grid container spacing={3}>
          {/* Vehicle Information */}
          <Grid size={{ xs: 12, md: 4 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  My Vehicle
                </Typography>
                {vehicle ? (
                  <Box>
                    <Typography variant="body1">
                      <strong>Plate Number:</strong> {vehicle.plateNumber}
                    </Typography>
                    <Typography variant="body1">
                      <strong>Model:</strong> {vehicle.model}
                    </Typography>
                    <Typography variant="body1">
                      <strong>Year:</strong> {vehicle.year}
                    </Typography>
                    <Typography variant="body1">
                      <strong>Capacity:</strong> {vehicle.capacity} passengers
                    </Typography>
                    <Chip
                      label={vehicle.status}
                      color={vehicle.status === 'Active' ? 'success' : 'default'}
                      sx={{ mt: 1 }}
                    />
                  </Box>
                ) : (
                  <Typography color="textSecondary">
                    No vehicle assigned
                  </Typography>
                )}
              </CardContent>
            </Card>
          </Grid>

          {/* Today's Trips */}
          <Grid size={{ xs: 12, md: 8 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                Today's Trips
              </Typography>
              {getTodaysTrips().length === 0 ? (
                <Typography color="textSecondary">
                  No trips scheduled for today
                </Typography>
              ) : (
                <List>
                  {getTodaysTrips().map((trip) => (
                    <ListItem key={trip.id}>
                      <ListItemText
                        primary={trip.routeName}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              Scheduled: {new Date(trip.scheduledStartTime).toLocaleString()}
                            </Typography>
                            {trip.actualStartTime && (
                              <Typography variant="body2">
                                Started: {new Date(trip.actualStartTime).toLocaleString()}
                              </Typography>
                            )}
                          </Box>
                        }
                      />
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Chip
                          label={trip.status}
                          color={getStatusColor(trip.status) as any}
                          size="small"
                        />
                        {trip.status === 'Scheduled' && (
                          <IconButton
                            color="primary"
                            onClick={() => handleStartTrip(trip.id)}
                            title="Start Trip"
                          >
                            <PlayArrow />
                          </IconButton>
                        )}
                        {trip.status === 'InProgress' && (
                          <IconButton
                            color="success"
                            onClick={() => handleCompleteTrip(trip.id)}
                            title="Complete Trip"
                          >
                            <CheckCircle />
                          </IconButton>
                        )}
                      </Box>
                    </ListItem>
                  ))}
                </List>
              )}
            </Paper>
          </Grid>

          {/* All Trips */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                All My Trips
              </Typography>
              {trips.length === 0 ? (
                <Typography color="textSecondary">
                  No trips assigned
                </Typography>
              ) : (
                <List>
                  {trips.slice(0, 10).map((trip) => (
                    <ListItem key={trip.id}>
                      <ListItemText
                        primary={trip.routeName}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              Scheduled: {new Date(trip.scheduledStartTime).toLocaleString()}
                            </Typography>
                            <Typography variant="body2">
                              Vehicle: {trip.vehiclePlateNumber}
                            </Typography>
                          </Box>
                        }
                      />
                      <Chip
                        label={trip.status}
                        color={getStatusColor(trip.status) as any}
                        size="small"
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </Paper>
          </Grid>
        </Grid>
      </Container>
    </Box>
  );
};

export default DriverInterface;
