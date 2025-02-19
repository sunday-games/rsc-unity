using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace SG.Ads
{
    public class UnityAds : Provider
#if UNITY_ADS
        , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#endif
    {
        private static class Placements
        {
            public const string BANNER = "banner";
            public const string VIDEO = "video";
            public const string REWARDED = "rewardedVideo";
        }

        // [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

        [Space(10)]
        public Store AppStore, GooglePlay;
        [Serializable]
        public class Store
        {
            public string GameId;
        }


        private bool _testMode => Utils.IsPlatformEditor();
        private string _gameId => (Utils.IsPlatform(Platform.iOS) ? AppStore : GooglePlay).GameId;

        private enum Status { Ready, Loading, Showing, Skipped, Finished, Failed }
        private Dictionary<string, Status> _status = new Dictionary<string, Status>
        {
            [Placements.BANNER] = Status.Finished,
            [Placements.VIDEO] = Status.Finished,
            [Placements.REWARDED] = Status.Finished,
        };

        public override void Init()
        {
            if (isInit)
                return;
#if UNITY_ADS
            Log.Ads.Debug($"Ads - Unity Ads Supported: {Advertisement.isSupported} - Init...");

            Advertisement.Initialize(_gameId, _testMode, this);
#endif
            isInit = true;
        }

#if UNITY_ADS
        public override bool isReadyInterstitial() => isReady(Placements.VIDEO);
        public override void ShowInterstitial(Action success, Action failed) =>
            StartCoroutine(ShowCoroutine(Placements.VIDEO, success, failed));

        public override bool isReadyVideoRewarded() => isReady(Placements.REWARDED);
        public override void ShowRewarded(Action success, Action failed) =>
            StartCoroutine(ShowCoroutine(Placements.REWARDED, success, failed));

        private IEnumerator ShowCoroutine(string placement, Action success, Action failed)
        {
            if (!Advertisement.isInitialized)
            {
                Log.Debug($"Ads - Unity - Show failed '{placement}': Not initialized yet");
                failed?.Invoke();
                yield break;
            }

            if (_status[placement] == Status.Showing)
            {
                Log.Debug($"Ads - Unity - Show failed '{placement}': Already Showing");
                failed?.Invoke();
                yield break;
            }

            if (!isReady(placement))
            {
                Log.Debug($"Ads - Unity - Show failed '{placement}': Not ready yet");
                failed?.Invoke();
                yield break;
            }

            Manager.onStarted?.Invoke();

            // TODO
            // Advertisement.Banner.SetPosition(bannerPosition);
            // Advertisement.Banner.Show(Placements.BANNER);

            Advertisement.Show(placement, showListener: this);

            //float startTime = Time.time;
            //while (statusInterstitial != StatusInterstitial.Shown || Time.time - startTime < 5f) yield return null;

            _status[placement] = Status.Showing;
            while (_status[placement] == Status.Showing) yield return null;

            Manager.onEnded?.Invoke();

            if (_status[placement] == Status.Finished)
                success?.Invoke();
            else
                failed?.Invoke();

            Load(placement);
        }

        private bool isReady(string placement)
        {
            var ready = _status[placement] == Status.Ready;

            Log.Debug($"Ads - Unity - IsReady '{placement}': {ready}");

            if (!ready)
                Load(placement);

            return ready;
        }

        private void Load(string placement)
        {
            if (_status[placement] == Status.Loading)
            {
                Log.Ads.Debug($"Ads - Unity Ads - Load not started because already loading: {placement}");
                return;
            }

            Log.Ads.Debug($"Ads - Unity Ads - Load Start: {placement}");
            Advertisement.Load(placement, loadListener: this);
            _status[placement] = Status.Loading;
        }

        #region Interface Implementations

        public void OnInitializationComplete() =>
            Log.Ads.Debug("Ads - Unity Ads - Init Success");

        public void OnInitializationFailed(UnityAdsInitializationError error, string message) =>
            Log.Ads.Debug($"Ads - Unity Ads - Init Failed: [{error}]: {message}");


        public void OnUnityAdsAdLoaded(string placement)
        {
            Log.Ads.Debug($"Ads - Unity Ads - Load Success: {placement}");
            _status[placement] = Status.Ready;
        }

        public void OnUnityAdsFailedToLoad(string placement, UnityAdsLoadError error, string message)
        {
            Log.Ads.Debug($"Ads - Unity Ads - Load Failed: [{error}:{placement}] {message}");
            _status[placement] = Status.Failed;
        }

        public void OnUnityAdsShowStart(string placement) =>
            Log.Ads.Debug($"Ads - Unity Ads - OnUnityAdsShowStart: {placement}");

        public void OnUnityAdsShowClick(string placement) =>
            Log.Ads.Debug($"Ads - Unity Ads - OnUnityAdsShowClick: {placement}");

        public void OnUnityAdsShowComplete(string placement, UnityAdsShowCompletionState state)
        {
            Log.Ads.Debug($"Ads - Unity Ads - OnUnityAdsShowComplete: [{state}]: {placement}");

            if (state == UnityAdsShowCompletionState.COMPLETED)
                _status[placement] = Status.Finished;
            else if (state == UnityAdsShowCompletionState.SKIPPED)
                _status[placement] = Status.Skipped;
            else
                _status[placement] = Status.Failed;
        }

        public void OnUnityAdsShowFailure(string placement, UnityAdsShowError error, string message)
        {
            Log.Ads.Debug($"Ads - Unity Ads - OnUnityAdsShowFailure: [{error}]: {message}");
            _status[placement] = Status.Failed;
        }

        #endregion
#endif
    }
}