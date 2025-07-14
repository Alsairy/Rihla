import React, { useState, useCallback, useRef } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  LinearProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  Tooltip,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Description as DocumentIcon,
  Image as ImageIcon,
  PictureAsPdf as PdfIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  CheckCircle as CheckIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface DocumentUploadProps {
  entityType: 'driver' | 'vehicle' | 'student';
  entityId?: number;
  documentType?: string;
  maxFiles?: number;
  maxSizeBytes?: number;
  allowedTypes?: string[];
  onUploadComplete?: (files: UploadedFile[]) => void;
  onUploadError?: (error: string) => void;
  existingFiles?: UploadedFile[];
  disabled?: boolean;
}

interface UploadedFile {
  id?: number;
  name: string;
  size: number;
  type: string;
  url?: string;
  uploadProgress?: number;
  status: 'uploading' | 'completed' | 'error';
  errorMessage?: string;
  documentType?: string;
  uploadedAt?: string;
}

interface FilePreview {
  file: UploadedFile;
  previewUrl?: string;
}

const DocumentUpload: React.FC<DocumentUploadProps> = ({
  entityType,
  entityId,
  documentType,
  maxFiles = 10,
  maxSizeBytes = 10 * 1024 * 1024, // 10MB default
  allowedTypes = [
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'image/jpeg',
    'image/png',
    'image/gif',
    'image/webp',
  ],
  onUploadComplete,
  onUploadError,
  existingFiles = [],
  disabled = false,
}) => {
  const [files, setFiles] = useState<UploadedFile[]>(existingFiles);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [previewDialog, setPreviewDialog] = useState<FilePreview | null>(null);
  const [dragActive, setDragActive] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const getFileIcon = (fileType: string) => {
    if (fileType.startsWith('image/')) return <ImageIcon />;
    if (fileType === 'application/pdf') return <PdfIcon />;
    return <DocumentIcon />;
  };

  const getFileTypeLabel = (fileType: string) => {
    const typeMap: { [key: string]: string } = {
      'application/pdf': 'PDF',
      'application/msword': 'DOC',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'DOCX',
      'image/jpeg': 'JPEG',
      'image/png': 'PNG',
      'image/gif': 'GIF',
      'image/webp': 'WebP',
    };
    return typeMap[fileType] || 'Unknown';
  };

  const validateFile = (file: File): string | null => {
    if (file.size > maxSizeBytes) {
      return `File size exceeds ${Math.round(maxSizeBytes / (1024 * 1024))}MB limit`;
    }

    if (!allowedTypes.includes(file.type)) {
      return `File type ${file.type} is not allowed`;
    }

    if (files.length >= maxFiles) {
      return `Maximum ${maxFiles} files allowed`;
    }

    return null;
  };

  const uploadFile = async (file: File): Promise<UploadedFile> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    if (entityId) formData.append('entityId', entityId.toString());
    if (documentType) formData.append('documentType', documentType);

    try {
      const response = await apiClient.post('/api/files/upload', formData) as any;

      return {
        id: response.id,
        name: file.name,
        size: file.size,
        type: file.type,
        url: response.url,
        status: 'completed',
        documentType: response.documentType,
        uploadedAt: response.uploadedAt,
      };
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Upload failed');
    }
  };

  const handleFilesSelected = useCallback(
    async (acceptedFiles: File[]) => {
      if (disabled) return;

      setError(null);
      setUploading(true);

      const validFiles: File[] = [];
      const errors: string[] = [];

      for (const file of acceptedFiles) {
        const validationError = validateFile(file);
        if (validationError) {
          errors.push(`${file.name}: ${validationError}`);
        } else {
          validFiles.push(file);
        }
      }

      if (errors.length > 0) {
        setError(errors.join(', '));
        setUploading(false);
        return;
      }

      const newFiles: UploadedFile[] = validFiles.map(file => ({
        name: file.name,
        size: file.size,
        type: file.type,
        status: 'uploading' as const,
        uploadProgress: 0,
      }));

      setFiles(prev => [...prev, ...newFiles]);

      const uploadPromises = validFiles.map(async (file) => {
        try {
          const uploadedFile = await uploadFile(file);
          setFiles(prev =>
            prev.map(f =>
              f.name === file.name && f.status === 'uploading'
                ? uploadedFile
                : f
            )
          );
          return uploadedFile;
        } catch (error: any) {
          setFiles(prev =>
            prev.map(f =>
              f.name === file.name && f.status === 'uploading'
                ? { ...f, status: 'error', errorMessage: error.message }
                : f
            )
          );
          throw error;
        }
      });

      try {
        const uploadedFiles = await Promise.allSettled(uploadPromises);
        const successfulUploads = uploadedFiles
          .filter((result): result is PromiseFulfilledResult<UploadedFile> => 
            result.status === 'fulfilled'
          )
          .map(result => result.value);

        const failedUploads = uploadedFiles
          .filter((result): result is PromiseRejectedResult => 
            result.status === 'rejected'
          );

        if (failedUploads.length > 0) {
          const errorMessages = failedUploads.map(result => result.reason.message);
          setError(`Some uploads failed: ${errorMessages.join(', ')}`);
          onUploadError?.(errorMessages.join(', '));
        }

        if (successfulUploads.length > 0) {
          onUploadComplete?.(successfulUploads);
        }
      } catch (error: any) {
        setError('Upload failed: ' + error.message);
        onUploadError?.(error.message);
      } finally {
        setUploading(false);
      }
    },
    [disabled, entityType, entityId, documentType, maxFiles, maxSizeBytes, allowedTypes, onUploadComplete, onUploadError, uploadFile, validateFile]
  );

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!disabled && !uploading) {
      setDragActive(true);
    }
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    
    if (disabled || uploading) return;
    
    const droppedFiles = e.dataTransfer.files;
    if (droppedFiles.length > 0) {
      handleFilesSelected(Array.from(droppedFiles));
    }
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = e.target.files;
    if (selectedFiles && selectedFiles.length > 0) {
      handleFilesSelected(Array.from(selectedFiles));
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleUploadClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleDeleteFile = async (fileToDelete: UploadedFile) => {
    if (fileToDelete.id) {
      try {
        await apiClient.delete(`/api/files/${fileToDelete.id}`);
      } catch (error) {
      }
    }

    setFiles(prev => prev.filter(f => f !== fileToDelete));
  };

  const handlePreviewFile = (file: UploadedFile) => {
    if (file.type.startsWith('image/') && file.url) {
      setPreviewDialog({ file, previewUrl: file.url });
    } else if (file.url) {
      window.open(file.url, '_blank');
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getStatusIcon = (file: UploadedFile) => {
    switch (file.status) {
      case 'completed':
        return <CheckIcon color="success" />;
      case 'error':
        return <ErrorIcon color="error" />;
      case 'uploading':
        return <WarningIcon color="warning" />;
      default:
        return null;
    }
  };

  return (
    <Box>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Upload Area */}
      <Paper
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={handleUploadClick}
        sx={{
          p: 3,
          border: '2px dashed',
          borderColor: dragActive ? 'primary.main' : 'grey.300',
          bgcolor: dragActive ? 'primary.50' : disabled ? 'grey.100' : 'background.paper',
          cursor: disabled || uploading ? 'not-allowed' : 'pointer',
          textAlign: 'center',
          transition: 'all 0.2s ease-in-out',
          '&:hover': {
            borderColor: disabled || uploading ? 'grey.300' : 'primary.main',
            bgcolor: disabled || uploading ? 'grey.100' : 'primary.50',
          },
        }}
      >
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={allowedTypes.join(',')}
          onChange={handleFileInputChange}
          style={{ display: 'none' }}
          disabled={disabled || uploading}
        />
        <UploadIcon
          sx={{
            fontSize: 48,
            color: disabled || uploading ? 'grey.400' : 'primary.main',
            mb: 2,
          }}
        />
        <Typography variant="h6" sx={{ mb: 1 }}>
          {dragActive
            ? 'Drop files here...'
            : disabled
            ? 'Upload disabled'
            : uploading
            ? 'Uploading...'
            : 'Drag & drop files here, or click to select'}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Supported formats: {allowedTypes.map(type => getFileTypeLabel(type)).join(', ')}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Max file size: {Math.round(maxSizeBytes / (1024 * 1024))}MB • Max files: {maxFiles}
        </Typography>
        {uploading && <LinearProgress sx={{ mt: 2 }} />}
      </Paper>

      {/* File List */}
      {files.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Files ({files.length}/{maxFiles})
          </Typography>
          <List>
            {files.map((file, index) => (
              <ListItem
                key={`${file.name}-${index}`}
                sx={{
                  border: '1px solid',
                  borderColor: 'grey.200',
                  borderRadius: 1,
                  mb: 1,
                  bgcolor: 'background.paper',
                }}
              >
                <ListItemIcon>
                  {getFileIcon(file.type)}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle2" noWrap>
                        {file.name}
                      </Typography>
                      <Chip
                        label={getFileTypeLabel(file.type)}
                        size="small"
                        variant="outlined"
                      />
                      {getStatusIcon(file)}
                    </Box>
                  }
                  secondary={
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        {formatFileSize(file.size)}
                        {file.documentType && ` • ${file.documentType}`}
                        {file.uploadedAt && ` • ${new Date(file.uploadedAt).toLocaleDateString()}`}
                      </Typography>
                      {file.status === 'uploading' && file.uploadProgress !== undefined && (
                        <LinearProgress
                          variant="determinate"
                          value={file.uploadProgress}
                          sx={{ mt: 1 }}
                        />
                      )}
                      {file.status === 'error' && file.errorMessage && (
                        <Typography variant="caption" color="error">
                          Error: {file.errorMessage}
                        </Typography>
                      )}
                    </Box>
                  }
                />
                <ListItemSecondaryAction>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {file.status === 'completed' && file.url && (
                      <Tooltip title="Preview">
                        <IconButton
                          edge="end"
                          size="small"
                          onClick={() => handlePreviewFile(file)}
                        >
                          <ViewIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Delete">
                      <IconButton
                        edge="end"
                        size="small"
                        color="error"
                        onClick={() => handleDeleteFile(file)}
                        disabled={file.status === 'uploading'}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        </Box>
      )}

      {/* Image Preview Dialog */}
      <Dialog
        open={!!previewDialog}
        onClose={() => setPreviewDialog(null)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6">
            {previewDialog?.file.name}
          </Typography>
        </DialogTitle>
        <DialogContent>
          {previewDialog?.previewUrl && (
            <Box sx={{ textAlign: 'center' }}>
              <img
                src={previewDialog.previewUrl}
                alt={previewDialog.file.name}
                style={{
                  maxWidth: '100%',
                  maxHeight: '70vh',
                  objectFit: 'contain',
                }}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPreviewDialog(null)}>Close</Button>
          {previewDialog?.file.url && (
            <Button
              variant="contained"
              onClick={() => window.open(previewDialog.file.url, '_blank')}
            >
              Open in New Tab
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DocumentUpload;
