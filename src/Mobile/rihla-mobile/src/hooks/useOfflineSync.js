import { useState, useEffect, useCallback } from 'react';
import syncService from '../services/syncService';
import { useAppSettingsStore } from '../stores';

const useOfflineSync = () => {
  const [syncStatus, setSyncStatus] = useState({
    isOnline: true,
    isSyncing: false,
    pendingChanges: 0,
    lastSyncTime: null,
    error: null
  });

  const { offline } = useAppSettingsStore();

  useEffect(() => {
    const initializeStatus = async () => {
      const connectionStatus = syncService.getConnectionStatus();
      const lastSync = await syncService.getLastSyncTime();
      
      setSyncStatus(prev => ({
        ...prev,
        ...connectionStatus,
        lastSyncTime: lastSync
      }));
    };

    initializeStatus();

    const unsubscribe = syncService.addListener((status) => {
      setSyncStatus(prev => ({
        ...prev,
        isOnline: status.isOnline,
        isSyncing: status.isSyncing,
        pendingChanges: status.data?.pendingChanges || prev.pendingChanges,
        error: status.event === 'sync_error' ? status.data : null
      }));

      if (status.event === 'sync_complete') {
        syncService.getLastSyncTime().then(lastSync => {
          setSyncStatus(prev => ({
            ...prev,
            lastSyncTime: lastSync,
            error: null
          }));
        });
      }
    });

    if (offline.syncEnabled && offline.autoSync) {
      syncService.startBackgroundSync();
    }

    return () => {
      unsubscribe();
      syncService.stopBackgroundSync();
    };
  }, [offline.syncEnabled, offline.autoSync]);

  const manualSync = useCallback(async () => {
    if (!syncStatus.isOnline || syncStatus.isSyncing) {
      return { success: false, message: 'Cannot sync while offline or already syncing' };
    }

    try {
      await syncService.performFullSync();
      return { success: true };
    } catch (error) {
      return { success: false, message: error.message };
    }
  }, [syncStatus.isOnline, syncStatus.isSyncing]);

  const addOfflineChange = useCallback(async (type, action, data, id = null) => {
    return await syncService.addPendingChange(type, action, data, id);
  }, []);

  const getOfflineData = useCallback(async (type) => {
    return await syncService.getOfflineData(type);
  }, []);

  const clearSyncError = useCallback(() => {
    setSyncStatus(prev => ({ ...prev, error: null }));
  }, []);

  return {
    syncStatus,
    manualSync,
    addOfflineChange,
    getOfflineData,
    clearSyncError,
    isOfflineMode: !syncStatus.isOnline,
    hasPendingChanges: syncStatus.pendingChanges > 0,
    canSync: syncStatus.isOnline && !syncStatus.isSyncing
  };
};

export default useOfflineSync;
