import { useState, useEffect } from 'react';
import localizationService from '../localization';

export const useLocalization = () => {
  const [currentLanguage, setCurrentLanguage] = useState(localizationService.getCurrentLanguage());
  const [isRTL, setIsRTL] = useState(localizationService.isRTLLanguage());
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const unsubscribe = localizationService.addListener((event) => {
      if (event.type === 'language_changed' || event.type === 'rtl_changed') {
        setCurrentLanguage(event.language);
        setIsRTL(event.isRTL);
        
        if (event.requiresRestart) {
          console.log('App restart required for RTL changes');
        }
      }
    });

    return unsubscribe;
  }, []);

  const changeLanguage = async (languageCode) => {
    setIsLoading(true);
    try {
      const success = await localizationService.setLanguage(languageCode);
      return success;
    } catch (error) {
      console.error('Error changing language:', error);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const t = (key, params) => {
    return localizationService.translate(key, params);
  };

  const formatNumber = (number, options) => {
    return localizationService.formatNumber(number, options);
  };

  const formatCurrency = (amount, currency) => {
    return localizationService.formatCurrency(amount, currency);
  };

  const formatDate = (date, options) => {
    return localizationService.formatDate(date, options);
  };

  const getAvailableLanguages = () => {
    return localizationService.getAvailableLanguages();
  };

  return {
    currentLanguage,
    isRTL,
    isLoading,
    t,
    changeLanguage,
    formatNumber,
    formatCurrency,
    formatDate,
    getAvailableLanguages
  };
};

export default useLocalization;
