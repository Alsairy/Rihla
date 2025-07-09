import React from 'react';
import { renderHook, act } from '@testing-library/react';
import { AuthProvider, useAuth } from '../AuthContext';
import { authService } from '../../services/authService';

jest.mock('../../services/authService', () => ({
  authService: {
    login: jest.fn(),
    register: jest.fn(),
    logout: jest.fn(),
  },
}));

const mockAuthService = authService as jest.Mocked<typeof authService>;

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <AuthProvider>{children}</AuthProvider>
);

describe('AuthContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  test('initializes with no user when no stored data', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.loading).toBe(false);
  });

  test('initializes with stored user data', () => {
    const mockUser = {
      id: 1,
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      role: 'Admin'
    };

    localStorage.setItem('rihla_user', JSON.stringify(mockUser));
    localStorage.setItem('rihla_token', 'test-token');

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.isAuthenticated).toBe(true);
  });

  test('login updates user state on success', async () => {
    const mockUser = {
      id: 1,
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      role: 'Admin'
    };

    const mockResponse = {
      user: mockUser,
      token: 'test-token',
      refreshToken: 'refresh-token'
    };

    (mockAuthService.login as jest.Mock).mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.login({
        email: 'test@example.com',
        password: 'password123'
      });
    });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.isAuthenticated).toBe(true);
    expect(localStorage.getItem('rihla_user')).toBe(JSON.stringify(mockUser));
    expect(localStorage.getItem('rihla_token')).toBe('test-token');
  });

  test('login handles errors correctly', async () => {
    (mockAuthService.login as jest.Mock).mockRejectedValue(new Error('Invalid credentials'));

    const { result } = renderHook(() => useAuth(), { wrapper });

    await expect(
      act(async () => {
        await result.current.login({
          email: 'test@example.com',
          password: 'wrongpassword'
        });
      })
    ).rejects.toThrow('Invalid credentials');

    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
  });

  test('register updates user state on success', async () => {
    const mockUser = {
      id: 2,
      email: 'newuser@example.com',
      firstName: 'New',
      lastName: 'User',
      role: 'Parent'
    };

    const mockResponse = {
      user: mockUser,
      token: 'test-token',
      refreshToken: 'refresh-token'
    };

    (mockAuthService.register as jest.Mock).mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.register({
        email: 'newuser@example.com',
        password: 'password123',
        username: 'newuser',
        role: 'Parent'
      });
    });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.isAuthenticated).toBe(true);
    expect(localStorage.getItem('rihla_user')).toBe(JSON.stringify(mockUser));
    expect(localStorage.getItem('rihla_token')).toBe('test-token');
  });

  test('logout clears user state and storage', async () => {
    const mockUser = {
      id: 1,
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      role: 'Admin'
    };

    localStorage.setItem('rihla_user', JSON.stringify(mockUser));
    localStorage.setItem('rihla_token', 'test-token');

    (mockAuthService.logout as jest.Mock).mockResolvedValue(undefined);

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.logout();
    });

    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    expect(localStorage.getItem('rihla_user')).toBeNull();
    expect(localStorage.getItem('rihla_token')).toBeNull();
  });

  test('logout handles errors gracefully', async () => {
    (mockAuthService.logout as jest.Mock).mockRejectedValue(new Error('Logout failed'));

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.logout();
    });

    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
  });
});
