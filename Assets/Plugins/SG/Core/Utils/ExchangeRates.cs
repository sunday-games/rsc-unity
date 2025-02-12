using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SG.BlockchainPlugin; // TODO remove

namespace SG
{
    public class ExchangeRates : MonoBehaviour
    {
        static ExchangeRates _instance;
        public static ExchangeRates instance => _instance ? _instance : _instance = FindFirstObjectByType<ExchangeRates>();

        public static Action onRatesUpdate;

        public static bool isRatesUpdated => rates != null;
        public static DateTime expirationDate { get; private set; }
        static Dictionary<string, Dictionary<string, object>> rates;

        static string server;
        static float delayTime = 10f;

        public static void Init(bool isMainnet)
        {
            var server = (isMainnet ? "https://exchange-rates.0x.games" : "https://exchange-rates-qa.0x.games") + "/currency-server/api/currencies";

            if (ExchangeRates.server == server)
                return;

            ExchangeRates.server = server;
            ExchangeRates.exchangeRatesPrefs = "Ox.BlockchainPlugin.ExchangeRates.data." + (isMainnet ? "prod" : "qa");

            expirationDate = DateTime.MinValue;
            rates = null;

            CacheLoad();

            instance.StopAllCoroutines();
            instance.StartCoroutine(DownloadExchangeRates());
        }

        static IEnumerator DownloadExchangeRates()
        {
            while (true)
            {
                if (DateTime.UtcNow > expirationDate)
                {
                    var download = new Download(server);
                    yield return download.RequestCoroutine();

                    if (!download.success)
                    {
                        yield return new WaitForSecondsRealtime(delayTime);
                        continue;
                    }

                    var data = Json.Deserialize(download.responseText) as Dictionary<string, object>;
                    // {"expirationDate":1548021926421,"currencies":{"EOS":{USD:39},"TRX":{USD:500000},"ETH":{USD:79706679419735}}}

                    if (data == null || !data.IsValue("expirationDate"))
                    {
                        yield return new WaitForSecondsRealtime(delayTime);
                        continue;
                    }

                    CacheSave(download.responseText);
                    SetData(data);
                }

                var seconds = (expirationDate - DateTime.UtcNow).TotalSeconds.ToFloat() + delayTime;
                Log.Info($"Exchange Rates - The next update is scheduled in {seconds / 60f / 60f} hours ({expirationDate.ToString("yyyy-MM-dd HH:mm:ss")} - {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")})");
                if (seconds > 0f)
                    yield return new WaitForSecondsRealtime(seconds);
            }
        }

        static void SetData(Dictionary<string, object> data)
        {
            expirationDate = data["expirationDate"].ToLong().ToDateTime();

            var currencies = data["currencies"] as Dictionary<string, object>;

            rates = new Dictionary<string, Dictionary<string, object>>();
            foreach (var crypto in currencies)
                rates.Add(crypto.Key, crypto.Value as Dictionary<string, object>);

            var log = "Exchange Rates - Updated from cointelegraph.com" + Const.lineBreak + 1.ToString(Currency.USD) +
                " = " + ConvertFormat(1, Currency.USD, Currency.EUR); // + ", " + ConvertFormat(1, Currency.USD, Currency.OXG);
            foreach (var blockchain in BlockchainManager.Instance.SupportedBlockchains)
                log += ", " + ConvertFormat(1, Currency.USD, blockchain.NativeToken);
            Log.Info(log);

            onRatesUpdate?.Invoke();
        }

        static string exchangeRatesPrefs;
        static void CacheLoad()
        {
            if (!PlayerPrefs.HasKey(exchangeRatesPrefs))
                return;

            Log.Info("Exchange Rates - Loading data from cache...");
            SetData(Json.Deserialize(PlayerPrefs.GetString(exchangeRatesPrefs)) as Dictionary<string, object>);
        }
        static void CacheSave(string data)
        {
            PlayerPrefs.SetString(exchangeRatesPrefs, data);
        }
        public static void CacheClear()
        {
            PlayerPrefs.DeleteKey(exchangeRatesPrefs);
        }

        // "EOS":{ "USD":20 },
        // 1 * 0.01 USD = 20 * 0.0001 EOS

        public static decimal Convert(decimal value, Currency from, Currency to)
        {
            if (!isRatesUpdated)
            {
                Log.Error("ExchangeRates - Data not loaded. Call 'Init' first!");
                return -1;
            }

            if (!from.IsFiat() && !rates.ContainsKey(from.Name))
            {
                Log.Error($"ExchangeRates - {from.Name} is not supported yet");
                return 0;
            }

            if (!to.IsFiat() && !rates.ContainsKey(to.Name))
            {
                Log.Error($"ExchangeRates - {to.Name} is not supported yet");
                return 0;
            }

            if (from == to)
                return value;
            else if (from.IsFiat() && !to.IsFiat())
                return GetRate(to, from) * (long) from.ToMin(value);
            else if (!from.IsFiat() && to.IsFiat())
                return to.FromMin(new BigInteger(value / GetRate(from, to)));
            else if (from.IsFiat() && to.IsFiat())
                return value * GetRate(Blockchain.current.NativeToken, from) / GetRate(Blockchain.current.NativeToken, to);
            else // if (!valueCurrency.IsFiat() && !resultCurrency.IsFiat())
            {
                if (rates[to.Name][Currency.USD.Name].ToDecimal() > rates[from.Name][Currency.USD.Name].ToDecimal())
                {
                    var rate = (long) (rates[to.Name][Currency.USD.Name].ToDecimal() / rates[from.Name][Currency.USD.Name].ToDecimal());
                    return value * from.FromMin(rate.ToBigInteger());
                }
                else
                {
                    return value * GetRate(to, Currency.USD) / GetRate(from, Currency.USD);
                }
            }
        }

        public static string ConvertFormat(decimal value, Currency from, Currency to, int decimals = -1) =>
            Convert(value, from, to).ToString(to, decimals);

        static decimal GetRate(Currency crypto, Currency fiat) =>
            crypto.FromMin(rates[crypto.Name][fiat.Name].ToBigInteger());
    }
}