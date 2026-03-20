'use client';

import { useState, useEffect, useCallback } from 'react';
import { domainUrlPrefix } from '@/store/backEndCalls';

const STORAGE_KEY = 'push_subscribed';

function urlBase64ToUint8Array(base64String: string): ArrayBuffer {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    const buffer = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; i++) {
        buffer[i] = rawData.charCodeAt(i);
    }
    return buffer.buffer;
}

export function usePushNotifications(accessToken: string | undefined) {
    const [isSubscribed, setIsSubscribed] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    // On mount, sync state from the actual PushManager (source of truth)
    useEffect(() => {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;

        navigator.serviceWorker.ready.then((registration) => {
            registration.pushManager.getSubscription().then((sub) => {
                setIsSubscribed(!!sub);
            });
        });
    }, []);

    const subscribe = useCallback(async () => {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            alert('Push notifications are not supported in this browser.');
            return;
        }
        if (!accessToken) {
            alert('You must be logged in to subscribe to notifications.');
            return;
        }

        setIsLoading(true);
        try {
            // 1. Fetch VAPID public key from backend
            const keyRes = await fetch(`${domainUrlPrefix()}/api/notifications/vapid-public-key`);
            if (!keyRes.ok) throw new Error('Failed to fetch VAPID key');
            const { publicKey } = await keyRes.json();

            // 2. Register service worker if needed and subscribe via PushManager
            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey),
            });

            // 3. Send subscription to backend
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

            if (!res.ok) throw new Error('Failed to save subscription on server');
            setIsSubscribed(true);
            localStorage.setItem(STORAGE_KEY, 'true');
        } catch (err) {
            console.error('Subscribe failed:', err);
        } finally {
            setIsLoading(false);
        }
    }, [accessToken]);

    const unsubscribe = useCallback(async () => {
        if (!accessToken) return;
        setIsLoading(true);
        try {
            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.getSubscription();
            if (subscription) {
                await subscription.unsubscribe();
            }

            await fetch(`${domainUrlPrefix()}/api/notifications/unsubscribe`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            setIsSubscribed(false);
            localStorage.removeItem(STORAGE_KEY);
        } catch (err) {
            console.error('Unsubscribe failed:', err);
        } finally {
            setIsLoading(false);
        }
    }, [accessToken]);

    return { isSubscribed, isLoading, subscribe, unsubscribe };
}
