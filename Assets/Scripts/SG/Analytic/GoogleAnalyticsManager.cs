using UnityEngine;
using System;
using System.Collections.Generic;

namespace SG
{
    public class GoogleAnalyticsManager : AnalyticsManager
    {
        public GoogleAnalyticsSettings iOS;
        public GoogleAnalyticsSettings android;
        public GoogleAnalyticsSettings amazon;
        public GoogleAnalyticsSettings facebook;
        [Serializable]
        public class GoogleAnalyticsSettings { public string id; }

        public void Setup()
        {
#if GOOGLE_ANALYTICS
            ga.UncaughtExceptionReporting = true;
            ga.sendLaunchEvent = true;
            ga.enableAdId = true;

            ga.productName = build.productName;
            ga.bundleIdentifier = build.ID;
            ga.bundleVersion = build.version;
            ga.IOSTrackingCode = iOS.id;
            ga.otherTrackingCode = facebook.id;
            ga.androidTrackingCode = build.androidStore == BuildSettings.AndroidStore.Amazon ? amazon.id : android.id;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(ga);
#endif

#endif
        }

#if GOOGLE_ANALYTICS
        [Space(10)]
        public GoogleAnalyticsV4 ga;

        public override void Init()
        {
            if (!build.googleAnalytics) return;

            try
            {
                ga.StartSession();
            }
            catch (Exception e)
            {
                LogError("Google Analytics - StartSession Exception: " + e.Message);
                build.googleAnalytics = false;
                return;
            }

            Analytic.onEvent.Add(Event);
            Analytic.onEventProperties.Add(EventProperties);
            Analytic.onEventRevenue.Add(EventRevenue);
        }

        void OnApplicationQuit()
        {
            if (build.googleAnalytics) ga.StopSession();
        }

        void Event(string category, string action)
        {
            ga.LogEvent(new EventHitBuilder().SetEventCategory(category).SetEventAction(action));
        }

        void Event(string category, string action, string label)
        {
            ga.LogEvent(new EventHitBuilder().SetEventCategory(category).SetEventAction(action).SetEventLabel(label));
        }

        void EventProperties(string name, Dictionary<string, object> properties)
        {
            if (properties.Count == 1)
                foreach (var property in properties)
                    Event(name, property.Key, property.Value.ToString());
        }

        //void Screen(string screenName)
        //{
        //    ga.LogScreen(screenName);
        //}

        const string defaultAffiliation = "default";
        void EventRevenue(PurchaseData data)
        {
            if (isDebug) return;

            ga.LogTransaction(data.transaction, defaultAffiliation, data.iap.revenue, 0.0, 0.0, data.iap.currencyCode);
            ga.LogItem(data.transaction, data.iap.name, data.iap.sku, data.iap.category.ToString(), data.iap.price, 1, data.iap.currencyCode);
        }
#endif
    }
}