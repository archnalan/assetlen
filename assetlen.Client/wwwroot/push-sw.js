// Shows an ASSETLEN notification and opens the page it points at. Push only:
// no fetch handler, so it never serves a stale app shell.
self.addEventListener('push', event => {
    let data = {};
    try { data = event.data ? event.data.json() : {}; } catch { data = { title: 'ASSETLEN', body: event.data && event.data.text() }; }
    event.waitUntil(self.registration.showNotification(data.title || 'ASSETLEN', {
        body: data.body || '',
        tag: data.tag,
        renotify: !!data.tag,
        icon: '/icon-192.png',
        badge: '/favicon-32.png',
        data: { url: data.url || '/' }
    }));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const url = new URL(event.notification.data && event.notification.data.url || '/', self.location.origin).href;
    event.waitUntil(clients.matchAll({ type: 'window', includeUncontrolled: true }).then(list => {
        for (const c of list) { if ('focus' in c) { c.navigate(url); return c.focus(); } }
        return clients.openWindow(url);
    }));
});

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
