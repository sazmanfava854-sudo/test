import { createTheme, type ThemeOptions } from '@mui/material/styles';
import { alpha } from '@mui/material/styles';

declare module '@mui/material/styles' {
  interface Theme {
    glass: {
      background: string;
      border: string;
      shadow: string;
      blur: string;
    };
  }
  interface ThemeOptions {
    glass?: {
      background: string;
      border: string;
      shadow: string;
      blur: string;
    };
  }
}

const brandPrimary = '#0065FF';
const brandSecondary = '#6554C0';

const getGlassStyles = (mode: 'light' | 'dark') => ({
  background:
    mode === 'light'
      ? 'rgba(255, 255, 255, 0.72)'
      : 'rgba(22, 27, 34, 0.75)',
  border:
    mode === 'light'
      ? '1px solid rgba(255, 255, 255, 0.45)'
      : '1px solid rgba(255, 255, 255, 0.08)',
  shadow:
    mode === 'light'
      ? '0 8px 32px rgba(0, 65, 255, 0.08), 0 2px 8px rgba(0, 0, 0, 0.04)'
      : '0 8px 32px rgba(0, 0, 0, 0.4), 0 2px 8px rgba(0, 65, 255, 0.1)',
  blur: 'blur(16px)',
});

const baseThemeOptions = (mode: 'light' | 'dark'): ThemeOptions => {
  const isLight = mode === 'light';
  const glass = getGlassStyles(mode);

  return {
    direction: 'rtl',
    palette: {
      mode,
      primary: {
        main: brandPrimary,
        light: '#4C9AFF',
        dark: '#0747A6',
        contrastText: '#FFFFFF',
      },
      secondary: {
        main: brandSecondary,
        light: '#8777D9',
        dark: '#403294',
        contrastText: '#FFFFFF',
      },
      background: {
        default: isLight ? '#F4F5F7' : '#0D1117',
        paper: isLight ? '#FFFFFF' : '#161B22',
      },
      text: {
        primary: isLight ? '#172B4D' : '#E6EDF3',
        secondary: isLight ? '#5E6C84' : '#8B949E',
      },
      divider: isLight ? alpha('#091E42', 0.08) : alpha('#FFFFFF', 0.08),
      success: { main: '#36B37E' },
      warning: { main: '#FFAB00' },
      error: { main: '#FF5630' },
      info: { main: '#00B8D9' },
    },
    typography: {
      fontFamily: '"Vazirmatn", "Roboto", "Helvetica", "Arial", sans-serif',
      h1: { fontWeight: 700, fontSize: '2rem' },
      h2: { fontWeight: 700, fontSize: '1.75rem' },
      h3: { fontWeight: 600, fontSize: '1.5rem' },
      h4: { fontWeight: 600, fontSize: '1.25rem' },
      h5: { fontWeight: 600, fontSize: '1.1rem' },
      h6: { fontWeight: 600, fontSize: '1rem' },
      subtitle1: { fontWeight: 500 },
      subtitle2: { fontWeight: 500, fontSize: '0.875rem' },
      body1: { fontSize: '0.9375rem' },
      body2: { fontSize: '0.875rem' },
      button: { fontWeight: 600, textTransform: 'none' as const },
    },
    shape: { borderRadius: 12 },
    glass,
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            scrollbarWidth: 'thin',
            '&::-webkit-scrollbar': { width: 8, height: 8 },
            '&::-webkit-scrollbar-thumb': {
              backgroundColor: isLight ? alpha('#091E42', 0.2) : alpha('#FFFFFF', 0.15),
              borderRadius: 4,
            },
          },
        },
      },
      MuiButton: {
        styleOverrides: {
          root: {
            borderRadius: 10,
            padding: '8px 20px',
            boxShadow: 'none',
            '&:hover': { boxShadow: 'none' },
            '&.MuiButton-containedPrimary': {
              background: `linear-gradient(135deg, ${brandPrimary} 0%, #0747A6 100%)`,
            },
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            borderRadius: 16,
            border: glass.border,
            boxShadow: glass.shadow,
            backdropFilter: glass.blur,
            WebkitBackdropFilter: glass.blur,
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
          },
        },
      },
      MuiDrawer: {
        styleOverrides: {
          paper: {
            backgroundImage: 'none',
            borderLeft: glass.border,
            backdropFilter: glass.blur,
            WebkitBackdropFilter: glass.blur,
          },
        },
      },
      MuiAppBar: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            backdropFilter: glass.blur,
            WebkitBackdropFilter: glass.blur,
          },
        },
      },
      MuiTextField: {
        defaultProps: { variant: 'outlined' },
      },
    },
  };
};

export const createAppTheme = (mode: 'light' | 'dark') =>
  createTheme(baseThemeOptions(mode));

export type AppThemeMode = 'light' | 'dark';

export const glassCardSx = (theme: ReturnType<typeof createAppTheme>) => ({
  background: theme.glass.background,
  border: theme.glass.border,
  boxShadow: theme.glass.shadow,
  backdropFilter: theme.glass.blur,
  WebkitBackdropFilter: theme.glass.blur,
  borderRadius: 3,
});
