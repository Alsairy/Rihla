import { apiClient } from './apiClient';
import AsyncStorage from '@react-native-async-storage/async-storage';

class AuthService {
  constructor() {
    this.token = null;
  }

  async setAuthToken(token) {
    this.token = token;
    if (token) {
      await AsyncStorage.setItem('token', token);
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      await AsyncStorage.removeItem('token');
      delete apiClient.defaults.headers.common['Authorization'];
    }
  }

  async login(credentials) {
    try {
      const response = await apiClient.post('/auth/login', credentials);
      console.log('Login response data:', response.data);
      console.log('response.data.data structure:', JSON.stringify(response.data.data, null, 2));
      
      if (response.data && response.data.success && response.data.data && response.data.data.token) {
        const { token, refreshToken, user } = response.data.data;
        console.log('Extracted token:', token ? 'Token present' : 'No token');
        console.log('Setting auth token...');
        await this.setAuthToken(token);
        console.log('Auth token set successfully');
        
        return {
          success: true,
          data: {
            user: user,
            token: token,
            refreshToken: refreshToken
          }
        };
      } else {
        console.log('Login failed - invalid response structure:', response.data);
        return {
          success: false,
          message: response.data?.message || 'Login failed'
        };
      }
    } catch (error) {
      console.log('Login error:', error.response?.data);
      return {
        success: false,
        message: error.response?.data?.message || 'Login failed'
      };
    }
  }

  async logout() {
    try {
      await apiClient.post('/auth/logout');
      await this.setAuthToken(null);
      return { success: true };
    } catch (error) {
      console.error('Logout error:', error);
      await this.setAuthToken(null);
      return { success: false };
    }
  }
}

export const authService = new AuthService();

