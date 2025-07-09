import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, Animated } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import syncService from '../services/syncService';

const OfflineIndicator = () => {
  const [isOnline, setIsOnline] = useState(true);
  const [isSyncing, setIsSyncing] = useState(false);
  const [pendingChanges, setPendingChanges] = useState(0);
  const [slideAnim] = useState(new Animated.Value(-50));

  useEffect(() => {
    const unsubscribe = syncService.addListener((status) => {
      setIsOnline(status.isOnline);
      setIsSyncing(status.isSyncing);
      setPendingChanges(status.pendingChanges || 0);

      if (!status.isOnline || status.isSyncing || (status.pendingChanges && status.pendingChanges > 0)) {
        Animated.timing(slideAnim, {
          toValue: 0,
          duration: 300,
          useNativeDriver: true,
        }).start();
      } else {
        Animated.timing(slideAnim, {
          toValue: -50,
          duration: 300,
          useNativeDriver: true,
        }).start();
      }
    });

    return unsubscribe;
  }, [slideAnim]);

  const getIndicatorContent = () => {
    if (!isOnline) {
      return {
        icon: 'cloud-offline-outline',
        text: pendingChanges > 0 ? `Offline - ${pendingChanges} pending changes` : 'Offline',
        color: '#ef4444',
        backgroundColor: '#fef2f2'
      };
    } else if (isSyncing) {
      return {
        icon: 'sync-outline',
        text: 'Syncing...',
        color: '#3b82f6',
        backgroundColor: '#eff6ff'
      };
    } else if (pendingChanges > 0) {
      return {
        icon: 'cloud-upload-outline',
        text: `${pendingChanges} changes synced`,
        color: '#10b981',
        backgroundColor: '#f0fdf4'
      };
    }
    return null;
  };

  const content = getIndicatorContent();
  if (!content) return null;

  return (
    <Animated.View 
      style={[
        styles.container, 
        { 
          backgroundColor: content.backgroundColor,
          transform: [{ translateY: slideAnim }]
        }
      ]}
    >
      <View style={styles.content}>
        <Ionicons 
          name={content.icon} 
          size={16} 
          color={content.color}
          style={isSyncing ? styles.spinningIcon : null}
        />
        <Text style={[styles.text, { color: content.color }]}>
          {content.text}
        </Text>
      </View>
    </Animated.View>
  );
};

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    zIndex: 1000,
    paddingTop: 44, // Account for status bar
    paddingBottom: 8,
    paddingHorizontal: 16,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(0,0,0,0.1)',
  },
  content: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: {
    fontSize: 14,
    fontWeight: '500',
    marginLeft: 8,
  },
  spinningIcon: {
  },
});

export default OfflineIndicator;
