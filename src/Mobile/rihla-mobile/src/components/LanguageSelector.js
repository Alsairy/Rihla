import React, { useState } from 'react';
import { View, Text, TouchableOpacity, StyleSheet, Modal, Alert } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useLocalization } from '../hooks/useLocalization';
import { useTheme } from '../contexts/ThemeContext';

const LanguageSelector = () => {
  const { currentLanguage, getAvailableLanguages, changeLanguage, isLoading, t } = useLocalization();
  const { colors, spacing, borderRadius, fontSize } = useTheme();
  const [modalVisible, setModalVisible] = useState(false);

  const languages = getAvailableLanguages();

  const handleLanguageChange = async (languageCode) => {
    if (languageCode === currentLanguage) {
      setModalVisible(false);
      return;
    }

    try {
      const success = await changeLanguage(languageCode);
      if (success) {
        setModalVisible(false);
        
        if (languageCode === 'ar' || currentLanguage === 'ar') {
          Alert.alert(
            t('settings.restartRequired'),
            t('settings.restartRequired'),
            [{ text: t('common.ok') }]
          );
        }
      } else {
        Alert.alert(t('common.error'), 'Failed to change language');
      }
    } catch (error) {
      console.error('Error changing language:', error);
      Alert.alert(t('common.error'), 'Failed to change language');
    }
  };

  const getCurrentLanguageName = () => {
    const language = languages.find(lang => lang.code === currentLanguage);
    return language ? language.nativeName : 'English';
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
    languageOption: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingVertical: spacing.md,
      paddingHorizontal: spacing.sm,
      borderRadius: borderRadius.md,
      marginBottom: spacing.sm,
    },
    languageOptionSelected: {
      backgroundColor: colors.surface,
    },
    languageText: {
      fontSize: fontSize.md,
      color: colors.text,
      flex: 1,
    },
    languageNativeText: {
      fontSize: fontSize.sm,
      color: colors.textSecondary,
      marginLeft: spacing.sm,
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
        disabled={isLoading}
      >
        <View style={styles.content}>
          <View style={styles.iconContainer}>
            <Ionicons name="language-outline" size={20} color={colors.primary} />
          </View>
          <View style={styles.textContainer}>
            <Text style={styles.title}>{t('settings.language')}</Text>
            <Text style={styles.subtitle}>{getCurrentLanguageName()}</Text>
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
            <Text style={styles.modalTitle}>{t('settings.language')}</Text>
            
            {languages.map((language) => (
              <TouchableOpacity
                key={language.code}
                style={[
                  styles.languageOption,
                  currentLanguage === language.code && styles.languageOptionSelected
                ]}
                onPress={() => handleLanguageChange(language.code)}
                disabled={isLoading}
              >
                <Text style={styles.languageText}>{language.name}</Text>
                <Text style={styles.languageNativeText}>{language.nativeName}</Text>
                {currentLanguage === language.code && (
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

export default LanguageSelector;
