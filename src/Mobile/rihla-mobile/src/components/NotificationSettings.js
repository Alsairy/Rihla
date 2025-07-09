import React, { useState, useEffect } from 'react';
import { View, Text, Switch, StyleSheet, ScrollView, Alert } from 'react-native';
import { useAppSettingsStore } from '../stores';
import notificationService from '../services/notificationService';

const NotificationSettings = () => {
  const { notifications, updateNotificationSettings, updateNotificationTypes } = useAppSettingsStore();
  const [isLoading, setIsLoading] = useState(false);

  const handleToggleNotifications = async (enabled) => {
    setIsLoading(true);
    try {
      await updateNotificationSettings({ enabled });
      
      if (enabled) {
        const initialized = await notificationService.initialize();
        if (!initialized) {
          Alert.alert(
            'Permission Required',
            'Please enable notifications in your device settings to receive alerts.',
            [{ text: 'OK' }]
          );
          await updateNotificationSettings({ enabled: false });
        }
      }
    } catch (error) {
      console.error('Error toggling notifications:', error);
      Alert.alert('Error', 'Failed to update notification settings');
    } finally {
      setIsLoading(false);
    }
  };

  const handleTogglePushNotifications = async (enabled) => {
    await updateNotificationSettings({ pushEnabled: enabled });
  };

  const handleToggleEmailNotifications = async (enabled) => {
    await updateNotificationSettings({ emailEnabled: enabled });
  };

  const handleToggleSmsNotifications = async (enabled) => {
    await updateNotificationSettings({ smsEnabled: enabled });
  };

  const handleToggleNotificationType = async (type, enabled) => {
    await updateNotificationTypes({ [type]: enabled });
  };

  const NotificationToggle = ({ title, subtitle, value, onToggle, disabled = false }) => (
    <View style={styles.toggleContainer}>
      <View style={styles.toggleContent}>
        <Text style={styles.toggleTitle}>{title}</Text>
        {subtitle && <Text style={styles.toggleSubtitle}>{subtitle}</Text>}
      </View>
      <Switch
        value={value}
        onValueChange={onToggle}
        disabled={disabled || isLoading}
        trackColor={{ false: '#767577', true: '#81b0ff' }}
        thumbColor={value ? '#f5dd4b' : '#f4f3f4'}
      />
    </View>
  );

  const SectionHeader = ({ title }) => (
    <Text style={styles.sectionHeader}>{title}</Text>
  );

  return (
    <ScrollView style={styles.container}>
      <SectionHeader title="General Settings" />
      
      <NotificationToggle
        title="Enable Notifications"
        subtitle="Receive notifications from the app"
        value={notifications.enabled}
        onToggle={handleToggleNotifications}
      />

      <NotificationToggle
        title="Push Notifications"
        subtitle="Receive push notifications on your device"
        value={notifications.pushEnabled}
        onToggle={handleTogglePushNotifications}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="Email Notifications"
        subtitle="Receive notifications via email"
        value={notifications.emailEnabled}
        onToggle={handleToggleEmailNotifications}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="SMS Notifications"
        subtitle="Receive notifications via SMS"
        value={notifications.smsEnabled}
        onToggle={handleToggleSmsNotifications}
        disabled={!notifications.enabled}
      />

      <SectionHeader title="Notification Types" />

      <NotificationToggle
        title="Trip Updates"
        subtitle="Notifications about trip status changes"
        value={notifications.types.tripUpdates}
        onToggle={(enabled) => handleToggleNotificationType('tripUpdates', enabled)}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="Emergency Alerts"
        subtitle="Critical emergency notifications"
        value={notifications.types.emergencyAlerts}
        onToggle={(enabled) => handleToggleNotificationType('emergencyAlerts', enabled)}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="Payment Reminders"
        subtitle="Payment due and receipt notifications"
        value={notifications.types.paymentReminders}
        onToggle={(enabled) => handleToggleNotificationType('paymentReminders', enabled)}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="Maintenance Notices"
        subtitle="Vehicle maintenance notifications"
        value={notifications.types.maintenanceNotices}
        onToggle={(enabled) => handleToggleNotificationType('maintenanceNotices', enabled)}
        disabled={!notifications.enabled}
      />

      <NotificationToggle
        title="General Announcements"
        subtitle="School transportation announcements"
        value={notifications.types.generalAnnouncements}
        onToggle={(enabled) => handleToggleNotificationType('generalAnnouncements', enabled)}
        disabled={!notifications.enabled}
      />

      <View style={styles.footer}>
        <Text style={styles.footerText}>
          You can change these settings at any time. Some notifications may be required for safety and security purposes.
        </Text>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  sectionHeader: {
    fontSize: 18,
    fontWeight: '600',
    color: '#1e293b',
    marginTop: 24,
    marginBottom: 12,
    marginHorizontal: 20,
  },
  toggleContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 16,
    paddingHorizontal: 20,
    backgroundColor: '#fff',
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  toggleContent: {
    flex: 1,
    marginRight: 16,
  },
  toggleTitle: {
    fontSize: 16,
    fontWeight: '500',
    color: '#1e293b',
    marginBottom: 4,
  },
  toggleSubtitle: {
    fontSize: 14,
    color: '#64748b',
  },
  footer: {
    padding: 20,
    marginTop: 20,
  },
  footerText: {
    fontSize: 14,
    color: '#64748b',
    textAlign: 'center',
    lineHeight: 20,
  },
});

export default NotificationSettings;
