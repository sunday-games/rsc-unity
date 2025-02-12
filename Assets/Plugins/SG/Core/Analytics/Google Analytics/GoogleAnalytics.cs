using System.Runtime.InteropServices;

namespace SG.Analytics
{
    public class GoogleAnalytics : Analytics
    {
        [UI.Button("Open")] public bool open;
        public void Open() { UI.Helpers.OpenLink("https://analytics.google.com/analytics/web"); }

        public string releaseId;
        public string debugId;

        public bool sendPageViewOnLoadingScreen = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        public override void Init(bool production)
        {
            base.production = production;
        }

        public override bool Event(string name) { return Event2("Undefined", name); }

        public override bool Event(string category, string name) { return Event2(category, name); }

        public override bool Event(string category, string name, string subname) { return Event3(category, name, subname); }

        public override bool View(string name)
        {
            Log.Info($"GoogleAnalytics - View: {name}");

            GoogleWebView(name);

            return true;
        }

        public override bool Login(string id)
        {
            return SetUser("userId", id);
        }

        public override bool Revenue(PurchaseData data)
        {
            if (logRevenueOnlyOnProduction && production == false) return false;

            var revenue = data.currency == Currency.USD ?
                data.revenue :
                ExchangeRates.Convert(data.revenue, data.currency, Currency.USD);

            Log.Info($"GoogleAnalytics - EventRevenue: id={data.id}, revenue=${revenue}, item.name={data.item.name}, item.category={ data.item.category}");

            GoogleWebRevenue(data.id, revenue.ToString(), data.item.name, data.item.category);

            return true;
        }

        bool Event2(string category, string name)
        {
            if (Utils.IsPlatform(Platform.Editor)) return false;

            Log.Info($"GoogleAnalytics - Event: category={category}, action={name}");

            GoogleWebEvent2(category, name);

            return true;
        }

        bool Event3(string category, string name, string subname)
        {
            Log.Info($"GoogleAnalytics - Event: category={category}, action={name}, label={subname}");

            GoogleWebEvent3(category, name, subname);

            return true;
        }

        bool SetUser(string key, string value)
        {
            Log.Info($"GoogleAnalytics - SetUser: {key}={value}");

            GoogleWebSetUser(key, value);

            return true;
        }

        [DllImport("__Internal")] static extern void GoogleWebEvent2(string category, string action);
        [DllImport("__Internal")] static extern void GoogleWebEvent3(string category, string action, string label);
        [DllImport("__Internal")] static extern void GoogleWebEvent4(string category, string action, string label, int value);
        [DllImport("__Internal")] static extern void GoogleWebView(string view);
        [DllImport("__Internal")] static extern void GoogleWebSetUser(string key, string value);
        [DllImport("__Internal")] static extern void GoogleWebRevenue(string transactionId, string revenue, string itemName, string itemCategory);
#endif
    }
}