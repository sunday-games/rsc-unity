using System;
using System.Numerics;
using System.Globalization;
using System.Collections.Generic;

namespace SG
{
    [Serializable]
    public class CurrencyValue
    {
        public double Amount;
        public string CurrencyCode;

        public Currency Currency
        {
            get
            {
                return Currency.Currencies.GetValue(CurrencyCode) ??
                    // TODO Player.Currencies ??
                    BlockchainPlugin.BlockchainManager.GetToken(CurrencyCode);
            }
        }

        public CurrencyValue(double amount, string currencyCode)
        {
            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public CurrencyValue(decimal amount, string currencyCode)
        {
            Amount = amount.ToDouble();
            CurrencyCode = currencyCode;
        }

        public CurrencyValue(string amount, string currencyCode)
        {
            Amount = amount.ToDouble();
            CurrencyCode = currencyCode;
        }

        public string ToString(int decimals = -1) =>
            Currency != null ? Amount.ToDecimal().ToString(Currency, decimals: decimals) : Amount + " " + CurrencyCode;
    }

    public class Currency
    {
        public static Currency USD = new Currency(name: "USD", decimals: 2, sign: "$");
        public static Currency EUR = new Currency(name: "EUR", decimals: 2, sign: "€");
        public static Currency GBP = new Currency(name: "GBP", decimals: 2, sign: "£");
        public static Currency JPY = new Currency(name: "JPY", decimals: 2, sign: "¥");
        public static Currency RUB = new Currency(name: "RUB", decimals: 2, sign: "₽");

        public static Dictionary<string, Currency> Currencies = new()
        {
            [USD.Name] = USD,
            [EUR.Name] = EUR,
            [GBP.Name] = GBP,
            [JPY.Name] = JPY,
            [RUB.Name] = RUB,
        };

        public static implicit operator bool(Currency с) => с != null;

        public string Name;
        public int Decimals;
        public string Sign;

        public string NameMin;

        public decimal UsdRate;

        private decimal _multiplierDe;

        public Currency(string name, int decimals = 0, string sign = null, string nameMin = null)
        {
            Name = name;
            Decimals = decimals;
            Sign = sign;
            NameMin = nameMin;

            _multiplierDe = (decimal) Math.Pow(10, decimals);
        }

        public bool IsFiat() => this == USD || this == EUR;

        public virtual decimal FromMin(BigInteger min) => min.ToDecimal() / _multiplierDe;
        public virtual decimal FromMin(long min) => min / _multiplierDe;

        public virtual BigInteger ToMin(decimal value) => BigInteger.Parse(string.Format("{0:F0}", value * _multiplierDe));
        public virtual BigInteger ToMin(string value) => ToMin(value.ToDecimal());

        public override string ToString() => Name;

#if SG_EXCHANGERATES
        public virtual string ToStringTwoDollarAccuracy(decimal value)
        {
            if (this == USD) return ToString(value);

            var usd = ExchangeRates.Convert(2, USD, this);
            var decimals = usd > 1 ? 0 : Math.Floor(1 / usd).ToStringFormated().Length;
            return ToString(value, decimals);
        }
#endif
    }

    public static class CurrencyEx
    {
        public static string ToString(this decimal value, Currency currency, int decimals = -1, bool sign = true, bool code = true)
        {
            var text = value.ToString(Utils.GetStringFormater(decimals < 0 ? currency.Decimals : decimals, '#'), CultureInfo.InvariantCulture);

            if (sign && currency.Sign.IsNotEmpty())
                return currency.Sign + text;
            else if (code)
                return $"{text} {currency.Name}";
            else
                return text;
        }

        public static string ToString(this BigInteger value, Currency currency, int decimals = -1, bool sign = true, bool code = true) =>
            currency.FromMin(value).ToString(currency, decimals, sign, code);
        public static string ToString(this int value, Currency currency, int decimals = -1, bool sign = true, bool code = true) =>
            ((decimal) value).ToString(currency, decimals, sign, code);

        public static string ToStringFormated2(this decimal value, Currency currency) =>
            value.ToStringFormated2() + " " + currency.Name;
    }
}