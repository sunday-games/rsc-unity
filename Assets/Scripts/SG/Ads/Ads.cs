using UnityEngine;
using System;

namespace SG
{
    public class Ads : Core
    {
        public int minLevel = 6;
        public int interstitialFrequency = 2;
        [HideInInspector]
        public int sessions = 0;

        #region SETUP
        AdsProvider providerInterstitial = null;
        AdsProvider providerVideoRewarded = null;

        public AdsSet editor;
        public AdsSet iOS;
        public AdsSet android;
        public AdsSet facebook;
        public AdsSet amazon;
        [Serializable]
        public class AdsSet
        {
            public AdsProvider interstitial;
            public AdsProvider videoRewarded;
        }

        public void Init()
        {
            if (platform == Platform.AppStore)
            {
                providerInterstitial = iOS.interstitial;
                providerVideoRewarded = iOS.videoRewarded;
            }
            else if (platform == Platform.GooglePlay)
            {
                providerInterstitial = android.interstitial;
                providerVideoRewarded = android.videoRewarded;
            }
            else if (platform == Platform.Amazon)
            {
                providerInterstitial = amazon.interstitial;
                providerVideoRewarded = amazon.videoRewarded;
            }
            else if (platform == Platform.Facebook)
            {
                providerInterstitial = facebook.interstitial;
                providerVideoRewarded = facebook.videoRewarded;
            }
            else if (platform == Platform.Editor)
            {
                providerInterstitial = editor.interstitial;
                providerVideoRewarded = editor.videoRewarded;
            }
        }
        #endregion

        void Start()
        {
            providerInterstitial?.Init();
            providerVideoRewarded?.Init();
        }

        public void ShowInterstitial(bool considerRevenue = false, bool considerLevel = false, bool considerSessions = false,
            Action success = null, Action failed = null, Action dontReady = null)
        {
            if (considerRevenue && user.revenue > 0f)
            {
                Debug.Log("Ads - User is premium guy - No ads served");
                dontReady?.Invoke();
                return;
            }

            if (considerLevel && user.level < minLevel)
            {
                Debug.Log("Ads - User's level less than " + minLevel + " - No ads served");
                dontReady?.Invoke();
                return;
            }

            if (interstitialFrequency < 0)
            {
                Debug.Log("Ads - Ads is OFF - No ads served");
                dontReady?.Invoke();
                return;
            }

            if (considerSessions && sessions < interstitialFrequency)
            {
                Debug.Log("Ads - Too early - No ads served");

                sessions++;

                dontReady?.Invoke();
                return;
            }

            if (providerInterstitial == null || !providerInterstitial.isReadyInterstitial())
            {
                dontReady?.Invoke();
                return;
            }

            if (considerSessions) sessions = 0;

            ++interstitialFrequency;

            providerInterstitial.ShowInterstitial(success, failed);
        }

        public bool isReadyVideoRewarded()
        {
            return providerVideoRewarded == null ? false : providerVideoRewarded.isReadyVideoRewarded();
        }
        public void ShowVideoRewarded(Action success, Action failed)
        {
            if (providerVideoRewarded == null) return;

            providerVideoRewarded.ShowVideoRewarded(success, failed);
        }
    }
}