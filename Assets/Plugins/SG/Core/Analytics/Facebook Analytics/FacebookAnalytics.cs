using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//#if FACEBOOK
//using Facebook.Unity;
//#endif

namespace SG.Analytics
{
    public class FacebookAnalytics : Analytics
    {
        [Space(10)]
        public string pixelProdId;
        public string pixelTestId;

        [UI.Button("OpenPixelAnalytics")] public bool openPixelAnalytics;
        public void OpenPixelAnalytics()
        {
            var id = Configurator.production ? pixelProdId : pixelTestId;
            UI.Helpers.OpenLink($"https://www.facebook.com/analytics/{id}/overview");
        }

#if FACEBOOK && !UNITY_EDITOR
        [Space(10)]
        [UI.Button("OpenAnalytics")] public bool openAnalytics;
        public void OpenAnalytics()
        {
            var fb = FindObjectOfType<FacebookManager>();
            var id = Configurator.production ? fb.appProdId : fb.appTestId;
            UI.Helpers.OpenLink($"https://www.facebook.com/analytics/{id}/overview");
        }

        public override void Init(bool production)
        {
            base.production = production;
        }

        public override bool Event(string name) { return LogAppEvent(name); }

        public override bool Event(string category, string name) { return LogAppEvent(category + " - " + name); }

        public override bool Event(string category, string name, string subname) { return LogAppEvent(category + " - " + name + " - " + subname); }

        public override bool Event(string name, Dictionary<string, object> parameters) { return LogAppEvent(name, parameters: parameters); }

        public override bool View(string name)
        {
            if (!FB.IsInitialized)
            {
                Log.Error($"FacebookAnalytics - FB is not initialized yet");
                return false;
            }

            name = FixName(name);

            Log.Info($"FacebookAnalytics - View: {name}");

            FB.LogAppEvent("View", parameters: new Dictionary<string, object> { ["name"] = name });

            FacebookWebView(name);

            return true;
        }

        public override bool Revenue(PurchaseData data)
        {
            if (!FB.IsInitialized)
            {
                Log.Error($"FacebookAnalytics - FB is not initialized yet");
                return false;
            }

            if (logRevenueOnlyOnProduction && production == false)
                return false;

            var productName = !data.item.id.IsEmpty() ?
                data.item.id :
                data.item.name;

            var revenue = data.currency == Currency.USD ?
                data.revenue :
                ExchangeRates.Convert(data.revenue, data.currency, Currency.USD);

            Log.Info($"FacebookAnalytics - Revenue: id={data.id}, revenue=${revenue}, productId={productName}, category={ data.item.category}");

            FB.LogPurchase((decimal)revenue, Currency.USD.code,
                new Dictionary<string, object>() {
                    { AppEventParameterName.Description, productName },
                    { "transactionId", data.id },
                });

            FacebookWebRevenue(data.id, productName, Currency.USD.code, revenue);

            return true;
        }

        bool LogAppEvent(string name, float? value = null, Dictionary<string, object> parameters = null)
        {
            if (!FB.IsInitialized)
            {
                Log.Error($"FacebookAnalytics - FB is not initialized yet");
                return false;
            }

            name = FixName(name);

            Log.Info($"FacebookAnalytics - Event: logEvent={name}" +
                (value == null ? "" : ", valueToSum=" + value) +
                (parameters == null ? "" : ", parameters=" + parameters.ToDebugString()));

            FB.LogAppEvent(name, value, parameters);

            if (parameters == null)
                FacebookWebEvent(name);
            else
                FacebookWebEventParameters(name, Json.Serialize(parameters));

            return true;
        }

        string FixName(string name)
        {
            if (name.Length <= 40) return name;

            Log.Error($"FacebookAnalytics - Invalid event name: '{name}'. It must be:" + Const.lineBreak +
                "1) 1-40 characters" + Const.lineBreak +
                "2) contain only alphanumerics, '_', '-', ' '" + Const.lineBreak +
                "3) starting with alphanumeric or '_'");
            return name.Substring(0, 40);
        }

#if UNITY_WEBGL
        [DllImport("__Internal")] static extern void FacebookWebEvent(string name);
        [DllImport("__Internal")] static extern void FacebookWebEventParameters(string name, string parameters);
        [DllImport("__Internal")] static extern void FacebookWebRevenue(string transactionId, string productName, string productCurrency, double productRevenue);
        [DllImport("__Internal")] static extern void FacebookWebRevenueProduct(string transactionId, string productId, string productCurrency, double productRevenue);
        [DllImport("__Internal")] static extern void FacebookWebView(string name);
#else
        static void FacebookWebEvent(string name) { }
        static void FacebookWebEventParameters(string name, string parameters) { }
        static void FacebookWebRevenue(string transactionId, string productName, string productCurrency, double productRevenue) { }
        static void FacebookWebRevenueProduct(string transactionId, string productId, string productCurrency, double productRevenue) { }
        static void FacebookWebView(string name) { }
#endif

#endif // FACEBOOK
    }
}