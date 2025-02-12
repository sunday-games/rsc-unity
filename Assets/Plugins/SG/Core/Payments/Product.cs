#if SG_PAYMENTS
using System;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.Payments
{
    // TODO Product structure changed on server - needs changes here
    [Serializable]
    public class Product
    {
        public static implicit operator bool(Product product) => product != null;

        public string id;
        public string name;
        public string description;
        public UnityEngine.Sprite image;
        public string imageUrl;
        public string appleId;
        public string googleId;
        public double price;

        [NonSerialized]
        public CurrencyValue storePrice;

        public Reward reward;
        [Serializable]
        public class Reward : IDictionarizable<Reward>
        {
            public CurrencyValue[] currency;
            public Function[] functions;

            protected Reward() { }
            public Reward FromDictionary(DictSO data)
            {
                //currency = data["currency"].ToClassArray<CurrencyValue>();
                functions = data["functions"].ToClassArray<Function>();
                return this;
            }
            public DictSO ToDictionary()
            {
                return new DictSO {
                    //{ "currency", currency.ToObjectList() },
                    { "functions", functions.ToObjectList() },
                };
            }

            [Serializable]
            public class Function : IDictionarizable<Function>
            {
                public string name;
                public Parameters parameters;
                public int callAmount = 1;

                [Serializable]
                public class Parameters
                {
                    public int type;
                }

                protected Function() { }
                public Function FromDictionary(DictSO data)
                {
                    name = data["name"].ToString();
                    parameters = new Parameters() { type = (data["parameters"] as DictSO)["type"].ToInt() };
                    callAmount = data["callAmount"].ToInt();
                    return this;
                }
                public DictSO ToDictionary()
                {
                    return new DictSO {
                        { "name", name },
                        { "parameters", new DictSO { { "type", parameters.type }, } },
                        { "callAmount", callAmount },
                    };
                }
            }

            //public double GetCurrencyAmount(Blockchain blockchain)
            //{
            //    foreach (var c in currency)
            //        if (c.blockchain == blockchain.name)
            //            return c.amount;
            //    return 0.0;
            //}

            public Function.Parameters GetParameters(string type)
            {
                foreach (var function in functions)
                    if (function.name == type)
                        return function.parameters;
                return null;
            }
        }

        public int Index => Configurator.Instance.appInfo.products.IndexOf(this);

        public decimal GetRevenue(PaymentProvider provider) => (decimal) price - GetFee(provider);
        public decimal GetFee(PaymentProvider provider) => (decimal) price * provider.GetFee(this);
    }
}
#endif