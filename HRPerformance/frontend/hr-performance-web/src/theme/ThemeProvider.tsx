import { useMemo, useState, useCallback, useEffect, createContext, useContext } from 'react';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { CacheProvider } from '@emotion/react';
import createCache from '@emotion/cache';
import { prefixer } from 'stylis';
import rtlPlugin from 'stylis-plugin-rtl';
import { createAppTheme, type AppThemeMode } from './theme';

interface ThemeContextValue {
  mode: AppThemeMode;
  toggleTheme: () => void;
  setMode: (mode: AppThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

const rtlCache = createCache({
  key: 'muirtl',
  stylisPlugins: [prefixer, rtlPlugin],
});

const THEME_STORAGE_KEY = 'hr-performance-theme';

export function AppThemeProvider({ children }: { children: React.ReactNode }) {
  const [mode, setModeState] = useState<AppThemeMode>(() => {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    return stored === 'dark' || stored === 'light' ? stored : 'light';
  });

  const theme = useMemo(() => createAppTheme(mode), [mode]);

  const setMode = useCallback((newMode: AppThemeMode) => {
    setModeState(newMode);
    localStorage.setItem(THEME_STORAGE_KEY, newMode);
  }, []);

  const toggleTheme = useCallback(() => {
    setMode(mode === 'light' ? 'dark' : 'light');
  }, [mode, setMode]);

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', mode);
  }, [mode]);

  const value = useMemo(
    () => ({ mode, toggleTheme, setMode }),
    [mode, toggleTheme, setMode],
  );

  return (
    <ThemeContext.Provider value={value}>
      <CacheProvider value={rtlCache}>
        <MuiThemeProvider theme={theme}>
          <CssBaseline />
          {children}
        </MuiThemeProvider>
      </CacheProvider>
    </ThemeContext.Provider>
  );
}

export function useAppTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useAppTheme must be used within AppThemeProvider');
  }
  return context;
}
