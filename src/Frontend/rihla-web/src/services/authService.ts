import { apiClient } from './apiClient';
import { User } from '../types';

export interface LoginRequest {
  email: string;
  password: string;
  mfaCode?: string;
  mfaToken?: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
  requiresMfa?: boolean;
  mfaToken?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  username: string;
  role: string;
}

class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<{
      success: boolean;
      data: LoginResponse;
      message: string;
    }>('/api/auth/login', credentials);

    if (response.success && response.data) {
      if (response.data.requiresMfa) {
        return response.data;
      }

      if (response.data.token) {
        localStorage.setItem('authToken', response.data.token);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        localStorage.setItem('user', JSON.stringify(response.data.user));
        return response.data;
      }
    }

    throw new Error(response.message || 'Login failed');
  }

  async register(userData: RegisterRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      '/api/auth/register',
      userData
    );

    if (response.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
    }

    return response;
  }

  async logout(): Promise<void> {
    try {
      await apiClient.post('/api/auth/logout');
    } catch (error) {
      console.error('Failed to logout from server:', error);
    } finally {
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  }

  async refreshToken(): Promise<string> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await apiClient.post<{
      success: boolean;
      data: { token: string; refreshToken: string; tokenExpiry: string };
    }>('/api/auth/refresh', {
      refreshToken,
    });

    if (response.data && response.data.token) {
      localStorage.setItem('authToken', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);
      return response.data.token;
    }

    throw new Error('Invalid refresh token response');
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}

export const authService = new AuthService();
