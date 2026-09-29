// Web push opt-in. The service worker lives at the app's root (push-sw.js) so it
// can show a notification when no page is open.
function b64ToBytes(b64) {
    const pad = '='.repeat((4 - b64.length % 4) % 4);
    const raw = atob((b64 + pad).replace(/-/g, '+').replace(/_/g, '/'));
    return Uint8Array.from(raw, c => c.charCodeAt(0));
}
function bytesToB64(buf) {
    return btoa(String.fromCharCode(...new Uint8Array(buf))).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

export function supported() {
    return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
}

export function permission() {
    return supported() ? Notification.permission : 'unsupported';
}

export async function current() {
    if (!supported()) return null;
    const reg = await navigator.serviceWorker.getRegistration('/');
    const sub = reg ? await reg.pushManager.getSubscription() : null;
    return sub ? sub.endpoint : null;
}

export async function subscribe(publicKey) {
    if (!supported()) throw new Error('This browser cannot receive notifications.');
    const granted = await Notification.requestPermission();
    if (granted !== 'granted') throw new Error('Notifications were not allowed.');
    const reg = await navigator.serviceWorker.register('/push-sw.js', { scope: '/' });
    await navigator.serviceWorker.ready;
    const sub = await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: b64ToBytes(publicKey) });
    return {
        endpoint: sub.endpoint,
        p256dh: bytesToB64(sub.getKey('p256dh')),
        auth: bytesToB64(sub.getKey('auth')),
        userAgent: navigator.userAgent.slice(0, 300)
    };
}

export async function unsubscribe() {
    const reg = await navigator.serviceWorker.getRegistration('/');
    const sub = reg ? await reg.pushManager.getSubscription() : null;
    if (!sub) return null;
    const endpoint = sub.endpoint;
    await sub.unsubscribe();
    return endpoint;
}
