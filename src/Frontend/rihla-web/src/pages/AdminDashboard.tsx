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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Avatar,
  Chip,
  IconButton,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  Alert,
  Fab,
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  DirectionsCar as CarIcon,
  Route as RouteIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  School as SchoolIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { useAuth } from '../hooks/useAuth';
import { apiClient } from '../services/apiClient';
import { DashboardStats, Student, Driver, Vehicle } from '../types';
import NotificationCenter from '../components/NotificationCenter';
import DriverRegistrationForm from '../components/forms/DriverRegistrationForm';
import VehicleRegistrationForm from '../components/forms/VehicleRegistrationForm';

const AdminDashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [showDriverForm, setShowDriverForm] = useState(false);
  const [showVehicleForm, setShowVehicleForm] = useState(false);
  const [expiringCertifications, setExpiringCertifications] = useState<any[]>(
    []
  );

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const [
          statsResponse,
          studentsResponse,
          driversResponse,
          vehiclesResponse,
        ] = await Promise.all([
          apiClient.get<DashboardStats>('/api/dashboard/statistics'),
          apiClient.get<{ data: Student[]; total: number }>(
            '/api/students?page=1&pageSize=10'
          ),
          apiClient.get<{ data: Driver[]; total: number }>(
            '/api/drivers?page=1&pageSize=10'
          ),
          apiClient.get<{ data: Vehicle[]; total: number }>(
            '/api/vehicles?page=1&pageSize=10'
          ),
        ]);

        setStats(statsResponse);
        setStudents(
          Array.isArray(studentsResponse?.data) ? studentsResponse.data : []
        );
        setDrivers(
          Array.isArray(driversResponse?.data) ? driversResponse.data : []
        );
        setVehicles(
          Array.isArray(vehiclesResponse?.data) ? vehiclesResponse.data : []
        );

        try {
          const certificationsResponse = await apiClient.get(
            '/api/drivers/certifications/expiring'
          );
          setExpiringCertifications(
            Array.isArray(certificationsResponse) ? certificationsResponse : []
          );
        } catch {
          setExpiringCertifications([]);
        }
      } catch {
        setStats(null);
        setStudents([]);
        setDrivers([]);
        setVehicles([]);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  const handleLogout = async () => {
    await logout();
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
            <DashboardIcon />
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
            Rihla Admin Dashboard
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
          {/* Statistics Cards */}
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                borderRadius: 3,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: '0 12px 40px rgba(102, 126, 234, 0.4)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ opacity: 0.8, mb: 1 }}>
                      Total Students
                    </Typography>
                    <Typography variant="h3" sx={{ fontWeight: 700 }}>
                      {stats?.totalStudents || 0}
                    </Typography>
                  </Box>
                  <Avatar
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      width: 56,
                      height: 56,
                    }}
                  >
                    <SchoolIcon sx={{ fontSize: 28 }} />
                  </Avatar>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <LinearProgress
                    variant="determinate"
                    value={75}
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: 'rgba(255,255,255,0.8)',
                      },
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{ opacity: 0.8, mt: 1, display: 'block' }}
                  >
                    +12% from last month
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                borderRadius: 3,
                background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(240, 147, 251, 0.3)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: '0 12px 40px rgba(240, 147, 251, 0.4)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ opacity: 0.8, mb: 1 }}>
                      Total Drivers
                    </Typography>
                    <Typography variant="h3" sx={{ fontWeight: 700 }}>
                      {stats?.totalDrivers || 0}
                    </Typography>
                  </Box>
                  <Avatar
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      width: 56,
                      height: 56,
                    }}
                  >
                    <PeopleIcon sx={{ fontSize: 28 }} />
                  </Avatar>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <LinearProgress
                    variant="determinate"
                    value={60}
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: 'rgba(255,255,255,0.8)',
                      },
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{ opacity: 0.8, mt: 1, display: 'block' }}
                  >
                    +8% from last month
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                borderRadius: 3,
                background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(79, 172, 254, 0.3)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: '0 12px 40px rgba(79, 172, 254, 0.4)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ opacity: 0.8, mb: 1 }}>
                      Total Vehicles
                    </Typography>
                    <Typography variant="h3" sx={{ fontWeight: 700 }}>
                      {stats?.totalVehicles || 0}
                    </Typography>
                  </Box>
                  <Avatar
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      width: 56,
                      height: 56,
                    }}
                  >
                    <CarIcon sx={{ fontSize: 28 }} />
                  </Avatar>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <LinearProgress
                    variant="determinate"
                    value={85}
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: 'rgba(255,255,255,0.8)',
                      },
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{ opacity: 0.8, mt: 1, display: 'block' }}
                  >
                    +5% from last month
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                borderRadius: 3,
                background: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(250, 112, 154, 0.3)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: '0 12px 40px rgba(250, 112, 154, 0.4)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ opacity: 0.8, mb: 1 }}>
                      Active Trips
                    </Typography>
                    <Typography variant="h3" sx={{ fontWeight: 700 }}>
                      {stats?.activeTrips || 0}
                    </Typography>
                  </Box>
                  <Avatar
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      width: 56,
                      height: 56,
                    }}
                  >
                    <RouteIcon sx={{ fontSize: 28 }} />
                  </Avatar>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <LinearProgress
                    variant="determinate"
                    value={92}
                    sx={{
                      bgcolor: 'rgba(255,255,255,0.2)',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: 'rgba(255,255,255,0.8)',
                      },
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{ opacity: 0.8, mt: 1, display: 'block' }}
                  >
                    +15% from last month
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          {/* Recent Students */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                  <SchoolIcon />
                </Avatar>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  Recent Students
                </Typography>
              </Box>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        Name
                      </TableCell>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        Email
                      </TableCell>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        Route
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {students.slice(0, 5).map(student => (
                      <TableRow
                        key={student.id}
                        sx={{
                          '&:hover': {
                            bgcolor: 'rgba(0,0,0,0.02)',
                          },
                        }}
                      >
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Avatar
                              sx={{
                                width: 32,
                                height: 32,
                                mr: 2,
                                bgcolor: 'primary.light',
                              }}
                            >
                              {student.firstName?.charAt(0)}
                            </Avatar>
                            {`${student.firstName} ${student.lastName}`}
                          </Box>
                        </TableCell>
                        <TableCell>{student.email}</TableCell>
                        <TableCell>
                          <Chip
                            label={student.routeName || 'Not Assigned'}
                            size="small"
                            color={student.routeName ? 'primary' : 'default'}
                            variant="outlined"
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>
          </Grid>

          {/* Recent Drivers */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 3,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid rgba(0,0,0,0.05)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar sx={{ bgcolor: 'secondary.main', mr: 2 }}>
                  <PeopleIcon />
                </Avatar>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  Recent Drivers
                </Typography>
              </Box>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        Name
                      </TableCell>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        Email
                      </TableCell>
                      <TableCell
                        sx={{ fontWeight: 600, color: 'text.secondary' }}
                      >
                        License
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {drivers.slice(0, 5).map(driver => (
                      <TableRow
                        key={driver.id}
                        sx={{
                          '&:hover': {
                            bgcolor: 'rgba(0,0,0,0.02)',
                          },
                        }}
                      >
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Avatar
                              sx={{
                                width: 32,
                                height: 32,
                                mr: 2,
                                bgcolor: 'secondary.light',
                              }}
                            >
                              {driver.firstName?.charAt(0)}
                            </Avatar>
                            {`${driver.firstName} ${driver.lastName}`}
                          </Box>
                        </TableCell>
                        <TableCell>{driver.email}</TableCell>
                        <TableCell>
                          <Chip
                            label={driver.licenseNumber}
                            size="small"
                            color="secondary"
                            variant="outlined"
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>
          </Grid>

          {/* Driver Certification Alerts */}
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
                <Avatar sx={{ bgcolor: 'warning.main', mr: 2 }}>
                  <WarningIcon />
                </Avatar>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  Driver Certification Alerts
                </Typography>
              </Box>

              {expiringCertifications.length > 0 ? (
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  {expiringCertifications.slice(0, 5).map((alert, index) => (
                    <Alert
                      key={index}
                      severity="warning"
                      sx={{ borderRadius: 2 }}
                    >
                      <Typography variant="body2">
                        <strong>{alert.driverName}</strong> -{' '}
                        {alert.certificationType} expires on {alert.expiryDate}
                      </Typography>
                    </Alert>
                  ))}
                  {expiringCertifications.length > 5 && (
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{ textAlign: 'center', mt: 1 }}
                    >
                      +{expiringCertifications.length - 5} more certifications
                      expiring soon
                    </Typography>
                  )}
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No certifications expiring in the next 30 days
                </Typography>
              )}
            </Paper>
          </Grid>
        </Grid>
      </Container>

      {/* Floating Action Buttons */}
      <Box sx={{ position: 'fixed', bottom: 24, right: 24, zIndex: 1000 }}>
        <Fab
          color="primary"
          aria-label="add driver"
          sx={{ mb: 2 }}
          onClick={() => setShowDriverForm(true)}
        >
          <PeopleIcon />
        </Fab>
        <br />
        <Fab
          color="secondary"
          aria-label="add vehicle"
          onClick={() => setShowVehicleForm(true)}
        >
          <CarIcon />
        </Fab>
      </Box>

      {/* Driver Registration Modal */}
      <Dialog
        open={showDriverForm}
        onClose={() => setShowDriverForm(false)}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: {
            borderRadius: 3,
            maxHeight: '90vh',
          },
        }}
      >
        <DialogTitle sx={{ pb: 1 }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Add New Driver
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ px: 3 }}>
          <DriverRegistrationForm
            onSuccess={() => {
              setShowDriverForm(false);
              const fetchDashboardData = async () => {
                try {
                  const [
                    statsResponse,
                    studentsResponse,
                    driversResponse,
                    vehiclesResponse,
                  ] = await Promise.all([
                    apiClient.get('/api/dashboard/stats'),
                    apiClient.get('/api/students'),
                    apiClient.get('/api/drivers'),
                    apiClient.get('/api/vehicles'),
                  ]);

                  setStats(statsResponse as DashboardStats);
                  setStudents(studentsResponse as Student[]);
                  setDrivers(driversResponse as Driver[]);
                  setVehicles(vehiclesResponse as Vehicle[]);
                } catch (error) {}
              };
              fetchDashboardData();
            }}
            onCancel={() => setShowDriverForm(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Vehicle Registration Modal */}
      <Dialog
        open={showVehicleForm}
        onClose={() => setShowVehicleForm(false)}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: {
            borderRadius: 3,
            maxHeight: '90vh',
          },
        }}
      >
        <DialogTitle sx={{ pb: 1 }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Add New Vehicle
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ px: 3 }}>
          <VehicleRegistrationForm
            onSuccess={() => {
              setShowVehicleForm(false);
              const fetchDashboardData = async () => {
                try {
                  const [
                    statsResponse,
                    studentsResponse,
                    driversResponse,
                    vehiclesResponse,
                  ] = await Promise.all([
                    apiClient.get('/api/dashboard/stats'),
                    apiClient.get('/api/students'),
                    apiClient.get('/api/drivers'),
                    apiClient.get('/api/vehicles'),
                  ]);

                  setStats(statsResponse as DashboardStats);
                  setStudents(studentsResponse as Student[]);
                  setDrivers(driversResponse as Driver[]);
                  setVehicles(vehiclesResponse as Vehicle[]);
                } catch (error) {}
              };
              fetchDashboardData();
            }}
            onCancel={() => setShowVehicleForm(false)}
          />
        </DialogContent>
      </Dialog>
    </Box>
  );
};

export default AdminDashboard;
