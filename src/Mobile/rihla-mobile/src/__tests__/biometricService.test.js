import { biometricService } from '../services/biometricService';

jest.mock('@react-native-async-storage/async-storage', () => ({
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
}));

describe('BiometricService', () => {
  const LocalAuthentication = require('expo-local-authentication');
  const AsyncStorage = require('@react-native-async-storage/async-storage');

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('isAvailable should return true when biometric hardware is available and enrolled', async () => {
    LocalAuthentication.hasHardwareAsync.mockResolvedValue(true);
    LocalAuthentication.isEnrolledAsync.mockResolvedValue(true);

    const result = await biometricService.isAvailable();

    expect(result).toBe(true);
    expect(LocalAuthentication.hasHardwareAsync).toHaveBeenCalled();
    expect(LocalAuthentication.isEnrolledAsync).toHaveBeenCalled();
  });

  test('isAvailable should return false when biometric hardware is not available', async () => {
    LocalAuthentication.hasHardwareAsync.mockResolvedValue(false);

    const result = await biometricService.isAvailable();

    expect(result).toBe(false);
    expect(LocalAuthentication.hasHardwareAsync).toHaveBeenCalled();
    expect(LocalAuthentication.isEnrolledAsync).not.toHaveBeenCalled();
  });

  test('isAvailable should return false when biometric is not enrolled', async () => {
    LocalAuthentication.hasHardwareAsync.mockResolvedValue(true);
    LocalAuthentication.isEnrolledAsync.mockResolvedValue(false);

    const result = await biometricService.isAvailable();

    expect(result).toBe(false);
  });

  test('authenticate should return success when biometric authentication succeeds', async () => {
    LocalAuthentication.authenticateAsync.mockResolvedValue({
      success: true,
    });

    const result = await biometricService.authenticate();

    expect(result.success).toBe(true);
    expect(LocalAuthentication.authenticateAsync).toHaveBeenCalledWith({
      promptMessage: 'Authenticate with biometrics',
      cancelLabel: 'Cancel',
      fallbackLabel: 'Use Password',
    });
  });

  test('authenticate should return failure when biometric authentication fails', async () => {
    LocalAuthentication.authenticateAsync.mockResolvedValue({
      success: false,
      error: 'Authentication failed',
    });

    const result = await biometricService.authenticate();

    expect(result.success).toBe(false);
    expect(result.error).toBe('Authentication failed');
  });

  test('isEnabled should return stored biometric preference', async () => {
    AsyncStorage.getItem.mockResolvedValue('true');

    const result = await biometricService.isEnabled();

    expect(result).toBe(true);
    expect(AsyncStorage.getItem).toHaveBeenCalledWith('biometricEnabled');
  });

  test('setEnabled should store biometric preference', async () => {
    await biometricService.setEnabled(true);

    expect(AsyncStorage.setItem).toHaveBeenCalledWith('biometricEnabled', 'true');
  });

  test('setEnabled should remove biometric preference when disabled', async () => {
    await biometricService.setEnabled(false);

    expect(AsyncStorage.removeItem).toHaveBeenCalledWith('biometricEnabled');
  });

  test('getSupportedTypes should return available biometric types', async () => {
    LocalAuthentication.hasHardwareAsync.mockResolvedValue(true);
    LocalAuthentication.isEnrolledAsync.mockResolvedValue(true);

    const result = await biometricService.getSupportedTypes();

    expect(Array.isArray(result)).toBe(true);
  });
});
