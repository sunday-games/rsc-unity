using System.Collections.Generic;

using UnityEngine.Analytics;

namespace SG
{
    public class UnityAnalyticsManager : AnalyticsManager
    {
        public override void Init()
        {
            if (!build.unityAnalytics) return;

            Analytic.onEvent.Add(Event);
            Analytic.onEventProperties.Add(EventProperties);
            Analytic.onEventRevenue.Add(EventRevenue);
        }

        void Event(string category, string name)
        {
            Analytics.CustomEvent(category + " - " + name, new Dictionary<string, object>());
        }

        void EventProperties(string name, Dictionary<string, object> properties)
        {
            Analytics.CustomEvent(name, properties);
        }

        void EventRevenue(PurchaseData data)
        {
            if (isDebug) return;

            if (platform == Platform.AppStore)
                Analytics.Transaction(data.iap.name, (decimal)data.iap.revenue, data.iap.currencyCode, data.receipt, null);
            else if (platform == Platform.GooglePlay)
                Analytics.Transaction(data.iap.name, (decimal)data.iap.revenue, data.iap.currencyCode, data.receipt, data.signature);
        }
    }
}