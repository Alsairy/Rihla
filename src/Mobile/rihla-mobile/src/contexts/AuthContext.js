import React, { createContext, useContext, useState, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authService } from '../services/authService';
import biometricService from '../services/biometricService';
import syncService from '../services/syncService';
import { useAppSettingsStore } from '../stores';

const AuthContext = createContext({});

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [token, setToken] = useState(null);
  const [biometricAvailable, setBiometricAvailable] = useState(false);
  const [biometricType, setBiometricType] = useState(null);

  // Initialize authentication state
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const biometricInfo = await biometricService.isAvailable();
        setBiometricAvailable(biometricInfo.isAvailable);
        
        if (biometricInfo.isAvailable) {
          const type = await biometricService.getSupportedBiometricType();
          setBiometricType(type);
        }

        const storedToken = await AsyncStorage.getItem('token');
        const storedUser = await AsyncStorage.getItem('user');

        if (storedToken && storedUser) {
          setToken(storedToken);
          setUser(JSON.parse(storedUser));
          setIsAuthenticated(true);
          authService.setAuthToken(storedToken);
          
          syncService.startBackgroundSync();
        }
      } catch (error) {
        console.error('Auth initialization failed:', error);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (credentials) => {
    try {
      setIsLoading(true);
      const response = await authService.login(credentials);
      
      if (response.success) {
        const { token: newToken, user: userData } = response.data;
        
        // Store in AsyncStorage
        await AsyncStorage.setItem('token', newToken);
        await AsyncStorage.setItem('user', JSON.stringify(userData));
        
        // Update state
        setToken(newToken);
        setUser(userData);
        setIsAuthenticated(true);
        
        // Set token for future requests
        authService.setAuthToken(newToken);
        
        syncService.startBackgroundSync();
        
        return { success: true };
      } else {
        return { success: false, message: response.message };
      }
    } catch (error) {
      console.error('Login error:', error);
      return { success: false, message: "Login failed" };
    } finally {
      setIsLoading(false);
    }
  };

  const loginWithBiometrics = async () => {
    try {
      setIsLoading(true);
      const result = await biometricService.authenticateWithBiometrics();
      
      if (result.success) {
        const { username, token: storedToken } = result.credentials;
        
        authService.setAuthToken(storedToken);
        const userResponse = await authService.getCurrentUser();
        
        if (userResponse.success) {
          // Update state
          setToken(storedToken);
          setUser(userResponse.data);
          setIsAuthenticated(true);
          
          syncService.startBackgroundSync();
          
          return { success: true };
        } else {
          await biometricService.clearBiometricCredentials();
          return { success: false, message: "Session expired. Please login again." };
        }
      } else {
        return { success: false, message: result.error };
      }
    } catch (error) {
      console.error('Biometric login error:', error);
      return { success: false, message: "Biometric authentication failed" };
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      // Clear AsyncStorage
      await AsyncStorage.removeItem('token');
      await AsyncStorage.removeItem('user');
      
      const { biometric } = useAppSettingsStore.getState();
      if (!biometric.enabled) {
        await biometricService.clearBiometricCredentials();
      }
      
      // Clear state
      setToken(null);
      setUser(null);
      setIsAuthenticated(false);
      
      // Clear auth token
      authService.setAuthToken(null);
      
      syncService.stopBackgroundSync();
      
      return { success: true };
    } catch (error) {
      console.error('Logout error:', error);
      return { success: false };
    }
  };

  const enableBiometricAuth = async () => {
    try {
      if (!user || !token) {
        return { success: false, message: "User not authenticated" };
      }

      const result = await biometricService.enableBiometricAuth(user.username || user.email, token);
      return result;
    } catch (error) {
      console.error('Enable biometric auth error:', error);
      return { success: false, message: "Failed to enable biometric authentication" };
    }
  };

  const disableBiometricAuth = async () => {
    try {
      const result = await biometricService.disableBiometricAuth();
      return result;
    } catch (error) {
      console.error('Disable biometric auth error:', error);
      return { success: false, message: "Failed to disable biometric authentication" };
    }
  };

  const isBiometricEnabled = () => {
    const { biometric } = useAppSettingsStore.getState();
    return biometric.enabled && biometricAvailable;
  };

  const value = {
    user,
    isAuthenticated,
    isLoading,
    token,
    biometricAvailable,
    biometricType,
    login,
    loginWithBiometrics,
    logout,
    enableBiometricAuth,
    disableBiometricAuth,
    isBiometricEnabled,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

