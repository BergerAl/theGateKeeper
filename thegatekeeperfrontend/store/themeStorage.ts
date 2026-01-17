import { themeStyle } from '../context/themes';

const THEME_STORAGE_KEY = 'gatekeeper-theme';

export const loadThemeFromStorage = (): themeStyle | null => {
  if (typeof window === 'undefined') return null;
  
  try {
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);
    return (savedTheme as themeStyle) || null;
  } catch (error) {
    console.warn('Failed to load theme from localStorage:', error);
    return null;
  }
};

export const saveThemeToStorage = (theme: themeStyle): void => {
  if (typeof window === 'undefined') return;
  
  try {
    localStorage.setItem(THEME_STORAGE_KEY, theme);
  } catch (error) {
    console.warn('Failed to save theme to localStorage:', error);
  }
};
