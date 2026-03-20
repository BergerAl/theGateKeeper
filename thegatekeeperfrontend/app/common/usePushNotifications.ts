'use client';

import { useState, useEffect, useCallback } from 'react';
import { domainUrlPrefix } from '@/store/backEndCalls';

const STORAGE_KEY = 'push_subscribed';

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
            // 1. Ensure service worker is registered and active
            await navigator.serviceWorker.register('/sw.js');
            const registration = await navigator.serviceWorker.ready;

            // 2. Fetch VAPID public key from backend
            const keyRes = await fetch(`${domainUrlPrefix()}/api/notifications/vapid-public-key`);
            if (!keyRes.ok) throw new Error('Could not reach notification service.');
            const { publicKey } = await keyRes.json();

            // 3. Subscribe via PushManager (prompts browser permission dialog)
            // Pass the key as a string — browsers handle base64url decoding natively,
            // avoiding manual ArrayBuffer conversion issues across browser versions.
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: publicKey,
            });

            // 4. Send subscription to backend
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

            if (!res.ok) throw new Error('Failed to save subscription on server.');
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
