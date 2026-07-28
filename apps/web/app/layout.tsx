import type { Metadata, Viewport } from 'next';
import { Hanken_Grotesk, Inter } from 'next/font/google';
import './globals.css';
import { ConfirmHost, PromptHost } from '@/components/ui/confirm';

const heading = Hanken_Grotesk({
  subsets: ['latin'],
  weight: ['400', '600', '700', '800'],
  variable: '--font-heading',
  display: 'swap',
});

const body = Inter({
  subsets: ['latin'],
  weight: ['400', '500', '600'],
  variable: '--font-body',
  display: 'swap',
});

export const metadata: Metadata = {
  metadataBase: new URL('https://hms.retailit.lk'),
  applicationName: 'RIT HMS',
  title: { default: 'RIT HMS — Hospitality Management', template: '%s · RIT HMS' },
  description: 'Run your restaurant end to end — POS, kitchen, inventory, delivery, accounting and loyalty. By Retail IT.',
  keywords: ['restaurant POS', 'hospitality management', 'Sri Lanka', 'Retail IT', 'KOT', 'inventory', 'Uber Eats', 'PickMe', 'loyalty'],
  authors: [{ name: 'Retail IT' }],
  openGraph: {
    type: 'website',
    siteName: 'RIT HMS',
    url: 'https://hms.retailit.lk',
    title: 'RIT HMS — Hospitality Management',
    description: 'Run your restaurant end to end — POS, kitchen, inventory, delivery, accounting and loyalty. By Retail IT.',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'RIT HMS — Hospitality Management',
    description: 'Run your restaurant end to end — POS, kitchen, inventory, delivery, accounting and loyalty.',
  },
};

export const viewport: Viewport = {
  themeColor: '#15803d',
  width: 'device-width',
  initialScale: 1,
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`light ${heading.variable} ${body.variable}`}>
      <head>
        {/* Material Symbols Outlined — icon font used across the Stitch designs */}
        <link
          rel="stylesheet"
          href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=block"
        />
      </head>
      <body className="font-sans">
        {children}
        <ConfirmHost />
        <PromptHost />
      </body>
    </html>
  );
}
