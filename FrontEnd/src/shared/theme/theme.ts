import { extendTheme } from '@chakra-ui/react';

const theme = extendTheme({
  colors: {
    brand: {
      50: '#eef6ff',
      100: '#d9ebff',
      200: '#add3ff',
      300: '#7fb8ff',
      400: '#4091ff',
      500: '#1769e0',
      600: '#0f55b8',
      700: '#0f438d',
      800: '#123a73',
      900: '#10284f',
    },
    brandScheme: {
      50: '#eef6ff',
      100: '#d9ebff',
      400: '#4091ff',
      500: '#1769e0',
      600: '#0f55b8',
      700: '#0f438d',
      900: '#10284f',
    },
    secondaryGray: {
      50: '#f8fafc',
      100: '#f1f5f9',
      200: '#e2e8f0',
      300: '#cbd5e1',
      400: '#94a3b8',
      500: '#64748b',
      600: '#475569',
      700: '#334155',
      800: '#1e293b',
      900: '#0f172a',
    },
    navy: {
      700: '#172033',
      800: '#111827',
      900: '#0b1120',
    },
  },
  fonts: {
    heading: 'Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
    body: 'Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
  },
  styles: {
    global: {
      body: {
        overflowX: 'hidden',
        bg: 'secondaryGray.50',
        color: 'secondaryGray.900',
        letterSpacing: '0',
      },
      option: {
        color: 'black',
      },
      '*:focus-visible': {
        boxShadow: '0 0 0 3px rgba(23, 105, 224, 0.28) !important',
        outline: 'none',
      },
    },
  },
  radii: {
    sm: '6px',
    md: '8px',
    lg: '8px',
    xl: '8px',
    '2xl': '8px',
  },
  components: {
    Button: {
      baseStyle: {
        borderRadius: '8px',
        fontWeight: '700',
      },
      variants: {
        brand: {
          bg: 'brand.500',
          color: 'white',
          _hover: { bg: 'brand.600', _disabled: { bg: 'brand.500' } },
          _active: { bg: 'brand.700' },
        },
        outline: {
          borderColor: 'secondaryGray.200',
        },
        'no-hover': {
          _hover: { bg: 'transparent' },
        },
      },
    },
    Input: {
      variants: {
        auth: {
          field: {
            borderRadius: '8px',
            borderColor: 'secondaryGray.200',
            _focus: { borderColor: 'brand.500', boxShadow: '0 0 0 1px var(--chakra-colors-brand-500)' },
          },
        },
      },
    },
    Card: {
      baseStyle: {
        container: {
          borderRadius: '8px',
        },
      },
    },
  },
});

export default theme;
