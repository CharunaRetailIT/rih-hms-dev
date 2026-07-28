import { ImageResponse } from 'next/og';

export const alt = 'RIT HMS — Hospitality Management by Retail IT';
export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';

/** Branded social-share card (Open Graph + Twitter). */
export default function OpengraphImage() {
  return new ImageResponse(
    (
      <div
        style={{
          width: '100%', height: '100%', display: 'flex', flexDirection: 'column',
          alignItems: 'flex-start', justifyContent: 'center', gap: 24,
          background: 'linear-gradient(135deg, #0f3d23 0%, #15803d 100%)',
          color: '#ffffff', padding: 90, fontFamily: 'sans-serif',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', fontSize: 96, fontWeight: 800, letterSpacing: -3 }}>
          <span>RETAIL</span>
          <span style={{ color: '#ffc329', marginLeft: 26 }}>IT</span>
        </div>
        <div style={{ fontSize: 46, fontWeight: 700 }}>RIT HMS — Hospitality Management</div>
        <div style={{ fontSize: 30, color: '#c9ecd5' }}>POS · Kitchen · Inventory · Delivery · Accounting · Loyalty</div>
      </div>
    ),
    { ...size },
  );
}
