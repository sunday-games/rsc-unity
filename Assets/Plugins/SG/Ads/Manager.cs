using UnityEngine;
using System;

namespace SG.Ads
{
    public class Manager : MonoBehaviour
    {
        public static Manager instance;

        public static Action onStarted;
        public static Action onEnded;

        public int interstitialFrequency = 2;
        [HideInInspector]
        public int sessions = 0;

        #region SETUP
        Provider providerInterstitial = null;
        Provider providerRewarded = null;

        public AdsSet editor;
        public AdsSet iOS;
        public AdsSet android;
        public AdsSet facebook;
        public AdsSet amazon;
        [Serializable]
        public class AdsSet
        {
            public Provider interstitial;
            public Provider videoRewarded;
        }

        public void Init()
        {
            if (Utils.IsPlatform(Platform.iOS))
            {
                providerInterstitial = iOS.interstitial;
                providerRewarded = iOS.videoRewarded;
            }
            else if (Utils.IsPlatform(Platform.Android) && Configurator.Instance.androidStore == AndroidStore.GooglePlay)
            {
                providerInterstitial = android.interstitial;
                providerRewarded = android.videoRewarded;
            }
            else if (Utils.IsPlatform(Platform.Android) && Configurator.Instance.androidStore == AndroidStore.Amazon)
            {
                providerInterstitial = amazon.interstitial;
                providerRewarded = amazon.videoRewarded;
            }
            else if (Utils.IsPlatform(Platform.WebGL) && Configurator.Instance.webStore == WebStore.Facebook)
            {
                providerInterstitial = facebook.interstitial;
                providerRewarded = facebook.videoRewarded;
            }
            else if (Utils.IsPlatform(Platform.Editor))
            {
                providerInterstitial = editor.interstitial;
                providerRewarded = editor.videoRewarded;
            }
        }
        #endregion

        void Awake()
        {
            Manager.instance = this;
            providerInterstitial?.Init();
            providerRewarded?.Init();
        }

        public void ShowInterstitial(bool considerRevenue = false, bool considerLevel = false, bool considerSessions = false,
            Action success = null, Action failed = null, Action dontReady = null)
        {
            //if (considerRevenue && user.revenue > 0f)
            //{
            //    Debug.Log("Ads - User is premium guy - No ads served");
            //    dontReady?.Invoke();
            //    return;
            //}

            //if (considerLevel && user.level < minLevel)
            //{
            //    Debug.Log("Ads - User's level less than " + minLevel + " - No ads served");
            //    dontReady?.Invoke();
            //    return;
            //}

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

        public bool isReadyRewarded => providerRewarded == null ? false : providerRewarded.isReadyVideoRewarded();
        public void ShowRewarded(Action success = null, Action failed = null)
        {
            if (providerRewarded == null) return;

            providerRewarded.ShowRewarded(success, failed);
        }
    }
}