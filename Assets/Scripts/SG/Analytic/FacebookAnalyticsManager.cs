using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if FACEBOOK
using Facebook.Unity;
#endif

namespace SG
{
    public class FacebookAnalyticsManager : AnalyticsManager
    {
        public override void Init()
        {
            if (!build.facebook || platform == Platform.Editor) return;

            Analytic.onEvent.Add(Event);
            Analytic.onEventProperties.Add(EventProperties);
            Analytic.onEventRevenue.Add(EventRevenue);
        }

        void Event(string category, string name)
        {
#if FACEBOOK
            if (fb.isInit) FB.LogAppEvent(category + " - " + name);
#endif
        }

        void EventProperties(string name, Dictionary<string, object> properties)
        {
#if FACEBOOK
            if (fb.isInit) FB.LogAppEvent(name, null, properties);
#endif
        }

        void EventRevenue(PurchaseData data)
        {
#if FACEBOOK
            if (fb.isInit && !isDebug) FB.LogPurchase((float)data.iap.revenue, data.iap.currencyCode, new Dictionary<string, object>() {
                { AppEventParameterName.ContentID, data.iap.sku },
                { AppEventParameterName.ContentType, data.iap.category },
                { "transId", data.transaction } });
#endif
        }
    }
}