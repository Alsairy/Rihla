import React from 'react';
import {
  View,
  Text,
  ScrollView,
  StyleSheet,
  TouchableOpacity,
  Alert,
  Switch,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../contexts/AuthContext';

export default function ProfileScreen() {
  const { user, logout } = useAuth();
  const [notificationsEnabled, setNotificationsEnabled] = React.useState(true);
  const [locationEnabled, setLocationEnabled] = React.useState(true);

  const handleLogout = () => {
    Alert.alert(
      'Logout',
      'Are you sure you want to logout?',
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Logout', 
          style: 'destructive',
          onPress: async () => {
            await logout();
          }
        },
      ]
    );
  };

  const ProfileItem = ({ icon, title, subtitle, onPress, rightComponent }) => (
    <TouchableOpacity style={styles.profileItem} onPress={onPress}>
      <View style={styles.profileItemLeft}>
        <View style={styles.iconContainer}>
          <Ionicons name={icon} size={20} color="#2563eb" />
        </View>
        <View style={styles.profileItemText}>
          <Text style={styles.profileItemTitle}>{title}</Text>
          {subtitle && <Text style={styles.profileItemSubtitle}>{subtitle}</Text>}
        </View>
      </View>
      {rightComponent || <Ionicons name="chevron-forward" size={20} color="#94a3b8" />}
    </TouchableOpacity>
  );

  const SectionHeader = ({ title }) => (
    <Text style={styles.sectionHeader}>{title}</Text>
  );

  return (
    <ScrollView style={styles.container}>
      {/* Profile Header */}
      <View style={styles.profileHeader}>
        <View style={styles.avatarContainer}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>
              {user?.name?.split(' ').map(n => n[0]).join('') || 'AU'}
            </Text>
          </View>
        </View>
        <Text style={styles.userName}>{user?.name || 'Admin User'}</Text>
        <Text style={styles.userEmail}>{user?.email || 'user@rihla.sa'}</Text>
        <Text style={styles.userRole}>{user?.role || 'Administrator'}</Text>
      </View>

      {/* Account Section */}
      <SectionHeader title="Account" />
      <View style={styles.section}>
        <ProfileItem
          icon="person-outline"
          title="Personal Information"
          subtitle="Update your personal details"
          onPress={() => Alert.alert('Info', 'Personal Information screen')}
        />
        <ProfileItem
          icon="shield-checkmark-outline"
          title="Security"
          subtitle="Password and security settings"
          onPress={() => Alert.alert('Info', 'Security settings screen')}
        />
        <ProfileItem
          icon="card-outline"
          title="Billing & Payments"
          subtitle="Manage payment methods"
          onPress={() => Alert.alert('Info', 'Billing screen')}
        />
      </View>

      {/* Preferences Section */}
      <SectionHeader title="Preferences" />
      <View style={styles.section}>
        <ProfileItem
          icon="notifications-outline"
          title="Notifications"
          subtitle="Push notifications and alerts"
          rightComponent={
            <Switch
              value={notificationsEnabled}
              onValueChange={setNotificationsEnabled}
              trackColor={{ false: '#e2e8f0', true: '#2563eb' }}
              thumbColor={notificationsEnabled ? '#ffffff' : '#f4f3f4'}
            />
          }
        />
        <ProfileItem
          icon="location-outline"
          title="Location Services"
          subtitle="Allow location tracking"
          rightComponent={
            <Switch
              value={locationEnabled}
              onValueChange={setLocationEnabled}
              trackColor={{ false: '#e2e8f0', true: '#2563eb' }}
              thumbColor={locationEnabled ? '#ffffff' : '#f4f3f4'}
            />
          }
        />
        <ProfileItem
          icon="language-outline"
          title="Language"
          subtitle="English"
          onPress={() => Alert.alert('Info', 'Language selection screen')}
        />
        <ProfileItem
          icon="moon-outline"
          title="Dark Mode"
          subtitle="Switch to dark theme"
          rightComponent={
            <Switch
              value={false}
              onValueChange={() => {}}
              trackColor={{ false: '#e2e8f0', true: '#2563eb' }}
              thumbColor={'#f4f3f4'}
            />
          }
        />
      </View>

      {/* Support Section */}
      <SectionHeader title="Support" />
      <View style={styles.section}>
        <ProfileItem
          icon="help-circle-outline"
          title="Help Center"
          subtitle="FAQs and support articles"
          onPress={() => Alert.alert('Info', 'Help Center screen')}
        />
        <ProfileItem
          icon="chatbubble-outline"
          title="Contact Support"
          subtitle="Get help from our team"
          onPress={() => Alert.alert('Info', 'Contact Support screen')}
        />
        <ProfileItem
          icon="star-outline"
          title="Rate App"
          subtitle="Rate us on the app store"
          onPress={() => Alert.alert('Info', 'Rate app functionality')}
        />
        <ProfileItem
          icon="document-text-outline"
          title="Terms & Privacy"
          subtitle="Legal information"
          onPress={() => Alert.alert('Info', 'Terms & Privacy screen')}
        />
      </View>

      {/* App Info Section */}
      <SectionHeader title="About" />
      <View style={styles.section}>
        <ProfileItem
          icon="information-circle-outline"
          title="App Version"
          subtitle="1.0.0"
          onPress={() => {}}
          rightComponent={<View />}
        />
        <ProfileItem
          icon="refresh-outline"
          title="Check for Updates"
          subtitle="Keep your app up to date"
          onPress={() => Alert.alert('Info', 'App is up to date')}
        />
      </View>

      {/* Logout Button */}
      <View style={styles.logoutContainer}>
        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={20} color="#ef4444" />
          <Text style={styles.logoutText}>Logout</Text>
        </TouchableOpacity>
      </View>

      {/* Footer */}
      <View style={styles.footer}>
        <Text style={styles.footerText}>Rihla School Transportation</Text>
        <Text style={styles.footerText}>Version 1.0.0</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  profileHeader: {
    backgroundColor: '#ffffff',
    alignItems: 'center',
    paddingVertical: 32,
    paddingHorizontal: 24,
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  avatarContainer: {
    marginBottom: 16,
  },
  avatar: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#2563eb',
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatarText: {
    color: '#ffffff',
    fontSize: 24,
    fontWeight: 'bold',
  },
  userName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  userEmail: {
    fontSize: 16,
    color: '#64748b',
    marginBottom: 4,
  },
  userRole: {
    fontSize: 14,
    color: '#2563eb',
    fontWeight: '600',
    backgroundColor: '#eff6ff',
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 12,
  },
  sectionHeader: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#1e293b',
    marginTop: 24,
    marginBottom: 8,
    marginHorizontal: 16,
  },
  section: {
    backgroundColor: '#ffffff',
    marginHorizontal: 16,
    borderRadius: 12,
    overflow: 'hidden',
  },
  profileItem: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#f1f5f9',
  },
  profileItemLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  iconContainer: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#eff6ff',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 12,
  },
  profileItemText: {
    flex: 1,
  },
  profileItemTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#1e293b',
  },
  profileItemSubtitle: {
    fontSize: 14,
    color: '#64748b',
    marginTop: 2,
  },
  logoutContainer: {
    marginHorizontal: 16,
    marginTop: 24,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#ffffff',
    borderRadius: 12,
    paddingVertical: 16,
    borderWidth: 1,
    borderColor: '#fecaca',
  },
  logoutText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#ef4444',
    marginLeft: 8,
  },
  footer: {
    alignItems: 'center',
    paddingVertical: 32,
  },
  footerText: {
    fontSize: 12,
    color: '#94a3b8',
    marginBottom: 4,
  },
});

