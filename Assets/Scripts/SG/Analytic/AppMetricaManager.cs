using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public class AppMetricaManager : AnalyticsManager
    {
        public static AppMetricaManager instance { get { return FindObjectOfType(typeof(AppMetricaManager)) as AppMetricaManager; } }

        public Test test;
        [System.Serializable]
        public class Test
        {
            public string apiKey;
        }
        public IOS iOS;
        [System.Serializable]
        public class IOS
        {
            public string apiKey;
        }
        public Android android;
        [System.Serializable]
        public class Android
        {
            public string apiKey;
        }

        public void Setup()
        {
#if APP_METRICA
        var appMetrica = FindObjectOfType(typeof(AppMetrica)) as AppMetrica;
#if UNITY_IOS
	    appMetrica.APIKey = iOS.apiKey;
#elif UNITY_ANDROID
        appMetrica.APIKey = android.apiKey;
#endif
        if (isDebug) appMetrica.APIKey = test.apiKey;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(appMetrica);
#endif
#endif
        }

#if APP_METRICA
    public IYandexAppMetrica metrica { get { return AppMetrica.Instance; } }

    public override void Init()
    {
        if (!build.appMetrica) return;

        metrica.SetCustomAppVersion(build.version);

        Analytic.onEvent.Add(Event);
        Analytic.onEventProperties.Add(EventProperties);
        Analytic.onEventRevenue.Add(EventRevenue);
    }

    void Event(string category, string name)
    {
        metrica.ReportEvent(category + " - " + name);
    }

    void EventProperties(string name, Dictionary<string, object> properties)
    {
        metrica.ReportEvent(name, new Hashtable(properties));
    }

    void EventRevenue(PurchaseData data)
    {
        metrica.ReportEvent(Names.Purchase, new Hashtable() {
            { Names.Name, data.iap.name },
            { Names.Revenue, data.iap.revenueUSD },
            { "FirstDate", user.firstDate },
            { "FirstVersion", user.firstVersion },
            { Names.Level, user.level },
            { "Coins", user.coins },
            { "Spins", user.spins },
            { "PermanentRecord", user.permanentRecord },
            { "Invited", user.invitedFriends.Count },
            { "MaxCatLevel", user.maxCatLevel },
            { "AverageCatLevel", user.averageCatLevel },
            { "Collection", user.collection.Count },
        });
    }
#endif
    }
}