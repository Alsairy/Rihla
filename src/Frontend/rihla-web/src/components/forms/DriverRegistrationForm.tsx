import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Paper,
  IconButton,
  LinearProgress,
} from '@mui/material';
import {
  Person as PersonIcon,
  ContactPhone as ContactPhoneIcon,
  Work as WorkIcon,
  Description as DescriptionIcon,
  Upload as UploadIcon,
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import { apiClient } from '../../services/apiClient';

interface DriverRegistrationFormProps {
  onSuccess?: (driver: any) => void;
  onCancel?: () => void;
}

interface DocumentUpload {
  file: File;
  type: string;
  uploaded: boolean;
  url?: string;
}

const DriverRegistrationForm: React.FC<DriverRegistrationFormProps> = ({
  onSuccess,
  onCancel,
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [uploadProgress, setUploadProgress] = useState<{
    [key: string]: number;
  }>({});

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    middleName: '',
    employeeId: '',
    dateOfBirth: '',
    phone: '',
    email: '',
    address: '',
    city: '',
    state: '',
    zipCode: '',
    country: 'Saudi Arabia',

    licenseNumber: '',
    licenseClass: '',
    licenseExpiry: '',
    endorsements: '',

    medicalCertExpiry: '',
    medicalRestrictions: '',

    hireDate: '',
    department: 'Transportation',
    position: 'School Bus Driver',
    salary: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    emergencyContactRelation: '',

    yearsOfExperience: '',
    previousEmployer: '',
    notes: '',
  });

  const [documents, setDocuments] = useState<{ [key: string]: DocumentUpload }>(
    {}
  );

  const documentTypes = [
    { key: 'license', label: 'Driver License Copy', required: true },
    { key: 'medical', label: 'Medical Certificate', required: true },
    { key: 'background', label: 'Background Check', required: true },
    { key: 'training', label: 'Training Certificate', required: false },
    { key: 'photo', label: 'Driver Photo', required: false },
  ];

  const licenseClasses = [
    'Class A CDL',
    'Class B CDL',
    'Class C CDL',
    'Regular License',
  ];

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setError(null);
  };

  const handleFileUpload = async (documentType: string, file: File) => {
    if (!file) return;

    setUploadProgress(prev => ({ ...prev, [documentType]: 0 }));

    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('documentType', documentType);

      const progressInterval = setInterval(() => {
        setUploadProgress(prev => {
          const current = prev[documentType] || 0;
          if (current >= 90) {
            clearInterval(progressInterval);
            return prev;
          }
          return { ...prev, [documentType]: current + 10 };
        });
      }, 200);

      setDocuments(prev => ({
        ...prev,
        [documentType]: {
          file,
          type: documentType,
          uploaded: true,
          url: URL.createObjectURL(file),
        },
      }));

      clearInterval(progressInterval);
      setUploadProgress(prev => ({ ...prev, [documentType]: 100 }));

      setTimeout(() => {
        setUploadProgress(prev => {
          const newProgress = { ...prev };
          delete newProgress[documentType];
          return newProgress;
        });
      }, 2000);
    } catch (error) {
      setError(`Failed to upload ${documentType} document`);
      setUploadProgress(prev => {
        const newProgress = { ...prev };
        delete newProgress[documentType];
        return newProgress;
      });
    }
  };

  const handleRemoveDocument = (documentType: string) => {
    setDocuments(prev => {
      const newDocs = { ...prev };
      if (newDocs[documentType]?.url) {
        URL.revokeObjectURL(newDocs[documentType].url!);
      }
      delete newDocs[documentType];
      return newDocs;
    });
  };

  const validateForm = (): string | null => {
    const requiredFields = [
      'firstName',
      'lastName',
      'employeeId',
      'dateOfBirth',
      'phone',
      'email',
      'address',
      'licenseNumber',
      'licenseClass',
      'licenseExpiry',
      'hireDate',
      'emergencyContactName',
      'emergencyContactPhone',
    ];

    for (const field of requiredFields) {
      if (!formData[field as keyof typeof formData]) {
        return `${field.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())} is required`;
      }
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      return 'Please enter a valid email address';
    }

    const phoneRegex = /^\+?[\d\s\-()]+$/;
    if (!phoneRegex.test(formData.phone)) {
      return 'Please enter a valid phone number';
    }

    const today = new Date();
    const licenseExpiry = new Date(formData.licenseExpiry);
    if (licenseExpiry <= today) {
      return 'License expiry date must be in the future';
    }

    const requiredDocs = documentTypes.filter(doc => doc.required);
    for (const doc of requiredDocs) {
      if (!documents[doc.key]) {
        return `${doc.label} is required`;
      }
    }

    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const driverData = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        middleName: formData.middleName,
        employeeId: formData.employeeId,
        dateOfBirth: formData.dateOfBirth,
        phone: formData.phone,
        email: formData.email,
        address: formData.address,
        city: formData.city,
        state: formData.state,
        zipCode: formData.zipCode,
        country: formData.country,
        licenseNumber: formData.licenseNumber,
        licenseClass: formData.licenseClass,
        licenseExpiry: formData.licenseExpiry,
        endorsements: formData.endorsements,
        medicalCertExpiry: formData.medicalCertExpiry,
        medicalRestrictions: formData.medicalRestrictions,
        hireDate: formData.hireDate,
        department: formData.department,
        position: formData.position,
        salary: parseFloat(formData.salary) || 0,
        emergencyContactName: formData.emergencyContactName,
        emergencyContactPhone: formData.emergencyContactPhone,
        emergencyContactRelation: formData.emergencyContactRelation,
        yearsOfExperience: parseInt(formData.yearsOfExperience) || 0,
        previousEmployer: formData.previousEmployer,
        notes: formData.notes,
        isActive: true,
      };

      const createdDriver = await apiClient.post<any>(
        '/api/drivers',
        driverData
      );

      for (const [docType, docData] of Object.entries(documents)) {
        if (docData.file) {
          const uploadFormData = new FormData();
          uploadFormData.append('file', docData.file);
          uploadFormData.append('documentType', docType);

          await apiClient.post(
            `/api/files/driver/${createdDriver.id}/document`,
            uploadFormData
          );
        }
      }

      setSuccess('Driver registered successfully!');

      if (onSuccess) {
        onSuccess(createdDriver);
      }

      setFormData({
        firstName: '',
        lastName: '',
        middleName: '',
        employeeId: '',
        dateOfBirth: '',
        phone: '',
        email: '',
        address: '',
        city: '',
        state: '',
        zipCode: '',
        country: 'Saudi Arabia',
        licenseNumber: '',
        licenseClass: '',
        licenseExpiry: '',
        endorsements: '',
        medicalCertExpiry: '',
        medicalRestrictions: '',
        hireDate: '',
        department: 'Transportation',
        position: 'School Bus Driver',
        salary: '',
        emergencyContactName: '',
        emergencyContactPhone: '',
        emergencyContactRelation: '',
        yearsOfExperience: '',
        previousEmployer: '',
        notes: '',
      });
      setDocuments({});
    } catch (error: any) {
      setError(
        error.response?.data?.message ||
          'Failed to register driver. Please try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card sx={{ maxWidth: 1200, mx: 'auto', my: 2 }}>
      <CardContent sx={{ p: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <PersonIcon sx={{ fontSize: 32, mr: 2, color: 'primary.main' }} />
          <Typography variant="h4" sx={{ fontWeight: 600 }}>
            Driver Registration
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {success && (
          <Alert severity="success" sx={{ mb: 3 }}>
            {success}
          </Alert>
        )}

        <form onSubmit={handleSubmit}>
          {/* Personal Information Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <PersonIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Personal Information
              </Typography>
            </Box>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="First Name"
                  value={formData.firstName}
                  onChange={e => handleInputChange('firstName', e.target.value)}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Middle Name"
                  value={formData.middleName}
                  onChange={e =>
                    handleInputChange('middleName', e.target.value)
                  }
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Last Name"
                  value={formData.lastName}
                  onChange={e => handleInputChange('lastName', e.target.value)}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Employee ID"
                  value={formData.employeeId}
                  onChange={e =>
                    handleInputChange('employeeId', e.target.value)
                  }
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Date of Birth"
                  type="date"
                  value={formData.dateOfBirth}
                  onChange={e =>
                    handleInputChange('dateOfBirth', e.target.value)
                  }
                  InputLabelProps={{ shrink: true }}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Phone"
                  value={formData.phone}
                  onChange={e => handleInputChange('phone', e.target.value)}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={e => handleInputChange('email', e.target.value)}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Address"
                  value={formData.address}
                  onChange={e => handleInputChange('address', e.target.value)}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="City"
                  value={formData.city}
                  onChange={e => handleInputChange('city', e.target.value)}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="State/Province"
                  value={formData.state}
                  onChange={e => handleInputChange('state', e.target.value)}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="ZIP Code"
                  value={formData.zipCode}
                  onChange={e => handleInputChange('zipCode', e.target.value)}
                />
              </Grid>
            </Grid>
          </Paper>

          {/* License Information Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <DescriptionIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                License Information
              </Typography>
            </Box>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="License Number"
                  value={formData.licenseNumber}
                  onChange={e =>
                    handleInputChange('licenseNumber', e.target.value)
                  }
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth required>
                  <InputLabel>License Class</InputLabel>
                  <Select
                    value={formData.licenseClass}
                    onChange={e =>
                      handleInputChange('licenseClass', e.target.value)
                    }
                    label="License Class"
                  >
                    {licenseClasses.map(cls => (
                      <MenuItem key={cls} value={cls}>
                        {cls}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="License Expiry"
                  type="date"
                  value={formData.licenseExpiry}
                  onChange={e =>
                    handleInputChange('licenseExpiry', e.target.value)
                  }
                  InputLabelProps={{ shrink: true }}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Medical Certificate Expiry"
                  type="date"
                  value={formData.medicalCertExpiry}
                  onChange={e =>
                    handleInputChange('medicalCertExpiry', e.target.value)
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Endorsements"
                  value={formData.endorsements}
                  onChange={e =>
                    handleInputChange('endorsements', e.target.value)
                  }
                  placeholder="e.g., Passenger, School Bus, Hazmat"
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Medical Restrictions"
                  value={formData.medicalRestrictions}
                  onChange={e =>
                    handleInputChange('medicalRestrictions', e.target.value)
                  }
                  multiline
                  rows={2}
                />
              </Grid>
            </Grid>
          </Paper>

          {/* Employment Information Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <WorkIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Employment Information
              </Typography>
            </Box>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Hire Date"
                  type="date"
                  value={formData.hireDate}
                  onChange={e => handleInputChange('hireDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Salary"
                  type="number"
                  value={formData.salary}
                  onChange={e => handleInputChange('salary', e.target.value)}
                  InputProps={{ startAdornment: 'SAR ' }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Years of Experience"
                  type="number"
                  value={formData.yearsOfExperience}
                  onChange={e =>
                    handleInputChange('yearsOfExperience', e.target.value)
                  }
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 8 }}>
                <TextField
                  fullWidth
                  label="Previous Employer"
                  value={formData.previousEmployer}
                  onChange={e =>
                    handleInputChange('previousEmployer', e.target.value)
                  }
                />
              </Grid>
            </Grid>
          </Paper>

          {/* Emergency Contact Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <ContactPhoneIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Emergency Contact
              </Typography>
            </Box>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Contact Name"
                  value={formData.emergencyContactName}
                  onChange={e =>
                    handleInputChange('emergencyContactName', e.target.value)
                  }
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Contact Phone"
                  value={formData.emergencyContactPhone}
                  onChange={e =>
                    handleInputChange('emergencyContactPhone', e.target.value)
                  }
                  required
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  fullWidth
                  label="Relationship"
                  value={formData.emergencyContactRelation}
                  onChange={e =>
                    handleInputChange(
                      'emergencyContactRelation',
                      e.target.value
                    )
                  }
                  placeholder="e.g., Spouse, Parent, Sibling"
                />
              </Grid>
            </Grid>
          </Paper>

          {/* Document Upload Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <UploadIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Document Upload
              </Typography>
            </Box>
            <Grid container spacing={3}>
              {documentTypes.map(docType => (
                <Grid size={{ xs: 12, sm: 6 }} key={docType.key}>
                  <Box
                    sx={{ border: '1px dashed #ccc', borderRadius: 2, p: 2 }}
                  >
                    <Typography variant="subtitle2" sx={{ mb: 1 }}>
                      {docType.label}
                      {docType.required && (
                        <span style={{ color: 'red' }}> *</span>
                      )}
                    </Typography>

                    {documents[docType.key] ? (
                      <Box
                        sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                      >
                        <CheckCircleIcon
                          sx={{ color: 'success.main', fontSize: 20 }}
                        />
                        <Typography variant="body2" sx={{ flex: 1 }}>
                          {documents[docType.key].file.name}
                        </Typography>
                        <IconButton
                          size="small"
                          onClick={() => handleRemoveDocument(docType.key)}
                          color="error"
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Box>
                    ) : (
                      <Button
                        variant="outlined"
                        component="label"
                        startIcon={<UploadIcon />}
                        fullWidth
                        sx={{ mt: 1 }}
                      >
                        Choose File
                        <input
                          type="file"
                          hidden
                          accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                          onChange={e => {
                            const file = e.target.files?.[0];
                            if (file) {
                              handleFileUpload(docType.key, file);
                            }
                          }}
                        />
                      </Button>
                    )}

                    {uploadProgress[docType.key] !== undefined && (
                      <Box sx={{ mt: 1 }}>
                        <LinearProgress
                          variant="determinate"
                          value={uploadProgress[docType.key]}
                        />
                        <Typography
                          variant="caption"
                          sx={{ mt: 0.5, display: 'block' }}
                        >
                          Uploading... {uploadProgress[docType.key]}%
                        </Typography>
                      </Box>
                    )}
                  </Box>
                </Grid>
              ))}
            </Grid>
          </Paper>

          {/* Notes Section */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
              Additional Notes
            </Typography>
            <TextField
              fullWidth
              label="Notes"
              value={formData.notes}
              onChange={e => handleInputChange('notes', e.target.value)}
              multiline
              rows={3}
              placeholder="Any additional information about the driver..."
            />
          </Paper>

          {/* Action Buttons */}
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
            {onCancel && (
              <Button
                variant="outlined"
                onClick={onCancel}
                disabled={loading}
                size="large"
              >
                Cancel
              </Button>
            )}
            <Button
              type="submit"
              variant="contained"
              disabled={loading}
              size="large"
              sx={{ minWidth: 150 }}
            >
              {loading ? 'Registering...' : 'Register Driver'}
            </Button>
          </Box>
        </form>
      </CardContent>
    </Card>
  );
};

export default DriverRegistrationForm;
