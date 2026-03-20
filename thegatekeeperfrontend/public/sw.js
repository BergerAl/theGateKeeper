self.addEventListener('install', (event) => {
    console.log('Service Worker installing...');
    event.waitUntil(self.skipWaiting());
});

self.addEventListener('activate', (event) => {
    console.log('Service Worker activated');
    event.waitUntil(self.clients.claim());
});

self.addEventListener('push', (event) => {
    let data = { title: 'The GateKeeper', body: 'New update!', icon: '/images/clown.png' };
    try {
        if (event.data) {
            data = { ...data, ...JSON.parse(event.data.text()) };
        }
    } catch (e) {
        console.error('Failed to parse push payload', e);
    }

    event.waitUntil(
        self.registration.showNotification(data.title, {
            body: data.body,
            icon: data.icon,
            badge: '/images/clown.png',
        })
    );
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            for (const client of clientList) {
                if (client.url && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow('/');
            }
        })
    );
});