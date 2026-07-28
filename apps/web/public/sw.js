// Web Push service worker (#floor-push). Deliberately tiny — its only job is to turn a
// push event into an OS notification, and a click into a focused/opened tab. No caching,
// no offline support; this is not a PWA shell.

self.addEventListener('push', (event) => {
  let data = {};
  try { data = event.data ? event.data.json() : {}; } catch { /* non-JSON payload — ignore */ }
  const title = data.title || 'RIT HMS';
  const options = {
    body: data.body || '',
    icon: '/favicon.ico',
    badge: '/favicon.ico',
    data: { url: data.url || '/' },
    tag: 'rit-hms-order',
    renotify: true,
  };
  event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const url = event.notification.data?.url || '/';
  event.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((list) => {
      for (const client of list) {
        if (client.url.includes(url) && 'focus' in client) return client.focus();
      }
      if (self.clients.openWindow) return self.clients.openWindow(url);
    })
  );
});
