import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import Constants from 'expo-constants';
import { Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { apiClient } from './apiClient';
import { useNotificationsStore, useAppSettingsStore } from '../stores';

const PUSH_TOKEN_KEY = 'expo_push_token';
const NOTIFICATION_SETTINGS_KEY = 'notification_settings';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});

class NotificationService {
  constructor() {
    this.expoPushToken = null;
    this.notificationListener = null;
    this.responseListener = null;
    this.isInitialized = false;
  }

  async initialize() {
    if (this.isInitialized) return;

    try {
      const { status: existingStatus } = await Notifications.getPermissionsAsync();
      let finalStatus = existingStatus;

      if (existingStatus !== 'granted') {
        const { status } = await Notifications.requestPermissionsAsync();
        finalStatus = status;
      }

      if (finalStatus !== 'granted') {
        console.warn('Push notification permissions not granted');
        return false;
      }

      this.expoPushToken = await this.registerForPushNotificationsAsync();
      
      if (this.expoPushToken) {
        await AsyncStorage.setItem(PUSH_TOKEN_KEY, this.expoPushToken);
        
        await this.sendTokenToBackend(this.expoPushToken);
      }

      this.setupNotificationListeners();

      if (Platform.OS === 'android') {
        await this.setupNotificationChannels();
      }

      this.isInitialized = true;
      return true;
    } catch (error) {
      console.error('Error initializing notifications:', error);
      return false;
    }
  }

  async registerForPushNotificationsAsync() {
    let token;

    if (Platform.OS === 'android') {
      await Notifications.setNotificationChannelAsync('default', {
        name: 'default',
        importance: Notifications.AndroidImportance.MAX,
        vibrationPattern: [0, 250, 250, 250],
        lightColor: '#FF231F7C',
      });
    }

    if (Device.isDevice) {
      const { status: existingStatus } = await Notifications.getPermissionsAsync();
      let finalStatus = existingStatus;
      
      if (existingStatus !== 'granted') {
        const { status } = await Notifications.requestPermissionsAsync();
        finalStatus = status;
      }
      
      if (finalStatus !== 'granted') {
        console.warn('Failed to get push token for push notification!');
        return null;
      }
      
      try {
        const projectId = Constants?.expoConfig?.extra?.eas?.projectId ?? Constants?.easConfig?.projectId;
        if (!projectId) {
          throw new Error('Project ID not found');
        }
        
        token = (await Notifications.getExpoPushTokenAsync({
          projectId,
        })).data;
      } catch (e) {
        token = `${e}`;
      }
    } else {
      console.warn('Must use physical device for Push Notifications');
    }

    return token;
  }

  async setupNotificationChannels() {
    const channels = [
      {
        id: 'trip-updates',
        name: 'Trip Updates',
        description: 'Notifications about trip status changes',
        importance: Notifications.AndroidImportance.HIGH,
        vibrationPattern: [0, 250, 250, 250],
        lightColor: '#2563eb',
      },
      {
        id: 'emergency-alerts',
        name: 'Emergency Alerts',
        description: 'Critical emergency notifications',
        importance: Notifications.AndroidImportance.MAX,
        vibrationPattern: [0, 500, 250, 500],
        lightColor: '#ef4444',
      },
      {
        id: 'payment-reminders',
        name: 'Payment Reminders',
        description: 'Payment due and receipt notifications',
        importance: Notifications.AndroidImportance.DEFAULT,
        vibrationPattern: [0, 250],
        lightColor: '#10b981',
      },
      {
        id: 'maintenance-notices',
        name: 'Maintenance Notices',
        description: 'Vehicle maintenance notifications',
        importance: Notifications.AndroidImportance.DEFAULT,
        vibrationPattern: [0, 250],
        lightColor: '#f59e0b',
      },
      {
        id: 'general-announcements',
        name: 'General Announcements',
        description: 'General school transportation announcements',
        importance: Notifications.AndroidImportance.DEFAULT,
        vibrationPattern: [0, 250],
        lightColor: '#8b5cf6',
      }
    ];

    for (const channel of channels) {
      await Notifications.setNotificationChannelAsync(channel.id, channel);
    }
  }

  setupNotificationListeners() {
    this.notificationListener = Notifications.addNotificationReceivedListener(notification => {
      this.handleNotificationReceived(notification);
    });

    this.responseListener = Notifications.addNotificationResponseReceivedListener(response => {
      this.handleNotificationResponse(response);
    });
  }

  handleNotificationReceived(notification) {
    const { addNotification } = useNotificationsStore.getState();
    
    const notificationData = {
      id: notification.request.identifier,
      title: notification.request.content.title,
      body: notification.request.content.body,
      data: notification.request.content.data,
      type: notification.request.content.data?.type || 'general',
      isRead: false,
      createdAt: new Date().toISOString(),
      receivedAt: new Date().toISOString()
    };

    addNotification(notificationData);

    const { notifications } = useAppSettingsStore.getState();
    const notificationType = notificationData.type;
    
    if (!notifications.enabled || !notifications.types[notificationType]) {
      return;
    }

    console.log('Notification received:', notificationData);
  }

  handleNotificationResponse(response) {
    const notification = response.notification;
    const actionIdentifier = response.actionIdentifier;
    
    console.log('Notification response:', {
      actionIdentifier,
      notification: notification.request.content,
      data: notification.request.content.data
    });

    const { markAsRead } = useNotificationsStore.getState();
    markAsRead(notification.request.identifier);

    const notificationData = notification.request.content.data;
    
    if (notificationData?.type === 'trip-update' && notificationData?.tripId) {
      this.navigateToTrip(notificationData.tripId);
    } else if (notificationData?.type === 'emergency-alert') {
      this.handleEmergencyAlert(notificationData);
    } else if (notificationData?.type === 'payment-reminder' && notificationData?.paymentId) {
      this.navigateToPayment(notificationData.paymentId);
    }
  }

  async sendTokenToBackend(token) {
    try {
      const response = await apiClient.post('/notifications/register-device', {
        pushToken: token,
        platform: Platform.OS,
        deviceInfo: {
          brand: Device.brand,
          modelName: Device.modelName,
          osName: Device.osName,
          osVersion: Device.osVersion
        }
      });

      if (response.success) {
        console.log('Push token registered successfully');
        return true;
      } else {
        console.error('Failed to register push token:', response.message);
        return false;
      }
    } catch (error) {
      console.error('Error sending token to backend:', error);
      return false;
    }
  }

  async sendLocalNotification(title, body, data = {}, channelId = 'default') {
    try {
      const { notifications } = useAppSettingsStore.getState();
      
      if (!notifications.enabled) {
        return null;
      }

      const notificationId = await Notifications.scheduleNotificationAsync({
        content: {
          title,
          body,
          data,
          sound: notifications.pushEnabled ? 'default' : false,
        },
        trigger: null, // Show immediately
        identifier: data.id || Date.now().toString(),
      });

      return notificationId;
    } catch (error) {
      console.error('Error sending local notification:', error);
      return null;
    }
  }

  async scheduleNotification(title, body, trigger, data = {}, channelId = 'default') {
    try {
      const { notifications } = useAppSettingsStore.getState();
      
      if (!notifications.enabled) {
        return null;
      }

      const notificationId = await Notifications.scheduleNotificationAsync({
        content: {
          title,
          body,
          data,
          sound: notifications.pushEnabled ? 'default' : false,
        },
        trigger,
        identifier: data.id || Date.now().toString(),
      });

      return notificationId;
    } catch (error) {
      console.error('Error scheduling notification:', error);
      return null;
    }
  }

  async cancelNotification(notificationId) {
    try {
      await Notifications.cancelScheduledNotificationAsync(notificationId);
      return true;
    } catch (error) {
      console.error('Error canceling notification:', error);
      return false;
    }
  }

  async cancelAllNotifications() {
    try {
      await Notifications.cancelAllScheduledNotificationsAsync();
      return true;
    } catch (error) {
      console.error('Error canceling all notifications:', error);
      return false;
    }
  }

  async getBadgeCount() {
    try {
      return await Notifications.getBadgeCountAsync();
    } catch (error) {
      console.error('Error getting badge count:', error);
      return 0;
    }
  }

  async setBadgeCount(count) {
    try {
      await Notifications.setBadgeCountAsync(count);
      return true;
    } catch (error) {
      console.error('Error setting badge count:', error);
      return false;
    }
  }

  async updateNotificationSettings(settings) {
    try {
      await AsyncStorage.setItem(NOTIFICATION_SETTINGS_KEY, JSON.stringify(settings));
      
      const { updateNotificationSettings } = useAppSettingsStore.getState();
      updateNotificationSettings(settings);
      
      return true;
    } catch (error) {
      console.error('Error updating notification settings:', error);
      return false;
    }
  }

  async getNotificationSettings() {
    try {
      const settingsJson = await AsyncStorage.getItem(NOTIFICATION_SETTINGS_KEY);
      return settingsJson ? JSON.parse(settingsJson) : null;
    } catch (error) {
      console.error('Error getting notification settings:', error);
      return null;
    }
  }

  navigateToTrip(tripId) {
    console.log('Navigate to trip:', tripId);
  }

  navigateToPayment(paymentId) {
    console.log('Navigate to payment:', paymentId);
  }

  handleEmergencyAlert(data) {
    console.log('Emergency alert:', data);
  }

  cleanup() {
    if (this.notificationListener) {
      Notifications.removeNotificationSubscription(this.notificationListener);
      this.notificationListener = null;
    }

    if (this.responseListener) {
      Notifications.removeNotificationSubscription(this.responseListener);
      this.responseListener = null;
    }

    this.isInitialized = false;
  }

  getExpoPushToken() {
    return this.expoPushToken;
  }

  isReady() {
    return this.isInitialized && this.expoPushToken !== null;
  }
}

export default new NotificationService();
