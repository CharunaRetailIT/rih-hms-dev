'use client';

import { apiClient } from '@/lib/api-client';

/** Browser support check — Safari/older browsers lack one or both APIs. */
export function pushSupported(): boolean {
  return typeof window !== 'undefined' && 'serviceWorker' in navigator && 'PushManager' in window;
}

function urlBase64ToUint8Array(base64: string): BufferSource {
  const padding = '='.repeat((4 - (base64.length % 4)) % 4);
  const b64 = (base64 + padding).replace(/-/g, '+').replace(/_/g, '/');
  const raw = atob(b64);
  const bytes = new Uint8Array(raw.length);
  for (let i = 0; i < raw.length; i++) bytes[i] = raw.charCodeAt(i);
  return bytes.buffer;
}

async function getRegistration(): Promise<ServiceWorkerRegistration> {
  return (await navigator.serviceWorker.getRegistration('/sw.js')) ?? (await navigator.serviceWorker.register('/sw.js'));
}

/** Current subscription state for THIS device (browser + machine), if any. */
export async function currentSubscription(): Promise<PushSubscription | null> {
  if (!pushSupported()) return null;
  const reg = await navigator.serviceWorker.getRegistration('/sw.js').catch(() => null);
  return (await reg?.pushManager.getSubscription()) ?? null;
}

/** Ask the browser for notification permission, subscribe, and register with the API.
 * Throws with a human-readable message on failure (denied permission, unsupported, no VAPID key). */
export async function enablePush(): Promise<void> {
  if (!pushSupported()) throw new Error('Push notifications aren’t supported in this browser.');

  const permission = await Notification.requestPermission();
  if (permission !== 'granted') throw new Error('Notification permission was not granted.');

  const { publicKey } = await apiClient<{ publicKey: string }>('/api/v1/push/vapid-public-key');

  const reg = await getRegistration();
  let sub = await reg.pushManager.getSubscription();
  if (!sub) {
    sub = await reg.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(publicKey),
    });
  }

  const json = sub.toJSON();
  await apiClient('/api/v1/push/subscribe', {
    method: 'POST',
    body: JSON.stringify({ endpoint: json.endpoint, keys: { p256dh: json.keys?.p256dh, auth: json.keys?.auth } }),
  });
}

/** Unsubscribe this device from both the browser and the API. */
export async function disablePush(): Promise<void> {
  const sub = await currentSubscription();
  if (!sub) return;
  const endpoint = sub.endpoint;
  await sub.unsubscribe();
  await apiClient('/api/v1/push/unsubscribe', { method: 'POST', body: JSON.stringify({ endpoint }) }).catch(() => {});
}
