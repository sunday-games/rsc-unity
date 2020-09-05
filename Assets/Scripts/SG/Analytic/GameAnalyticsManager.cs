using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if GAME_ANALYTICS
using GameAnalyticsSDK;
#endif

namespace SG
{
    public class GameAnalyticsManager : AnalyticsManager
    {
        public IOS iOS;
        [System.Serializable]
        public class IOS
        {
            public string gameKey;
            public string secretKey;
        }
        public Android android;
        [System.Serializable]
        public class Android
        {
            public string gameKey;
            public string secretKey;
        }
        public Amazon amazon;
        [System.Serializable]
        public class Amazon
        {
            public string gameKey;
            public string secretKey;
        }
        public FacebookCanvas facebook;
        [System.Serializable]
        public class FacebookCanvas
        {
            public string gameKey;
            public string secretKey;
        }

        public void Setup()
        {
#if GAME_ANALYTICS
        settings.Build[0] = build.version;
        settings.UpdateGameKey(0, iOS.gameKey);
        settings.UpdateSecretKey(0, iOS.secretKey);
        settings.Build[1] = build.version;
        settings.UpdateGameKey(1, build.androidStore == AndroidStore.Amazon ? amazon.gameKey : android.gameKey);
        settings.UpdateSecretKey(1, build.androidStore == AndroidStore.Amazon ? amazon.secretKey : android.secretKey);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(settings);
#endif
#endif
        }

#if GAME_ANALYTICS
    public GameAnalyticsSDK.Settings settings;

    public override void Init()
    {
        if (!build.gameAnalytics) return;

        Analytic.onEvent2.Add(Event2);
        Analytic.onEvent3.Add(Event3);
        Analytic.onEventRevenue.Add(EventRevenue);
        Analytic.onEventLevelUp.Add(EventLevelUp);
        Analytic.onEventAchievement.Add(EventAchievement);
    }

    void Event2(string category, string name)
    {
        GameAnalytics.NewDesignEvent(category + ":" + name);
    }

    void Event3(string category, string action, string label)
    {
        GameAnalytics.NewDesignEvent(category + ":" + action + ":" + label);
    }

    const string defaultAffiliation = "default";
    void EventRevenue(PurchaseData data)
    {
            if (isDebug) return;

#if UNITY_IPHONE
            GameAnalytics.NewBusinessEventIOS(data.iap.currencyCode, (int)(data.iap.revenue * 100), data.iap.category, data.iap.name, data.iap.category, data.receipt);
#elif UNITY_ANDROID
        if (platform == Platform.GooglePlay)
            GameAnalytics.NewBusinessEventGooglePlay(data.iap.currencyCode, (int)(data.iap.revenue * 100), data.iap.category, data.iap.name, data.iap.category, data.receipt, data.signature);
#endif
    }

    enum Metrics : int { Coins = 1, PermanentRecord = 2 }
    void EventLevelUp(string level, string coins)
    {
        GameAnalytics.NewProgressionEvent(GA_Progression.GAProgressionStatus.GAProgressionStatusComplete, Names.Level, level);
        GameAnalytics.NewProgressionEvent(GA_Progression.GAProgressionStatus.GAProgressionStatusComplete, Names.Level, level.ToString());
    }

    void EventAchievement(string achievement, string level)
    {
        GameAnalytics.NewDesignEvent(Names.Achievements + ":" + achievement + ":" + level);
    }
#endif
    }
}