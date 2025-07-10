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

  async login(email, password) {
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Login failed');
      }

      const data = await response.json();
      this.setAuthToken(data.token);
      
      return data;
    } catch (error) {
      throw error;
    }
  }

  async logout() {
    try {
      await fetch('/api/auth/logout', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      this.setAuthToken(null);
    } catch (error) {
      this.setAuthToken(null);
      throw error;
    }
  }

  async register(userData) {
    try {
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Registration failed');
      }

      const data = await response.json();
      this.setAuthToken(data.token);
      
      return data;
    } catch (error) {
      throw error;
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

