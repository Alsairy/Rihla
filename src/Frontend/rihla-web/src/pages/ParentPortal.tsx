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
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';
import { Student, Trip } from '../types';

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

        setStudents(studentsResponse);
        setTrips(tripsResponse);
      } catch (error) {
        console.error('Error fetching parent data:', error);
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
    <Box sx={{ flexGrow: 1 }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Rihla Parent Portal
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
          {/* My Children */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                My Children
              </Typography>
              {students.length === 0 ? (
                <Typography color="textSecondary">
                  No children registered
                </Typography>
              ) : (
                <List>
                  {students.map((student) => (
                    <ListItem key={student.id}>
                      <ListItemText
                        primary={`${student.firstName} ${student.lastName}`}
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              Route: {student.routeName || 'Not Assigned'}
                            </Typography>
                            <Typography variant="body2">
                              Phone: {student.phoneNumber}
                            </Typography>
                          </Box>
                        }
                      />
                      <Chip
                        label={student.isActive ? 'Active' : 'Inactive'}
                        color={student.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </Paper>
          </Grid>

          {/* Recent Trips */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                Recent Trips
              </Typography>
              {trips.length === 0 ? (
                <Typography color="textSecondary">
                  No recent trips
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
                              Driver: {trip.driverName}
                            </Typography>
                            <Typography variant="body2">
                              Vehicle: {trip.vehiclePlateNumber}
                            </Typography>
                            <Typography variant="body2">
                              Scheduled: {new Date(trip.scheduledStartTime).toLocaleString()}
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

          {/* Quick Actions */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                Quick Actions
              </Typography>
              <Grid container spacing={2}>
                <Grid size="auto">
                  <Button variant="outlined" color="primary">
                    View Trip History
                  </Button>
                </Grid>
                <Grid size="auto">
                  <Button variant="outlined" color="primary">
                    Contact Driver
                  </Button>
                </Grid>
                <Grid size="auto">
                  <Button variant="outlined" color="primary">
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
