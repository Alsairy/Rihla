import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Alert,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  Paper,
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  CloudUpload,
  CloudDownload,
  Wifi,
  WifiOff,
  CheckCircle,
  Error,
  Delete,
  Refresh,
  Storage,
  Sync,
  Schedule,
  Info,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface OfflineRecord {
  id: string;
  studentId: number;
  studentName: string;
  tripId: number;
  tripName: string;
  status: 'Present' | 'Absent' | 'Late';
  boardingTime?: string;
  alightingTime?: string;
  notes: string;
  attendanceDate: string;
  method: 'Manual' | 'RFID' | 'Photo';
  timestamp: string;
  synced: boolean;
  retryCount: number;
  lastError?: string;
}

interface SyncStatus {
  totalRecords: number;
  syncedRecords: number;
  failedRecords: number;
  inProgress: boolean;
  lastSyncTime?: string;
  errors: string[];
}

const OfflineAttendanceSync: React.FC = () => {
  const [offlineRecords, setOfflineRecords] = useState<OfflineRecord[]>([]);
  const [syncStatus, setSyncStatus] = useState<SyncStatus>({
    totalRecords: 0,
    syncedRecords: 0,
    failedRecords: 0,
    inProgress: false,
    errors: [],
  });
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [loading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selectedRecord, setSelectedRecord] = useState<OfflineRecord | null>(
    null
  );
  const [detailDialog, setDetailDialog] = useState(false);
  const [autoSync] = useState(true);

  const loadOfflineRecords = useCallback(() => {
    try {
      const stored = localStorage.getItem('offlineAttendanceRecords');
      if (stored) {
        const records: OfflineRecord[] = JSON.parse(stored);
        setOfflineRecords(records);
        updateSyncStatus(records);
      }
    } catch {
      setError('Failed to load offline records from storage');
    }
  }, []);

  const syncAllRecords = useCallback(async () => {
    if (!isOnline || syncStatus.inProgress) return;

    const unsyncedRecords = offlineRecords.filter(r => !r.synced);
    if (unsyncedRecords.length === 0) return;

    setSyncStatus(prev => ({ ...prev, inProgress: true }));

    const updatedRecords = [...offlineRecords];
    const syncPromises = unsyncedRecords.map(async record => {
      try {
        const response = await apiClient.post('/api/attendance/sync', record);
        if ((response as any).data.success) {
          const recordIndex = updatedRecords.findIndex(r => r.id === record.id);
          if (recordIndex !== -1) {
            updatedRecords[recordIndex] = {
              ...updatedRecords[recordIndex],
              synced: true,
              lastError: undefined,
              retryCount: 0,
            };
          }
        } else {
          const recordIndex = updatedRecords.findIndex(r => r.id === record.id);
          if (recordIndex !== -1) {
            updatedRecords[recordIndex] = {
              ...updatedRecords[recordIndex],
              lastError: 'Sync failed',
              retryCount: (updatedRecords[recordIndex].retryCount || 0) + 1,
            };
          }
        }
      } catch (error) {
        const recordIndex = updatedRecords.findIndex(r => r.id === record.id);
        if (recordIndex !== -1) {
          updatedRecords[recordIndex] = {
            ...updatedRecords[recordIndex],
            lastError: (error as Error).message ?? 'Unknown error',
            retryCount: (updatedRecords[recordIndex].retryCount || 0) + 1,
          };
        }
      }
    });

    await Promise.all(syncPromises);
    setOfflineRecords(updatedRecords);
    updateSyncStatus(updatedRecords);
    setSyncStatus(prev => ({ ...prev, inProgress: false }));
  }, [isOnline, offlineRecords, syncStatus.inProgress]);

  useEffect(() => {
    loadOfflineRecords();

    const handleOnline = () => {
      setIsOnline(true);
      if (autoSync) {
        syncAllRecords();
      }
    };

    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, [autoSync, loadOfflineRecords, syncAllRecords]);

  useEffect(() => {
    if (isOnline && autoSync) {
      const interval = setInterval(() => {
        syncAllRecords();
      }, 30000); // Auto-sync every 30 seconds when online

      return () => clearInterval(interval);
    }
  }, [isOnline, autoSync, syncAllRecords]);

  const saveOfflineRecords = (records: OfflineRecord[]) => {
    try {
      localStorage.setItem('offlineAttendanceRecords', JSON.stringify(records));
      setOfflineRecords(records);
      updateSyncStatus(records);
    } catch {
      setError('Failed to save offline records to storage');
    }
  };

  const updateSyncStatus = (records: OfflineRecord[]) => {
    const totalRecords = records.length;
    const syncedRecords = records.filter(r => r.synced).length;
    const failedRecords = records.filter(
      r => !r.synced && r.retryCount > 0
    ).length;
    const errors = records
      .filter(r => r.lastError)
      .map(r => `${r.studentName}: ${r.lastError}`)
      .slice(0, 5); // Show only first 5 errors

    setSyncStatus({
      totalRecords,
      syncedRecords,
      failedRecords,
      inProgress: false,
      lastSyncTime: syncedRecords > 0 ? new Date().toISOString() : undefined,
      errors,
    });
  };


  const deleteRecord = (recordId: string) => {
    const updatedRecords = offlineRecords.filter(r => r.id !== recordId);
    saveOfflineRecords(updatedRecords);
    setSuccess('Record deleted successfully');
  };

  const clearSyncedRecords = () => {
    const unsyncedRecords = offlineRecords.filter(r => !r.synced);
    saveOfflineRecords(unsyncedRecords);
    setSuccess('Synced records cleared successfully');
  };

  const exportRecords = () => {
    const dataStr = JSON.stringify(offlineRecords, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `offline-attendance-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const importRecords = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = e => {
      try {
        const importedRecords: OfflineRecord[] = JSON.parse(
          e.target?.result as string
        );
        const mergedRecords = [...offlineRecords];

        importedRecords.forEach(importedRecord => {
          const existingIndex = mergedRecords.findIndex(
            r => r.id === importedRecord.id
          );
          if (existingIndex === -1) {
            mergedRecords.push(importedRecord);
          }
        });

        saveOfflineRecords(mergedRecords);
        setSuccess(`Imported ${importedRecords.length} records`);
      } catch {
        setError('Failed to import records. Invalid file format.');
      }
    };
    reader.readAsText(file);
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
        return '📱';
      case 'Photo':
        return '📷';
      case 'Manual':
        return '✏️';
      default:
        return '📝';
    }
  };

  const getSyncProgress = () => {
    if (syncStatus.totalRecords === 0) return 0;
    return (syncStatus.syncedRecords / syncStatus.totalRecords) * 100;
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Offline Attendance Sync
      </Typography>

      {/* Connection Status */}
      <Alert
        severity={isOnline ? 'success' : 'warning'}
        icon={isOnline ? <Wifi /> : <WifiOff />}
        sx={{ mb: 2 }}
      >
        {isOnline
          ? 'Online - Auto-sync enabled'
          : 'Offline - Records will be synced when connection is restored'}
      </Alert>

      {/* Sync Status Overview */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Storage color="primary" />
                <Box>
                  <Typography variant="h6">
                    {syncStatus.totalRecords}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Records
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <CheckCircle color="success" />
                <Box>
                  <Typography variant="h6">
                    {syncStatus.syncedRecords}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Synced
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Schedule color="warning" />
                <Box>
                  <Typography variant="h6">
                    {syncStatus.totalRecords -
                      syncStatus.syncedRecords -
                      syncStatus.failedRecords}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Pending
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Error color="error" />
                <Box>
                  <Typography variant="h6">
                    {syncStatus.failedRecords}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Failed
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Sync Progress */}
      {syncStatus.totalRecords > 0 && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 1,
              }}
            >
              <Typography variant="h6">Sync Progress</Typography>
              <Typography variant="body2" color="text.secondary">
                {syncStatus.syncedRecords} / {syncStatus.totalRecords} synced
              </Typography>
            </Box>
            <LinearProgress
              variant="determinate"
              value={getSyncProgress()}
              sx={{ height: 8, borderRadius: 4 }}
            />
            {syncStatus.lastSyncTime && (
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ mt: 1, display: 'block' }}
              >
                Last sync: {new Date(syncStatus.lastSyncTime).toLocaleString()}
              </Typography>
            )}
          </CardContent>
        </Card>
      )}

      {/* Control Panel */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="contained"
                  startIcon={
                    syncStatus.inProgress ? (
                      <CircularProgress size={20} />
                    ) : (
                      <Sync />
                    )
                  }
                  onClick={syncAllRecords}
                  disabled={
                    !isOnline ||
                    syncStatus.inProgress ||
                    offlineRecords.filter(r => !r.synced).length === 0
                  }
                >
                  {syncStatus.inProgress ? 'Syncing...' : 'Sync All'}
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<Refresh />}
                  onClick={loadOfflineRecords}
                  disabled={loading}
                >
                  Refresh
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<Delete />}
                  onClick={clearSyncedRecords}
                  disabled={syncStatus.syncedRecords === 0}
                >
                  Clear Synced
                </Button>
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                <Button
                  variant="outlined"
                  startIcon={<CloudDownload />}
                  onClick={exportRecords}
                  disabled={offlineRecords.length === 0}
                >
                  Export
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<CloudUpload />}
                  component="label"
                >
                  Import
                  <input
                    type="file"
                    accept=".json"
                    hidden
                    onChange={importRecords}
                  />
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Error Summary */}
      {syncStatus.errors.length > 0 && (
        <Alert severity="error" sx={{ mb: 3 }}>
          <Typography variant="subtitle2" gutterBottom>
            Recent Sync Errors:
          </Typography>
          <List dense>
            {syncStatus.errors.map((error, index) => (
              <ListItem key={index} sx={{ py: 0 }}>
                <Typography variant="body2">• {error}</Typography>
              </ListItem>
            ))}
          </List>
        </Alert>
      )}

      {/* Records List */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Offline Records ({offlineRecords.length})
          </Typography>
          {offlineRecords.length === 0 ? (
            <Typography color="text.secondary">
              No offline records found
            </Typography>
          ) : (
            <List>
              {offlineRecords.map((record, index) => (
                <React.Fragment key={record.id}>
                  <ListItem>
                    <ListItemIcon>
                      <Typography variant="h6">
                        {getMethodIcon(record.method)}
                      </Typography>
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box
                          sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                        >
                          <Typography variant="subtitle1">
                            {record.studentName}
                          </Typography>
                          <Chip
                            label={record.status}
                            color={getStatusColor(record.status) as any}
                            size="small"
                          />
                          {record.synced ? (
                            <Chip label="Synced" color="success" size="small" />
                          ) : record.retryCount > 0 ? (
                            <Chip
                              label={`Failed (${record.retryCount})`}
                              color="error"
                              size="small"
                            />
                          ) : (
                            <Chip
                              label="Pending"
                              color="warning"
                              size="small"
                            />
                          )}
                        </Box>
                      }
                      secondary={
                        <Box>
                          <Typography variant="body2">
                            {record.tripName} • {record.method} •{' '}
                            {new Date(record.timestamp).toLocaleString()}
                          </Typography>
                          {record.lastError && (
                            <Typography variant="caption" color="error">
                              Error: {record.lastError}
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="View Details">
                        <IconButton
                          size="small"
                          onClick={() => {
                            setSelectedRecord(record);
                            setDetailDialog(true);
                          }}
                        >
                          <Info />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete Record">
                        <IconButton
                          size="small"
                          onClick={() => deleteRecord(record.id)}
                          color="error"
                        >
                          <Delete />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </ListItem>
                  {index < offlineRecords.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Record Detail Dialog */}
      <Dialog
        open={detailDialog}
        onClose={() => setDetailDialog(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Record Details</DialogTitle>
        <DialogContent>
          {selectedRecord && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Student Information
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Student Name
                    </Typography>
                    <Typography variant="body1">
                      {selectedRecord.studentName}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Student ID
                    </Typography>
                    <Typography variant="body1">
                      {selectedRecord.studentId}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Attendance Details
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Status
                    </Typography>
                    <Chip
                      label={selectedRecord.status}
                      color={getStatusColor(selectedRecord.status) as any}
                      size="small"
                    />
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Method
                    </Typography>
                    <Typography variant="body1">
                      {selectedRecord.method}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Date
                    </Typography>
                    <Typography variant="body1">
                      {new Date(
                        selectedRecord.attendanceDate
                      ).toLocaleDateString()}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Recorded At
                    </Typography>
                    <Typography variant="body1">
                      {new Date(selectedRecord.timestamp).toLocaleString()}
                    </Typography>
                  </Grid>
                  {selectedRecord.boardingTime && (
                    <Grid size={{ xs: 6 }}>
                      <Typography variant="body2" color="text.secondary">
                        Boarding Time
                      </Typography>
                      <Typography variant="body1">
                        {new Date(
                          selectedRecord.boardingTime
                        ).toLocaleTimeString()}
                      </Typography>
                    </Grid>
                  )}
                  {selectedRecord.alightingTime && (
                    <Grid size={{ xs: 6 }}>
                      <Typography variant="body2" color="text.secondary">
                        Alighting Time
                      </Typography>
                      <Typography variant="body1">
                        {new Date(
                          selectedRecord.alightingTime
                        ).toLocaleTimeString()}
                      </Typography>
                    </Grid>
                  )}
                </Grid>
                {selectedRecord.notes && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="text.secondary">
                      Notes
                    </Typography>
                    <Typography variant="body1">
                      {selectedRecord.notes}
                    </Typography>
                  </Box>
                )}
              </Paper>

              <Paper sx={{ p: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Sync Status
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Status
                    </Typography>
                    <Chip
                      label={selectedRecord.synced ? 'Synced' : 'Pending'}
                      color={selectedRecord.synced ? 'success' : 'warning'}
                      size="small"
                    />
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Retry Count
                    </Typography>
                    <Typography variant="body1">
                      {selectedRecord.retryCount}
                    </Typography>
                  </Grid>
                  {selectedRecord.lastError && (
                    <Grid size={{ xs: 12 }}>
                      <Typography variant="body2" color="text.secondary">
                        Last Error
                      </Typography>
                      <Typography variant="body1" color="error">
                        {selectedRecord.lastError}
                      </Typography>
                    </Grid>
                  )}
                </Grid>
              </Paper>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialog(false)}>Close</Button>
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

export default OfflineAttendanceSync;
