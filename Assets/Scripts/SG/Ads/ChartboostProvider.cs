using UnityEngine;
using System;
using System.Collections;

#if CHARTBOOST
using ChartboostSDK;
#endif

namespace SG
{
    public class ChartboostProvider : AdsProvider
    {
        public IOS iOS;
        [Serializable]
        public class IOS
        {
            public string ID;
            // MAIN 54eb403bc909a6649cb9afc0
            // TNT 559a7e1a43150f5fd27dd8b9
            public string signature;
            // MAIN 986cf7177d1c0a87dce9dcd5431b46bb9498f75e
            // TNT 554f0d5d037ab73c825b405c4dab00820f8d5c0f
        }
        public Android android;
        [Serializable]
        public class Android
        {
            public string ID;
            // MAIN 54eb367f0d6025177453c41e
            // TNT 559a7e1a43150f5fd27dd8ba
            public string signature;
            // MAIN b9149ad348ff523ca8f8a2ba9475f0211ce8a308
            // TNT dbb768783ff31c5debf115518850c549103927c6
        }
        public Amazon amazon;
        [Serializable]
        public class Amazon
        {
            public string ID;
            // MAIN 54eb462b0d6025178cf68dd6
            // Exotron 55376ce90d6025609bd3d152
            public string signature;
            // MAIN 6c79d03af741d687f266b0b524933e9339c3055d
            // Exotron 7d90177594ec6873377f66ef33a8f3f5cf29eff3
        }

        public override void Init()
        {
#if CHARTBOOST
        if (isInit) return;
        else isInit = true;

        Debug.Log("Ads - Chartboost - Init...");

        settings.isLoggingEnabled = Debug.isDebugBuild;

        Chartboost.didFailToLoadInterstitial += didFailToLoadInterstitial;
        Chartboost.didDismissInterstitial += didDismissInterstitial;
        Chartboost.didCloseInterstitial += didCloseInterstitial;
        Chartboost.didClickInterstitial += didClickInterstitial;
        Chartboost.didCacheInterstitial += didCacheInterstitial;
        Chartboost.shouldDisplayInterstitial += shouldDisplayInterstitial;
        Chartboost.didDisplayInterstitial += didDisplayInterstitial;

        Chartboost.cacheInterstitial(CBLocation.Default);
#endif
        }

#if CHARTBOOST
    [Space(10)]
    public CBSettings settings;

    bool isInterstitialClick = false;

    public override bool isReadyInterstitial()
    {
        bool isReady = Chartboost.hasInterstitial(CBLocation.Default);

        Debug.Log("Ads - Chartboost - Interstitial isReady - " + (isReady ? "YES" : "NO"));

        Analytic.Event("Ads", "Chartboost Interstitial", isReady ? "YES" : "NO");

        if (!isReady) Chartboost.cacheInterstitial(CBLocation.Default);

        return isReady;
    }

    Action interstitialSuccess = null;
    Action interstitialFailed = null;
    public override void ShowInterstitial(Action success, Action failed)
    {
        interstitialSuccess = success;
        interstitialFailed = failed;

        isInterstitialClick = false;

        Chartboost.showInterstitial(CBLocation.Default);
    }

    void didFailToLoadInterstitial(CBLocation location, CBImpressionError error)
    {
        Debug.Log(string.Format("Ads - Chartboost Interstitial - Fail To Load ({0}): {1}", location, error));
    }
    void didCacheInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Cached (" + location + ")");
    }
    bool shouldDisplayInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Showing... (" + location + ")");

        ui.Block();

        return true;
    }
    void didDisplayInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Displayed (" + location + ")");
    }
    void didClickInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Clicked (" + location + ")");

        isInterstitialClick = true;
    }
    void didDismissInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Dismissing... (" + location + ")");

        ui.Unblock();

        if (isInterstitialClick)
        {
            Analytic.Event("Ads", "Chartboost Interstitial Result", "Click");
            if (interstitialSuccess != null) interstitialSuccess();
        }
        else
        {
            Analytic.Event("Ads", "Chartboost Interstitial Result", "Dismiss");
            if (interstitialFailed != null) interstitialFailed();
        }
    }
    void didCloseInterstitial(CBLocation location)
    {
        Debug.Log("Ads - Chartboost Interstitial - Closed (" + location + ")");
    }
#endif
    }
}
