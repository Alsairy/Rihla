import { getLocales, getNumberFormatSettings } from 'react-native-localize';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { I18nManager } from 'react-native';

import en from './translations/en.json';
import ar from './translations/ar.json';

const LANGUAGE_KEY = 'selected_language';

class LocalizationService {
  constructor() {
    this.translations = { en, ar };
    this.currentLanguage = 'en';
    this.isRTL = false;
    this.listeners = [];
  }

  async initialize() {
    try {
      const savedLanguage = await AsyncStorage.getItem(LANGUAGE_KEY);
      
      if (savedLanguage && this.translations[savedLanguage]) {
        this.currentLanguage = savedLanguage;
      } else {
        const locales = getLocales();
        const deviceLanguage = locales[0]?.languageCode;
        
        if (deviceLanguage && this.translations[deviceLanguage]) {
          this.currentLanguage = deviceLanguage;
        }
      }

      this.isRTL = this.currentLanguage === 'ar';
      
      I18nManager.allowRTL(true);
      I18nManager.forceRTL(this.isRTL);

      console.log(`Localization initialized: ${this.currentLanguage}, RTL: ${this.isRTL}`);
      return true;
    } catch (error) {
      console.error('Error initializing localization:', error);
      return false;
    }
  }

  async setLanguage(languageCode) {
    if (!this.translations[languageCode]) {
      console.warn(`Language ${languageCode} not supported`);
      return false;
    }

    try {
      const previousLanguage = this.currentLanguage;
      const previousRTL = this.isRTL;

      this.currentLanguage = languageCode;
      this.isRTL = languageCode === 'ar';

      await AsyncStorage.setItem(LANGUAGE_KEY, languageCode);

      if (previousRTL !== this.isRTL) {
        I18nManager.forceRTL(this.isRTL);
        
        this.notifyListeners({
          type: 'rtl_changed',
          language: languageCode,
          isRTL: this.isRTL,
          requiresRestart: true
        });
      } else {
        this.notifyListeners({
          type: 'language_changed',
          language: languageCode,
          isRTL: this.isRTL,
          requiresRestart: false
        });
      }

      return true;
    } catch (error) {
      console.error('Error setting language:', error);
      return false;
    }
  }

  translate(key, params = {}) {
    const translation = this.getNestedTranslation(this.translations[this.currentLanguage], key);
    
    if (!translation) {
      const fallback = this.getNestedTranslation(this.translations.en, key);
      if (!fallback) {
        console.warn(`Translation missing for key: ${key}`);
        return key;
      }
      return this.interpolate(fallback, params);
    }

    return this.interpolate(translation, params);
  }

  getNestedTranslation(obj, key) {
    return key.split('.').reduce((o, k) => (o && o[k]) ? o[k] : null, obj);
  }

  interpolate(template, params) {
    return template.replace(/\{\{(\w+)\}\}/g, (match, key) => {
      return params[key] !== undefined ? params[key] : match;
    });
  }

  getCurrentLanguage() {
    return this.currentLanguage;
  }

  getAvailableLanguages() {
    return [
      { code: 'en', name: 'English', nativeName: 'English' },
      { code: 'ar', name: 'Arabic', nativeName: 'العربية' }
    ];
  }

  isRTLLanguage() {
    return this.isRTL;
  }

  getNumberFormat() {
    const settings = getNumberFormatSettings();
    return {
      decimalSeparator: settings.decimalSeparator,
      groupingSeparator: settings.groupingSeparator
    };
  }

  formatNumber(number, options = {}) {
    const { decimalPlaces = 2, useGrouping = true } = options;
    
    if (this.currentLanguage === 'ar') {
      return new Intl.NumberFormat('ar-SA', {
        minimumFractionDigits: decimalPlaces,
        maximumFractionDigits: decimalPlaces,
        useGrouping
      }).format(number);
    } else {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: decimalPlaces,
        maximumFractionDigits: decimalPlaces,
        useGrouping
      }).format(number);
    }
  }

  formatCurrency(amount, currency = 'SAR') {
    if (this.currentLanguage === 'ar') {
      return new Intl.NumberFormat('ar-SA', {
        style: 'currency',
        currency: currency
      }).format(amount);
    } else {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
      }).format(amount);
    }
  }

  formatDate(date, options = {}) {
    const defaultOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    };

    const formatOptions = { ...defaultOptions, ...options };

    if (this.currentLanguage === 'ar') {
      return new Intl.DateTimeFormat('ar-SA', formatOptions).format(date);
    } else {
      return new Intl.DateTimeFormat('en-US', formatOptions).format(date);
    }
  }

  addListener(callback) {
    this.listeners.push(callback);
    
    return () => {
      this.listeners = this.listeners.filter(listener => listener !== callback);
    };
  }

  notifyListeners(event) {
    this.listeners.forEach(callback => {
      try {
        callback(event);
      } catch (error) {
        console.error('Error in localization listener:', error);
      }
    });
  }

  t(key, params) {
    return this.translate(key, params);
  }
}

export default new LocalizationService();
