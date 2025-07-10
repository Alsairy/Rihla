import * as LocalAuthentication from 'expo-local-authentication';

class BiometricService {
  async isAvailable() {
    try {
      const hasHardware = await LocalAuthentication.hasHardwareAsync();
      if (!hasHardware) {
        return false;
      }
      
      const isEnrolled = await LocalAuthentication.isEnrolledAsync();
      return isEnrolled;
    } catch (error) {
      return false;
    }
  }

  async authenticate() {
    try {
      const result = await LocalAuthentication.authenticateAsync({
        promptMessage: 'Authenticate with biometrics',
        cancelLabel: 'Cancel',
        fallbackLabel: 'Use Password',
      });

      return {
        success: result.success,
        error: result.error || null,
      };
    } catch (error) {
      return {
        success: false,
        error: error.message || 'Authentication failed',
      };
    }
  }

  async isEnabled() {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    try {
      const enabled = await AsyncStorage.getItem('biometricEnabled');
      return enabled === 'true';
    } catch (error) {
      return false;
    }
  }

  async setEnabled(enabled) {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    try {
      if (enabled) {
        await AsyncStorage.setItem('biometricEnabled', 'true');
      } else {
        await AsyncStorage.removeItem('biometricEnabled');
      }
    } catch (error) {
    }
  }

  async getSupportedTypes() {
    try {
      const hasHardware = await LocalAuthentication.hasHardwareAsync();
      const isEnrolled = await LocalAuthentication.isEnrolledAsync();
      
      if (hasHardware && isEnrolled) {
        return await LocalAuthentication.supportedAuthenticationTypesAsync();
      }
      
      return [];
    } catch (error) {
      return [];
    }
  }
}

export const biometricService = new BiometricService();
