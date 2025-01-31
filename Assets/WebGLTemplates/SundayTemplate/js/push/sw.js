/* eslint-env browser, serviceworker, es6 */
'use strict';
/*
 * Visit Github page[Browser-push](https://lahiiru.github.io/browser-push) for guide lines.
 */
/* eslint-disable max-len */

const applicationServerPublicKey = 'BN8mGgNKGQ5AtPq7P9j3198M6mdGSie1o75ex5FW-rU9pMC4jBEYVSveYZej6yxJXIV2BfFCttsbyDGJI1RNiR0';

/* eslint-enable max-len */

function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

self.addEventListener('push', function(event) {
    console.log('[Service Worker] Push Received.');
    console.log(`[Service Worker] Push had this data: "${event.data.text()}"`);

    const title = event.data.json().title;
    const options = {
		body: event.data.json().message,
		icon: 'img/icon.png',
		badge: 'img/badge.png',
		requireInteraction: true
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', function(event) {
    console.log('[Service Worker] Notification click Received.');

    event.notification.close();

	const urlToOpen = new URL('https://play.0xuniverse.com', self.location.origin).href;

	const promiseChain = clients.matchAll({
		type: 'window',
		includeUncontrolled: true
	})
		.then((windowClients) => {
			let matchingClient = null;

			for (let i = 0; i < windowClients.length; i++) {
				const windowClient = windowClients[i];
				if (windowClient.url === urlToOpen) {
					matchingClient = windowClient;
					break;
				}
			}

			if (matchingClient) {
				return matchingClient.focus();
			} else {
				return clients.openWindow(urlToOpen);
			}
		});

	event.waitUntil(promiseChain);
});

self.addEventListener('pushsubscriptionchange', function(event) {
    console.log('[Service Worker]: \'pushsubscriptionchange\' event fired.');
    const applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
    event.waitUntil(
        self.registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: applicationServerKey
        }).then(function(newSubscription) {
            /* TODO: Send the subscription object to application server.
            *       notifications are sent from the server using this object.
            */
            console.log('[Service Worker] New subscription: ', newSubscription);
            console.log(JSON.stringify(newSubscription));
        })
    );
});
