const CACHE = '0xUniverse-v1';

self.addEventListener('install', function(evt) {
  console.log('The service worker is being installed.');
  evt.waitUntil(precache());
});

self.addEventListener('fetch', function(evt) {
  console.log('The service worker is serving the asset.');
  evt.respondWith(fromCache(evt.request));
  evt.waitUntil(update(evt.request));
});

function getUnityArtefactsPrefix(){
  const domain = self.location.hostname;
  switch(domain){
    case "play.0xuniverse.com" : return "release";
    case "0x.games" : return "qa";
    default: return "Build";
  }
}

function precache() {
  return caches.open(CACHE).then(function (cache) {
    const buildPrefix = getUnityArtefactsPrefix();
    return cache.addAll([
      '/index.html',
      '/img/bg.jpg',
      '/img/loading_text.gif',
      '/img/logo.gif',
      '/img/logo.png',
      '/js/clipboard.js',
      '/js/cache.js',
      '/js/urlParams.js',
      '/js/blockchain/blockchain.min.js',
      '/js/blockchain/web3.min.js',
      '/js/game/UnityLoader.js',
      '/js/game/UnityLoader_formated.js',
      '/js/push/main.js',
      '/js/push/sw.js',
      '/js/push/images/badge.png',
      '/js/push/images/icon.png',
      `/Build/${buildPrefix}.json`,
      `/Build/${buildPrefix}.asm.code.unityweb`,
      `/Build/${buildPrefix}.asm.framework.unityweb`,
      `/Build/${buildPrefix}.asm.memory.unityweb`,
      `/Build/${buildPrefix}.data.unityweb`
    ]);
  });
}

function fromCache(request) {
  return caches.open(CACHE).then(function (cache) {
    return cache.match(request).then(function (matching) {
      return matching || Promise.reject('no-match');
    });
  });
}

function update(request) {
  return caches.open(CACHE).then(function (cache) {
    return fetch(request).then(function (response) {
      return cache.put(request, response);
    });
  });
}