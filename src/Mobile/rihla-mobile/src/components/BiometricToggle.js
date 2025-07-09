import React, { useState, useEffect } from 'react';
import { View, Text, Switch, Alert, StyleSheet } from 'react-native';
import { useAuth } from '../contexts/AuthContext';
import { useAppSettingsStore } from '../stores';

const BiometricToggle = () => {
  const { biometricAvailable, biometricType, enableBiometricAuth, disableBiometricAuth, isBiometricEnabled } = useAuth();
  const { biometric, updateBiometricSettings } = useAppSettingsStore();
  const [isEnabled, setIsEnabled] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsEnabled(isBiometricEnabled());
  }, [biometric.enabled, biometricAvailable]);

  const handleToggle = async (value) => {
    if (!biometricAvailable) {
      Alert.alert(
        'Biometric Authentication Unavailable',
        'Biometric authentication is not available on this device or no biometric data is enrolled.',
        [{ text: 'OK' }]
      );
      return;
    }

    setIsLoading(true);

    try {
      if (value) {
        const result = await enableBiometricAuth();
        
        if (result.success) {
          setIsEnabled(true);
          Alert.alert(
            'Biometric Authentication Enabled',
            `${biometricType} authentication has been enabled for quick login.`,
            [{ text: 'OK' }]
          );
        } else {
          Alert.alert(
            'Failed to Enable Biometric Authentication',
            result.message || 'An error occurred while enabling biometric authentication.',
            [{ text: 'OK' }]
          );
        }
      } else {
        Alert.alert(
          'Disable Biometric Authentication',
          'Are you sure you want to disable biometric authentication? You will need to enter your password to login.',
          [
            { text: 'Cancel', style: 'cancel' },
            {
              text: 'Disable',
              style: 'destructive',
              onPress: async () => {
                const result = await disableBiometricAuth();
                
                if (result.success) {
                  setIsEnabled(false);
                  Alert.alert(
                    'Biometric Authentication Disabled',
                    'Biometric authentication has been disabled.',
                    [{ text: 'OK' }]
                  );
                } else {
                  Alert.alert(
                    'Failed to Disable Biometric Authentication',
                    result.message || 'An error occurred while disabling biometric authentication.',
                    [{ text: 'OK' }]
                  );
                }
              }
            }
          ]
        );
      }
    } catch (error) {
      console.error('Biometric toggle error:', error);
      Alert.alert(
        'Error',
        'An unexpected error occurred. Please try again.',
        [{ text: 'OK' }]
      );
    } finally {
      setIsLoading(false);
    }
  };

  if (!biometricAvailable) {
    return (
      <View style={styles.container}>
        <View style={styles.content}>
          <Text style={styles.title}>Biometric Authentication</Text>
          <Text style={styles.subtitle}>Not available on this device</Text>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.content}>
        <Text style={styles.title}>Biometric Authentication</Text>
        <Text style={styles.subtitle}>
          Use {biometricType} for quick and secure login
        </Text>
      </View>
      <Switch
        value={isEnabled}
        onValueChange={handleToggle}
        disabled={isLoading}
        trackColor={{ false: '#767577', true: '#81b0ff' }}
        thumbColor={isEnabled ? '#f5dd4b' : '#f4f3f4'}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 16,
    paddingHorizontal: 20,
    backgroundColor: '#fff',
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  content: {
    flex: 1,
  },
  title: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 14,
    color: '#666',
  },
});

export default BiometricToggle;
