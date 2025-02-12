#if SG_PAYMENTS
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SG.Payments
{
    public abstract class PaymentProvider : MonoBehaviour
    {
        public static List<Type> GetSupportedProviders()
        {
            var types = new List<Type>();

#if (PAYPAL || CARDPAY) && UNITY_EDITOR
            types.Add(typeof(DebugProvider));
#endif

#if EPIC_GAMES && !EOS_DISABLE && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
            types.Add(typeof(Epic));
#endif

#if XSOLLA
            types.Add(typeof(Xsolla));
#endif

#if PAYPAL && UNITY_WEBGL
            types.Add(typeof(PayPal));
#endif
#if CARDPAY && UNITY_WEBGL
            types.Add(typeof(CardPay));
#endif

#if UNITY_PURCHASING && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
            types.Add(typeof(Mobile));
#endif
            return types;
        }

        public static implicit operator bool(PaymentProvider provider) => provider != null;

        public static Dictionary<Names, PaymentProvider> All;
        public static PaymentProvider Default => All.First().Value;

        public static PaymentProvider Deserialize(string name) =>
            name.IsEnum<Names>() ? All[name.ToEnum<Names>()] : null;

        [HideInInspector]
        public Names Name;
        public enum Names { PAYPAL, CARDPAY, APPLE, GOOGLE, XSOLLA, EPIC, DEBUG }

        public virtual bool IsCanBuy(Product product) { return true; }

        public virtual CurrencyValue GetPrice(Product product) => default;

        public virtual decimal GetFee(Product product) => default;

        protected Configurator config = Configurator.Instance;
        public Sprite iconColor => config.GetIconColor(Name.ToString());
        public Sprite iconWhite => config.GetIconWhite(Name.ToString());

        public virtual void Setup() { }

        public virtual bool IsInit() => false;

        public virtual void Init(Action<Result> callback = null) => callback?.Invoke(Result.Error("Not implemented"));

        public virtual bool IsLogin() => false;

        public virtual void Login(bool force = true, Action<Result> callback = null) => callback?.Invoke(null);

        public void OpenCheckout(Order order, Action<Result> callback = null)
        {
            order.paymentProvider = this;

            StartCoroutine(OpenCheckoutCoroutine(order, callback));
        }
        protected virtual IEnumerator OpenCheckoutCoroutine(Order order, Action<Result> callback = null)
        {
            callback?.Invoke(Result.Error("Not implemented"));
            yield break;
        }

        public virtual void Redeem(Action<List<Order>> callback = null) => callback?.Invoke(null);

        public virtual void Clean() { }

        public static class Errors
        {
            public const string UNKNOWN = "Unknown";
            public const string USER_CANCELED = "OperationCanceled";

            public static Dictionary<string, string> DefaultMessages = new Dictionary<string, string>
            {
                [UNKNOWN] = "An unknown error has occurred. Please contact the game developer",
                [USER_CANCELED] = "You canceled the operation (no further action required)",
            };
        }
    }
}
#endif