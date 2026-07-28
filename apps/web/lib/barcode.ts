/**
 * Self-contained EAN-13 barcode generation + SVG rendering (no external lib, works offline).
 * Used by the Menu screen to optionally system-generate a scannable barcode for a product
 * and print a label.
 */

// Left-hand "odd" (A / L) encodings.
const A: Record<string, string> = {
  '0': '0001101', '1': '0011001', '2': '0010011', '3': '0111101', '4': '0100011',
  '5': '0110001', '6': '0101111', '7': '0111011', '8': '0110111', '9': '0001011',
};
// Left-hand "even" (B / G) encodings.
const B: Record<string, string> = {
  '0': '0100111', '1': '0110011', '2': '0011011', '3': '0100001', '4': '0011101',
  '5': '0111001', '6': '0000101', '7': '0010001', '8': '0001001', '9': '0010111',
};
// Right-hand (C / R) encodings.
const C: Record<string, string> = {
  '0': '1110010', '1': '1100110', '2': '1101100', '3': '1000010', '4': '1011100',
  '5': '1001110', '6': '1010000', '7': '1000100', '8': '1001000', '9': '1110100',
};
// First digit → A/B parity pattern for the left group.
const PARITY: Record<string, string> = {
  '0': 'AAAAAA', '1': 'AABABB', '2': 'AABBAB', '3': 'AABBBA', '4': 'ABAABB',
  '5': 'ABBAAB', '6': 'ABBBAA', '7': 'ABABAB', '8': 'ABABBA', '9': 'ABBABA',
};

/** EAN-13 check digit for 12 data digits. */
export function ean13CheckDigit(data12: string): number {
  let sum = 0;
  for (let i = 0; i < 12; i++) sum += Number(data12[i]) * (i % 2 === 0 ? 1 : 3);
  return (10 - (sum % 10)) % 10;
}

export function isValidEan13(code: string): boolean {
  if (!/^\d{13}$/.test(code)) return false;
  return ean13CheckDigit(code.slice(0, 12)) === Number(code[12]);
}

/**
 * Generate a fresh, valid EAN-13 using the in-store / restricted-distribution prefix "20"
 * (GS1 reserves 20–29 for internal use — these never collide with real retail products).
 */
export function generateEan13(): string {
  let body = '20';
  for (let i = 0; i < 10; i++) body += Math.floor(Math.random() * 10);
  return body + ean13CheckDigit(body);
}

/** The 95-module bit string for a valid EAN-13 (guard + left group + centre + right group + guard). */
function ean13Bits(code: string): string {
  const parity = PARITY[code[0]];
  const left = code.slice(1, 7);
  const right = code.slice(7, 13);
  let bits = '101';
  for (let i = 0; i < 6; i++) bits += (parity[i] === 'A' ? A : B)[left[i]];
  bits += '01010';
  for (let i = 0; i < 6; i++) bits += C[right[i]];
  bits += '101';
  return bits;
}

/** Render a barcode as an inline SVG string. Falls back to a text-only label if the value isn't a valid EAN-13. */
export function barcodeSvg(code: string, opts?: { module?: number; height?: number }): string {
  const module = opts?.module ?? 2;
  const height = opts?.height ?? 64;
  const quiet = 12 * module;
  const textH = 16;
  if (!isValidEan13(code)) {
    const w = Math.max(160, code.length * 10 + 24);
    return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="40" viewBox="0 0 ${w} 40"><rect width="${w}" height="40" fill="#fff"/><text x="${w / 2}" y="24" text-anchor="middle" font-family="monospace" font-size="13" fill="#111">${code || '—'}</text></svg>`;
  }
  const bits = ean13Bits(code);
  const width = quiet * 2 + bits.length * module;
  const totalH = height + textH;
  let rects = '';
  let x = quiet;
  for (const bit of bits) {
    if (bit === '1') rects += `<rect x="${x}" y="0" width="${module}" height="${height}" fill="#000"/>`;
    x += module;
  }
  const text = `<text x="${width / 2}" y="${height + 13}" text-anchor="middle" font-family="monospace" font-size="13" letter-spacing="2" fill="#111">${code}</text>`;
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${totalH}" viewBox="0 0 ${width} ${totalH}"><rect width="${width}" height="${totalH}" fill="#fff"/>${rects}${text}</svg>`;
}

/** A printable label document: product name, price, and the barcode. Auto-prints on open. */
export function barcodeLabelHtml(code: string, name: string, priceLine: string): string {
  const svg = barcodeSvg(code, { module: 2, height: 64 });
  const esc = (s: string) => s.replace(/[&<>]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' }[c] as string));
  return `<!doctype html><html><head><meta charset="utf-8"><title>Label ${esc(code)}</title>
<style>
  @page { margin: 4mm; }
  body { font-family: system-ui, -apple-system, sans-serif; margin: 0; padding: 8px; text-align: center; }
  .name { font-weight: 700; font-size: 13px; margin: 0 0 2px; }
  .price { font-size: 12px; color: #111; margin: 0 0 6px; }
  .bc svg { max-width: 100%; height: auto; }
  @media print { .hint { display: none; } }
  .hint { margin-top: 10px; font-size: 11px; color: #666; }
  button { font: inherit; padding: 6px 14px; border: 1px solid #ccc; border-radius: 6px; background: #fff; cursor: pointer; }
</style></head>
<body>
  <p class="name">${esc(name || 'Item')}</p>
  ${priceLine ? `<p class="price">${esc(priceLine)}</p>` : ''}
  <div class="bc">${svg}</div>
  <div class="hint"><button onclick="window.print()">Print</button> · set copies in the print dialog</div>
  <script>window.onload=function(){setTimeout(function(){window.focus();window.print();},250);};</script>
</body></html>`;
}
