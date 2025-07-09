import { authService } from '../services/authService';

jest.mock('@react-native-async-storage/async-storage', () => ({
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
}));

global.fetch = jest.fn();

describe('AuthService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    fetch.mockClear();
  });

  test('login should authenticate user successfully', async () => {
    const mockResponse = {
      user: {
        id: 1,
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'Driver'
      },
      token: 'test-token',
      refreshToken: 'refresh-token'
    };

    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse,
    });

    const result = await authService.login('test@example.com', 'password123');

    expect(result).toEqual(mockResponse);
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/login'),
      expect.objectContaining({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: 'test@example.com',
          password: 'password123',
        }),
      })
    );
  });

  test('login should handle authentication failure', async () => {
    fetch.mockResolvedValueOnce({
      ok: false,
      status: 401,
      json: async () => ({ message: 'Invalid credentials' }),
    });

    await expect(
      authService.login('test@example.com', 'wrongpassword')
    ).rejects.toThrow('Invalid credentials');
  });

  test('register should create new user successfully', async () => {
    const mockResponse = {
      user: {
        id: 2,
        email: 'newuser@example.com',
        firstName: 'New',
        lastName: 'User',
        role: 'Parent'
      },
      token: 'test-token',
      refreshToken: 'refresh-token'
    };

    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse,
    });

    const result = await authService.register({
      email: 'newuser@example.com',
      password: 'password123',
      firstName: 'New',
      lastName: 'User',
      role: 'Parent'
    });

    expect(result).toEqual(mockResponse);
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/register'),
      expect.objectContaining({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      })
    );
  });

  test('logout should clear user session', async () => {
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ success: true }),
    });

    await authService.logout();

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/logout'),
      expect.objectContaining({
        method: 'POST',
      })
    );
  });

  test('getCurrentUser should return stored user data', async () => {
    const mockUser = {
      id: 1,
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      role: 'Driver'
    };

    const AsyncStorage = require('@react-native-async-storage/async-storage');
    AsyncStorage.getItem.mockResolvedValue(JSON.stringify(mockUser));

    const result = await authService.getCurrentUser();

    expect(result).toEqual(mockUser);
    expect(AsyncStorage.getItem).toHaveBeenCalledWith('user');
  });

  test('isAuthenticated should return true when token exists', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    AsyncStorage.getItem.mockResolvedValue('test-token');

    const result = await authService.isAuthenticated();

    expect(result).toBe(true);
    expect(AsyncStorage.getItem).toHaveBeenCalledWith('authToken');
  });

  test('isAuthenticated should return false when no token exists', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    AsyncStorage.getItem.mockResolvedValue(null);

    const result = await authService.isAuthenticated();

    expect(result).toBe(false);
  });
});
