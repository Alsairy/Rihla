import React, { useState } from 'react';
import { View, Text, TouchableOpacity, StyleSheet, Modal } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '../contexts/ThemeContext';
import { useLocalization } from '../hooks/useLocalization';

const ThemeSelector = () => {
  const { themeMode, setTheme, colors, spacing, borderRadius, fontSize } = useTheme();
  const { t } = useLocalization();
  const [modalVisible, setModalVisible] = useState(false);

  const themeOptions = [
    { 
      key: 'light', 
      name: t('settings.light'), 
      icon: 'sunny-outline',
      description: 'Light theme for better visibility in bright environments'
    },
    { 
      key: 'dark', 
      name: t('settings.dark'), 
      icon: 'moon-outline',
      description: 'Dark theme for reduced eye strain in low light'
    },
    { 
      key: 'system', 
      name: t('settings.system'), 
      icon: 'phone-portrait-outline',
      description: 'Automatically match your device settings'
    }
  ];

  const handleThemeChange = async (theme) => {
    await setTheme(theme);
    setModalVisible(false);
  };

  const getCurrentThemeName = () => {
    const theme = themeOptions.find(option => option.key === themeMode);
    return theme ? theme.name : t('settings.light');
  };

  const getCurrentThemeIcon = () => {
    const theme = themeOptions.find(option => option.key === themeMode);
    return theme ? theme.icon : 'sunny-outline';
  };

  const styles = StyleSheet.create({
    container: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingVertical: spacing.md,
      paddingHorizontal: spacing.md,
      backgroundColor: colors.card,
      borderBottomWidth: 1,
      borderBottomColor: colors.border,
    },
    content: {
      flexDirection: 'row',
      alignItems: 'center',
      flex: 1,
    },
    iconContainer: {
      width: 40,
      height: 40,
      borderRadius: 20,
      backgroundColor: colors.surface,
      justifyContent: 'center',
      alignItems: 'center',
      marginRight: spacing.md,
    },
    textContainer: {
      flex: 1,
    },
    title: {
      fontSize: fontSize.md,
      fontWeight: '600',
      color: colors.text,
      marginBottom: 4,
    },
    subtitle: {
      fontSize: fontSize.sm,
      color: colors.textSecondary,
    },
    modalOverlay: {
      flex: 1,
      backgroundColor: colors.overlay,
      justifyContent: 'center',
      alignItems: 'center',
    },
    modalContent: {
      backgroundColor: colors.card,
      borderRadius: borderRadius.lg,
      padding: spacing.lg,
      width: '80%',
      maxWidth: 300,
    },
    modalTitle: {
      fontSize: fontSize.lg,
      fontWeight: '600',
      color: colors.text,
      textAlign: 'center',
      marginBottom: spacing.lg,
    },
    themeOption: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingVertical: spacing.md,
      paddingHorizontal: spacing.sm,
      borderRadius: borderRadius.md,
      marginBottom: spacing.sm,
    },
    themeOptionSelected: {
      backgroundColor: colors.surface,
    },
    themeIconContainer: {
      width: 40,
      height: 40,
      borderRadius: 20,
      backgroundColor: colors.background,
      justifyContent: 'center',
      alignItems: 'center',
      marginRight: spacing.md,
    },
    themeTextContainer: {
      flex: 1,
    },
    themeText: {
      fontSize: fontSize.md,
      color: colors.text,
      fontWeight: '500',
    },
    themeDescription: {
      fontSize: fontSize.xs,
      color: colors.textSecondary,
      marginTop: 2,
    },
    closeButton: {
      marginTop: spacing.lg,
      paddingVertical: spacing.md,
      backgroundColor: colors.primary,
      borderRadius: borderRadius.md,
      alignItems: 'center',
    },
    closeButtonText: {
      color: colors.card,
      fontSize: fontSize.md,
      fontWeight: '600',
    },
  });

  return (
    <>
      <TouchableOpacity 
        style={styles.container} 
        onPress={() => setModalVisible(true)}
      >
        <View style={styles.content}>
          <View style={styles.iconContainer}>
            <Ionicons name={getCurrentThemeIcon()} size={20} color={colors.primary} />
          </View>
          <View style={styles.textContainer}>
            <Text style={styles.title}>{t('settings.theme')}</Text>
            <Text style={styles.subtitle}>{getCurrentThemeName()}</Text>
          </View>
        </View>
        <Ionicons name="chevron-forward" size={20} color={colors.textLight} />
      </TouchableOpacity>

      <Modal
        animationType="fade"
        transparent={true}
        visible={modalVisible}
        onRequestClose={() => setModalVisible(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>{t('settings.theme')}</Text>
            
            {themeOptions.map((option) => (
              <TouchableOpacity
                key={option.key}
                style={[
                  styles.themeOption,
                  themeMode === option.key && styles.themeOptionSelected
                ]}
                onPress={() => handleThemeChange(option.key)}
              >
                <View style={styles.themeIconContainer}>
                  <Ionicons name={option.icon} size={20} color={colors.primary} />
                </View>
                <View style={styles.themeTextContainer}>
                  <Text style={styles.themeText}>{option.name}</Text>
                  <Text style={styles.themeDescription}>{option.description}</Text>
                </View>
                {themeMode === option.key && (
                  <Ionicons name="checkmark" size={20} color={colors.primary} />
                )}
              </TouchableOpacity>
            ))}

            <TouchableOpacity
              style={styles.closeButton}
              onPress={() => setModalVisible(false)}
            >
              <Text style={styles.closeButtonText}>{t('common.close')}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>
    </>
  );
};

export default ThemeSelector;
