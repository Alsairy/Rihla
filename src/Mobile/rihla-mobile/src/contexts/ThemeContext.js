import React, { createContext, useContext, useState, useEffect } from 'react';
import { Appearance, StatusBar } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

const THEME_KEY = 'selected_theme';

const lightTheme = {
  name: 'light',
  colors: {
    primary: '#2563eb',
    primaryDark: '#1d4ed8',
    primaryLight: '#3b82f6',
    secondary: '#64748b',
    background: '#ffffff',
    surface: '#f8fafc',
    card: '#ffffff',
    text: '#1e293b',
    textSecondary: '#64748b',
    textLight: '#94a3b8',
    border: '#e2e8f0',
    borderLight: '#f1f5f9',
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6',
    overlay: 'rgba(0, 0, 0, 0.5)',
    shadow: 'rgba(0, 0, 0, 0.1)',
    disabled: '#d1d5db',
    placeholder: '#9ca3af',
    notification: '#ef4444',
    accent: '#8b5cf6',
    highlight: '#fef3c7',
    gradientStart: '#2563eb',
    gradientEnd: '#3b82f6'
  },
  spacing: {
    xs: 4,
    sm: 8,
    md: 16,
    lg: 24,
    xl: 32,
    xxl: 48
  },
  borderRadius: {
    sm: 4,
    md: 8,
    lg: 12,
    xl: 16,
    full: 9999
  },
  fontSize: {
    xs: 12,
    sm: 14,
    md: 16,
    lg: 18,
    xl: 20,
    xxl: 24,
    xxxl: 32
  },
  fontWeight: {
    normal: '400',
    medium: '500',
    semibold: '600',
    bold: '700'
  },
  shadows: {
    sm: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.05,
      shadowRadius: 2,
      elevation: 1
    },
    md: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 2 },
      shadowOpacity: 0.1,
      shadowRadius: 4,
      elevation: 3
    },
    lg: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.15,
      shadowRadius: 8,
      elevation: 5
    }
  }
};

const darkTheme = {
  ...lightTheme,
  name: 'dark',
  colors: {
    primary: '#3b82f6',
    primaryDark: '#2563eb',
    primaryLight: '#60a5fa',
    secondary: '#94a3b8',
    background: '#0f172a',
    surface: '#1e293b',
    card: '#334155',
    text: '#f8fafc',
    textSecondary: '#cbd5e1',
    textLight: '#94a3b8',
    border: '#475569',
    borderLight: '#334155',
    success: '#22c55e',
    warning: '#fbbf24',
    error: '#f87171',
    info: '#60a5fa',
    overlay: 'rgba(0, 0, 0, 0.7)',
    shadow: 'rgba(0, 0, 0, 0.3)',
    disabled: '#64748b',
    placeholder: '#64748b',
    notification: '#f87171',
    accent: '#a78bfa',
    highlight: '#374151',
    gradientStart: '#3b82f6',
    gradientEnd: '#60a5fa'
  }
};

const ThemeContext = createContext({
  theme: lightTheme,
  isDark: false,
  toggleTheme: () => {},
  setTheme: () => {},
  themeMode: 'light'
});

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

export const ThemeProvider = ({ children }) => {
  const [themeMode, setThemeMode] = useState('system'); // 'light', 'dark', 'system'
  const [isDark, setIsDark] = useState(false);
  const [theme, setTheme] = useState(lightTheme);

  useEffect(() => {
    initializeTheme();
    
    const subscription = Appearance.addChangeListener(({ colorScheme }) => {
      if (themeMode === 'system') {
        const systemIsDark = colorScheme === 'dark';
        setIsDark(systemIsDark);
        setTheme(systemIsDark ? darkTheme : lightTheme);
        updateStatusBar(systemIsDark);
      }
    });

    return () => subscription?.remove();
  }, [themeMode]);

  const initializeTheme = async () => {
    try {
      const savedTheme = await AsyncStorage.getItem(THEME_KEY);
      const initialThemeMode = savedTheme || 'system';
      
      setThemeMode(initialThemeMode);
      
      if (initialThemeMode === 'system') {
        const systemColorScheme = Appearance.getColorScheme();
        const systemIsDark = systemColorScheme === 'dark';
        setIsDark(systemIsDark);
        setTheme(systemIsDark ? darkTheme : lightTheme);
        updateStatusBar(systemIsDark);
      } else {
        const isThemeDark = initialThemeMode === 'dark';
        setIsDark(isThemeDark);
        setTheme(isThemeDark ? darkTheme : lightTheme);
        updateStatusBar(isThemeDark);
      }
    } catch (error) {
      console.error('Error initializing theme:', error);
      setIsDark(false);
      setTheme(lightTheme);
      updateStatusBar(false);
    }
  };

  const updateStatusBar = (dark) => {
    StatusBar.setBarStyle(dark ? 'light-content' : 'dark-content', true);
  };

  const changeTheme = async (newThemeMode) => {
    try {
      setThemeMode(newThemeMode);
      await AsyncStorage.setItem(THEME_KEY, newThemeMode);

      if (newThemeMode === 'system') {
        const systemColorScheme = Appearance.getColorScheme();
        const systemIsDark = systemColorScheme === 'dark';
        setIsDark(systemIsDark);
        setTheme(systemIsDark ? darkTheme : lightTheme);
        updateStatusBar(systemIsDark);
      } else {
        const isThemeDark = newThemeMode === 'dark';
        setIsDark(isThemeDark);
        setTheme(isThemeDark ? darkTheme : lightTheme);
        updateStatusBar(isThemeDark);
      }
    } catch (error) {
      console.error('Error changing theme:', error);
    }
  };

  const toggleTheme = () => {
    const newThemeMode = isDark ? 'light' : 'dark';
    changeTheme(newThemeMode);
  };

  const value = {
    theme,
    isDark,
    themeMode,
    toggleTheme,
    setTheme: changeTheme,
    colors: theme.colors,
    spacing: theme.spacing,
    borderRadius: theme.borderRadius,
    fontSize: theme.fontSize,
    fontWeight: theme.fontWeight,
    shadows: theme.shadows
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
};

export default ThemeContext;
