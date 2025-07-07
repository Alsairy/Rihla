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
  Avatar,
  Divider,
  CircularProgress,
  Tooltip,
  Badge,
} from '@mui/material';
import {
  PlayArrow,
  CheckCircle,
  DirectionsCar as CarIcon,
  DirectionsBus as BusIcon,
  Schedule as ScheduleIcon,
  Notifications as NotificationsIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  Today as TodayIcon,
  Assignment as AssignmentIcon,
  Person as PersonIcon,
  LocationOn as LocationIcon,
  Speed as SpeedIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';
import { Trip, Vehicle } from '../types';

const DriverInterface: React.FC = () => {
  const { user, logout } = useAuth();
  const [trips, setTrips] = useState<Trip[]>([]);
  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [loading, setLoading] = useState(true);
  const [notifications] = useState(3); // Mock notification count

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
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress size={60} thickness={4} sx={{ color: '#667eea' }} />
        <Typography variant="h6" sx={{ ml: 2, fontWeight: 500 }}>
          Loading driver data...
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, bgcolor: '#f8fafc', minHeight: '100vh' }}>
      <AppBar 
        position="static" 
        elevation={3}
        sx={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          borderBottom: '1px solid rgba(255,255,255,0.1)',
          boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
        }}
      >
        <Toolbar sx={{ py: 1.5 }}>
          <Avatar
            sx={{
              mr: 2,
              bgcolor: 'rgba(255,255,255,0.2)',
              width: 45,
              height: 45,
              boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
              border: '2px solid rgba(255,255,255,0.3)',
            }}
          >
            <BusIcon />
          </Avatar>
          <Typography 
            variant="h5" 
            component="div" 
            sx={{ 
              flexGrow: 1,
              fontWeight: 700,
              letterSpacing: 0.5,
              textShadow: '0 2px 4px rgba(0,0,0,0.1)',
            }}
          >
            Rihla Driver Interface
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Tooltip title="Notifications">
              <IconButton color="inherit">
                <Badge badgeContent={notifications} color="error">
                  <NotificationsIcon />
                </Badge>
              </IconButton>
            </Tooltip>
            <Tooltip title="Settings">
              <IconButton color="inherit">
                <SettingsIcon />
              </IconButton>
            </Tooltip>
            <Box sx={{ 
              display: 'flex', 
              alignItems: 'center', 
              gap: 1,
              bgcolor: 'rgba(255,255,255,0.1)',
              borderRadius: 2,
              px: 2,
              py: 0.5,
            }}>
              <Avatar sx={{ 
                width: 36, 
                height: 36, 
                bgcolor: 'rgba(255,255,255,0.2)',
                border: '2px solid rgba(255,255,255,0.5)',
              }}>
                {user?.username?.charAt(0).toUpperCase()}
              </Avatar>
              <Box>
                <Typography variant="body1" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                  {user?.username}
                </Typography>
                <Typography variant="caption" sx={{ opacity: 0.8 }}>
                  Driver
                </Typography>
              </Box>
            </Box>
            <Button 
              color="inherit" 
              onClick={handleLogout}
              startIcon={<LogoutIcon />}
              sx={{
                ml: 2,
                borderRadius: 2,
                textTransform: 'none',
                fontWeight: 500,
                bgcolor: 'rgba(255,255,255,0.1)',
                px: 2,
                py: 1,
                '&:hover': {
                  bgcolor: 'rgba(255,255,255,0.2)',
                  transform: 'translateY(-2px)',
                  boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
                },
                transition: 'all 0.3s ease',
              }}
            >
              Logout
            </Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        <Grid container spacing={4}>
          {/* Vehicle Information */}
          <Grid size={{ xs: 12, md: 4 }}>
            <Card
              sx={{
                borderRadius: 3,
                background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                color: 'white',
                boxShadow: '0 10px 40px rgba(79, 172, 254, 0.4)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                overflow: 'hidden',
                position: 'relative',
                '&:hover': {
                  transform: 'translateY(-6px)',
                  boxShadow: '0 15px 50px rgba(79, 172, 254, 0.5)',
                },
                '&::before': {
                  content: '""',
                  position: 'absolute',
                  top: 0,
                  right: 0,
                  width: '150px',
                  height: '150px',
                  background: 'radial-gradient(circle, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0) 70%)',
                  borderRadius: '50%',
                }
              }}
            >
              <CardContent sx={{ p: 4 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <Avatar sx={{ 
                    bgcolor: 'rgba(255,255,255,0.2)', 
                    mr: 2, 
                    width: 56, 
                    height: 56,
                    boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                    border: '2px solid rgba(255,255,255,0.3)',
                  }}>
                    <CarIcon sx={{ fontSize: 28 }} />
                  </Avatar>
                  <Typography variant="h5" sx={{ 
                    fontWeight: 700,
                    letterSpacing: 0.5,
                    textShadow: '0 2px 4px rgba(0,0,0,0.1)',
                  }}>
                    My Vehicle
                  </Typography>
                </Box>
                
                {vehicle ? (
                  <Box>
                    <Box sx={{ 
                      mb: 3, 
                      p: 2, 
                      borderRadius: 2, 
                      bgcolor: 'rgba(255,255,255,0.1)',
                      backdropFilter: 'blur(5px)',
                    }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                        <SpeedIcon sx={{ mr: 1, fontSize: 20, opacity: 0.8 }} />
                        <Typography variant="body2" sx={{ opacity: 0.8, fontWeight: 500 }}>
                          Plate Number
                        </Typography>
                      </Box>
                      <Typography variant="h6" sx={{ fontWeight: 700, letterSpacing: 0.5 }}>
                        {vehicle.plateNumber}
                      </Typography>
                    </Box>
                    
                    <Box sx={{ 
                      mb: 3, 
                      p: 2, 
                      borderRadius: 2, 
                      bgcolor: 'rgba(255,255,255,0.1)',
                      backdropFilter: 'blur(5px)',
                    }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                        <CarIcon sx={{ mr: 1, fontSize: 20, opacity: 0.8 }} />
                        <Typography variant="body2" sx={{ opacity: 0.8, fontWeight: 500 }}>
                          Model &amp; Year
                        </Typography>
                      </Box>
                      <Typography variant="h6" sx={{ fontWeight: 700, letterSpacing: 0.5 }}>
                        {vehicle.model} ({vehicle.year})
                      </Typography>
                    </Box>
                    
                    <Box sx={{ 
                      mb: 3, 
                      p: 2, 
                      borderRadius: 2, 
                      bgcolor: 'rgba(255,255,255,0.1)',
                      backdropFilter: 'blur(5px)',
                    }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                        <PersonIcon sx={{ mr: 1, fontSize: 20, opacity: 0.8 }} />
                        <Typography variant="body2" sx={{ opacity: 0.8, fontWeight: 500 }}>
                          Capacity
                        </Typography>
                      </Box>
                      <Typography variant="h6" sx={{ fontWeight: 700, letterSpacing: 0.5 }}>
                        {vehicle.capacity} passengers
                      </Typography>
                    </Box>
                    
                    <Chip
                      label={vehicle.status}
                      color={vehicle.status === 'Active' ? 'success' : 'default'}
                      sx={{ 
                        mt: 1,
                        bgcolor: vehicle.status === 'Active' ? 'rgba(46, 125, 50, 0.3)' : 'rgba(255,255,255,0.2)',
                        color: 'white',
                        fontWeight: 600,
                        px: 2,
                        py: 2.5,
                        borderRadius: 3,
                        '& .MuiChip-label': {
                          px: 1,
                        }
                      }}
                    />
                  </Box>
                ) : (
                  <Box sx={{ 
                    textAlign: 'center', 
                    py: 6,
                    px: 2,
                    borderRadius: 3,
                    bgcolor: 'rgba(255,255,255,0.1)',
                  }}>
                    <Avatar sx={{ 
                      bgcolor: 'rgba(255,255,255,0.2)', 
                      width: 70, 
                      height: 70, 
                      mx: 'auto', 
                      mb: 3,
                      boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                    }}>
                      <CarIcon sx={{ fontSize: 35 }} />
                    </Avatar>
                    <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
                      No vehicle assigned
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.8 }}>
                      Contact administration for vehicle assignment
                    </Typography>
                  </Box>
                )}
              </CardContent>
            </Card>
          </Grid>

          {/* Today's Trips */}
          <Grid size={{ xs: 12, md: 8 }}>
            <Paper 
              sx={{ 
                p: 4, 
                borderRadius: 3,
                boxShadow: '0 8px 30px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
                height: '100%',
                position: 'relative',
                overflow: 'hidden',
                '&::after': {
                  content: '""',
                  position: 'absolute',
                  top: 0,
                  right: 0,
                  width: '150px',
                  height: '150px',
                  background: 'radial-gradient(circle, rgba(103, 126, 234, 0.05) 0%, rgba(103, 126, 234, 0) 70%)',
                  borderRadius: '50%',
                  zIndex: 0,
                }
              }}
            >
              <Box sx={{ 
                display: 'flex', 
                alignItems: 'center', 
                mb: 4,
                position: 'relative',
                zIndex: 1,
              }}>
                <Avatar sx={{ 
                  bgcolor: 'primary.main', 
                  mr: 2, 
                  width: 56, 
                  height: 56,
                  boxShadow: '0 4px 12px rgba(103, 126, 234, 0.3)',
                }}>
                  <TodayIcon sx={{ fontSize: 28 }} />
                </Avatar>
                <Typography variant="h5" sx={{ 
                  fontWeight: 700,
                  letterSpacing: 0.5,
                  background: 'linear-gradient(90deg, #667eea 0%, #764ba2 100%)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                }}>
                  Today's Trips
                </Typography>
              </Box>
              
              {getTodaysTrips().length === 0 ? (
                <Box sx={{ 
                  textAlign: 'center', 
                  py: 8,
                  px: 3,
                  borderRadius: 4,
                  bgcolor: 'rgba(0,0,0,0.02)',
                  border: '1px dashed rgba(0,0,0,0.1)',
                  position: 'relative',
                  zIndex: 1,
                }}>
                  <Avatar sx={{ 
                    bgcolor: 'grey.100', 
                    width: 90, 
                    height: 90, 
                    mx: 'auto', 
                    mb: 3,
                    boxShadow: '0 4px 12px rgba(0,0,0,0.05)',
                  }}>
                    <TodayIcon sx={{ fontSize: 45, color: 'grey.400' }} />
                  </Avatar>
                  <Typography variant="h5" color="textSecondary" gutterBottom sx={{ fontWeight: 600 }}>
                    No trips scheduled for today
                  </Typography>
                  <Typography variant="body1" color="textSecondary" sx={{ maxWidth: '400px', mx: 'auto' }}>
                    Enjoy your day off or check back later for updates
                  </Typography>
                </Box>
              ) : (
                <List sx={{ 
                  p: 0,
                  position: 'relative',
                  zIndex: 1,
                  '& .MuiListItem-root': {
                    transition: 'all 0.3s ease',
                    borderRadius: 2,
                    '&:hover': {
                      bgcolor: 'rgba(0,0,0,0.02)',
                      transform: 'translateX(4px)',
                    }
                  }
                }}>
                  {getTodaysTrips().map((trip, index) => (
                    <React.Fragment key={trip.id}>
                      <ListItem sx={{ 
                        px: 2, 
                        py: 2.5,
                        mb: 1,
                        border: '1px solid rgba(0,0,0,0.05)',
                        borderRadius: 2,
                        boxShadow: '0 2px 8px rgba(0,0,0,0.03)',
                      }}>
                        <Avatar sx={{ 
                          bgcolor: 'primary.light', 
                          mr: 2,
                          width: 50,
                          height: 50,
                          boxShadow: '0 2px 8px rgba(103, 126, 234, 0.2)',
                        }}>
                          <BusIcon sx={{ fontSize: 24 }} />
                        </Avatar>
                        <ListItemText
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <Typography variant="h6" sx={{ fontWeight: 700, mr: 1 }}>
                                {trip.routeName}
                              </Typography>
                              <LocationIcon sx={{ fontSize: 16, color: 'text.secondary', ml: 1 }} />
                            </Box>
                          }
                          secondary={
                            <Box sx={{ mt: 1 }}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                                <ScheduleIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
                                <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                                  Scheduled: {new Date(trip.scheduledStartTime).toLocaleString()}
                                </Typography>
                              </Box>
                              {trip.actualStartTime && (
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <PlayArrow sx={{ fontSize: 16, color: 'success.main' }} />
                                  <Typography variant="body2" color="success.main" sx={{ fontWeight: 500 }}>
                                    Started: {new Date(trip.actualStartTime).toLocaleString()}
                                  </Typography>
                                </Box>
                              )}
                            </Box>
                          }
                        />
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                          <Chip
                            label={trip.status}
                            color={getStatusColor(trip.status) as any}
                            size="medium"
                            sx={{
                              fontWeight: 600,
                              borderRadius: 2,
                              px: 1,
                            }}
                          />
                          {trip.status === 'Scheduled' && (
                            <Tooltip title="Start Trip">
                              <IconButton
                                color="primary"
                                onClick={() => handleStartTrip(trip.id)}
                                sx={{
                                  bgcolor: 'primary.main',
                                  color: 'white',
                                  width: 42,
                                  height: 42,
                                  boxShadow: '0 4px 10px rgba(103, 126, 234, 0.3)',
                                  '&:hover': {
                                    bgcolor: 'primary.dark',
                                    transform: 'scale(1.1)',
                                    boxShadow: '0 6px 15px rgba(103, 126, 234, 0.4)',
                                  },
                                  transition: 'all 0.3s ease',
                                }}
                              >
                                <PlayArrow />
                              </IconButton>
                            </Tooltip>
                          )}
                          {trip.status === 'InProgress' && (
                            <Tooltip title="Complete Trip">
                              <IconButton
                                color="success"
                                onClick={() => handleCompleteTrip(trip.id)}
                                sx={{
                                  bgcolor: 'success.main',
                                  color: 'white',
                                  width: 42,
                                  height: 42,
                                  boxShadow: '0 4px 10px rgba(46, 125, 50, 0.3)',
                                  '&:hover': {
                                    bgcolor: 'success.dark',
                                    transform: 'scale(1.1)',
                                    boxShadow: '0 6px 15px rgba(46, 125, 50, 0.4)',
                                  },
                                  transition: 'all 0.3s ease',
                                }}
                              >
                                <CheckCircle />
                              </IconButton>
                            </Tooltip>
                          )}
                        </Box>
                      </ListItem>
                      {index < getTodaysTrips().length - 1 && <Box sx={{ my: 1 }} />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </Paper>
          </Grid>

          {/* All Trips */}
          <Grid size={{ xs: 12 }}>
            <Paper 
              sx={{ 
                p: 3, 
                borderRadius: 3,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar sx={{ bgcolor: 'secondary.main', mr: 2, width: 48, height: 48 }}>
                  <AssignmentIcon sx={{ fontSize: 24 }} />
                </Avatar>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  All My Trips
                </Typography>
              </Box>
              
              {trips.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 6 }}>
                  <Avatar sx={{ bgcolor: 'grey.100', width: 80, height: 80, mx: 'auto', mb: 2 }}>
                    <AssignmentIcon sx={{ fontSize: 40, color: 'grey.400' }} />
                  </Avatar>
                  <Typography variant="h6" color="textSecondary" gutterBottom>
                    No trips assigned
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Contact your supervisor for trip assignments
                  </Typography>
                </Box>
              ) : (
                <List sx={{ p: 0 }}>
                  {trips.slice(0, 10).map((trip, index) => (
                    <React.Fragment key={trip.id}>
                      <ListItem sx={{ px: 0, py: 2 }}>
                        <Avatar sx={{ bgcolor: 'secondary.light', mr: 2 }}>
                          <BusIcon />
                        </Avatar>
                        <ListItemText
                          primary={
                            <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                              {trip.routeName}
                            </Typography>
                          }
                          secondary={
                            <Box sx={{ mt: 1 }}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                                <ScheduleIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
                                <Typography variant="body2" color="text.secondary">
                                  Scheduled: {new Date(trip.scheduledStartTime).toLocaleString()}
                                </Typography>
                              </Box>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <CarIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
                                <Typography variant="body2" color="text.secondary">
                                  Vehicle: {trip.vehiclePlateNumber}
                                </Typography>
                              </Box>
                            </Box>
                          }
                        />
                        <Chip
                          label={trip.status}
                          color={getStatusColor(trip.status) as any}
                          size="small"
                          sx={{ ml: 1 }}
                        />
                      </ListItem>
                      {index < trips.slice(0, 10).length - 1 && <Divider />}
                    </React.Fragment>
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
export {};
