import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { AuthProvider } from './contexts/AuthContext';
import LoginPage from './pages/LoginPage';
import AdminDashboard from './pages/AdminDashboard';
import ParentPortal from './pages/ParentPortal';
import DriverInterface from './pages/DriverInterface';
import MapPage from './pages/MapPage';
import ProtectedRoute from './components/layout/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <ErrorBoundary>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <AuthProvider>
          <ErrorBoundary>
            <Router>
              <Routes>
                <Route path="/login" element={
                  <ErrorBoundary>
                    <LoginPage />
                  </ErrorBoundary>
                } />
                <Route
                  path="/admin/*"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Admin">
                        <AdminDashboard />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route
                  path="/parent/*"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Parent">
                        <ParentPortal />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route
                  path="/driver/*"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Driver">
                        <DriverInterface />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route
                  path="/map"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Admin">
                        <MapPage />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route
                  path="/parent/map"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Parent">
                        <MapPage />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route
                  path="/driver/map"
                  element={
                    <ErrorBoundary>
                      <ProtectedRoute requiredRole="Driver">
                        <MapPage />
                      </ProtectedRoute>
                    </ErrorBoundary>
                  }
                />
                <Route path="/" element={<Navigate to="/login" replace />} />
              </Routes>
            </Router>
          </ErrorBoundary>
        </AuthProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}

export default App;
