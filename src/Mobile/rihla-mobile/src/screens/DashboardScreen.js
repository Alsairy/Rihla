import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  StyleSheet,
  RefreshControl,
  TouchableOpacity,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../contexts/AuthContext';
import apiClient from '../services/apiClient';

export default function DashboardScreen() {
  const [refreshing, setRefreshing] = useState(false);
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/dashboard/statistics');
      setDashboardData(response.data);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = React.useCallback(async () => {
    setRefreshing(true);
    await fetchDashboardData();
    setRefreshing(false);
  }, []);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const StatCard = ({ title, value, change, icon, color = '#2563eb' }) => (
    <View style={[styles.statCard, { borderLeftColor: color }]}>
      <View style={styles.statHeader}>
        <View style={styles.statTitleContainer}>
          <Ionicons name={icon} size={20} color={color} />
          <Text style={styles.statTitle}>{title}</Text>
        </View>
      </View>
      <Text style={styles.statValue}>{value}</Text>
      {change && (
        <Text style={[styles.statChange, { color: change.startsWith('+') ? '#10b981' : '#ef4444' }]}>
          {change} from last month
        </Text>
      )}
    </View>
  );

  const AlertCard = ({ title, message, time, type = 'info' }) => (
    <View style={styles.alertCard}>
      <View style={styles.alertHeader}>
        <Ionicons 
          name={type === 'warning' ? 'warning' : type === 'error' ? 'alert-circle' : 'information-circle'} 
          size={20} 
          color={type === 'warning' ? '#f59e0b' : type === 'error' ? '#ef4444' : '#2563eb'} 
        />
        <Text style={styles.alertTitle}>{title}</Text>
      </View>
      <Text style={styles.alertMessage}>{message}</Text>
      <Text style={styles.alertTime}>{time}</Text>
    </View>
  );

  return (
    <ScrollView 
      style={styles.container}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
      }
    >
      {/* Header */}
      <View style={styles.header}>
        <View>
          <Text style={styles.welcomeText}>Welcome back!</Text>
          <Text style={styles.userName}>{user?.name || 'Admin User'}</Text>
        </View>
        <TouchableOpacity style={styles.notificationButton}>
          <Ionicons name="notifications-outline" size={24} color="#64748b" />
          <View style={styles.notificationBadge}>
            <Text style={styles.notificationBadgeText}>3</Text>
          </View>
        </TouchableOpacity>
      </View>

      {/* Stats Grid */}
      <View style={styles.statsContainer}>
        {loading ? (
          <Text style={styles.loadingText}>Loading dashboard data...</Text>
        ) : dashboardData ? (
          <>
            <StatCard
              title="Total Students"
              value={dashboardData.totalStudents?.toString() || "0"}
              change={dashboardData.studentGrowth || "+0%"}
              icon="people"
              color="#2563eb"
            />
            <StatCard
              title="Active Drivers"
              value={dashboardData.activeDrivers?.toString() || "0"}
              change={dashboardData.driverGrowth || "+0%"}
              icon="person"
              color="#10b981"
            />
            <StatCard
              title="Total Vehicles"
              value={dashboardData.totalVehicles?.toString() || "0"}
              change={dashboardData.vehicleGrowth || "0%"}
              icon="bus"
              color="#f59e0b"
            />
            <StatCard
              title="Active Routes"
              value={dashboardData.activeRoutes?.toString() || "0"}
              change={dashboardData.routeGrowth || "0%"}
              icon="map"
              color="#ef4444"
            />
            <StatCard
              title="Today's Trips"
              value={dashboardData.todaysTrips?.toString() || "0"}
              change={dashboardData.tripGrowth || "+0%"}
              icon="location"
              color="#8b5cf6"
            />
            <StatCard
              title="Attendance Rate"
              value={`${dashboardData.attendanceRate || 0}%`}
              change={dashboardData.attendanceGrowth || "+0%"}
              icon="checkmark-circle"
              color="#06b6d4"
            />
          </>
        ) : (
          <Text style={styles.errorText}>Failed to load dashboard data</Text>
        )}
      </View>

      {/* Quick Actions */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Quick Actions</Text>
        <View style={styles.quickActions}>
          <TouchableOpacity style={styles.actionButton}>
            <Ionicons name="add-circle" size={24} color="#2563eb" />
            <Text style={styles.actionText}>Add Student</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton}>
            <Ionicons name="car" size={24} color="#2563eb" />
            <Text style={styles.actionText}>Track Vehicle</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton}>
            <Ionicons name="calendar" size={24} color="#2563eb" />
            <Text style={styles.actionText}>Schedule Trip</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton}>
            <Ionicons name="document-text" size={24} color="#2563eb" />
            <Text style={styles.actionText}>Generate Report</Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* Recent Alerts */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Recent Alerts</Text>
        <AlertCard
          title="Vehicle Maintenance"
          message="Vehicle BUS-001 maintenance due"
          time="2 hours ago"
          type="warning"
        />
        <AlertCard
          title="New Registration"
          message="New student registration pending"
          time="4 hours ago"
          type="info"
        />
        <AlertCard
          title="Route Delay"
          message="Route 5 delayed by 15 minutes"
          time="6 hours ago"
          type="error"
        />
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 20,
    backgroundColor: '#ffffff',
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  welcomeText: {
    fontSize: 14,
    color: '#64748b',
  },
  userName: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  notificationButton: {
    position: 'relative',
  },
  notificationBadge: {
    position: 'absolute',
    top: -8,
    right: -8,
    backgroundColor: '#ef4444',
    borderRadius: 10,
    width: 20,
    height: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  notificationBadgeText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  statsContainer: {
    padding: 16,
  },
  statCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderLeftWidth: 4,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  statHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  statTitleContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  statTitle: {
    fontSize: 14,
    color: '#64748b',
    marginLeft: 8,
  },
  statValue: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  statChange: {
    fontSize: 12,
    fontWeight: '500',
  },
  section: {
    padding: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 16,
  },
  quickActions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  actionButton: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    alignItems: 'center',
    width: '48%',
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  actionText: {
    fontSize: 12,
    color: '#374151',
    marginTop: 8,
    textAlign: 'center',
  },
  alertCard: {
    backgroundColor: '#ffffff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  alertHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  alertTitle: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1e293b',
    marginLeft: 8,
  },
  alertMessage: {
    fontSize: 14,
    color: '#64748b',
    marginBottom: 8,
  },
  alertTime: {
    fontSize: 12,
    color: '#94a3b8',
  },
  loadingText: {
    fontSize: 16,
    color: '#64748b',
    textAlign: 'center',
    padding: 20,
  },
  errorText: {
    fontSize: 16,
    color: '#ef4444',
    textAlign: 'center',
    padding: 20,
  },
});

