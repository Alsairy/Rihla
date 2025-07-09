import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';

export const useLocalization = () => {
  const { t, i18n } = useTranslation();
  const [isRTL, setIsRTL] = useState(i18n.language === 'ar');

  useEffect(() => {
    const handleLanguageChange = (lng: string) => {
      setIsRTL(lng === 'ar');
    };

    i18n.on('languageChanged', handleLanguageChange);
    
    return () => {
      i18n.off('languageChanged', handleLanguageChange);
    };
  }, [i18n]);

  const changeLanguage = (languageCode: string) => {
    return i18n.changeLanguage(languageCode);
  };

  const formatCurrency = (amount: number, currency: string = 'SAR') => {
    if (i18n.language === 'ar') {
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
  };

  const formatDate = (date: Date, options?: Intl.DateTimeFormatOptions) => {
    const defaultOptions: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    };

    const formatOptions = { ...defaultOptions, ...options };

    if (i18n.language === 'ar') {
      return new Intl.DateTimeFormat('ar-SA', formatOptions).format(date);
    } else {
      return new Intl.DateTimeFormat('en-US', formatOptions).format(date);
    }
  };

  const formatNumber = (number: number, options?: Intl.NumberFormatOptions) => {
    if (i18n.language === 'ar') {
      return new Intl.NumberFormat('ar-SA', options).format(number);
    } else {
      return new Intl.NumberFormat('en-US', options).format(number);
    }
  };

  return {
    t,
    i18n,
    isRTL,
    currentLanguage: i18n.language,
    changeLanguage,
    formatCurrency,
    formatDate,
    formatNumber
  };
};

export default useLocalization;
