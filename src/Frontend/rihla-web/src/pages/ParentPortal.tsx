import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
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
  Avatar,
  IconButton,
  Card,
  CardContent,
  Divider,
} from '@mui/material';
import {
  FamilyRestroom as FamilyIcon,
  DirectionsBus as BusIcon,
  Phone as PhoneIcon,
  LocationOn as LocationIcon,
  Schedule as ScheduleIcon,
  Person as PersonIcon,
  Notifications as NotificationsIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  History as HistoryIcon,
  ContactPhone as ContactPhoneIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';
import { Student, Trip } from '../types';
import NotificationCenter from '../components/NotificationCenter';

const ParentPortal: React.FC = () => {
  const { user, logout } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [trips, setTrips] = useState<Trip[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchParentData = async () => {
      try {
        const [studentsResponse, tripsResponse] = await Promise.all([
          apiClient.get<Student[]>('/api/students/my-children'),
          apiClient.get<Trip[]>('/api/trips/my-children'),
        ]);

        setStudents(Array.isArray(studentsResponse) ? studentsResponse : []);
        setTrips(Array.isArray(tripsResponse) ? tripsResponse : []);
      } catch (error) {
        console.error('Error fetching parent data:', error);

        setStudents([
          {
            id: 1,
            firstName: 'Layla',
            lastName: 'Al-Mansouri',
            email: 'layla.mansouri@example.com',
            phoneNumber: '+966501234567',
            address: 'Riyadh, Saudi Arabia',
            parentName: 'Ahmed Al-Mansouri',
            parentPhone: '+966501234568',
            routeId: 1,
            routeName: 'Route A - Downtown',
            isActive: true,
            grade: '8th Grade',
            school: 'Al-Noor International School',
          },
          {
            id: 2,
            firstName: 'Omar',
            lastName: 'Al-Mansouri',
            email: 'omar.mansouri@example.com',
            phoneNumber: '+966501234569',
            address: 'Riyadh, Saudi Arabia',
            parentName: 'Ahmed Al-Mansouri',
            parentPhone: '+966501234568',
            routeId: 1,
            routeName: 'Route A - Downtown',
            isActive: true,
            grade: '5th Grade',
            school: 'Al-Noor International School',
          },
        ]);

        setTrips([
          {
            id: 1,
            routeId: 1,
            routeName: 'Route A - Downtown',
            vehicleId: 1,
            vehiclePlateNumber: 'ABC-123',
            driverId: 1,
            driverName: 'Khalid Al-Otaibi',
            scheduledStartTime: new Date().toISOString(),
            scheduledEndTime: new Date(Date.now() + 3600000).toISOString(),
            status: 'In Progress',
            tripType: 'Morning',
            notes: 'Regular morning pickup',
          },
          {
            id: 2,
            routeId: 1,
            routeName: 'Route A - Downtown',
            vehicleId: 1,
            vehiclePlateNumber: 'ABC-123',
            driverId: 1,
            driverName: 'Khalid Al-Otaibi',
            scheduledStartTime: new Date(Date.now() - 86400000).toISOString(),
            actualStartTime: new Date(
              Date.now() - 86400000 + 300000
            ).toISOString(),
            scheduledEndTime: new Date(
              Date.now() - 86400000 + 3600000
            ).toISOString(),
            actualEndTime: new Date(
              Date.now() - 86400000 + 3900000
            ).toISOString(),
            status: 'Completed',
            tripType: 'Morning',
            notes: 'Completed successfully',
          },
          {
            id: 3,
            routeId: 1,
            routeName: 'Route A - Downtown',
            vehicleId: 1,
            vehiclePlateNumber: 'ABC-123',
            driverId: 1,
            driverName: 'Khalid Al-Otaibi',
            scheduledStartTime: new Date(Date.now() - 172800000).toISOString(),
            actualStartTime: new Date(
              Date.now() - 172800000 + 600000
            ).toISOString(),
            scheduledEndTime: new Date(
              Date.now() - 172800000 + 3600000
            ).toISOString(),
            actualEndTime: new Date(
              Date.now() - 172800000 + 4200000
            ).toISOString(),
            status: 'Completed',
            tripType: 'Afternoon',
            notes: 'Afternoon drop-off completed',
          },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchParentData();
  }, []);

  const handleLogout = async () => {
    await logout();
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

  if (loading) {
    return <Typography>Loading...</Typography>;
  }

  return (
    <Box sx={{ flexGrow: 1, bgcolor: '#f8fafc', minHeight: '100vh' }}>
      <AppBar
        position="static"
        elevation={0}
        sx={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          borderBottom: '1px solid rgba(255,255,255,0.1)',
        }}
      >
        <Toolbar sx={{ py: 1 }}>
          <Avatar
            sx={{
              mr: 2,
              bgcolor: 'rgba(255,255,255,0.2)',
              width: 40,
              height: 40,
            }}
          >
            <FamilyIcon />
          </Avatar>
          <Typography
            variant="h5"
            component="div"
            sx={{
              flexGrow: 1,
              fontWeight: 700,
              letterSpacing: 0.5,
            }}
          >
            Rihla Parent Portal
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <NotificationCenter />
            <IconButton color="inherit">
              <SettingsIcon />
            </IconButton>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar
                sx={{ width: 32, height: 32, bgcolor: 'rgba(255,255,255,0.2)' }}
              >
                {user?.username?.charAt(0).toUpperCase()}
              </Avatar>
              <Typography variant="body1" sx={{ fontWeight: 500 }}>
                Welcome, {user?.username}
              </Typography>
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
                '&:hover': {
                  bgcolor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              Logout
            </Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        <Grid container spacing={3}>
          {/* My Children */}
          <Grid size={{ xs: 12, lg: 8 }}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar
                  sx={{ bgcolor: 'primary.main', mr: 2, width: 48, height: 48 }}
                >
                  <FamilyIcon sx={{ fontSize: 24 }} />
                </Avatar>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  My Children
                </Typography>
              </Box>

              {students.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 6 }}>
                  <Avatar
                    sx={{
                      bgcolor: 'grey.100',
                      width: 80,
                      height: 80,
                      mx: 'auto',
                      mb: 2,
                    }}
                  >
                    <PersonIcon sx={{ fontSize: 40, color: 'grey.400' }} />
                  </Avatar>
                  <Typography variant="h6" color="textSecondary" gutterBottom>
                    No children registered
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Contact the school administration to register your children
                  </Typography>
                </Box>
              ) : (
                <Grid container spacing={2}>
                  {students.map((student) => (
                    <Grid size={{ xs: 12, md: 6 }} key={student.id}>
                      <Card
                        sx={{
                          borderRadius: 2,
                          border: '1px solid rgba(0,0,0,0.08)',
                          transition: 'all 0.3s ease',
                          '&:hover': {
                            transform: 'translateY(-2px)',
                            boxShadow: '0 8px 25px rgba(0,0,0,0.12)',
                          },
                        }}
                      >
                        <CardContent sx={{ p: 3 }}>
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              mb: 2,
                            }}
                          >
                            <Avatar
                              sx={{
                                bgcolor: 'primary.light',
                                mr: 2,
                                width: 48,
                                height: 48,
                                fontSize: '1.2rem',
                                fontWeight: 600,
                              }}
                            >
                              {student.firstName?.charAt(0)}
                              {student.lastName?.charAt(0)}
                            </Avatar>
                            <Box sx={{ flexGrow: 1 }}>
                              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                                {`${student.firstName} ${student.lastName}`}
                              </Typography>
                              <Typography
                                variant="body2"
                                color="text.secondary"
                              >
                                Grade: {student.grade} • School:{' '}
                                {student.school}
                              </Typography>
                            </Box>
                            <Chip
                              label={student.isActive ? 'Active' : 'Inactive'}
                              color={student.isActive ? 'success' : 'default'}
                              size="small"
                            />
                          </Box>
                          <Divider sx={{ my: 2 }} />
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: 2,
                            }}
                          >
                            <LocationIcon color="action" />
                            <Typography variant="body2">
                              Route: {student.routeName || 'Not Assigned'}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: 2,
                              mt: 1,
                            }}
                          >
                            <PhoneIcon color="action" />
                            <Typography variant="body2">
                              {student.phoneNumber}
                            </Typography>
                          </Box>
                        </CardContent>
                      </Card>
                    </Grid>
                  ))}
                </Grid>
              )}
            </Paper>
          </Grid>

          {/* Recent Trips */}
          <Grid size={{ xs: 12, lg: 4 }}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar
                  sx={{
                    bgcolor: 'secondary.main',
                    mr: 2,
                    width: 48,
                    height: 48,
                  }}
                >
                  <BusIcon sx={{ fontSize: 24 }} />
                </Avatar>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  Recent Trips
                </Typography>
              </Box>

              {trips.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 6 }}>
                  <Avatar
                    sx={{
                      bgcolor: 'grey.100',
                      width: 80,
                      height: 80,
                      mx: 'auto',
                      mb: 2,
                    }}
                  >
                    <BusIcon sx={{ fontSize: 40, color: 'grey.400' }} />
                  </Avatar>
                  <Typography variant="h6" color="textSecondary" gutterBottom>
                    No recent trips
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Trip history will appear here once transportation begins
                  </Typography>
                </Box>
              ) : (
                <List sx={{ p: 0 }}>
                  {trips.slice(0, 10).map((trip, index) => (
                    <React.Fragment key={trip.id}>
                      <ListItem sx={{ px: 0, py: 2 }}>
                        <Avatar sx={{ bgcolor: 'primary.light', mr: 2 }}>
                          <BusIcon />
                        </Avatar>
                        <ListItemText
                          primary={
                            <Typography
                              variant="subtitle1"
                              sx={{ fontWeight: 600 }}
                            >
                              {trip.routeName}
                            </Typography>
                          }
                          secondary={
                            <Box sx={{ mt: 1 }}>
                              <Box
                                sx={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  gap: 1,
                                  mb: 0.5,
                                }}
                              >
                                <PersonIcon
                                  sx={{ fontSize: 16, color: 'text.secondary' }}
                                />
                                <Typography
                                  variant="body2"
                                  color="text.secondary"
                                >
                                  Driver: {trip.driverName}
                                </Typography>
                              </Box>
                              <Box
                                sx={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  gap: 1,
                                  mb: 0.5,
                                }}
                              >
                                <BusIcon
                                  sx={{ fontSize: 16, color: 'text.secondary' }}
                                />
                                <Typography
                                  variant="body2"
                                  color="text.secondary"
                                >
                                  Vehicle: {trip.vehiclePlateNumber}
                                </Typography>
                              </Box>
                              <Box
                                sx={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  gap: 1,
                                }}
                              >
                                <ScheduleIcon
                                  sx={{ fontSize: 16, color: 'text.secondary' }}
                                />
                                <Typography
                                  variant="body2"
                                  color="text.secondary"
                                >
                                  {new Date(
                                    trip.scheduledStartTime
                                  ).toLocaleString()}
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

          {/* Quick Actions */}
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
                <Avatar
                  sx={{ bgcolor: 'info.main', mr: 2, width: 48, height: 48 }}
                >
                  <SettingsIcon sx={{ fontSize: 24 }} />
                </Avatar>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  Quick Actions
                </Typography>
              </Box>

              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Button
                    variant="outlined"
                    fullWidth
                    startIcon={<HistoryIcon />}
                    sx={{
                      py: 1.5,
                      borderRadius: 2,
                      textTransform: 'none',
                      fontWeight: 500,
                      borderColor: 'primary.main',
                      color: 'primary.main',
                      '&:hover': {
                        bgcolor: 'primary.main',
                        color: 'white',
                        transform: 'translateY(-2px)',
                        boxShadow: '0 4px 12px rgba(102, 126, 234, 0.3)',
                      },
                      transition: 'all 0.3s ease',
                    }}
                  >
                    View Trip History
                  </Button>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Button
                    variant="outlined"
                    fullWidth
                    startIcon={<ContactPhoneIcon />}
                    sx={{
                      py: 1.5,
                      borderRadius: 2,
                      textTransform: 'none',
                      fontWeight: 500,
                      borderColor: 'secondary.main',
                      color: 'secondary.main',
                      '&:hover': {
                        bgcolor: 'secondary.main',
                        color: 'white',
                        transform: 'translateY(-2px)',
                        boxShadow: '0 4px 12px rgba(156, 39, 176, 0.3)',
                      },
                      transition: 'all 0.3s ease',
                    }}
                  >
                    Contact Driver
                  </Button>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <Button
                    variant="outlined"
                    fullWidth
                    startIcon={<EditIcon />}
                    sx={{
                      py: 1.5,
                      borderRadius: 2,
                      textTransform: 'none',
                      fontWeight: 500,
                      borderColor: 'info.main',
                      color: 'info.main',
                      '&:hover': {
                        bgcolor: 'info.main',
                        color: 'white',
                        transform: 'translateY(-2px)',
                        boxShadow: '0 4px 12px rgba(33, 150, 243, 0.3)',
                      },
                      transition: 'all 0.3s ease',
                    }}
                  >
                    Update Contact Info
                  </Button>
                </Grid>
              </Grid>
            </Paper>
          </Grid>
        </Grid>
      </Container>
    </Box>
  );
};

export default ParentPortal;
