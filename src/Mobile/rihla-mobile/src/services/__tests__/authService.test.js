import { authService } from '../authService';
import AsyncStorage from '@react-native-async-storage/async-storage';

jest.mock('@react-native-async-storage/async-storage');

describe('AuthService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should successfully login with valid credentials', async () => {
      const mockResponse = {
        ok: true,
        json: () => Promise.resolve({
          token: 'mock-token',
          user: { id: 1, email: 'admin@rihla.sa', role: 'Admin' }
        })
      };
      
      global.fetch = jest.fn(() => Promise.resolve(mockResponse));
      
      const result = await authService.login('admin@rihla.sa', 'admin123');
      
      expect(result.success).toBe(true);
      expect(result.token).toBe('mock-token');
      expect(AsyncStorage.setItem).toHaveBeenCalledWith('authToken', 'mock-token');
    });

    it('should handle login failure with invalid credentials', async () => {
      const mockResponse = {
        ok: false,
        status: 401,
        json: () => Promise.resolve({ message: 'Invalid credentials' })
      };
      
      global.fetch = jest.fn(() => Promise.resolve(mockResponse));
      
      const result = await authService.login('invalid@email.com', 'wrongpassword');
      
      expect(result.success).toBe(false);
      expect(result.error).toBe('Invalid credentials');
    });

    it('should handle network errors', async () => {
      global.fetch = jest.fn(() => Promise.reject(new Error('Network error')));
      
      const result = await authService.login('admin@rihla.sa', 'admin123');
      
      expect(result.success).toBe(false);
      expect(result.error).toContain('Network error');
    });
  });

  describe('logout', () => {
    it('should clear stored token on logout', async () => {
      await authService.logout();
      
      expect(AsyncStorage.removeItem).toHaveBeenCalledWith('authToken');
    });
  });

  describe('getStoredToken', () => {
    it('should retrieve stored token', async () => {
      AsyncStorage.getItem.mockResolvedValue('stored-token');
      
      const token = await authService.getStoredToken();
      
      expect(token).toBe('stored-token');
      expect(AsyncStorage.getItem).toHaveBeenCalledWith('authToken');
    });
  });
});
