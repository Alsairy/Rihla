import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import enTranslations from './translations/en.json';
import arTranslations from './translations/ar.json';

const resources = {
  en: {
    translation: enTranslations,
  },
  ar: {
    translation: arTranslations,
  },
};

i18n.use(initReactI18next).init({
  resources,
  lng: 'en', // default language
  fallbackLng: 'en',
  debug: false, // Set to false for production, can be enabled manually for development

  interpolation: {
    escapeValue: false, // React already does escaping
  },

  react: {
    useSuspense: false,
  },

  detection: {
    order: ['localStorage', 'navigator', 'htmlTag'],
    caches: ['localStorage'],
    lookupLocalStorage: 'i18nextLng',
  },
});

i18n.on('languageChanged', (lng) => {
  const isRTL = lng === 'ar';
  document.documentElement.dir = isRTL ? 'rtl' : 'ltr';
  document.documentElement.lang = lng;

  document.documentElement.style.setProperty(
    '--text-direction',
    isRTL ? 'rtl' : 'ltr'
  );
});

export default i18n;
