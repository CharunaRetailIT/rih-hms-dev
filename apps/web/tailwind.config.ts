import type { Config } from 'tailwindcss';

/**
 * RIT HMS design tokens — aligned to the Stitch design output
 * (project 5754133516877607841). Primary green #15803d, Hanken Grotesk
 * headlines, Inter body, Material Symbols Outlined icons.
 */
const config: Config = {
  darkMode: 'class',
  content: [
    './app/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#15803d',          // green (Stitch)
          foreground: '#ffffff',
          dark: '#00652c',
          tint: '#d3ffd5',             // primary-container
        },
        'on-primary': '#ffffff',
        'primary-container': '#d3ffd5',
        secondary: {
          DEFAULT: '#795900',
          foreground: '#ffffff',
          tint: '#ffc329',             // secondary-container
        },
        'secondary-container': '#ffc329',
        accent: { DEFAULT: '#1E40AF', foreground: '#ffffff' },
        // surfaces (Stitch)
        surface: '#f8f9ff',
        'surface-container': '#e6eeff',
        background: '#f8f9ff',
        card: '#ffffff',
        foreground: '#121c2a',         // on-surface
        'on-surface': '#121c2a',
        outline: '#6f7a6e',
        muted: { DEFAULT: '#F1F5F9', foreground: '#475569' },
        border: '#E2E8F0',
        error: '#ba1a1a',
        // status palette (kept)
        status: {
          paid: '#16A34A', ready: '#16A34A', pending: '#F59E0B', preparing: '#F59E0B',
          void: '#DC2626', error: '#DC2626', progress: '#1E40AF', delivery: '#1E40AF', idle: '#94A3B8',
        },
        slate: {
          50: '#F8FAFC', 100: '#F1F5F9', 200: '#E2E8F0', 300: '#CBD5E1', 400: '#94A3B8',
          500: '#64748B', 600: '#475569', 700: '#334155', 800: '#1E293B', 900: '#0F172A',
        },
      },
      fontFamily: {
        heading: ['var(--font-heading)', 'Hanken Grotesk', 'sans-serif'],
        headline: ['var(--font-heading)', 'Hanken Grotesk', 'sans-serif'],
        sans: ['var(--font-body)', 'Inter', 'sans-serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'monospace'],
      },
      borderRadius: { sm: '0.25rem', DEFAULT: '0.25rem', md: '0.375rem', lg: '0.5rem', xl: '0.75rem', full: '9999px' },
    },
  },
  plugins: [],
};
export default config;
