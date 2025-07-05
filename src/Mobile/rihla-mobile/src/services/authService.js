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
      // Mock authentication for testing
      const mockUser = {
        id: 1,
        email: credentials.email,
        name: 'Admin User',
        role: 'admin'
      };
      
      const mockToken = 'mock-jwt-token-for-testing';
      
      return {
        success: true,
        data: {
          user: mockUser,
          token: mockToken
        }
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Login failed'
      };
    }
  }

  async logout() {
    try {
      return { success: true };
    } catch (error) {
      console.error('Logout error:', error);
      return { success: false };
    }
  }
}

export const authService = new AuthService();

