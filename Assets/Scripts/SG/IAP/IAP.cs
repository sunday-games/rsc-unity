using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public enum IAPCategory { Consumables }
    public enum IAPTier { usd1, usd2, usd3, usd4, usd5, usd10 }
    public class IAP : Core
    {
        public static string defaultCurrency = "USD";
        public static Dictionary<IAPTier, double> dictRevenueUSD = new Dictionary<IAPTier, double>
        {
            { IAPTier.usd1, 0.7 },
            { IAPTier.usd2, 1.4 },
            { IAPTier.usd3, 2.1 },
            { IAPTier.usd4, 2.8 },
            { IAPTier.usd5, 3.5 },
            { IAPTier.usd10, 7.0 },
        };
        public static Dictionary<IAPTier, double> dictPriceUSD = new Dictionary<IAPTier, double>
        {
            { IAPTier.usd1, 0.99 },
            { IAPTier.usd2, 1.99 },
            { IAPTier.usd3, 2.99 },
            { IAPTier.usd4, 3.99 },
            { IAPTier.usd5, 4.99 },
            { IAPTier.usd10, 9.99 },
        };
        public static Dictionary<IAPTier, double> dictPriceRUB = new Dictionary<IAPTier, double>
        {
            { IAPTier.usd1, 49 },
            { IAPTier.usd2, 99 },
            { IAPTier.usd3, 149 },
            { IAPTier.usd4, 199 },
            { IAPTier.usd5, 249 },
            { IAPTier.usd10, 499 },
        };

        public static IAP[] IAPs;

        public static IAP FromSKU(string sku)
        {
            foreach (IAP iap in IAPs) if (iap.sku == sku) return iap;
            return null;
        }

        public static IAP FromName(string name)
        {
            foreach (IAP iap in IAPs) if (iap.name == name) return iap;
            return null;
        }

        public IAPCategory category;
        public IAPTier tier;
        [Space(10)]
        public IAPType type;
        public long amount;
        public long discount;

        public string sku { get { return iapManager.skuPrefix + name; } }
        public double revenueUSD { get { return dictRevenueUSD[tier]; } }
        public double priceUSD { get { return dictPriceUSD[tier]; } }
        public double priceRUB { get { return dictPriceRUB[tier]; } }

        [HideInInspector]
        public string priceFormated = string.Empty;
        [HideInInspector]
        public double price = 0.0;
        [HideInInspector]
        public double revenue { get { return price * 0.7; } }
        [HideInInspector]
        public string currencyCode = string.Empty;

        public string priceLocalized
        {
            get
            {
                if (iapManager.store == null || !iapManager.isInitialized)
                    return Localization.language == SystemLanguage.Russian ? priceRUB + " руб" : "$" + priceUSD;

                string localized = string.IsNullOrEmpty(priceFormated) ? price + " " + currencyCode : priceFormated;

                if (localized.Contains(".00")) localized = localized.Replace(".00", string.Empty);
                else if (localized.Contains(",00")) localized = localized.Replace(",00", string.Empty);

                if (localized.Contains("RUB")) localized = localized.Replace("RUB", "руб");
                else if (localized.Contains("руб.")) localized = localized.Replace("руб.", "руб");
                else if (localized.Contains(" USD")) localized = "$" + localized.Replace(" USD", string.Empty);
                else if (localized.Contains("USD")) localized = "$" + localized.Replace("USD", string.Empty);
                else if (localized.Contains(" GBP")) localized = "£" + localized.Replace(" GBP", string.Empty);
                else if (localized.Contains("GBP")) localized = "£" + localized.Replace("GBP", string.Empty);
                else if (localized.Contains(" EUR")) localized = "€" + localized.Replace(" EUR", string.Empty);
                else if (localized.Contains("EUR")) localized = "€" + localized.Replace("EUR", string.Empty);

                return localized;
            }
        }
    }
}