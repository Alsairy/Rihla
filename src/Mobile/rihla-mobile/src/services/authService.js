import { apiClient } from './apiClient';

class AuthService {
  constructor() {
    this.token = null;
  }

  setAuthToken(token) {
    this.token = token;
    if (token) {
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      delete apiClient.defaults.headers.common['Authorization'];
    }
  }

  async login(credentials) {
    try {
      const response = await apiClient.post('/auth/login', credentials);
      
      if (response.data && response.data.success && response.data.data.token) {
        const { token, refreshToken, user } = response.data.data;
        this.setAuthToken(token);
        
        return {
          success: true,
          data: {
            user: user,
            token: token,
            refreshToken: refreshToken
          }
        };
      } else {
        return {
          success: false,
          message: response.data?.message || 'Login failed'
        };
      }
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Login failed'
      };
    }
  }

  async logout() {
    try {
      await apiClient.post('/auth/logout');
      this.setAuthToken(null);
      return { success: true };
    } catch (error) {
      this.setAuthToken(null);
      return { success: false };
    }
  }

  async register(userData) {
    try {
      const response = await apiClient.post('/auth/register', userData);
      
      if (response.data && response.data.success && response.data.data.token) {
        const { token, refreshToken, user } = response.data.data;
        this.setAuthToken(token);
        
        return {
          user: user,
          token: token,
          refreshToken: refreshToken
        };
      } else {
        throw new Error(response.data?.message || 'Registration failed');
      }
    } catch (error) {
      throw new Error(error.response?.data?.message || 'Registration failed');
    }
  }

  async getCurrentUser() {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    try {
      const userStr = await AsyncStorage.getItem('user');
      return userStr ? JSON.parse(userStr) : null;
    } catch (error) {
      return null;
    }
  }

  async isAuthenticated() {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    try {
      const token = await AsyncStorage.getItem('authToken');
      return !!token;
    } catch (error) {
      return false;
    }
  }
}

export const authService = new AuthService();

