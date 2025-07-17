import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Tabs,
  Tab,
} from '@mui/material';
import {
  QrCodeScanner,
  CameraAlt,
  Wifi,
  WifiOff,
  CheckCircle,
  Person,
  Refresh,
  Upload,
  Download,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface Student {
  id: number;
  firstName: string;
  lastName: string;
  studentNumber: string;
  rfidTag?: string;
}

interface Trip {
  id: number;
  routeName: string;
  vehicleName: string;
  driverName: string;
  scheduledStartTime: string;
  status: string;
}

interface AttendanceRecord {
  id: number;
  studentId: number;
  studentName: string;
  tripId: number;
  status: 'Present' | 'Absent' | 'Late';
  boardingTime?: string;
  method: 'Manual' | 'RFID' | 'Photo' | 'Offline';
  timestamp: string;
}

interface OfflineRecord {
  studentId: number;
  tripId: number;
  status: 'Present' | 'Absent' | 'Late';
  boardingTime?: string;
  alightingTime?: string;
  notes: string;
  attendanceDate: string;
  method: string;
}

const MultiMethodAttendanceTracker: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [students, setStudents] = useState<Student[]>([]);
  const [trips, setTrips] = useState<Trip[]>([]);
  const [selectedTrip, setSelectedTrip] = useState<number | null>(null);
  const [attendanceRecords, setAttendanceRecords] = useState<
    AttendanceRecord[]
  >([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [offlineRecords, setOfflineRecords] = useState<OfflineRecord[]>([]);

  const [rfidInput, setRfidInput] = useState('');
  const [rfidScanning, setRfidScanning] = useState(false);

  const [photoDialog, setPhotoDialog] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [capturedPhoto, setCapturedPhoto] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);

  const [manualStudentId, setManualStudentId] = useState<number | null>(null);
  const [manualStatus, setManualStatus] = useState<
    'Present' | 'Absent' | 'Late'
  >('Present');

  const loadInitialData = useCallback(async () => {
    setLoading(true);
    try {
      const [studentsResponse, tripsResponse] = await Promise.all([
        apiClient.get('/api/students'),
        apiClient.get('/api/trips/active'),
      ]);

      setStudents((studentsResponse as any).data?.items || []);
      setTrips((tripsResponse as any).data?.items || []);

      if ((tripsResponse as any).data?.items?.length > 0) {
        setSelectedTrip((tripsResponse as any).data.items[0].id);
        loadAttendanceRecords((tripsResponse as any).data.items[0].id);
      }
    } catch {
      setError('Failed to load initial data');
    } finally {
      setLoading(false);
    }
  }, []);

  const syncOfflineRecords = useCallback(async () => {
    if (offlineRecords.length === 0) return;

    setLoading(true);
    try {
      const response = await apiClient.post('/api/attendance/sync-offline', {
        offlineRecords,
      });

      if ((response as any).data.success) {
        setOfflineRecords([]);
        setSuccess(`Synced ${offlineRecords.length} offline records`);
        if (selectedTrip) {
          loadAttendanceRecords(selectedTrip);
        }
      } else {
        setError('Failed to sync offline records');
      }
    } catch {
      setError('Failed to sync offline records');
    } finally {
      setLoading(false);
    }
  }, [offlineRecords, selectedTrip]);

  useEffect(() => {
    loadInitialData();

    const handleOnline = () => {
      setIsOnline(true);
      syncOfflineRecords();
    };

    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, [loadInitialData, syncOfflineRecords]);

  const loadAttendanceRecords = async (tripId: number) => {
    try {
      const response = await apiClient.get(`/api/attendance/trip/${tripId}`);
      setAttendanceRecords((response as any).data || []);
    } catch {
      setError('Failed to load attendance records');
    }
  };

  const handleRfidScan = async () => {
    if (!rfidInput.trim() || !selectedTrip) return;

    setRfidScanning(true);
    try {
      if (isOnline) {
        const response = await apiClient.post('/api/attendance/rfid', {
          rfidTag: rfidInput.trim(),
          tripId: selectedTrip,
          stopId: 1, // Default stop
          timestamp: new Date().toISOString(),
        });

        if ((response as any).data.success) {
          setSuccess('RFID attendance recorded successfully');
          loadAttendanceRecords(selectedTrip);
        } else {
          setError(
            (response as any).data.message || 'Failed to record RFID attendance'
          );
        }
      } else {
        const student = students.find(s => s.rfidTag === rfidInput.trim());
        if (student) {
          const offlineRecord: OfflineRecord = {
            studentId: student.id,
            tripId: selectedTrip,
            status: 'Present',
            boardingTime: new Date().toISOString(),
            notes: `RFID scan: ${rfidInput.trim()}`,
            attendanceDate: new Date().toISOString(),
            method: 'RFID',
          };

          setOfflineRecords(prev => [...prev, offlineRecord]);
          setSuccess('RFID attendance stored offline');
        } else {
          setError('Student with RFID tag not found');
        }
      }

      setRfidInput('');
    } catch {
      setError('Failed to process RFID scan');
    } finally {
      setRfidScanning(false);
    }
  };

  const startPhotoCapture = async (student: Student) => {
    setSelectedStudent(student);
    setPhotoDialog(true);

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: true });
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
    } catch {
      setError('Failed to access camera');
    }
  };

  const capturePhoto = () => {
    if (videoRef.current && canvasRef.current) {
      const canvas = canvasRef.current;
      const video = videoRef.current;

      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;

      const ctx = canvas.getContext('2d');
      if (ctx) {
        ctx.drawImage(video, 0, 0);
        const photoData = canvas.toDataURL('image/jpeg', 0.8);
        setCapturedPhoto(photoData);
      }
    }
  };

  const submitPhotoAttendance = async () => {
    if (!selectedStudent || !capturedPhoto || !selectedTrip) return;

    setLoading(true);
    try {
      if (isOnline) {
        const response = await apiClient.post('/api/attendance/photo', {
          studentId: selectedStudent.id,
          tripId: selectedTrip,
          stopId: 1,
          photoBase64: capturedPhoto.split(',')[1], // Remove data:image/jpeg;base64, prefix
          timestamp: new Date().toISOString(),
        });

        if ((response as any).data.success) {
          setSuccess('Photo attendance recorded successfully');
          loadAttendanceRecords(selectedTrip);
        } else {
          setError(
            (response as any).data.message ||
              'Failed to record photo attendance'
          );
        }
      } else {
        const offlineRecord: OfflineRecord = {
          studentId: selectedStudent.id,
          tripId: selectedTrip,
          status: 'Present',
          boardingTime: new Date().toISOString(),
          notes: 'Photo verification',
          attendanceDate: new Date().toISOString(),
          method: 'Photo',
        };

        setOfflineRecords(prev => [...prev, offlineRecord]);
        setSuccess('Photo attendance stored offline');
      }

      closePhotoDialog();
    } catch {
      setError('Failed to submit photo attendance');
    } finally {
      setLoading(false);
    }
  };

  const closePhotoDialog = () => {
    setPhotoDialog(false);
    setSelectedStudent(null);
    setCapturedPhoto(null);

    if (videoRef.current?.srcObject) {
      const stream = videoRef.current.srcObject as MediaStream;
      stream.getTracks().forEach(track => track.stop());
    }
  };

  const handleManualAttendance = async () => {
    if (!manualStudentId || !selectedTrip) return;

    setLoading(true);
    try {
      if (isOnline) {
        const response = await apiClient.post('/api/attendance', {
          studentId: manualStudentId,
          tripId: selectedTrip,
          status: manualStatus,
          attendanceDate: new Date().toISOString(),
          boardingTime:
            manualStatus === 'Present' ? new Date().toISOString() : null,
        });

        if ((response as any).data.success) {
          setSuccess('Manual attendance recorded successfully');
          loadAttendanceRecords(selectedTrip);
        } else {
          setError(
            (response as any).data.message ||
              'Failed to record manual attendance'
          );
        }
      } else {
        const offlineRecord: OfflineRecord = {
          studentId: manualStudentId,
          tripId: selectedTrip,
          status: manualStatus,
          boardingTime:
            manualStatus === 'Present' ? new Date().toISOString() : undefined,
          notes: 'Manual entry',
          attendanceDate: new Date().toISOString(),
          method: 'Manual',
        };

        setOfflineRecords(prev => [...prev, offlineRecord]);
        setSuccess('Manual attendance stored offline');
      }

      setManualStudentId(null);
      setManualStatus('Present');
    } catch {
      setError('Failed to record manual attendance');
    } finally {
      setLoading(false);
    }
  };

  const exportOfflineData = () => {
    const dataStr = JSON.stringify(offlineRecords, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `offline-attendance-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Present':
        return 'success';
      case 'Absent':
        return 'error';
      case 'Late':
        return 'warning';
      default:
        return 'default';
    }
  };

  const getMethodIcon = (method: string) => {
    switch (method) {
      case 'RFID':
        return <QrCodeScanner />;
      case 'Photo':
        return <CameraAlt />;
      case 'Manual':
        return <Person />;
      case 'Offline':
        return <WifiOff />;
      default:
        return <CheckCircle />;
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Multi-Method Attendance Tracker
      </Typography>

      {/* Connection Status */}
      <Alert
        severity={isOnline ? 'success' : 'warning'}
        icon={isOnline ? <Wifi /> : <WifiOff />}
        sx={{ mb: 2 }}
      >
        {isOnline
          ? 'Online - Real-time sync enabled'
          : 'Offline - Data will be synced when connection is restored'}
        {!isOnline && offlineRecords.length > 0 && (
          <Chip
            label={`${offlineRecords.length} records pending sync`}
            size="small"
            sx={{ ml: 1 }}
          />
        )}
      </Alert>

      {/* Trip Selection */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Select Trip</InputLabel>
                <Select
                  value={selectedTrip || ''}
                  onChange={e => {
                    const tripId = Number(e.target.value);
                    setSelectedTrip(tripId);
                    loadAttendanceRecords(tripId);
                  }}
                >
                  {trips.map(trip => (
                    <MenuItem key={trip.id} value={trip.id}>
                      {trip.routeName} - {trip.vehicleName} (
                      {new Date(trip.scheduledStartTime).toLocaleTimeString()})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="outlined"
                  startIcon={<Refresh />}
                  onClick={() =>
                    selectedTrip && loadAttendanceRecords(selectedTrip)
                  }
                  disabled={loading}
                >
                  Refresh
                </Button>
                {!isOnline && offlineRecords.length > 0 && (
                  <>
                    <Button
                      variant="contained"
                      startIcon={<Upload />}
                      onClick={syncOfflineRecords}
                      disabled={loading}
                    >
                      Sync ({offlineRecords.length})
                    </Button>
                    <Button
                      variant="outlined"
                      startIcon={<Download />}
                      onClick={exportOfflineData}
                    >
                      Export
                    </Button>
                  </>
                )}
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Attendance Methods Tabs */}
      <Card sx={{ mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={(_, newValue) => setActiveTab(newValue)}
        >
          <Tab label="RFID Scanner" icon={<QrCodeScanner />} />
          <Tab label="Photo Capture" icon={<CameraAlt />} />
          <Tab label="Manual Entry" icon={<Person />} />
        </Tabs>

        <CardContent>
          {/* RFID Scanner Tab */}
          {activeTab === 0 && (
            <Box>
              <Typography variant="h6" gutterBottom>
                RFID Scanner
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, md: 8 }}>
                  <TextField
                    fullWidth
                    label="Scan or Enter RFID Tag"
                    value={rfidInput}
                    onChange={e => setRfidInput(e.target.value)}
                    onKeyPress={e => e.key === 'Enter' && handleRfidScan()}
                    placeholder="Tap RFID card or enter tag manually"
                    disabled={rfidScanning}
                  />
                </Grid>
                <Grid size={{ xs: 12, md: 4 }}>
                  <Button
                    fullWidth
                    variant="contained"
                    onClick={handleRfidScan}
                    disabled={
                      !rfidInput.trim() || rfidScanning || !selectedTrip
                    }
                    startIcon={
                      rfidScanning ? (
                        <CircularProgress size={20} />
                      ) : (
                        <QrCodeScanner />
                      )
                    }
                  >
                    {rfidScanning ? 'Processing...' : 'Record Attendance'}
                  </Button>
                </Grid>
              </Grid>
            </Box>
          )}

          {/* Photo Capture Tab */}
          {activeTab === 1 && (
            <Box>
              <Typography variant="h6" gutterBottom>
                Photo Verification
              </Typography>
              <Grid container spacing={2}>
                {students.map(student => (
                  <Grid key={student.id} size={{ xs: 12, sm: 6, md: 4 }}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle1">
                          {student.firstName} {student.lastName}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {student.studentNumber}
                        </Typography>
                        <Button
                          fullWidth
                          variant="outlined"
                          startIcon={<CameraAlt />}
                          onClick={() => startPhotoCapture(student)}
                          sx={{ mt: 1 }}
                          disabled={!selectedTrip}
                        >
                          Take Photo
                        </Button>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}

          {/* Manual Entry Tab */}
          {activeTab === 2 && (
            <Box>
              <Typography variant="h6" gutterBottom>
                Manual Attendance Entry
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, md: 4 }}>
                  <FormControl fullWidth>
                    <InputLabel>Select Student</InputLabel>
                    <Select
                      value={manualStudentId || ''}
                      onChange={e => setManualStudentId(Number(e.target.value))}
                    >
                      {students.map(student => (
                        <MenuItem key={student.id} value={student.id}>
                          {student.firstName} {student.lastName} (
                          {student.studentNumber})
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid size={{ xs: 12, md: 4 }}>
                  <FormControl fullWidth>
                    <InputLabel>Status</InputLabel>
                    <Select
                      value={manualStatus}
                      onChange={e =>
                        setManualStatus(
                          e.target.value as 'Present' | 'Absent' | 'Late'
                        )
                      }
                    >
                      <MenuItem value="Present">Present</MenuItem>
                      <MenuItem value="Absent">Absent</MenuItem>
                      <MenuItem value="Late">Late</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid size={{ xs: 12, md: 4 }}>
                  <Button
                    fullWidth
                    variant="contained"
                    onClick={handleManualAttendance}
                    disabled={!manualStudentId || !selectedTrip || loading}
                    startIcon={
                      loading ? <CircularProgress size={20} /> : <Person />
                    }
                  >
                    Record Attendance
                  </Button>
                </Grid>
              </Grid>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* Attendance Records */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Today's Attendance Records
          </Typography>
          {attendanceRecords.length === 0 ? (
            <Typography color="text.secondary">
              No attendance records found for selected trip
            </Typography>
          ) : (
            <List>
              {attendanceRecords.map(record => (
                <ListItem key={record.id}>
                  <ListItemIcon>{getMethodIcon(record.method)}</ListItemIcon>
                  <ListItemText
                    primary={record.studentName}
                    secondary={
                      <Box
                        sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                      >
                        <Chip
                          label={record.status}
                          color={getStatusColor(record.status) as any}
                          size="small"
                        />
                        <Typography variant="caption">
                          {record.method} •{' '}
                          {new Date(record.timestamp).toLocaleTimeString()}
                        </Typography>
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Photo Capture Dialog */}
      <Dialog
        open={photoDialog}
        onClose={closePhotoDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Photo Verification - {selectedStudent?.firstName}{' '}
          {selectedStudent?.lastName}
        </DialogTitle>
        <DialogContent>
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: 2,
            }}
          >
            {!capturedPhoto ? (
              <>
                <video
                  ref={videoRef}
                  autoPlay
                  playsInline
                  style={{ width: '100%', maxWidth: 400, borderRadius: 8 }}
                />
                <Button
                  variant="contained"
                  startIcon={<CameraAlt />}
                  onClick={capturePhoto}
                >
                  Capture Photo
                </Button>
              </>
            ) : (
              <>
                <img
                  src={capturedPhoto}
                  alt="Captured"
                  style={{ width: '100%', maxWidth: 400, borderRadius: 8 }}
                />
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Button
                    variant="outlined"
                    onClick={() => setCapturedPhoto(null)}
                  >
                    Retake
                  </Button>
                  <Button
                    variant="contained"
                    onClick={submitPhotoAttendance}
                    disabled={loading}
                    startIcon={
                      loading ? <CircularProgress size={20} /> : <CheckCircle />
                    }
                  >
                    Confirm Attendance
                  </Button>
                </Box>
              </>
            )}
          </Box>
          <canvas ref={canvasRef} style={{ display: 'none' }} />
        </DialogContent>
        <DialogActions>
          <Button onClick={closePhotoDialog}>Cancel</Button>
        </DialogActions>
      </Dialog>

      {/* Status Messages */}
      {error && (
        <Alert severity="error" sx={{ mt: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert
          severity="success"
          sx={{ mt: 2 }}
          onClose={() => setSuccess(null)}
        >
          {success}
        </Alert>
      )}
    </Box>
  );
};

export default MultiMethodAttendanceTracker;
