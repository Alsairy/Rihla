import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert,
  LinearProgress,
  Avatar,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  DirectionsCar as CarIcon,
  Build as BuildIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Security as SecurityIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Assignment as AssignmentIcon,
  LocalGasStation as FuelIcon,
  Speed as SpeedIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';
import { signalRService } from '../services/signalRService';
import VehicleRegistrationForm from './forms/VehicleRegistrationForm';
import MaintenanceScheduler from './MaintenanceScheduler';

interface VehicleManagementDashboardProps {
  showAddButton?: boolean;
}

interface Vehicle {
  id: number;
  vehicleId: string;
  make: string;
  model: string;
  year: number;
  licensePlate: string;
  color: string;
  capacity: number;
  fuelType: string;
  mileage: number;
  status: 'Active' | 'Maintenance' | 'Inactive' | 'OutOfService';
  insuranceExpiryDate: string;
  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  assignedRoute?: string;
  assignedDriver?: string;
}

interface MaintenanceAlert {
  id: number;
  vehicleId: number;
  vehicleName: string;
  maintenanceType: string;
  dueDate: string;
  overdueDays?: number;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
}

interface InsuranceAlert {
  id: number;
  vehicleId: number;
  vehicleName: string;
  expiryDate: string;
  daysUntilExpiry: number;
}

const VehicleManagementDashboard: React.FC<VehicleManagementDashboardProps> = ({
  showAddButton = true,
}) => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [maintenanceAlerts, setMaintenanceAlerts] = useState<MaintenanceAlert[]>([]);
  const [insuranceAlerts, setInsuranceAlerts] = useState<InsuranceAlert[]>([]);
  const [fleetStats, setFleetStats] = useState({
    totalVehicles: 0,
    activeVehicles: 0,
    inMaintenance: 0,
    outOfService: 0,
    averageAge: 0,
    totalMileage: 0,
  });

  const [showAddVehicleDialog, setShowAddVehicleDialog] = useState(false);
  const [showMaintenanceDialog, setShowMaintenanceDialog] = useState(false);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(null);
  const [showVehicleDetails, setShowVehicleDetails] = useState(false);
  const [selectedVehicle, setSelectedVehicle] = useState<Vehicle | null>(null);

  const [statusFilter, setStatusFilter] = useState<string>('All');
  const [fuelTypeFilter, setFuelTypeFilter] = useState<string>('All');

  useEffect(() => {
    fetchVehicleData();

    const setupRealTimeUpdates = () => {
      signalRService.onVehicleStatusChanged((vehicleId: string, oldStatus: string, newStatus: string) => {
        setVehicles(prev => prev.map(vehicle => 
          vehicle.id.toString() === vehicleId 
            ? { ...vehicle, status: newStatus as "Active" | "Maintenance" | "Inactive" | "OutOfService" }
            : vehicle
        ));
        fetchVehicleData();
      });

      signalRService.onVehicleMaintenanceUpdated((vehicleId: string, maintenanceType: string, status: string) => {
        fetchVehicleData();
      });

      signalRService.onMaintenanceAlertCreated((alert: any) => {
        setMaintenanceAlerts(prev => [...prev, alert]);
      });

      signalRService.onInsuranceExpirationAlert((alert: any) => {
        setInsuranceAlerts(prev => [...prev, alert]);
      });
    };

    if (signalRService.isConnectionActive()) {
      setupRealTimeUpdates();
    } else {
      signalRService.startConnection().then(() => {
        setupRealTimeUpdates();
      });
    }

    return () => {
      signalRService.removeAllListeners();
    };
  }, []);

  const fetchVehicleData = async () => {
    setLoading(true);
    try {
      const [
        vehiclesResponse,
        maintenanceAlertsResponse,
        insuranceAlertsResponse,
        statsResponse,
      ] = await Promise.all([
        apiClient.get('/api/vehicles'),
        apiClient.get('/api/maintenance/alerts'),
        apiClient.get('/api/vehicles/insurance/expiring'),
        apiClient.get('/api/vehicles/stats'),
      ]);

      const vehiclesData = Array.isArray(vehiclesResponse) ? vehiclesResponse : (vehiclesResponse as any)?.data || [];
      setVehicles(vehiclesData);
      setMaintenanceAlerts(Array.isArray(maintenanceAlertsResponse) ? maintenanceAlertsResponse : []);
      setInsuranceAlerts(Array.isArray(insuranceAlertsResponse) ? insuranceAlertsResponse : []);
      
      if (statsResponse && typeof statsResponse === 'object') {
        setFleetStats(statsResponse as any);
      } else {
        calculateFleetStats(vehiclesData);
      }
    } catch (error) {
      console.error('Error fetching vehicle data:', error);
      setError('Failed to load vehicle data');
    } finally {
      setLoading(false);
    }
  };

  const calculateFleetStats = (vehiclesData: Vehicle[]) => {
    const currentYear = new Date().getFullYear();
    const stats = {
      totalVehicles: vehiclesData.length,
      activeVehicles: vehiclesData.filter(v => v.status === 'Active').length,
      inMaintenance: vehiclesData.filter(v => v.status === 'Maintenance').length,
      outOfService: vehiclesData.filter(v => v.status === 'OutOfService').length,
      averageAge: vehiclesData.length > 0 
        ? Math.round(vehiclesData.reduce((sum, v) => sum + (currentYear - v.year), 0) / vehiclesData.length)
        : 0,
      totalMileage: vehiclesData.reduce((sum, v) => sum + (v.mileage || 0), 0),
    };
    setFleetStats(stats);
  };

  const handleDeleteVehicle = async (vehicleId: number) => {
    if (!window.confirm('Are you sure you want to delete this vehicle?')) return;

    try {
      await apiClient.delete(`/api/vehicles/${vehicleId}`);
      fetchVehicleData();
    } catch (error) {
      console.error('Error deleting vehicle:', error);
      setError('Failed to delete vehicle');
    }
  };

  const handleUpdateVehicleStatus = async (vehicleId: number, newStatus: string) => {
    try {
      await apiClient.put(`/api/vehicles/${vehicleId}/status`, { status: newStatus });
      fetchVehicleData();
    } catch (error) {
      console.error('Error updating vehicle status:', error);
      setError('Failed to update vehicle status');
    }
  };

  const openVehicleDetails = (vehicle: Vehicle) => {
    setSelectedVehicle(vehicle);
    setShowVehicleDetails(true);
  };

  const openMaintenanceScheduler = (vehicleId: number) => {
    setSelectedVehicleId(vehicleId);
    setShowMaintenanceDialog(true);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active': return 'success';
      case 'Maintenance': return 'warning';
      case 'Inactive': return 'default';
      case 'OutOfService': return 'error';
      default: return 'default';
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Critical': return 'error';
      case 'High': return 'warning';
      case 'Medium': return 'info';
      case 'Low': return 'success';
      default: return 'default';
    }
  };

  const filteredVehicles = vehicles.filter(vehicle => {
    const statusMatch = statusFilter === 'All' || vehicle.status === statusFilter;
    const fuelMatch = fuelTypeFilter === 'All' || vehicle.fuelType === fuelTypeFilter;
    return statusMatch && fuelMatch;
  });

  const uniqueStatuses = ['All', ...Array.from(new Set(vehicles.map(v => v.status)))];
  const uniqueFuelTypes = ['All', ...Array.from(new Set(vehicles.map(v => v.fuelType)))];

  return (
    <Box sx={{ p: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {loading && <LinearProgress sx={{ mb: 3 }} />}

      {/* Fleet Overview Stats */}
      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 4 }}>
        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <CarIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {fleetStats.totalVehicles}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Total Vehicles
              </Typography>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #4CAF50 0%, #45a049 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <CheckIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {fleetStats.activeVehicles}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Active
              </Typography>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #FF9800 0%, #F57C00 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <BuildIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {fleetStats.inMaintenance}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                In Maintenance
              </Typography>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #F44336 0%, #D32F2F 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <ErrorIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {fleetStats.outOfService}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Out of Service
              </Typography>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #9C27B0 0%, #7B1FA2 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <ScheduleIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {fleetStats.averageAge}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Avg Age (Years)
              </Typography>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ flex: '1 1 200px', minWidth: '180px' }}>
          <Card sx={{ borderRadius: 3, background: 'linear-gradient(135deg, #00BCD4 0%, #0097A7 100%)', color: 'white' }}>
            <CardContent sx={{ textAlign: 'center', py: 3 }}>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', mx: 'auto', mb: 2, width: 56, height: 56 }}>
                <SpeedIcon sx={{ fontSize: 28 }} />
              </Avatar>
              <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                {Math.round(fleetStats.totalMileage / 1000)}K
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                Total Mileage
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Alerts Section */}
      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 4 }}>
        {/* Maintenance Alerts */}
        {maintenanceAlerts.length > 0 && (
          <Box sx={{ flex: '1 1 400px', minWidth: '300px' }}>
            <Card sx={{ borderRadius: 3, border: '2px solid', borderColor: 'warning.main' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Avatar sx={{ bgcolor: 'warning.main', mr: 2 }}>
                    <BuildIcon />
                  </Avatar>
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Maintenance Alerts ({maintenanceAlerts.length})
                  </Typography>
                </Box>
                
                <List sx={{ maxHeight: 300, overflow: 'auto' }}>
                  {maintenanceAlerts.slice(0, 5).map((alert, index) => (
                    <ListItem key={alert.id} sx={{ px: 0 }}>
                      <ListItemIcon>
                        <Chip
                          label={alert.priority}
                          color={getPriorityColor(alert.priority) as any}
                          size="small"
                        />
                      </ListItemIcon>
                      <ListItemText
                        primary={`${alert.vehicleName} - ${alert.maintenanceType}`}
                        secondary={`Due: ${new Date(alert.dueDate).toLocaleDateString()}${
                          alert.overdueDays ? ` (${alert.overdueDays} days overdue)` : ''
                        }`}
                      />
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={() => openMaintenanceScheduler(alert.vehicleId)}
                      >
                        Schedule
                      </Button>
                    </ListItem>
                  ))}
                </List>
                
                {maintenanceAlerts.length > 5 && (
                  <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', mt: 1 }}>
                    +{maintenanceAlerts.length - 5} more alerts
                  </Typography>
                )}
              </CardContent>
            </Card>
          </Box>
        )}

        {/* Insurance Expiration Alerts */}
        {insuranceAlerts.length > 0 && (
          <Box sx={{ flex: '1 1 400px', minWidth: '300px' }}>
            <Card sx={{ borderRadius: 3, border: '2px solid', borderColor: 'error.main' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Avatar sx={{ bgcolor: 'error.main', mr: 2 }}>
                    <SecurityIcon />
                  </Avatar>
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Insurance Expiring ({insuranceAlerts.length})
                  </Typography>
                </Box>
                
                <List sx={{ maxHeight: 300, overflow: 'auto' }}>
                  {insuranceAlerts.slice(0, 5).map((alert, index) => (
                    <ListItem key={alert.id} sx={{ px: 0 }}>
                      <ListItemIcon>
                        <SecurityIcon color={alert.daysUntilExpiry <= 30 ? 'error' : 'warning'} />
                      </ListItemIcon>
                      <ListItemText
                        primary={alert.vehicleName}
                        secondary={`Expires: ${new Date(alert.expiryDate).toLocaleDateString()} (${alert.daysUntilExpiry} days)`}
                      />
                      <Chip
                        label={alert.daysUntilExpiry <= 30 ? 'Critical' : 'Warning'}
                        color={alert.daysUntilExpiry <= 30 ? 'error' : 'warning'}
                        size="small"
                      />
                    </ListItem>
                  ))}
                </List>
                
                {insuranceAlerts.length > 5 && (
                  <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', mt: 1 }}>
                    +{insuranceAlerts.length - 5} more expiring soon
                  </Typography>
                )}
              </CardContent>
            </Card>
          </Box>
        )}
      </Box>

      {/* Action Bar */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Status</InputLabel>
            <Select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              label="Status"
            >
              {uniqueStatuses.map((status) => (
                <MenuItem key={status} value={status}>
                  {status}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Fuel Type</InputLabel>
            <Select
              value={fuelTypeFilter}
              onChange={(e) => setFuelTypeFilter(e.target.value)}
              label="Fuel Type"
            >
              {uniqueFuelTypes.map((fuelType) => (
                <MenuItem key={fuelType} value={fuelType}>
                  {fuelType}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {showAddButton && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setShowAddVehicleDialog(true)}
            sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}
          >
            Add Vehicle
          </Button>
        )}
      </Box>

      {/* Vehicle Fleet Table */}
      <Card sx={{ borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Fleet Management ({filteredVehicles.length} vehicles)
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Vehicle</TableCell>
                  <TableCell>Details</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Mileage</TableCell>
                  <TableCell>Assignment</TableCell>
                  <TableCell>Insurance</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredVehicles.map((vehicle) => (
                  <TableRow key={vehicle.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Avatar sx={{ bgcolor: 'primary.light', mr: 2, width: 40, height: 40 }}>
                          <CarIcon />
                        </Avatar>
                        <Box>
                          <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                            {vehicle.vehicleId}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {vehicle.licensePlate}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {vehicle.make} {vehicle.model}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {vehicle.year} • {vehicle.color} • {vehicle.capacity} seats
                      </Typography>
                    </TableCell>
                    
                    <TableCell>
                      <Chip
                        label={vehicle.status}
                        color={getStatusColor(vehicle.status) as any}
                        size="small"
                      />
                    </TableCell>
                    
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <SpeedIcon sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
                        <Typography variant="body2">
                          {vehicle.mileage?.toLocaleString() || 0} km
                        </Typography>
                      </Box>
                    </TableCell>
                    
                    <TableCell>
                      <Box>
                        {vehicle.assignedRoute && (
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Route: {vehicle.assignedRoute}
                          </Typography>
                        )}
                        {vehicle.assignedDriver && (
                          <Typography variant="caption" color="text.secondary">
                            Driver: {vehicle.assignedDriver}
                          </Typography>
                        )}
                        {!vehicle.assignedRoute && !vehicle.assignedDriver && (
                          <Typography variant="body2" color="text.secondary">
                            Unassigned
                          </Typography>
                        )}
                      </Box>
                    </TableCell>
                    
                    <TableCell>
                      <Typography variant="body2">
                        {new Date(vehicle.insuranceExpiryDate).toLocaleDateString()}
                      </Typography>
                      {new Date(vehicle.insuranceExpiryDate) <= new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) && (
                        <Chip label="Expiring Soon" color="warning" size="small" sx={{ mt: 0.5 }} />
                      )}
                    </TableCell>
                    
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="View Details">
                          <IconButton size="small" onClick={() => openVehicleDetails(vehicle)}>
                            <ViewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Maintenance">
                          <IconButton size="small" onClick={() => openMaintenanceScheduler(vehicle.id)}>
                            <BuildIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Edit">
                          <IconButton size="small">
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton 
                            size="small" 
                            color="error"
                            onClick={() => handleDeleteVehicle(vehicle.id)}
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {filteredVehicles.length === 0 && (
            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
              No vehicles found matching the current filters.
            </Typography>
          )}
        </CardContent>
      </Card>

      {/* Add Vehicle Dialog */}
      <Dialog
        open={showAddVehicleDialog}
        onClose={() => setShowAddVehicleDialog(false)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: 3, maxHeight: '90vh' } }}
      >
        <DialogTitle>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Add New Vehicle
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ px: 3 }}>
          <VehicleRegistrationForm
            onSuccess={() => {
              setShowAddVehicleDialog(false);
              fetchVehicleData();
            }}
            onCancel={() => setShowAddVehicleDialog(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Vehicle Details Dialog */}
      <Dialog
        open={showVehicleDetails}
        onClose={() => setShowVehicleDetails(false)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: 3 } }}
      >
        <DialogTitle>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Vehicle Details
          </Typography>
        </DialogTitle>
        <DialogContent>
          {selectedVehicle && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, pt: 2 }}>
              <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
                <Box sx={{ flex: '1 1 300px', minWidth: '250px' }}>
                  <Typography variant="subtitle2" color="text.secondary">Vehicle ID</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.vehicleId}</Typography>
                  
                  <Typography variant="subtitle2" color="text.secondary">Make & Model</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.make} {selectedVehicle.model}</Typography>
                  
                  <Typography variant="subtitle2" color="text.secondary">Year</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.year}</Typography>
                  
                  <Typography variant="subtitle2" color="text.secondary">License Plate</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.licensePlate}</Typography>
                </Box>
                
                <Box sx={{ flex: '1 1 300px', minWidth: '250px' }}>
                  <Typography variant="subtitle2" color="text.secondary">Status</Typography>
                  <Chip 
                    label={selectedVehicle.status} 
                    color={getStatusColor(selectedVehicle.status) as any}
                    sx={{ mb: 2 }}
                  />
                  
                  <Typography variant="subtitle2" color="text.secondary">Capacity</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.capacity} passengers</Typography>
                  
                  <Typography variant="subtitle2" color="text.secondary">Fuel Type</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.fuelType}</Typography>
                  
                  <Typography variant="subtitle2" color="text.secondary">Current Mileage</Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.mileage?.toLocaleString()} km</Typography>
                </Box>
              </Box>
              
              <Box>
                <Divider sx={{ my: 2 }} />
                <Typography variant="subtitle2" color="text.secondary">Insurance Expiry</Typography>
                <Typography variant="body1" sx={{ mb: 2 }}>
                  {new Date(selectedVehicle.insuranceExpiryDate).toLocaleDateString()}
                </Typography>
                
                {selectedVehicle.assignedRoute && (
                  <>
                    <Typography variant="subtitle2" color="text.secondary">Assigned Route</Typography>
                    <Typography variant="body1" sx={{ mb: 2 }}>{selectedVehicle.assignedRoute}</Typography>
                  </>
                )}
                
                {selectedVehicle.assignedDriver && (
                  <>
                    <Typography variant="subtitle2" color="text.secondary">Assigned Driver</Typography>
                    <Typography variant="body1">{selectedVehicle.assignedDriver}</Typography>
                  </>
                )}
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowVehicleDetails(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Maintenance Scheduler Dialog */}
      <Dialog
        open={showMaintenanceDialog}
        onClose={() => setShowMaintenanceDialog(false)}
        maxWidth="lg"
        fullWidth
        PaperProps={{ sx: { borderRadius: 3, maxHeight: '90vh' } }}
      >
        <DialogTitle>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Maintenance Scheduler
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ px: 0 }}>
          {selectedVehicleId && (
            <MaintenanceScheduler
              vehicleId={selectedVehicleId}
              showVehicleSelector={false}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowMaintenanceDialog(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default VehicleManagementDashboard;
