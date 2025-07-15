import React, { useState } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  LinearProgress,
  FormControlLabel,
  Checkbox,
  Paper,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  IconButton,
} from '@mui/material';
import {
  DirectionsCar as CarIcon,
  CloudUpload as UploadIcon,
  CheckCircle as CheckIcon,
  Delete as DeleteIcon,
  Description as DocumentIcon,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { apiClient } from '../../services/apiClient';

interface VehicleRegistrationFormProps {
  onSuccess: () => void;
  onCancel: () => void;
}

interface DocumentUpload {
  file: File;
  type: string;
  progress: number;
  uploaded: boolean;
}

const VehicleRegistrationForm: React.FC<VehicleRegistrationFormProps> = ({
  onSuccess,
  onCancel,
}) => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [documents, setDocuments] = useState<DocumentUpload[]>([]);

  const [vehicleId, setVehicleId] = useState('');
  const [vin, setVin] = useState('');
  const [make, setMake] = useState('');
  const [model, setModel] = useState('');
  const [year, setYear] = useState('');
  const [licensePlate, setLicensePlate] = useState('');
  const [color, setColor] = useState('');

  const [capacity, setCapacity] = useState('');
  const [fuelType, setFuelType] = useState('');
  const [mileage, setMileage] = useState('');
  const [engineSize, setEngineSize] = useState('');

  const [insuranceProvider, setInsuranceProvider] = useState('');
  const [insurancePolicyNumber, setInsurancePolicyNumber] = useState('');
  const [insuranceExpiryDate, setInsuranceExpiryDate] = useState('');
  const [insuranceCoverage, setInsuranceCoverage] = useState('');

  const [lastMaintenanceDate, setLastMaintenanceDate] = useState('');
  const [nextMaintenanceDate, setNextMaintenanceDate] = useState('');
  const [maintenanceInterval, setMaintenanceInterval] = useState('');

  const [safetyEquipment, setSafetyEquipment] = useState({
    firstAidKit: false,
    fireExtinguisher: false,
    emergencyTriangle: false,
    seatBelts: false,
    childSafetyLocks: false,
    gpsTracking: false,
    dashCamera: false,
    reverseCamera: false,
    emergencyExits: false,
    communicationDevice: false,
  });

  const [notes, setNotes] = useState('');

  const handleSafetyEquipmentChange = (equipment: string) => {
    setSafetyEquipment(prev => ({
      ...prev,
      [equipment]: !prev[equipment as keyof typeof prev],
    }));
  };

  const handleDocumentUpload = async (
    event: React.ChangeEvent<HTMLInputElement>,
    type: string
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const newDocument: DocumentUpload = {
      file,
      type,
      progress: 0,
      uploaded: false,
    };

    setDocuments(prev => [...prev, newDocument]);

    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('documentType', type);

      await apiClient.post('/api/files/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      setDocuments(prev =>
        prev.map(doc =>
          doc.file === file ? { ...doc, uploaded: true, progress: 100 } : doc
        )
      );
    } catch {
      setDocuments(prev => prev.filter(doc => doc.file !== file));
      setError('Failed to upload document. Please try again.');
    }
  };

  const removeDocument = (fileToRemove: File) => {
    setDocuments(prev => prev.filter(doc => doc.file !== fileToRemove));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const vehicleData = {
        vehicleId,
        vin,
        make,
        model,
        year: parseInt(year),
        licensePlate,
        color,
        capacity: parseInt(capacity),
        fuelType,
        mileage: parseFloat(mileage),
        engineSize,
        insuranceProvider,
        insurancePolicyNumber,
        insuranceExpiryDate,
        insuranceCoverage,
        lastMaintenanceDate,
        nextMaintenanceDate,
        maintenanceInterval: parseInt(maintenanceInterval),
        safetyEquipment,
        notes,
        tenantId: user?.tenantId,
      };

      await apiClient.post('/api/vehicles', vehicleData);
      onSuccess();
    } catch (error: any) {
      setError(
        error.response?.data?.message ||
          'Failed to register vehicle. Please try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'CNG', 'LPG'];
  const maintenanceIntervals = [
    { value: 30, label: '30 days' },
    { value: 60, label: '60 days' },
    { value: 90, label: '90 days' },
    { value: 180, label: '6 months' },
    { value: 365, label: '1 year' },
  ];

  const documentTypes = [
    'Registration Certificate',
    'Insurance Policy',
    'Inspection Certificate',
    'Emission Certificate',
    'Driver Manual',
    'Maintenance Records',
  ];

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ maxWidth: 800, mx: 'auto', p: 2 }}
    >
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {loading && <LinearProgress sx={{ mb: 3 }} />}

      {/* Basic Vehicle Information */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
            <CarIcon sx={{ mr: 2, color: 'primary.main' }} />
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Basic Vehicle Information
            </Typography>
          </Box>

          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Vehicle ID"
                value={vehicleId}
                onChange={e => setVehicleId(e.target.value)}
                required
                placeholder="VEH-001"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="VIN Number"
                value={vin}
                onChange={e => setVin(e.target.value)}
                required
                placeholder="1HGBH41JXMN109186"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Make"
                value={make}
                onChange={e => setMake(e.target.value)}
                required
                placeholder="Toyota"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Model"
                value={model}
                onChange={e => setModel(e.target.value)}
                required
                placeholder="Hiace"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Year"
                type="number"
                value={year}
                onChange={e => setYear(e.target.value)}
                required
                inputProps={{ min: 1990, max: new Date().getFullYear() + 1 }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="License Plate"
                value={licensePlate}
                onChange={e => setLicensePlate(e.target.value)}
                required
                placeholder="ABC-123"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Color"
                value={color}
                onChange={e => setColor(e.target.value)}
                placeholder="Yellow"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Specifications */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Vehicle Specifications
          </Typography>

          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Passenger Capacity"
                type="number"
                value={capacity}
                onChange={e => setCapacity(e.target.value)}
                required
                inputProps={{ min: 1, max: 100 }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth required>
                <InputLabel>Fuel Type</InputLabel>
                <Select
                  value={fuelType}
                  onChange={e => setFuelType(e.target.value)}
                  label="Fuel Type"
                >
                  {fuelTypes.map(type => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Current Mileage (km)"
                type="number"
                value={mileage}
                onChange={e => setMileage(e.target.value)}
                inputProps={{ min: 0 }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Engine Size (L)"
                value={engineSize}
                onChange={e => setEngineSize(e.target.value)}
                placeholder="2.4L"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Insurance Information */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Insurance Information
          </Typography>

          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Insurance Provider"
                value={insuranceProvider}
                onChange={e => setInsuranceProvider(e.target.value)}
                required
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Policy Number"
                value={insurancePolicyNumber}
                onChange={e => setInsurancePolicyNumber(e.target.value)}
                required
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Expiry Date"
                type="date"
                value={insuranceExpiryDate}
                onChange={e => setInsuranceExpiryDate(e.target.value)}
                required
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Coverage Amount"
                value={insuranceCoverage}
                onChange={e => setInsuranceCoverage(e.target.value)}
                placeholder="$100,000"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Maintenance Schedule */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Maintenance Schedule
          </Typography>

          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Last Maintenance Date"
                type="date"
                value={lastMaintenanceDate}
                onChange={e => setLastMaintenanceDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <TextField
                fullWidth
                label="Next Maintenance Date"
                type="date"
                value={nextMaintenanceDate}
                onChange={e => setNextMaintenanceDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <FormControl fullWidth>
                <InputLabel>Maintenance Interval</InputLabel>
                <Select
                  value={maintenanceInterval}
                  onChange={e => setMaintenanceInterval(e.target.value)}
                  label="Maintenance Interval"
                >
                  {maintenanceIntervals.map(interval => (
                    <MenuItem key={interval.value} value={interval.value}>
                      {interval.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Safety Equipment Checklist */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Safety Equipment Checklist
          </Typography>

          <Grid container spacing={2}>
            {Object.entries(safetyEquipment).map(([key, checked]) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={key}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={checked}
                      onChange={() => handleSafetyEquipmentChange(key)}
                      color="primary"
                    />
                  }
                  label={key
                    .replace(/([A-Z])/g, ' $1')
                    .replace(/^./, str => str.toUpperCase())}
                />
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>

      {/* Document Upload */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Document Upload
          </Typography>

          <Grid container spacing={2}>
            {documentTypes.map(docType => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={docType}>
                <Paper
                  sx={{
                    p: 2,
                    border: '2px dashed',
                    borderColor: 'grey.300',
                    textAlign: 'center',
                    cursor: 'pointer',
                    '&:hover': {
                      borderColor: 'primary.main',
                      bgcolor: 'primary.50',
                    },
                  }}
                  component="label"
                >
                  <input
                    type="file"
                    hidden
                    accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                    onChange={e => handleDocumentUpload(e, docType)}
                  />
                  <UploadIcon sx={{ fontSize: 40, color: 'grey.400', mb: 1 }} />
                  <Typography variant="body2" color="text.secondary">
                    {docType}
                  </Typography>
                </Paper>
              </Grid>
            ))}
          </Grid>

          {documents.length > 0 && (
            <Box sx={{ mt: 3 }}>
              <Typography variant="subtitle2" sx={{ mb: 2 }}>
                Uploaded Documents
              </Typography>
              <List>
                {documents.map((doc, index) => (
                  <ListItem key={index} sx={{ px: 0 }}>
                    <ListItemIcon>
                      <DocumentIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary={doc.file.name}
                      secondary={
                        <Box
                          sx={{ display: 'flex', alignItems: 'center', mt: 1 }}
                        >
                          <LinearProgress
                            variant="determinate"
                            value={doc.progress}
                            sx={{ flexGrow: 1, mr: 2 }}
                          />
                          {doc.uploaded ? (
                            <CheckIcon color="success" />
                          ) : (
                            <Typography variant="caption">
                              {doc.progress}%
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                    <IconButton
                      edge="end"
                      onClick={() => removeDocument(doc.file)}
                      size="small"
                    >
                      <DeleteIcon />
                    </IconButton>
                  </ListItem>
                ))}
              </List>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* Additional Notes */}
      <Card sx={{ mb: 3, borderRadius: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Additional Notes
          </Typography>
          <TextField
            fullWidth
            multiline
            rows={4}
            label="Notes"
            value={notes}
            onChange={e => setNotes(e.target.value)}
            placeholder="Any additional information about the vehicle..."
          />
        </CardContent>
      </Card>

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
        <Button
          variant="outlined"
          onClick={onCancel}
          disabled={loading}
          size="large"
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={loading}
          size="large"
          sx={{
            minWidth: 120,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          }}
        >
          {loading ? 'Registering...' : 'Register Vehicle'}
        </Button>
      </Box>
    </Box>
  );
};

export default VehicleRegistrationForm;
