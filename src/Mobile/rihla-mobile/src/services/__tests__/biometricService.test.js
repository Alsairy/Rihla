import { biometricService } from '../biometricService';
import * as LocalAuthentication from 'expo-local-authentication';
import * as SecureStore from 'expo-secure-store';

jest.mock('expo-local-authentication');
jest.mock('expo-secure-store');

describe('BiometricService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('isBiometricAvailable', () => {
    it('should return true when biometric hardware is available and enrolled', async () => {
      LocalAuthentication.hasHardwareAsync.mockResolvedValue(true);
      LocalAuthentication.isEnrolledAsync.mockResolvedValue(true);
      
      const result = await biometricService.isBiometricAvailable();
      
      expect(result).toBe(true);
    });

    it('should return false when biometric hardware is not available', async () => {
      LocalAuthentication.hasHardwareAsync.mockResolvedValue(false);
      LocalAuthentication.isEnrolledAsync.mockResolvedValue(true);
      
      const result = await biometricService.isBiometricAvailable();
      
      expect(result).toBe(false);
    });

    it('should return false when biometric is not enrolled', async () => {
      LocalAuthentication.hasHardwareAsync.mockResolvedValue(true);
      LocalAuthentication.isEnrolledAsync.mockResolvedValue(false);
      
      const result = await biometricService.isBiometricAvailable();
      
      expect(result).toBe(false);
    });
  });

  describe('authenticateWithBiometrics', () => {
    it('should successfully authenticate with biometrics', async () => {
      LocalAuthentication.authenticateAsync.mockResolvedValue({ success: true });
      SecureStore.getItemAsync.mockResolvedValue(JSON.stringify({
        username: 'admin@rihla.sa',
        token: 'stored-token'
      }));
      
      const result = await biometricService.authenticateWithBiometrics();
      
      expect(result.success).toBe(true);
      expect(result.credentials.username).toBe('admin@rihla.sa');
      expect(result.credentials.token).toBe('stored-token');
    });

    it('should handle biometric authentication failure', async () => {
      LocalAuthentication.authenticateAsync.mockResolvedValue({ 
        success: false, 
        error: 'Authentication failed' 
      });
      
      const result = await biometricService.authenticateWithBiometrics();
      
      expect(result.success).toBe(false);
      expect(result.error).toBe('Authentication failed');
    });

    it('should handle missing stored credentials', async () => {
      LocalAuthentication.authenticateAsync.mockResolvedValue({ success: true });
      SecureStore.getItemAsync.mockResolvedValue(null);
      
      const result = await biometricService.authenticateWithBiometrics();
      
      expect(result.success).toBe(false);
      expect(result.error).toBe('No stored credentials found');
    });
  });

  describe('storeBiometricCredentials', () => {
    it('should store credentials securely', async () => {
      SecureStore.setItemAsync.mockResolvedValue();
      
      await biometricService.storeBiometricCredentials('user@test.com', 'test-token');
      
      expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
        'biometric_credentials',
        JSON.stringify({ username: 'user@test.com', token: 'test-token' })
      );
    });
  });
});
