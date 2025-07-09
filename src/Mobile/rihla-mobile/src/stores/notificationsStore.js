import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';

const useNotificationsStore = create(
  persist(
    (set, get) => ({
      notifications: [],
      unreadCount: 0,
      loading: false,
      error: null,
      lastUpdated: null,
      
      setNotifications: (notifications) => {
        const unreadCount = notifications.filter(n => !n.isRead).length;
        set({ 
          notifications, 
          unreadCount,
          lastUpdated: new Date().toISOString(),
          error: null 
        });
      },
      
      addNotification: (notification) => set((state) => {
        const newNotifications = [notification, ...state.notifications];
        const unreadCount = newNotifications.filter(n => !n.isRead).length;
        return {
          notifications: newNotifications,
          unreadCount,
          lastUpdated: new Date().toISOString()
        };
      }),
      
      markAsRead: (notificationId) => set((state) => {
        const updatedNotifications = state.notifications.map(notification =>
          notification.id === notificationId 
            ? { ...notification, isRead: true }
            : notification
        );
        const unreadCount = updatedNotifications.filter(n => !n.isRead).length;
        return {
          notifications: updatedNotifications,
          unreadCount,
          lastUpdated: new Date().toISOString()
        };
      }),
      
      markAllAsRead: () => set((state) => ({
        notifications: state.notifications.map(notification => ({
          ...notification,
          isRead: true
        })),
        unreadCount: 0,
        lastUpdated: new Date().toISOString()
      })),
      
      removeNotification: (notificationId) => set((state) => {
        const updatedNotifications = state.notifications.filter(
          notification => notification.id !== notificationId
        );
        const unreadCount = updatedNotifications.filter(n => !n.isRead).length;
        return {
          notifications: updatedNotifications,
          unreadCount,
          lastUpdated: new Date().toISOString()
        };
      }),
      
      setLoading: (loading) => set({ loading }),
      
      setError: (error) => set({ error }),
      
      clearError: () => set({ error: null }),
      
      getUnreadNotifications: () => {
        const state = get();
        return state.notifications.filter(notification => !notification.isRead);
      },
      
      getNotificationsByType: (type) => {
        const state = get();
        return state.notifications.filter(notification => notification.type === type);
      },
      
      clearNotifications: () => set({
        notifications: [],
        unreadCount: 0,
        error: null,
        lastUpdated: null
      })
    }),
    {
      name: 'notifications-storage',
      storage: createJSONStorage(() => AsyncStorage),
      partialize: (state) => ({
        notifications: state.notifications,
        unreadCount: state.unreadCount,
        lastUpdated: state.lastUpdated
      })
    }
  )
);

export default useNotificationsStore;
