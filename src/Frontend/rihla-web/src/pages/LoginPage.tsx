import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  CircularProgress,
  Avatar,
  InputAdornment,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  DirectionsBus as BusIcon,
  Email as EmailIcon,
  Lock as LockIcon,
  Visibility,
  VisibilityOff,
  Security as SecurityIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [failedAttempts, setFailedAttempts] = useState(0);
  const [isAccountLocked, setIsAccountLocked] = useState(false);
  const [lockoutEndTime, setLockoutEndTime] = useState<Date | null>(null);
  const [lockoutTimeRemaining, setLockoutTimeRemaining] = useState<string>('');
  const [showMfaDialog, setShowMfaDialog] = useState(false);
  const [mfaCode, setMfaCode] = useState('');
  const [mfaError, setMfaError] = useState('');
  const [pendingMfaToken, setPendingMfaToken] = useState('');
  const emailRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);
  const { login, user } = useAuth();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (user) {
      switch (user.role) {
        case 'Admin':
          navigate('/admin');
          break;
        case 'Parent':
          navigate('/parent');
          break;
        case 'Driver':
          navigate('/driver');
          break;
        default:
          navigate('/');
      }
    }
  }, [user, navigate]);

  useEffect(() => {
    let interval: ReturnType<typeof setInterval>;
    
    if (isAccountLocked && lockoutEndTime) {
      interval = setInterval(() => {
        const now = new Date();
        const timeLeft = lockoutEndTime.getTime() - now.getTime();
        
        if (timeLeft <= 0) {
          setIsAccountLocked(false);
          setLockoutEndTime(null);
          setLockoutTimeRemaining('');
          setFailedAttempts(0);
          setError('');
        } else {
          const minutes = Math.floor(timeLeft / (1000 * 60));
          const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);
          setLockoutTimeRemaining(`${minutes}:${seconds.toString().padStart(2, '0')}`);
        }
      }, 1000);
    }
    
    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [isAccountLocked, lockoutEndTime]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMfaError('');
    setLoading(true);

    if (isAccountLocked) {
      setError(`Your account is locked. Please try again in ${lockoutTimeRemaining}.`);
      setLoading(false);
      return;
    }

    const emailValue = email || emailRef.current?.value || '';
    const passwordValue = password || passwordRef.current?.value || '';

    console.log('Login attempt with:', {
      email: emailValue,
      password: passwordValue ? '***' : 'empty',
      failedAttempts: failedAttempts,
    });

    try {
      const response = await login({ email: emailValue, password: passwordValue });
      
      if (response?.requiresMfa) {
        setPendingMfaToken(response.mfaToken || '');
        setShowMfaDialog(true);
        setLoading(false);
        return;
      }
      
      setFailedAttempts(0);
      setIsAccountLocked(false);
      setLockoutEndTime(null);
      
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Login failed. Please try again.';
      
      if (errorMessage.toLowerCase().includes('account is locked') || 
          errorMessage.toLowerCase().includes('account locked')) {
        const minutesMatch = errorMessage.match(/(\d+)\s*minutes?/i);
        const lockoutMinutes = minutesMatch ? parseInt(minutesMatch[1]) : 30;
        
        setError(`Your account is locked due to too many failed login attempts. Please try again in ${lockoutMinutes} minutes or contact an administrator.`);
        setIsAccountLocked(true);
        setLockoutEndTime(new Date(Date.now() + lockoutMinutes * 60 * 1000));
        setFailedAttempts(5);
        
      } else if (err.response?.status === 401 || errorMessage.toLowerCase().includes('invalid')) {
        const newFailedAttempts = failedAttempts + 1;
        setFailedAttempts(newFailedAttempts);
        
        if (newFailedAttempts >= 5) {
          setError('Your account has been locked due to too many failed login attempts. Please try again in 30 minutes or contact an administrator.');
          setIsAccountLocked(true);
          setLockoutEndTime(new Date(Date.now() + 30 * 60 * 1000));
        } else {
          const attemptsLeft = 5 - newFailedAttempts;
          setError(`Invalid email or password. ${attemptsLeft} attempt${attemptsLeft !== 1 ? 's' : ''} remaining before account lockout.`);
        }
        
      } else if (errorMessage.toLowerCase().includes('mfa') || errorMessage.toLowerCase().includes('verification')) {
        setError('Multi-factor authentication is required. Please contact your administrator.');
        
      } else {
        setError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClickShowPassword = () => {
    setShowPassword(!showPassword);
  };

  const handleMfaVerification = async () => {
    if (!mfaCode.trim()) {
      setMfaError('Please enter the verification code.');
      return;
    }

    setLoading(true);
    setMfaError('');

    try {
      await login({ 
        email: email, 
        password: password, 
        mfaCode: mfaCode.trim(),
        mfaToken: pendingMfaToken 
      });
      
      setShowMfaDialog(false);
      setMfaCode('');
      setPendingMfaToken('');
      setFailedAttempts(0);
      
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'MFA verification failed.';
      
      if (errorMessage.toLowerCase().includes('invalid') || 
          errorMessage.toLowerCase().includes('incorrect')) {
        setMfaError('Invalid verification code. Please try again.');
      } else {
        setMfaError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleCloseMfaDialog = () => {
    setShowMfaDialog(false);
    setMfaCode('');
    setMfaError('');
    setPendingMfaToken('');
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 2,
      }}
    >
      <Container component="main" maxWidth="sm">
        <Paper
          elevation={24}
          sx={{
            padding: { xs: 3, sm: 6 },
            borderRadius: 4,
            background: 'rgba(255, 255, 255, 0.95)',
            backdropFilter: 'blur(10px)',
            boxShadow: '0 20px 40px rgba(0,0,0,0.1)',
          }}
        >
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
            }}
          >
            <Avatar
              sx={{
                m: 1,
                bgcolor: 'primary.main',
                width: 80,
                height: 80,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              }}
            >
              <BusIcon sx={{ fontSize: 40 }} />
            </Avatar>

            <Typography
              component="h1"
              variant="h3"
              align="center"
              sx={{
                fontWeight: 700,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                mb: 1,
              }}
            >
              Rihla
            </Typography>

            <Typography
              component="h2"
              variant="h6"
              align="center"
              color="text.secondary"
              sx={{
                mb: 4,
                fontWeight: 400,
                letterSpacing: 0.5,
              }}
            >
              School Transportation Management System
            </Typography>

            {error && (
              <Alert
                severity={isAccountLocked ? "warning" : "error"}
                sx={{
                  mb: 3,
                  width: '100%',
                  borderRadius: 2,
                }}
              >
                {error}
                {isAccountLocked && lockoutTimeRemaining && (
                  <Typography variant="body2" sx={{ mt: 1, fontWeight: 'bold' }}>
                    Time remaining: {lockoutTimeRemaining}
                  </Typography>
                )}
              </Alert>
            )}

            {failedAttempts > 0 && failedAttempts < 5 && !isAccountLocked && (
              <Alert
                severity="warning"
                sx={{
                  mb: 3,
                  width: '100%',
                  borderRadius: 2,
                }}
              >
                Security Notice: {failedAttempts} failed login attempt{failedAttempts !== 1 ? 's' : ''}. 
                Your account will be locked after 5 failed attempts.
              </Alert>
            )}

            <form onSubmit={handleSubmit} style={{ width: '100%' }}>
              <TextField
                margin="normal"
                required
                fullWidth
                id="email"
                label="Email Address"
                name="email"
                autoComplete="email"
                autoFocus
                inputRef={emailRef}
                value={email}
                onChange={e => {
                  console.log('Email onChange:', e.target.value);
                  setEmail(e.target.value);
                }}
                onInput={e => {
                  console.log(
                    'Email onInput:',
                    (e.target as HTMLInputElement).value
                  );
                  setEmail((e.target as HTMLInputElement).value);
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <EmailIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                sx={{
                  mb: 2,
                  '& .MuiOutlinedInput-root': {
                    borderRadius: 2,
                    '&:hover fieldset': {
                      borderColor: 'primary.main',
                    },
                  },
                }}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                name="password"
                label="Password"
                type={showPassword ? 'text' : 'password'}
                id="password"
                autoComplete="current-password"
                inputRef={passwordRef}
                value={password}
                onChange={e => {
                  console.log(
                    'Password onChange:',
                    e.target.value ? '***' : 'empty'
                  );
                  setPassword(e.target.value);
                }}
                onInput={e => {
                  console.log(
                    'Password onInput:',
                    (e.target as HTMLInputElement).value ? '***' : 'empty'
                  );
                  setPassword((e.target as HTMLInputElement).value);
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockIcon color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={handleClickShowPassword}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{
                  mb: 3,
                  '& .MuiOutlinedInput-root': {
                    borderRadius: 2,
                    '&:hover fieldset': {
                      borderColor: 'primary.main',
                    },
                  },
                }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                disabled={loading || isAccountLocked}
                sx={{
                  mt: 2,
                  mb: 2,
                  py: 1.5,
                  borderRadius: 2,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  textTransform: 'none',
                  background: isAccountLocked 
                    ? 'rgba(0, 0, 0, 0.12)' 
                    : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  boxShadow: isAccountLocked 
                    ? 'none' 
                    : '0 8px 20px rgba(102, 126, 234, 0.3)',
                  '&:hover': {
                    background: isAccountLocked 
                      ? 'rgba(0, 0, 0, 0.12)' 
                      : 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                    boxShadow: isAccountLocked 
                      ? 'none' 
                      : '0 12px 24px rgba(102, 126, 234, 0.4)',
                    transform: isAccountLocked ? 'none' : 'translateY(-2px)',
                  },
                  '&:disabled': {
                    background: 'rgba(0, 0, 0, 0.12)',
                  },
                  transition: 'all 0.3s ease',
                }}
              >
                {loading ? (
                  <CircularProgress size={24} color="inherit" />
                ) : isAccountLocked ? (
                  `Account Locked (${lockoutTimeRemaining})`
                ) : (
                  'Sign In to Dashboard'
                )}
              </Button>
            </form>

            <Typography
              variant="body2"
              color="text.secondary"
              align="center"
              sx={{ mt: 3 }}
            >
              Secure access to your transportation management portal
            </Typography>
          </Box>
        </Paper>
      </Container>

      {/* MFA Verification Dialog */}
      <Dialog 
        open={showMfaDialog} 
        onClose={handleCloseMfaDialog}
        maxWidth="sm"
        fullWidth
        PaperProps={{
          sx: {
            borderRadius: 3,
            padding: 2,
          }
        }}
      >
        <DialogTitle sx={{ textAlign: 'center', pb: 1 }}>
          <SecurityIcon sx={{ fontSize: 48, color: 'primary.main', mb: 1 }} />
          <Typography variant="h5" component="div" fontWeight="bold">
            Multi-Factor Authentication
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Enter the 6-digit verification code from your authenticator app
          </Typography>
        </DialogTitle>
        
        <DialogContent sx={{ pt: 2 }}>
          {mfaError && (
            <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
              {mfaError}
            </Alert>
          )}
          
          <TextField
            autoFocus
            fullWidth
            label="Verification Code"
            type="text"
            value={mfaCode}
            onChange={(e) => {
              const value = e.target.value.replace(/\D/g, '').slice(0, 6);
              setMfaCode(value);
            }}
            placeholder="000000"
            inputProps={{
              maxLength: 6,
              style: { 
                textAlign: 'center', 
                fontSize: '1.5rem',
                letterSpacing: '0.5rem',
                fontFamily: 'monospace'
              }
            }}
            sx={{
              '& .MuiOutlinedInput-root': {
                borderRadius: 2,
              },
            }}
            onKeyPress={(e) => {
              if (e.key === 'Enter' && mfaCode.length === 6) {
                handleMfaVerification();
              }
            }}
          />
          
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2, textAlign: 'center' }}>
            Can't access your authenticator app? Contact your system administrator for assistance.
          </Typography>
        </DialogContent>
        
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button 
            onClick={handleCloseMfaDialog}
            variant="outlined"
            sx={{ borderRadius: 2 }}
          >
            Cancel
          </Button>
          <Button 
            onClick={handleMfaVerification}
            variant="contained"
            disabled={loading || mfaCode.length !== 6}
            sx={{ 
              borderRadius: 2,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            }}
          >
            {loading ? <CircularProgress size={20} color="inherit" /> : 'Verify'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default LoginPage;
