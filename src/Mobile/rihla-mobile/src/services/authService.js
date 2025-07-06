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
      console.error('Logout error:', error);
      this.setAuthToken(null);
      return { success: false };
    }
  }
}

export const authService = new AuthService();

