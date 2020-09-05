using UnityEngine;
using System;
using System.Collections;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace SG
{
    public class UnityProvider : AdsProvider
#if UNITY_ADS
        , IUnityAdsListener
#endif
    {
        public string videoPlacementId = "video";
        public string videoRewardedPlacementId = "rewardedVideo";

        [Space(10)]
        public AppStore appStore;
        [Serializable]
        public class AppStore
        {
            public string key;
        }
        public GooglePlay googlePlay;
        [Serializable]
        public class GooglePlay
        {
            public string key;
        }

        public override void Init()
        {
#if UNITY_ADS
            if (!isInit)
            {
                LogDebug("Ads - Unity Ads - Init...");

                Advertisement.AddListener(this);
                Advertisement.Initialize(
                    gameId: platform == Platform.AppStore ? appStore.key : googlePlay.key,
                    testMode: platform == Platform.Editor);

                isInit = true;
            }
#endif
        }

#if UNITY_ADS
        public override bool isReadyInterstitial()
        {
            return isReady(videoPlacementId);
        }
        public override void ShowInterstitial(Action success, Action failed)
        {
            StartCoroutine(ShowCoroutine(videoPlacementId, success, failed));
        }

        public override bool isReadyVideoRewarded()
        {
            return isReady(videoRewardedPlacementId);
        }
        public override void ShowVideoRewarded(Action success, Action failed)
        {
            StartCoroutine(ShowCoroutine(videoRewardedPlacementId, success, failed));
        }

        Status status = Status.Finished;
        enum Status { Shown, Skipped, Finished, Failed }

        IEnumerator ShowCoroutine(string id, Action success, Action failed)
        {
            if (!Advertisement.isInitialized || status == Status.Shown || !isReady(id))
            {
                failed?.Invoke();
                yield break;
            }

            ui.Block();
            music.SetVolume(0f);

            Advertisement.Show(id);

            //float startTime = Time.time;
            //while (statusInterstitial != StatusInterstitial.Shown || Time.time - startTime < 5f) yield return null;

            status = Status.Shown;
            while (status == Status.Shown) yield return null;

            Analytic.EventPropertiesImportant("Ads", $"Unity {id} Result", status);

            music.SetVolume(music.volumeNormal);
            ui.Unblock();

            if (status == Status.Finished)
                success?.Invoke();
            else
                failed?.Invoke();
        }

        bool isReady(string id)
        {
            var ready = Advertisement.IsReady(id);

            LogDebug($"Ads - Unity - IsReady '{id}': {ready}");
       
            return ready;
        }

        public void OnUnityAdsDidFinish(string id, ShowResult result)
        {
            LogDebug($"Ads - Unity - OnUnityAdsDidFinish '{id}': {result}");

            if (result == ShowResult.Finished)
            {
                status = Status.Finished;
            }
            else if (result == ShowResult.Skipped)
            {
                status = Status.Skipped;
            }
            else if (result == ShowResult.Failed)
            {
                LogError("Ads - Unity - The ad did not finish due to an error");
                status = Status.Failed;
            }
        }

        public void OnUnityAdsReady(string id) { LogDebug($"Ads - Unity - OnUnityAdsReady '{id}'"); }

        public void OnUnityAdsDidStart(string id) { LogDebug($"Ads - Unity - OnUnityAdsDidStart '{id}'"); }

        public void OnUnityAdsDidError(string error) { LogError($"Ads - Unity - OnUnityAdsDidError: {error}"); }

        public void OnDestroy() { Advertisement.RemoveListener(this); }
#endif
    }
}