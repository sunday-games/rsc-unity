'use strict';

const applicationServerPublicKey = 'BN8mGgNKGQ5AtPq7P9j3198M6mdGSie1o75ex5FW-rU9pMC4jBEYVSveYZej6yxJXIV2BfFCttsbyDGJI1RNiR0';
let swRegistration = null;

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

function createPush(window, unityNotify) {
    const formatResponse = function(success, name, value){
        return {
          success,
          [name]: value
        };
    };      

    return {
        subscribe: function() {
            const applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
            let param = "";
            return swRegistration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerKey
            }).then(function(subscription) {
                console.log('User is subscribed:', subscription);
                unityNotify("Subscribe", formatResponse(true, "data", {subscription}));
            })
            .catch(function(err) {
                console.log('Failed to subscribe the user: ', err);
                unityNotify("Subscribe", formatResponse(false, "error", err.toString()));
            });
        },
        
        init: function(){
            if ('serviceWorker' in navigator && 'PushManager' in window) {
                console.log('Service Worker and Push is supported');
                navigator.serviceWorker.register('js/push/sw.js').then(function(swReg) {
                    console.log('Service Worker is registered', swReg);
                    swRegistration = swReg;
                }).catch(function(error) {
                    console.error('Service Worker Error', error);
                });
            } else {
                console.warn('Push messaging is not supported');
            }
        }
    }
}

window.createPush = createPush;