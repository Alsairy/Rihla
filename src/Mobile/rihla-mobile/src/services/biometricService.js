import * as LocalAuthentication from 'expo-local-authentication';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useAppSettingsStore } from '../stores';

const BIOMETRIC_CREDENTIALS_KEY = 'biometric_credentials';

class BiometricService {
  async isAvailable() {
    try {
      const hasHardware = await LocalAuthentication.hasHardwareAsync();
      const isEnrolled = await LocalAuthentication.isEnrolledAsync();
      const supportedTypes = await LocalAuthentication.supportedAuthenticationTypesAsync();
      
      return {
        hasHardware,
        isEnrolled,
        supportedTypes,
        isAvailable: hasHardware && isEnrolled
      };
    } catch (error) {
      console.error('Error checking biometric availability:', error);
      return {
        hasHardware: false,
        isEnrolled: false,
        supportedTypes: [],
        isAvailable: false
      };
    }
  }

  async getSupportedBiometricType() {
    try {
      const supportedTypes = await LocalAuthentication.supportedAuthenticationTypesAsync();
      
      if (supportedTypes.includes(LocalAuthentication.AuthenticationType.FACIAL_RECOGNITION)) {
        return 'Face ID';
      } else if (supportedTypes.includes(LocalAuthentication.AuthenticationType.FINGERPRINT)) {
        return 'Touch ID';
      } else if (supportedTypes.includes(LocalAuthentication.AuthenticationType.IRIS)) {
        return 'Iris';
      }
      
      return 'Biometric';
    } catch (error) {
      console.error('Error getting biometric type:', error);
      return 'Biometric';
    }
  }

  async authenticate(reason = 'Please authenticate to continue') {
    try {
      const biometricAuth = await LocalAuthentication.authenticateAsync({
        promptMessage: reason,
        cancelLabel: 'Cancel',
        fallbackLabel: 'Use Password',
        disableDeviceFallback: false,
      });

      return {
        success: biometricAuth.success,
        error: biometricAuth.error,
        warning: biometricAuth.warning
      };
    } catch (error) {
      console.error('Biometric authentication error:', error);
      return {
        success: false,
        error: 'Authentication failed',
        warning: null
      };
    }
  }

  async storeBiometricCredentials(username, token) {
    try {
      const credentials = {
        username,
        token,
        timestamp: new Date().toISOString()
      };
      
      await AsyncStorage.setItem(BIOMETRIC_CREDENTIALS_KEY, JSON.stringify(credentials));
      return true;
    } catch (error) {
      console.error('Error storing biometric credentials:', error);
      return false;
    }
  }

  async getBiometricCredentials() {
    try {
      const credentialsJson = await AsyncStorage.getItem(BIOMETRIC_CREDENTIALS_KEY);
      if (!credentialsJson) {
        return null;
      }

      const credentials = JSON.parse(credentialsJson);
      
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
      
      if (new Date(credentials.timestamp) < thirtyDaysAgo) {
        await this.clearBiometricCredentials();
        return null;
      }

      return credentials;
    } catch (error) {
      console.error('Error getting biometric credentials:', error);
      return null;
    }
  }

  async clearBiometricCredentials() {
    try {
      await AsyncStorage.removeItem(BIOMETRIC_CREDENTIALS_KEY);
      return true;
    } catch (error) {
      console.error('Error clearing biometric credentials:', error);
      return false;
    }
  }

  async enableBiometricAuth(username, token) {
    try {
      const availability = await this.isAvailable();
      if (!availability.isAvailable) {
        throw new Error('Biometric authentication is not available on this device');
      }

      const authResult = await this.authenticate('Enable biometric authentication for quick login');
      if (!authResult.success) {
        throw new Error('Biometric authentication failed');
      }

      const stored = await this.storeBiometricCredentials(username, token);
      if (!stored) {
        throw new Error('Failed to store biometric credentials');
      }

      const { updateBiometricSettings } = useAppSettingsStore.getState();
      const biometricType = await this.getSupportedBiometricType();
      
      updateBiometricSettings({
        enabled: true,
        type: biometricType
      });

      return {
        success: true,
        type: biometricType
      };
    } catch (error) {
      console.error('Error enabling biometric auth:', error);
      return {
        success: false,
        error: error.message
      };
    }
  }

  async disableBiometricAuth() {
    try {
      await this.clearBiometricCredentials();
      
      const { updateBiometricSettings } = useAppSettingsStore.getState();
      updateBiometricSettings({
        enabled: false,
        type: null
      });

      return { success: true };
    } catch (error) {
      console.error('Error disabling biometric auth:', error);
      return {
        success: false,
        error: error.message
      };
    }
  }

  async authenticateWithBiometrics() {
    try {
      const availability = await this.isAvailable();
      if (!availability.isAvailable) {
        return {
          success: false,
          error: 'Biometric authentication is not available'
        };
      }

      const credentials = await this.getBiometricCredentials();
      if (!credentials) {
        return {
          success: false,
          error: 'No biometric credentials found'
        };
      }

      const biometricType = await this.getSupportedBiometricType();
      const authResult = await this.authenticate(`Use ${biometricType} to sign in`);
      
      if (authResult.success) {
        return {
          success: true,
          credentials: {
            username: credentials.username,
            token: credentials.token
          }
        };
      } else {
        return {
          success: false,
          error: authResult.error || 'Biometric authentication failed'
        };
      }
    } catch (error) {
      console.error('Error authenticating with biometrics:', error);
      return {
        success: false,
        error: 'Authentication failed'
      };
    }
  }
}

export default new BiometricService();
