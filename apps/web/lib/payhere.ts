// PayHere on-domain JS SDK (payhere.js) loader. The card popup renders as an iframe overlay on
// our site; the card is entered inside PayHere's iframe (PCI-safe) and we only ever get a token.
export type PayHereSdk = {
  onCompleted?: (orderId: string) => void;
  onDismissed?: () => void;
  onError?: (error: string) => void;
  startPayment: (payment: Record<string, unknown>) => void;
};

export function loadPayHere(): Promise<PayHereSdk> {
  return new Promise((resolve, reject) => {
    const w = window as unknown as { payhere?: PayHereSdk };
    if (w.payhere) return resolve(w.payhere);
    const done = () => {
      const p = (window as unknown as { payhere?: PayHereSdk }).payhere;
      p ? resolve(p) : reject(new Error('PayHere SDK unavailable'));
    };
    const existing = document.getElementById('payhere-js') as HTMLScriptElement | null;
    if (existing) {
      existing.addEventListener('load', done);
      existing.addEventListener('error', () => reject(new Error('Failed to load PayHere')));
      return;
    }
    const s = document.createElement('script');
    s.id = 'payhere-js';
    s.src = 'https://www.payhere.lk/lib/payhere.js';
    s.onload = done;
    s.onerror = () => reject(new Error('Failed to load PayHere'));
    document.body.appendChild(s);
  });
}
