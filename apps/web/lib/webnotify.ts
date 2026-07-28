'use client';

/**
 * Lightweight desktop/web notifications via the browser Notification API.
 * Shows OS-level alerts while the app is open (no service worker / push server
 * needed). Background push when the tab is closed is a separate, heavier piece
 * (service worker + Web-Push/VAPID) tracked as the rider/tab-ordering push work.
 */

export type NotifPerm = 'granted' | 'denied' | 'default' | 'unsupported';

export function notifySupported(): boolean {
  return typeof window !== 'undefined' && 'Notification' in window;
}

export function notifyPermission(): NotifPerm {
  if (!notifySupported()) return 'unsupported';
  return Notification.permission as NotifPerm;
}

/** Ask for permission (must be called from a user gesture). Returns true if granted. */
export async function ensureNotifyPermission(): Promise<boolean> {
  if (!notifySupported()) return false;
  if (Notification.permission === 'granted') return true;
  if (Notification.permission === 'denied') return false;
  try { return (await Notification.requestPermission()) === 'granted'; }
  catch { return false; }
}

/** Show a notification if permission is already granted (no-op otherwise). */
export function showNotification(title: string, body?: string, href?: string): boolean {
  if (!notifySupported() || Notification.permission !== 'granted') return false;
  try {
    const n = new Notification(title, { body, tag: 'rit-hms', renotify: true } as NotificationOptions);
    if (href) n.onclick = () => { window.focus(); window.location.href = href; };
    return true;
  } catch { /* some browsers require a service worker for Notification(); report failure */ return false; }
}
