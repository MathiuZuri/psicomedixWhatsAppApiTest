/* Manifest version: kiENaqZ/ */
const CACHE_VERSION = 'v1';
const CACHE_NAME = `clinica-wasm-${CACHE_VERSION}`;

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        ).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    if (event.request.url.includes('/api/') || event.request.method !== 'GET') return;

    event.respondWith(
        caches.match(event.request).then(cached => {
            const fetched = fetch(event.request).then(response => {
                if (response.ok) {
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                }
                return response;
            });
            return cached || fetched;
        })
    );
});