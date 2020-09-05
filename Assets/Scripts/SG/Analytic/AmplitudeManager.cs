using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public class AmplitudeManager : AnalyticsManager
    {
        public Test test;
        [System.Serializable]
        public class Test
        {
            public string apiKey; // 5dc9bf8d67b5e01f1511d563cadad40f
        }
        public IOS iOS;
        [System.Serializable]
        public class IOS
        {
            public string apiKey; // c7d17f859ddc983e0b64874af09252e4
        }
        public Android android;
        [System.Serializable]
        public class Android
        {
            public string apiKey; // afe6403990a3f1e61694d9869d4a9def
        }

#if AMPLITUDE
    Amplitude amplitude { get { return Amplitude.Instance; } }

    public override void Init()
    {
        if (!build.amplitude) return;

        string apiKey = "";
        if (platform == Platform.AppStore) apiKey = iOS.apiKey;
        else if (platform == Platform.GooglePlay) apiKey = android.apiKey;
        else if (platform == Platform.Editor) apiKey = test.apiKey;

        if (string.IsNullOrEmpty(apiKey)) return;

        if (isDebug)
        {
            apiKey = test.apiKey;
            amplitude.logging = true;
        }

        LogDebug("Analytic - Amplitude Init...");
        amplitude.init(apiKey);

        Analytic.onEventImportant.Add(Event);
        Analytic.onEventPropertiesImportant.Add(EventProperties);
        Analytic.onEventRevenue.Add(EventRevenue);
        Analytic.onEventUserLogin.Add(UserLogin);
        Analytic.onSetUserProperties.Add(SetUserProperties);
    }

    void Event(string category, string name)
    {
        amplitude.logEvent(category + " - " + name);
    }

    void EventProperties(string name, Dictionary<string, object> properties)
    {
        amplitude.logEvent(name, properties);
    }

    void EventRevenue(PurchaseData data)
    {
        amplitude.logRevenue(data.iap.name, 1, data.iap.revenueUSD, data.receipt, data.signature);
    }

    void UserLogin()
    {
        if (user.isId) amplitude.setUserId(user.id);
    }

    void SetUserProperties(Dictionary<string, object> properties)
    {
        amplitude.setUserProperties(properties);
    }
#endif
    }
}