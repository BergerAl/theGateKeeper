'use client';

import { useState, useEffect, useCallback } from 'react';
import { domainUrlPrefix } from '@/store/backEndCalls';

const STORAGE_KEY = 'push_subscribed';

// Chrome requires a Uint8Array for applicationServerKey, not a plain string.
// Explicit ArrayBuffer construction ensures the correct generic type (Uint8Array<ArrayBuffer>).
function urlBase64ToUint8Array(base64String: string): Uint8Array<ArrayBuffer> {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const buffer = new ArrayBuffer(rawData.length);
    const outputArray = new Uint8Array(buffer);
    for (let i = 0; i < rawData.length; i++) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

export function usePushNotifications(accessToken: string | undefined) {
    const [isSubscribed, setIsSubscribed] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // On mount, sync state from the actual PushManager (source of truth)
    useEffect(() => {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;

        navigator.serviceWorker.register('/sw.js').then((reg) => {
            reg.pushManager.getSubscription().then((sub) => {
                setIsSubscribed(!!sub);
            });
        }).catch(() => { /* sw registration failed silently on mount */ });
    }, []);

    const subscribe = useCallback(() => {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            setError('Push notifications are not supported in this browser.');
            return;
        }
        if (!accessToken) {
            setError('You must be logged in to subscribe to notifications.');
            return;
        }

        setIsLoading(true);
        setError(null);

        const run = async () => {
            // 1. Check if the user has explicitly blocked notifications
            if (Notification.permission === 'denied') {
                throw new Error('Notification permission is blocked. Please allow notifications for this site in your browser settings and try again.');
            }

            // 2. Ensure service worker is registered and active
            await navigator.serviceWorker.register('/sw.js');
            const registration = await navigator.serviceWorker.ready;

            // 3. Fetch VAPID public key from backend
            const keyRes = await fetch(`${domainUrlPrefix()}/api/notifications/vapid-public-key`);
            if (!keyRes.ok) throw new Error('Could not reach notification service.');
            const { publicKey } = await keyRes.json();

            // 4. Subscribe via PushManager (prompts browser permission dialog)
            // Chrome requires a Uint8Array — passing a plain string causes "push service error".
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey),
            });

            // 5. Send subscription to backend
            const subJson = subscription.toJSON();
            const res = await fetch(`${domainUrlPrefix()}/api/notifications/subscribe`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${accessToken}`,
                },
                body: JSON.stringify({
                    endpoint: subJson.endpoint,
                    p256dh: subJson.keys?.p256dh,
                    auth: subJson.keys?.auth,
                }),
            });

            if (!res.ok) throw new Error(`Failed to save subscription (HTTP ${res.status}).`);
            setIsSubscribed(true);
            localStorage.setItem(STORAGE_KEY, 'true');
        };

        run()
            .catch((err: unknown) => {
                const msg = err instanceof Error ? err.message : 'Failed to subscribe.';
                setError(msg);
                console.warn('Subscribe failed:', err);
            })
            .finally(() => setIsLoading(false));
    }, [accessToken]);

    const unsubscribe = useCallback(() => {
        if (!accessToken) return;
        setIsLoading(true);
        setError(null);

        const run = async () => {
            await navigator.serviceWorker.register('/sw.js');
            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.getSubscription();
            if (subscription) await subscription.unsubscribe();

            await fetch(`${domainUrlPrefix()}/api/notifications/unsubscribe`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            setIsSubscribed(false);
            localStorage.removeItem(STORAGE_KEY);
        };

        run()
            .catch((err: unknown) => {
                const msg = err instanceof Error ? err.message : 'Failed to unsubscribe.';
                setError(msg);
                console.warn('Unsubscribe failed:', err);
            })
            .finally(() => setIsLoading(false));
    }, [accessToken]);

    return { isSubscribed, isLoading, error, subscribe, unsubscribe };
}
