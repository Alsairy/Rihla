import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { useTripsStore, useNotificationsStore, useAppSettingsStore } from '../stores';
import { apiClient } from './apiClient';

const SYNC_KEYS = {
  TRIPS: 'offline_trips',
  NOTIFICATIONS: 'offline_notifications',
  STUDENTS: 'offline_students',
  DRIVERS: 'offline_drivers',
  VEHICLES: 'offline_vehicles',
  ROUTES: 'offline_routes',
  PENDING_CHANGES: 'pending_changes',
  LAST_SYNC: 'last_sync_timestamp'
};

const SYNC_INTERVALS = {
  BACKGROUND: 5 * 60 * 1000, // 5 minutes
  FOREGROUND: 30 * 1000,     // 30 seconds
  RETRY: 60 * 1000           // 1 minute
};

class SyncService {
  constructor() {
    this.isOnline = true;
    this.isSyncing = false;
    this.syncQueue = [];
    this.backgroundSyncInterval = null;
    this.retryTimeout = null;
    this.listeners = [];
    
    this.initializeNetworkListener();
  }

  initializeNetworkListener() {
    NetInfo.addEventListener(state => {
      const wasOnline = this.isOnline;
      this.isOnline = state.isConnected && state.isInternetReachable;
      
      if (!wasOnline && this.isOnline) {
        this.notifyListeners('online');
        this.performFullSync();
      } else if (wasOnline && !this.isOnline) {
        this.notifyListeners('offline');
        this.stopBackgroundSync();
      }
    });
  }

  addListener(callback) {
    this.listeners.push(callback);
    return () => {
      this.listeners = this.listeners.filter(listener => listener !== callback);
    };
  }

  notifyListeners(event, data = null) {
    this.listeners.forEach(listener => {
      try {
        listener({ event, data, isOnline: this.isOnline, isSyncing: this.isSyncing });
      } catch (error) {
        console.error('Sync listener error:', error);
      }
    });
  }

  async cacheData(key, data) {
    try {
      const cacheData = {
        data,
        timestamp: new Date().toISOString(),
        version: 1
      };
      await AsyncStorage.setItem(key, JSON.stringify(cacheData));
      return true;
    } catch (error) {
      console.error(`Error caching data for ${key}:`, error);
      return false;
    }
  }

  async getCachedData(key, maxAge = 24 * 60 * 60 * 1000) { // 24 hours default
    try {
      const cachedJson = await AsyncStorage.getItem(key);
      if (!cachedJson) return null;

      const cached = JSON.parse(cachedJson);
      const age = new Date().getTime() - new Date(cached.timestamp).getTime();
      
      if (age > maxAge) {
        await AsyncStorage.removeItem(key);
        return null;
      }

      return cached.data;
    } catch (error) {
      console.error(`Error getting cached data for ${key}:`, error);
      return null;
    }
  }

  async addPendingChange(type, action, data, id = null) {
    try {
      const pendingChanges = await this.getPendingChanges();
      const change = {
        id: Date.now().toString(),
        type,
        action, // 'create', 'update', 'delete'
        data,
        entityId: id,
        timestamp: new Date().toISOString(),
        retryCount: 0
      };

      pendingChanges.push(change);
      await AsyncStorage.setItem(SYNC_KEYS.PENDING_CHANGES, JSON.stringify(pendingChanges));
      
      if (this.isOnline) {
        this.processPendingChanges();
      }

      return change.id;
    } catch (error) {
      console.error('Error adding pending change:', error);
      return null;
    }
  }

  async getPendingChanges() {
    try {
      const changesJson = await AsyncStorage.getItem(SYNC_KEYS.PENDING_CHANGES);
      return changesJson ? JSON.parse(changesJson) : [];
    } catch (error) {
      console.error('Error getting pending changes:', error);
      return [];
    }
  }

  async removePendingChange(changeId) {
    try {
      const pendingChanges = await this.getPendingChanges();
      const filteredChanges = pendingChanges.filter(change => change.id !== changeId);
      await AsyncStorage.setItem(SYNC_KEYS.PENDING_CHANGES, JSON.stringify(filteredChanges));
      return true;
    } catch (error) {
      console.error('Error removing pending change:', error);
      return false;
    }
  }

  async performFullSync() {
    if (this.isSyncing || !this.isOnline) return;

    try {
      this.isSyncing = true;
      this.notifyListeners('sync_start');

      await this.processPendingChanges();

      await Promise.all([
        this.syncTrips(),
        this.syncNotifications(),
        this.syncStudents(),
        this.syncDrivers(),
        this.syncVehicles(),
        this.syncRoutes()
      ]);

      await AsyncStorage.setItem(SYNC_KEYS.LAST_SYNC, new Date().toISOString());
      
      this.notifyListeners('sync_complete');
    } catch (error) {
      console.error('Full sync error:', error);
      this.notifyListeners('sync_error', error.message);
      this.scheduleRetry();
    } finally {
      this.isSyncing = false;
    }
  }

  async processPendingChanges() {
    const pendingChanges = await this.getPendingChanges();
    
    for (const change of pendingChanges) {
      try {
        let success = false;

        switch (change.type) {
          case 'trips':
            success = await this.syncPendingTripChange(change);
            break;
          case 'notifications':
            success = await this.syncPendingNotificationChange(change);
            break;
          case 'students':
            success = await this.syncPendingStudentChange(change);
            break;
          case 'drivers':
            success = await this.syncPendingDriverChange(change);
            break;
          case 'vehicles':
            success = await this.syncPendingVehicleChange(change);
            break;
          case 'routes':
            success = await this.syncPendingRouteChange(change);
            break;
        }

        if (success) {
          await this.removePendingChange(change.id);
        } else {
          change.retryCount++;
          if (change.retryCount >= 3) {
            await this.removePendingChange(change.id);
            this.notifyListeners('sync_conflict', {
              type: change.type,
              action: change.action,
              data: change.data
            });
          }
        }
      } catch (error) {
        console.error(`Error processing pending change ${change.id}:`, error);
      }
    }
  }

  async syncTrips() {
    try {
      const response = await apiClient.get('/trips');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.TRIPS, response.data);
        
        const { setTrips } = useTripsStore.getState();
        setTrips(response.data);
      }
    } catch (error) {
      console.error('Error syncing trips:', error);
      const cachedTrips = await this.getCachedData(SYNC_KEYS.TRIPS);
      if (cachedTrips) {
        const { setTrips } = useTripsStore.getState();
        setTrips(cachedTrips);
      }
    }
  }

  async syncNotifications() {
    try {
      const response = await apiClient.get('/notifications');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.NOTIFICATIONS, response.data);
        
        const { setNotifications } = useNotificationsStore.getState();
        setNotifications(response.data);
      }
    } catch (error) {
      console.error('Error syncing notifications:', error);
      const cachedNotifications = await this.getCachedData(SYNC_KEYS.NOTIFICATIONS);
      if (cachedNotifications) {
        const { setNotifications } = useNotificationsStore.getState();
        setNotifications(cachedNotifications);
      }
    }
  }

  async syncStudents() {
    try {
      const response = await apiClient.get('/students');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.STUDENTS, response.data);
      }
    } catch (error) {
      console.error('Error syncing students:', error);
    }
  }

  async syncDrivers() {
    try {
      const response = await apiClient.get('/drivers');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.DRIVERS, response.data);
      }
    } catch (error) {
      console.error('Error syncing drivers:', error);
    }
  }

  async syncVehicles() {
    try {
      const response = await apiClient.get('/vehicles');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.VEHICLES, response.data);
      }
    } catch (error) {
      console.error('Error syncing vehicles:', error);
    }
  }

  async syncRoutes() {
    try {
      const response = await apiClient.get('/routes');
      if (response.success) {
        await this.cacheData(SYNC_KEYS.ROUTES, response.data);
      }
    } catch (error) {
      console.error('Error syncing routes:', error);
    }
  }

  async syncPendingTripChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'create':
          response = await apiClient.post('/trips', change.data);
          break;
        case 'update':
          response = await apiClient.put(`/trips/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/trips/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending trip change:', error);
      return false;
    }
  }

  async syncPendingNotificationChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'update':
          response = await apiClient.put(`/notifications/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/notifications/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending notification change:', error);
      return false;
    }
  }

  async syncPendingStudentChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'create':
          response = await apiClient.post('/students', change.data);
          break;
        case 'update':
          response = await apiClient.put(`/students/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/students/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending student change:', error);
      return false;
    }
  }

  async syncPendingDriverChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'create':
          response = await apiClient.post('/drivers', change.data);
          break;
        case 'update':
          response = await apiClient.put(`/drivers/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/drivers/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending driver change:', error);
      return false;
    }
  }

  async syncPendingVehicleChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'create':
          response = await apiClient.post('/vehicles', change.data);
          break;
        case 'update':
          response = await apiClient.put(`/vehicles/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/vehicles/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending vehicle change:', error);
      return false;
    }
  }

  async syncPendingRouteChange(change) {
    try {
      let response;
      switch (change.action) {
        case 'create':
          response = await apiClient.post('/routes', change.data);
          break;
        case 'update':
          response = await apiClient.put(`/routes/${change.entityId}`, change.data);
          break;
        case 'delete':
          response = await apiClient.delete(`/routes/${change.entityId}`);
          break;
      }
      return response && response.success;
    } catch (error) {
      console.error('Error syncing pending route change:', error);
      return false;
    }
  }

  startBackgroundSync() {
    if (this.backgroundSyncInterval) return;

    const { offline } = useAppSettingsStore.getState();
    if (!offline.syncEnabled || !offline.autoSync) return;

    this.backgroundSyncInterval = setInterval(() => {
      if (this.isOnline && !this.isSyncing) {
        this.performFullSync();
      }
    }, offline.syncInterval || SYNC_INTERVALS.BACKGROUND);
  }

  stopBackgroundSync() {
    if (this.backgroundSyncInterval) {
      clearInterval(this.backgroundSyncInterval);
      this.backgroundSyncInterval = null;
    }
  }

  scheduleRetry() {
    if (this.retryTimeout) return;

    this.retryTimeout = setTimeout(() => {
      this.retryTimeout = null;
      if (this.isOnline) {
        this.performFullSync();
      }
    }, SYNC_INTERVALS.RETRY);
  }

  async getOfflineData(type) {
    const key = SYNC_KEYS[type.toUpperCase()];
    if (!key) return null;
    
    return await this.getCachedData(key);
  }

  async getLastSyncTime() {
    try {
      const timestamp = await AsyncStorage.getItem(SYNC_KEYS.LAST_SYNC);
      return timestamp ? new Date(timestamp) : null;
    } catch (error) {
      console.error('Error getting last sync time:', error);
      return null;
    }
  }

  getConnectionStatus() {
    return {
      isOnline: this.isOnline,
      isSyncing: this.isSyncing,
      pendingChanges: this.getPendingChanges().length
    };
  }

  cleanup() {
    this.stopBackgroundSync();
    if (this.retryTimeout) {
      clearTimeout(this.retryTimeout);
      this.retryTimeout = null;
    }
    this.listeners = [];
  }
}

export default new SyncService();
