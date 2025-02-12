using UnityEngine;
using System;
using System.Collections;

#if APPODEAL
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
#endif

namespace SG.Ads
{
    public class Appodeal : Provider
#if APPODEAL
        , IInterstitialAdListener, IRewardedVideoAdListener, IPermissionGrantedListener//, IBannerAdListener, IMrecAdListener, INonSkippableVideoAdListener
#endif
    {
        public AppStore appStore;
        [Serializable]
        public class AppStore
        {
            public string key;
            // MAIN e53ef13b0b87da49e4fa044855cf414c0fbbfbe070896a11
            // RIKI 5d63c6d2ac74150f04fb9bcc8162fa077b2f5aa2fe311e8d
            // WYSE 813147ffa2e9fba3ae2c3f900aeb3e04f04b6acd7b7fe0bc
            // TNT 95827aa8c34975fb8f6106202d9d466b65c657ff536466e7
            // MY TNT baed6b77243dc1f2557fa73266babcc7d65116d6e5d7c6c9
        }
        public GooglePlay googlePlay;
        [Serializable]
        public class GooglePlay
        {
            public string key;
            // MAIN 1a75afa2415097c8a86d40f154ec56634d969c2e0be8c78d
            // RIKI 359c604343036d0f16e62d995ff1473d47bb1a1f6554c585
            // WYSE b50a987924e54c3aa25a300fc0a50515786b02aedac8b561
            // TNT 9ccff072076c0340cae0754d0761df1c443a2e9d83bd57f3
            // MY TNT 6231151615be055ed919d11fb514f1d1f298cde36ba2c410
        }
        public Amazon amazon;
        [Serializable]
        public class Amazon
        {
            public string key;
            // MAIN d3cfb1ff1e0718d48191df13b1d3ae7292ada43d41a2d650
        }

        public override void Init()
        {
            if (isInit) return;
            else isInit = true;

            Log.Debug("Ads - Appodeal - Init...");

#if APPODEAL
            Appodeal.requestAndroidMPermissions(this);

            if (isDebug) Appodeal.setLogLevel(Appodeal.LogLevel.Debug);

            Appodeal.setTesting(isDebug);

            //var settings = new UserSettings();
            //settings.setAge(25).setGender(UserSettings.Gender.OTHER).setUserId("best_user_ever");

            Appodeal.disableLocationPermissionCheck(); // Для отключения всплывающего сообщения  "ACCESS_COARSE_LOCATION permission is missing"

            // Отключение всех рекламных сетей, которым требуют это разрешение, может привести к низкой заполняемости видео.
            // Appodeal.disableWriteExternalStoragePermissionCheck(); // Для отключения всплывающего сообщения "WRITE_EXTERNAL_STORAGE permission is missing"

            //Appodeal.setChildDirectedTreatment(false); // Отключение сбора данных для детских приложений
            //Appodeal.muteVideosIfCallsMuted(true); // Отключение звука в видео рекламе (Только для Android)

            //Appodeal.setExtraData(ExtraData.APPSFLYER_ID, "1527256526604-2129416");

            Appodeal.setInterstitialCallbacks(this);
            Appodeal.setRewardedVideoCallbacks(this);

            //Appodeal.setSegmentFilter("newBoolean", true);
            //Appodeal.setSegmentFilter("newInt", 1234567890);
            //Appodeal.setSegmentFilter("newDouble", 123.123456789);
            //Appodeal.setSegmentFilter("newString", "newStringFromSDK");

            var appKey = platform == Platform.AppStore ? appStore.key : (platform == Platform.GooglePlay ? googlePlay.key : amazon.key);
            var userHasAgreedToProvidePersonalData = true;
            Appodeal.initialize(appKey, Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, userHasAgreedToProvidePersonalData);
#endif
        }

#if APPODEAL
        StatusInterstitial statusInterstitial = StatusInterstitial.FailedToLoad;
        enum StatusInterstitial { FailedToLoad, Loaded, Shown, Clicked, Closed, ClosedAfterClicked }

        public override bool isReadyInterstitial()
        {
            statusInterstitial = Appodeal.isLoaded(Appodeal.INTERSTITIAL) ? StatusInterstitial.Loaded : StatusInterstitial.FailedToLoad;

            if (statusInterstitial != StatusInterstitial.Loaded)
                Appodeal.cache(Appodeal.INTERSTITIAL);

            LogDebug("Ads - Appodeal - Interstitial isReady - " + (statusInterstitial == StatusInterstitial.Loaded ? "YES" : "NO"));

            Analytic.EventProperties("Ads", "Appodeal Interstitial", statusInterstitial == StatusInterstitial.Loaded ? "YES" : "NO");

            return statusInterstitial == StatusInterstitial.Loaded;
        }

        public override void ShowInterstitial(Action success, Action failed) { StartCoroutine(ShowInterstitialCoroutine(success, failed)); }
        IEnumerator ShowInterstitialCoroutine(Action success, Action failed)
        {
            if (statusInterstitial != StatusInterstitial.Loaded)
            {
                failed?.Invoke();
                yield break;
            }

            Appodeal.show(Appodeal.INTERSTITIAL);

            float startTime = Time.time;
            while (statusInterstitial != StatusInterstitial.Shown || Time.time - startTime < 5f) yield return null;

            if (statusInterstitial != StatusInterstitial.Shown)
            {
                failed?.Invoke();
                yield break;
            }

            ui.Block();

            while (statusInterstitial != StatusInterstitial.ClosedAfterClicked && statusInterstitial != StatusInterstitial.Closed) yield return null;

            ui.Unblock();

            if (statusInterstitial == StatusInterstitial.ClosedAfterClicked)
            {
                Log("Ads - Appodeal - Interstitial Result - Click");

                Analytic.EventProperties("Ads", "Appodeal Interstitial Result", "Click");

                success?.Invoke();
            }
            else
            {
                Log("Ads - Appodeal - Interstitial Result - Dismiss");

                Analytic.EventProperties("Ads", "Appodeal Interstitial Result", "Dismiss");

                failed?.Invoke();
            }
        }

        // Вызывается, когда полноэкранная реклама загрузилась. Флаг precache указывает, является ли реклама прекешем.
        public void onInterstitialLoaded(bool isPrecache)
        {
            LogDebug($"Ads - Appodeal - onInterstitialLoaded (precache: {isPrecache})");
        }
        // Вызывается, когда полноэкранная реклама не загрузилась
        public void onInterstitialFailedToLoad()
        {
            LogDebug("Ads - Appodeal - Interstitial failed to load");
        }
        // Вызывается после показа полноэкранной рекламы
        public void onInterstitialShown()
        {
            LogDebug("Ads - Appodeal - Interstitial shown");
            statusInterstitial = StatusInterstitial.Shown;
        }
        // Вызывается при клике на полноэкранную рекламу
        public void onInterstitialClicked()
        {
            LogDebug("Ads - Appodeal - Interstitial clicked");
            statusInterstitial = StatusInterstitial.Clicked;
        }
        // Вызывается при закрытии полноэкранной рекламы
        public void onInterstitialClosed()
        {
            LogDebug("Ads - Appodeal - Interstitial closed");
            statusInterstitial = statusInterstitial == StatusInterstitial.Clicked ? StatusInterstitial.ClosedAfterClicked : StatusInterstitial.Closed;
        }
        // Вызывается, когда полноэкранная реклама больше не доступна
        public void onInterstitialExpired()
        {
            LogDebug("Ads - Appodeal - Interstitial expired");
        }


        StatusVideoRewarded statusVideoRewarded = StatusVideoRewarded.FailedToLoad;
        enum StatusVideoRewarded { FailedToLoad, Loaded, Shown, Closed, Finished }

        public override bool isReadyVideoRewarded()
        {
            statusVideoRewarded = Appodeal.isLoaded(Appodeal.REWARDED_VIDEO) ? StatusVideoRewarded.Loaded : StatusVideoRewarded.FailedToLoad;

            if (statusVideoRewarded != StatusVideoRewarded.Loaded)
                Appodeal.cache(Appodeal.REWARDED_VIDEO);

            LogDebug("Ads - Appodeal - VideoRewarded Request - " + (statusVideoRewarded == StatusVideoRewarded.Loaded ? "YES" : "NO"));

            Analytic.EventProperties("Ads", "Appodeal VideoRewarded Request", statusVideoRewarded == StatusVideoRewarded.Loaded ? "YES" : "NO");

            return statusVideoRewarded == StatusVideoRewarded.Loaded;
        }

        public override void ShowVideoRewarded(Action success, Action failed) { StartCoroutine(ShowVideoRewardedCoroutine(success, failed)); }
        IEnumerator ShowVideoRewardedCoroutine(Action success, Action failed)
        {
            if (statusVideoRewarded != StatusVideoRewarded.Loaded)
            {
                failed?.Invoke();
                yield break;
            }

            ui.Block();
            music.SetVolume(0f);

            Log("Ads - Appodeal - Predicted eCPM for Rewarded Video: " + Appodeal.getPredictedEcpm(Appodeal.REWARDED_VIDEO));
            Log($"Ads - Appodeal - Reward currency: {Appodeal.getRewardParameters().Key}, amount: {Appodeal.getRewardParameters().Value}");

            Appodeal.show(Appodeal.REWARDED_VIDEO);

            while (statusVideoRewarded != StatusVideoRewarded.Closed) yield return null;

            Log("Ads - Appodeal Result Finished");

            Analytic.EventPropertiesImportant("Ads", "Appodeal VideoRewarded Result", "Finished");

            ui.Unblock();
            music.SetVolume(music.volumeNormal);

            success?.Invoke();
        }

        public void onRewardedVideoLoaded(bool isPrecache)
        {
            LogDebug($"Ads - Appodeal - RewardedVideo loaded (precache: {isPrecache})");
        }
        public void onRewardedVideoFailedToLoad()
        {
            LogDebug("Ads - Appodeal - RewardedVideo failed to load");
        }
        public void onRewardedVideoShown()
        {
            LogDebug("Ads - Appodeal - RewardedVideo shown");
            statusVideoRewarded = StatusVideoRewarded.Shown;
        }
        public void onRewardedVideoClosed(bool finished)
        {
            LogDebug($"Ads - Appodeal - RewardedVideo closed (finished: {finished})");
            statusVideoRewarded = StatusVideoRewarded.Closed;
        }
        public void onRewardedVideoFinished(double amount, string name)
        {
            LogDebug($"Ads - Appodeal - RewardedVideo finished (amount: {amount}, name: {name})");
            statusVideoRewarded = StatusVideoRewarded.Finished;
        }
        public void onRewardedVideoExpired()
        {
            LogDebug("Ads - Appodeal - RewardedVideo expired");
        }
        public void onRewardedVideoClicked()
        {
            LogDebug("Ads - Appodeal - RewardedVideo clicked");
        }


        public void writeExternalStorageResponse(int result)
        {
            if (result == 0)
                Log("Ads - Appodeal - WRITE_EXTERNAL_STORAGE permission granted");
            else
                Log("Ads - Appodeal - WRITE_EXTERNAL_STORAGE permission grant refused");
        }
        public void accessCoarseLocationResponse(int result)
        {
            if (result == 0)
                Log("Ads - Appodeal - ACCESS_COARSE_LOCATION permission granted");
            else
                Log("Ads - Appodeal - ACCESS_COARSE_LOCATION permission grant refused");
        }
#endif
    }
}