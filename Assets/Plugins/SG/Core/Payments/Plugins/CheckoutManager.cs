#if SG_PAYMENTS && (PAYPAL || CARDPAY)
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SG.Payments
{
    public class CheckoutManager : MonoBehaviour
    {
        public static string goName = "Checkout";

        public static Action<PaymentProvider> onCheckoutOpened;
        public static Action<PaymentProvider, Order> onCheckoutClosed;

#if UNITY_WEBGL
        static Action<Result> callback;
        public static void Open(Dictionary<string, object> config, Action<Result> callback = null)
        {
            CheckoutManager.callback = callback;

            config.Add("callback", new Dictionary<string, object> {
                { "object", FindObjectOfType<CheckoutManager>().name },
                { "function", "Checkout" }, // { "function", nameof(NotifyCheckout) },
            });

            var configSerialized = Json.Serialize(config);

            Log.Info("CheckoutManager - OpenCheckout: " + configSerialized);

            OpenCheckout(configSerialized);
        }

        [DllImport("__Internal")]
        static extern void OpenCheckout(string config);

        public void NotifyCheckout(string json)
        {
            Log.Info("CheckoutManager - NotifyCheckout: " + json);
            callback?.Invoke(new Result(json));
        }
#endif
    }
}
#endif