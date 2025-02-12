using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Analytics;
using Unity.Services.Core.Environments;

namespace SG.Analytics
{
    public class UnityAnalytics : Analytics
    {
        [UI.Button("Open")] public bool open;
        public void Open() => UI.Helpers.OpenLink($"https://cloud.unity.com/home/organizations/{Configurator.Instance.appInfo.unityOrg}/projects/{Configurator.Instance.appInfo.unityId}");

        private IAnalyticsService _analytics => AnalyticsService.Instance;

        public override void Init(bool production)
        {
            base.production = production;

            StartDataCollection();
            async void StartDataCollection()
            {
                try
                {
                    if (UnityServices.State == ServicesInitializationState.Uninitialized)
                        await UnityServices.InitializeAsync(
                            new InitializationOptions().SetEnvironmentName(production ? "production" : "qa"));

                    if (Configurator.Instance.Settings.analyticsCollection)
                        _analytics.StartDataCollection();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }

        public override void SetDataCollection(bool dataCollection)
        {
            if (dataCollection)
                _analytics.StartDataCollection();
            else
                _analytics.StopDataCollection();
        }

        public override bool Event(string name) => CustomEvent(name);

        public override bool Event(string category, string name) => CustomEvent(category + " - " + name);

        public override bool Event(string category, string name, string subname) => CustomEvent(category + " - " + name + " - " + subname);

        public override bool Event(string name, Dictionary<string, object> parameters) => CustomEvent(name, parameters);

        public override bool View(string name) => CustomEvent("Screen - " + name);

        public override bool Login(string id)
        {
            UnityServices.ExternalUserId = id;

            Log.Info($"UnityAnalytics - UserID: {_analytics.GetAnalyticsUserID()}");

            return true;
        }

        public override bool Revenue(PurchaseData data)
        {
            // https://docs.unity.com/ugs/en-us/manual/analytics/manual/record-transaction-events
            // If you are using Unity IAP, you don't need to record transactions manually

            if (logRevenueOnlyOnProduction && production == false)
                return false;

            var revenue = data.currency == Currency.USD ?
                data.revenue :
                ExchangeRates.Convert(data.revenue, data.currency, Currency.USD);

            Log.Info($"UnityAnalytics - EventRevenue: id={data.id}, revenue=${revenue}, item.name={data.item.name}, item.category={ data.item.category}");

            var transaction = new TransactionEvent
            {
                TransactionId = data.id,
                TransactionName = data.item.name,
                TransactionType = TransactionType.PURCHASE,
                // TransactionServer = TransactionServer.APPLE,
                // TransactionReceipt = "ewok9Ja81............991KS=="
                SpentRealCurrency = new TransactionRealCurrency
                {
                    RealCurrencyType = "USD",
                    RealCurrencyAmount = _analytics.ConvertCurrencyToMinorUnits("USD", (double) revenue)
                },
            };

            //transaction.ReceivedItems.Add(new TransactionItem
            //{
            //    ItemName = "Golden Battle Axe",
            //    ItemType = "Weapon",
            //    ItemAmount = 1
            //});
            transaction.ReceivedVirtualCurrencies.Add(new TransactionVirtualCurrency
            {
                VirtualCurrencyName = data.item.name,
                VirtualCurrencyType = VirtualCurrencyType.PREMIUM,
                VirtualCurrencyAmount = 100 // TODO
            });

            _analytics.RecordEvent(transaction);

            _analytics.Flush();

            return true;
        }

        bool CustomEvent(string name)
        {
            Log.Info($"UnityAnalytics - Event: {name}");

            _analytics.RecordEvent(name);

            return true;
        }

        //class MyEvent : Unity.Services.Analytics.Event
        //{
        //    public MyEvent() : base("myEvent") { }

        //    public string FabulousString { set { SetParameter("fabulousString", value); } }
        //    public int SparklingInt { set { SetParameter("sparklingInt", value); } }
        //    public float SpectacularFloat { set { SetParameter("spectacularFloat", value); } }
        //    public bool PeculiarBool { set { SetParameter("peculiarBool", value); } }
        //}

        bool CustomEvent(string name, Dictionary<string, object> parameters)
        {
            Log.Error("UnityAnalytics - CustomEvents not emplimented");

            return false;

            //Log.Info($"UnityAnalytics - Event: {name}" + (parameters == null ? "" : ", Parameters: " + parameters.ToDebugString()));

            //var myEvent = new MyEvent
            //{
            //    FabulousString = "hello there",
            //    SparklingInt = 1337,
            //    SpectacularFloat = 0.451f,
            //    PeculiarBool = true
            //};

            //AnalyticsService.Instance.RecordEvent(myEvent);

            //return true;
        }

        //bool AcquisitionSourceEvent(string name, Vector3 position)
        //{
        //    AnalyticsService.Instance.RecordEvent(new AcquisitionSourceEvent()
        //    {
        //        AcquisitionChannel = "Unity",
        //        AcquisitionCampaignId = "gwOfhjSjyiNfGdGlUjLK",
        //        AcquisitionCreativeId = "creative_name",
        //        AcquisitionCampaignName = "Game Name OS Country",
        //        AcquisitionProvider = "AppsFlyer",
        //        AcquisitionCampaignType = "CPI",
        //        AcquisitionCost = 12,
        //        AcquisitionCostCurrency = "EUR",
        //        AcquisitionNetwork = "Unity",
        //    });

        //    return true;
        //}
    }
}