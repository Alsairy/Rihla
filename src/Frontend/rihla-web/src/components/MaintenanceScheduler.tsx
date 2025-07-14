import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  Avatar,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Build as BuildIcon,
  Schedule as ScheduleIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  Error as ErrorIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  CarRepair as CarRepairIcon,
  LocalGasStation as OilIcon,
  Security as SecurityIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../services/apiClient';

interface MaintenanceSchedulerProps {
  vehicleId?: number;
  showVehicleSelector?: boolean;
}

interface Vehicle {
  id: number;
  vehicleId: string;
  make: string;
  model: string;
  year: number;
  licensePlate: string;
}

interface MaintenanceRecord {
  id: number;
  vehicleId: number;
  maintenanceType: string;
  description: string;
  scheduledDate: string;
  completedDate?: string;
  cost?: number;
  status: 'Scheduled' | 'InProgress' | 'Completed' | 'Overdue';
  nextServiceDate?: string;
  notes?: string;
  performedBy?: string;
}

interface MaintenanceInterval {
  id: number;
  vehicleId: number;
  maintenanceType: string;
  intervalDays: number;
  intervalMileage?: number;
  lastServiceDate?: string;
  nextServiceDate: string;
  isActive: boolean;
}

const MaintenanceScheduler: React.FC<MaintenanceSchedulerProps> = ({
  vehicleId,
  showVehicleSelector = true,
}) => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(vehicleId || null);
  const [maintenanceRecords, setMaintenanceRecords] = useState<MaintenanceRecord[]>([]);
  const [maintenanceIntervals, setMaintenanceIntervals] = useState<MaintenanceInterval[]>([]);
  const [overdueMaintenances, setOverdueMaintenances] = useState<MaintenanceRecord[]>([]);

  const [showScheduleDialog, setShowScheduleDialog] = useState(false);
  const [showIntervalDialog, setShowIntervalDialog] = useState(false);
  const [editingRecord, setEditingRecord] = useState<MaintenanceRecord | null>(null);
  const [editingInterval, setEditingInterval] = useState<MaintenanceInterval | null>(null);

  const [maintenanceType, setMaintenanceType] = useState('');
  const [description, setDescription] = useState('');
  const [scheduledDate, setScheduledDate] = useState('');
  const [estimatedCost, setEstimatedCost] = useState('');
  const [notes, setNotes] = useState('');

  const [intervalType, setIntervalType] = useState('');
  const [intervalDays, setIntervalDays] = useState('');
  const [intervalMileage, setIntervalMileage] = useState('');

  const maintenanceTypes = [
    { value: 'Oil Change', icon: <OilIcon />, color: '#4CAF50' },
    { value: 'Brake Inspection', icon: <SecurityIcon />, color: '#FF9800' },
    { value: 'Safety Inspection', icon: <SecurityIcon />, color: '#F44336' },
    { value: 'Tire Rotation', icon: <CarRepairIcon />, color: '#2196F3' },
    { value: 'Engine Service', icon: <BuildIcon />, color: '#9C27B0' },
    { value: 'Transmission Service', icon: <BuildIcon />, color: '#607D8B' },
    { value: 'Air Filter Replacement', icon: <BuildIcon />, color: '#795548' },
    { value: 'Battery Check', icon: <BuildIcon />, color: '#FF5722' },
    { value: 'Coolant Service', icon: <BuildIcon />, color: '#00BCD4' },
    { value: 'General Inspection', icon: <CheckIcon />, color: '#8BC34A' },
  ];

  useEffect(() => {
    if (showVehicleSelector) {
      fetchVehicles();
    }
  }, [showVehicleSelector]);

  useEffect(() => {
    if (selectedVehicleId) {
      fetchMaintenanceData();
    }
  }, [selectedVehicleId]);

  const fetchVehicles = async () => {
    try {
      const response = await apiClient.get('/api/vehicles');
      const vehiclesData = Array.isArray(response) ? response : (response as any)?.data || [];
      setVehicles(vehiclesData);
    } catch (error) {
      console.error('Error fetching vehicles:', error);
      setError('Failed to load vehicles');
    }
  };

  const fetchMaintenanceData = async () => {
    if (!selectedVehicleId) return;

    setLoading(true);
    try {
      const [recordsResponse, intervalsResponse, overdueResponse] = await Promise.all([
        apiClient.get(`/api/maintenance/vehicle/${selectedVehicleId}`),
        apiClient.get(`/api/maintenance/intervals/${selectedVehicleId}`),
        apiClient.get(`/api/maintenance/overdue/${selectedVehicleId}`),
      ]);

      setMaintenanceRecords(Array.isArray(recordsResponse) ? recordsResponse : []);
      setMaintenanceIntervals(Array.isArray(intervalsResponse) ? intervalsResponse : []);
      setOverdueMaintenances(Array.isArray(overdueResponse) ? overdueResponse : []);
    } catch (error) {
      console.error('Error fetching maintenance data:', error);
      setError('Failed to load maintenance data');
    } finally {
      setLoading(false);
    }
  };

  const handleScheduleMaintenance = async () => {
    if (!selectedVehicleId || !maintenanceType || !scheduledDate) return;

    try {
      const maintenanceData = {
        vehicleId: selectedVehicleId,
        maintenanceType,
        description,
        scheduledDate,
        estimatedCost: estimatedCost ? parseFloat(estimatedCost) : undefined,
        notes,
        status: 'Scheduled',
        tenantId: user?.tenantId,
      };

      if (editingRecord) {
        await apiClient.put(`/api/maintenance/${editingRecord.id}`, maintenanceData);
      } else {
        await apiClient.post('/api/maintenance', maintenanceData);
      }

      setShowScheduleDialog(false);
      resetScheduleForm();
      fetchMaintenanceData();
    } catch (error) {
      console.error('Error scheduling maintenance:', error);
      setError('Failed to schedule maintenance');
    }
  };

  const handleSetInterval = async () => {
    if (!selectedVehicleId || !intervalType || !intervalDays) return;

    try {
      const intervalData = {
        vehicleId: selectedVehicleId,
        maintenanceType: intervalType,
        intervalDays: parseInt(intervalDays),
        intervalMileage: intervalMileage ? parseInt(intervalMileage) : undefined,
        isActive: true,
        tenantId: user?.tenantId,
      };

      if (editingInterval) {
        await apiClient.put(`/api/maintenance/intervals/${editingInterval.id}`, intervalData);
      } else {
        await apiClient.post('/api/maintenance/intervals', intervalData);
      }

      setShowIntervalDialog(false);
      resetIntervalForm();
      fetchMaintenanceData();
    } catch (error) {
      console.error('Error setting maintenance interval:', error);
      setError('Failed to set maintenance interval');
    }
  };

  const handleCompleteMaintenace = async (recordId: number) => {
    try {
      await apiClient.put(`/api/maintenance/${recordId}/complete`, {
        completedDate: new Date().toISOString(),
        status: 'Completed',
      });
      fetchMaintenanceData();
    } catch (error) {
      console.error('Error completing maintenance:', error);
      setError('Failed to complete maintenance');
    }
  };

  const handleDeleteRecord = async (recordId: number) => {
    try {
      await apiClient.delete(`/api/maintenance/${recordId}`);
      fetchMaintenanceData();
    } catch (error) {
      console.error('Error deleting maintenance record:', error);
      setError('Failed to delete maintenance record');
    }
  };

  const resetScheduleForm = () => {
    setMaintenanceType('');
    setDescription('');
    setScheduledDate('');
    setEstimatedCost('');
    setNotes('');
    setEditingRecord(null);
  };

  const resetIntervalForm = () => {
    setIntervalType('');
    setIntervalDays('');
    setIntervalMileage('');
    setEditingInterval(null);
  };

  const openEditDialog = (record: MaintenanceRecord) => {
    setEditingRecord(record);
    setMaintenanceType(record.maintenanceType);
    setDescription(record.description);
    setScheduledDate(record.scheduledDate.split('T')[0]);
    setEstimatedCost(record.cost?.toString() || '');
    setNotes(record.notes || '');
    setShowScheduleDialog(true);
  };

  const openEditIntervalDialog = (interval: MaintenanceInterval) => {
    setEditingInterval(interval);
    setIntervalType(interval.maintenanceType);
    setIntervalDays(interval.intervalDays.toString());
    setIntervalMileage(interval.intervalMileage?.toString() || '');
    setShowIntervalDialog(true);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed': return 'success';
      case 'InProgress': return 'info';
      case 'Scheduled': return 'default';
      case 'Overdue': return 'error';
      default: return 'default';
    }
  };

  const getMaintenanceTypeIcon = (type: string) => {
    const maintenanceType = maintenanceTypes.find(mt => mt.value === type);
    return maintenanceType?.icon || <BuildIcon />;
  };

  const calculateNextServiceDate = (lastServiceDate: string, intervalDays: number) => {
    const lastDate = new Date(lastServiceDate);
    const nextDate = new Date(lastDate.getTime() + (intervalDays * 24 * 60 * 60 * 1000));
    return nextDate.toISOString().split('T')[0];
  };

  const selectedVehicle = vehicles.find(v => v.id === selectedVehicleId);

  return (
    <Box sx={{ p: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {loading && <LinearProgress sx={{ mb: 3 }} />}

      {/* Vehicle Selector */}
      {showVehicleSelector && (
        <Card sx={{ mb: 3, borderRadius: 3 }}>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                <ScheduleIcon />
              </Avatar>
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Maintenance Scheduler
              </Typography>
            </Box>
            
            <FormControl fullWidth>
              <InputLabel>Select Vehicle</InputLabel>
              <Select
                value={selectedVehicleId || ''}
                onChange={(e) => setSelectedVehicleId(Number(e.target.value))}
                label="Select Vehicle"
              >
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>
                    {vehicle.vehicleId} - {vehicle.make} {vehicle.model} ({vehicle.licensePlate})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </CardContent>
        </Card>
      )}

      {selectedVehicleId && (
        <>
          {/* Overdue Maintenance Alerts */}
          {overdueMaintenances.length > 0 && (
            <Card sx={{ mb: 3, borderRadius: 3, border: '2px solid', borderColor: 'error.main' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Avatar sx={{ bgcolor: 'error.main', mr: 2 }}>
                    <WarningIcon />
                  </Avatar>
                  <Typography variant="h6" sx={{ fontWeight: 600, color: 'error.main' }}>
                    Overdue Maintenance ({overdueMaintenances.length})
                  </Typography>
                </Box>
                
                <List>
                  {overdueMaintenances.map((maintenance, index) => (
                    <ListItem key={maintenance.id} sx={{ px: 0 }}>
                      <ListItemIcon>
                        {getMaintenanceTypeIcon(maintenance.maintenanceType)}
                      </ListItemIcon>
                      <ListItemText
                        primary={maintenance.maintenanceType}
                        secondary={`Scheduled: ${new Date(maintenance.scheduledDate).toLocaleDateString()}`}
                      />
                      <Button
                        variant="contained"
                        color="error"
                        size="small"
                        onClick={() => handleCompleteMaintenace(maintenance.id)}
                      >
                        Mark Complete
                      </Button>
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}

          {/* Action Buttons */}
          <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => setShowScheduleDialog(true)}
              sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}
            >
              Schedule Maintenance
            </Button>
            <Button
              variant="outlined"
              startIcon={<ScheduleIcon />}
              onClick={() => setShowIntervalDialog(true)}
            >
              Set Interval
            </Button>
          </Box>

          {/* Maintenance Intervals */}
          <Card sx={{ mb: 3, borderRadius: 3 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
                Maintenance Intervals
              </Typography>
              
              {maintenanceIntervals.length > 0 ? (
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  {maintenanceIntervals.map((interval) => (
                    <Box key={interval.id} sx={{ flex: '1 1 300px', minWidth: '250px' }}>
                      <Paper
                        sx={{
                          p: 2,
                          borderRadius: 2,
                          border: '1px solid',
                          borderColor: 'grey.200',
                        }}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                          {getMaintenanceTypeIcon(interval.maintenanceType)}
                          <Typography variant="subtitle2" sx={{ ml: 1, fontWeight: 600 }}>
                            {interval.maintenanceType}
                          </Typography>
                        </Box>
                        <Typography variant="body2" color="text.secondary">
                          Every {interval.intervalDays} days
                          {interval.intervalMileage && ` or ${interval.intervalMileage} km`}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Next: {new Date(interval.nextServiceDate).toLocaleDateString()}
                        </Typography>
                        <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
                          <IconButton
                            size="small"
                            onClick={() => openEditIntervalDialog(interval)}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      </Paper>
                    </Box>
                  ))}
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No maintenance intervals configured. Click "Set Interval" to add one.
                </Typography>
              )}
            </CardContent>
          </Card>

          {/* Maintenance History */}
          <Card sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
                Maintenance History
              </Typography>
              
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Type</TableCell>
                      <TableCell>Description</TableCell>
                      <TableCell>Scheduled Date</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Cost</TableCell>
                      <TableCell>Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {maintenanceRecords.map((record) => (
                      <TableRow key={record.id}>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            {getMaintenanceTypeIcon(record.maintenanceType)}
                            <Typography variant="body2" sx={{ ml: 1 }}>
                              {record.maintenanceType}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>{record.description}</TableCell>
                        <TableCell>
                          {new Date(record.scheduledDate).toLocaleDateString()}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={record.status}
                            color={getStatusColor(record.status) as any}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          {record.cost ? `$${record.cost.toFixed(2)}` : '-'}
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            {record.status !== 'Completed' && (
                              <Tooltip title="Mark Complete">
                                <IconButton
                                  size="small"
                                  color="success"
                                  onClick={() => handleCompleteMaintenace(record.id)}
                                >
                                  <CheckIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            )}
                            <Tooltip title="Edit">
                              <IconButton
                                size="small"
                                onClick={() => openEditDialog(record)}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Delete">
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteRecord(record.id)}
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

              {maintenanceRecords.length === 0 && (
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                  No maintenance records found. Schedule your first maintenance above.
                </Typography>
              )}
            </CardContent>
          </Card>
        </>
      )}

      {/* Schedule Maintenance Dialog */}
      <Dialog
        open={showScheduleDialog}
        onClose={() => {
          setShowScheduleDialog(false);
          resetScheduleForm();
        }}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          {editingRecord ? 'Edit Maintenance' : 'Schedule Maintenance'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <FormControl fullWidth required>
                <InputLabel>Maintenance Type</InputLabel>
                <Select
                  value={maintenanceType}
                  onChange={(e) => setMaintenanceType(e.target.value)}
                  label="Maintenance Type"
                >
                  {maintenanceTypes.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        {type.icon}
                        <Typography sx={{ ml: 1 }}>{type.value}</Typography>
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                fullWidth
                label="Description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                multiline
                rows={2}
              />
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <TextField
                    fullWidth
                    label="Scheduled Date"
                    type="date"
                    value={scheduledDate}
                    onChange={(e) => setScheduledDate(e.target.value)}
                    required
                    InputLabelProps={{ shrink: true }}
                  />
                </Box>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <TextField
                    fullWidth
                    label="Estimated Cost"
                    type="number"
                    value={estimatedCost}
                    onChange={(e) => setEstimatedCost(e.target.value)}
                    InputProps={{ startAdornment: '$' }}
                  />
                </Box>
              </Box>
              <TextField
                fullWidth
                label="Notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                multiline
                rows={3}
              />
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              setShowScheduleDialog(false);
              resetScheduleForm();
            }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleScheduleMaintenance}
            disabled={!maintenanceType || !scheduledDate}
          >
            {editingRecord ? 'Update' : 'Schedule'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Set Interval Dialog */}
      <Dialog
        open={showIntervalDialog}
        onClose={() => {
          setShowIntervalDialog(false);
          resetIntervalForm();
        }}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          {editingInterval ? 'Edit Maintenance Interval' : 'Set Maintenance Interval'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <FormControl fullWidth required>
                <InputLabel>Maintenance Type</InputLabel>
                <Select
                  value={intervalType}
                  onChange={(e) => setIntervalType(e.target.value)}
                  label="Maintenance Type"
                >
                  {maintenanceTypes.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        {type.icon}
                        <Typography sx={{ ml: 1 }}>{type.value}</Typography>
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <TextField
                    fullWidth
                    label="Interval (Days)"
                    type="number"
                    value={intervalDays}
                    onChange={(e) => setIntervalDays(e.target.value)}
                    required
                    inputProps={{ min: 1 }}
                  />
                </Box>
                <Box sx={{ flex: '1 1 250px', minWidth: '200px' }}>
                  <TextField
                    fullWidth
                    label="Interval (Mileage)"
                    type="number"
                    value={intervalMileage}
                    onChange={(e) => setIntervalMileage(e.target.value)}
                    placeholder="Optional"
                    inputProps={{ min: 1 }}
                  />
                </Box>
              </Box>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              setShowIntervalDialog(false);
              resetIntervalForm();
            }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSetInterval}
            disabled={!intervalType || !intervalDays}
          >
            {editingInterval ? 'Update' : 'Set Interval'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MaintenanceScheduler;
