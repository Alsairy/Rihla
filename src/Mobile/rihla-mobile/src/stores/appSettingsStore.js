import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';

const useAppSettingsStore = create(
  persist(
    (set, get) => ({
      theme: 'light',
      language: 'en',
      notifications: {
        push: true,
        email: true,
        sms: false,
        tripUpdates: true,
        emergencyAlerts: true,
        maintenanceAlerts: false
      },
      biometric: {
        enabled: false,
        type: null
      },
      location: {
        enabled: true,
        highAccuracy: false
      },
      offline: {
        syncEnabled: true,
        autoSync: true,
        syncInterval: 300000
      },
      display: {
        fontSize: 'medium',
        mapType: 'standard',
        showTraffic: false
      },
      privacy: {
        shareLocation: true,
        analytics: true,
        crashReporting: true
      },
      
      setTheme: (theme) => set({ theme }),
      
      setLanguage: (language) => set({ language }),
      
      updateNotificationSettings: (settings) => set((state) => ({
        notifications: { ...state.notifications, ...settings }
      })),
      
      updateBiometricSettings: (settings) => set((state) => ({
        biometric: { ...state.biometric, ...settings }
      })),
      
      updateLocationSettings: (settings) => set((state) => ({
        location: { ...state.location, ...settings }
      })),
      
      updateOfflineSettings: (settings) => set((state) => ({
        offline: { ...state.offline, ...settings }
      })),
      
      updateDisplaySettings: (settings) => set((state) => ({
        display: { ...state.display, ...settings }
      })),
      
      updatePrivacySettings: (settings) => set((state) => ({
        privacy: { ...state.privacy, ...settings }
      })),
      
      toggleNotification: (key) => set((state) => ({
        notifications: {
          ...state.notifications,
          [key]: !state.notifications[key]
        }
      })),
      
      toggleBiometric: () => set((state) => ({
        biometric: {
          ...state.biometric,
          enabled: !state.biometric.enabled
        }
      })),
      
      toggleLocation: () => set((state) => ({
        location: {
          ...state.location,
          enabled: !state.location.enabled
        }
      })),
      
      resetToDefaults: () => set({
        theme: 'light',
        language: 'en',
        notifications: {
          push: true,
          email: true,
          sms: false,
          tripUpdates: true,
          emergencyAlerts: true,
          maintenanceAlerts: false
        },
        biometric: {
          enabled: false,
          type: null
        },
        location: {
          enabled: true,
          highAccuracy: false
        },
        offline: {
          syncEnabled: true,
          autoSync: true,
          syncInterval: 300000
        },
        display: {
          fontSize: 'medium',
          mapType: 'standard',
          showTraffic: false
        },
        privacy: {
          shareLocation: true,
          analytics: true,
          crashReporting: true
        }
      }),
      
      getSettings: () => get(),
      
      isNotificationEnabled: (key) => {
        const state = get();
        return state.notifications[key] || false;
      },
      
      isDarkMode: () => {
        const state = get();
        return state.theme === 'dark';
      }
    }),
    {
      name: 'app-settings-storage',
      storage: createJSONStorage(() => AsyncStorage)
    }
  )
);

export default useAppSettingsStore;
