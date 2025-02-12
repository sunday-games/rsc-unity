#if SG_PAYMENTS && CARDPAY
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.Payments
{
    public class CardPay : PaymentProvider
    {
        public DebugData debug;
        [Serializable]
        public class DebugData
        {
            public string absoluteURL = "https://sunday.games/tools/?text=abc";
        }

        public override void Init()
        {
            Name = Names.CardPay;
            name = Name.ToString();

            transform.parent.CreateComponent<CheckoutManager>(name: CheckoutManager.goName);

            base.Init();
        }

        public override decimal GetFee(Product product) => 0.03M;

        protected override IEnumerator OpenCheckoutCoroutine(Order order, Action<Order> callback = null)
        {
            CheckoutManager.onCheckoutOpened?.Invoke(this);

            if (order.payer.account.IsEmpty())
            {
                order.error = "Account is empty";
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            if (order.payer.email.IsEmpty())
            {
                order.error = "Email is empty";
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            var uri = new Uri(Utils.IsPlatform(Platform.Editor) ? debug.absoluteURL : Application.absoluteURL);
            var baseClientUrl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + uri.AbsolutePath + "checkout.html";
            
            var download = new Download(config.appInfo.blockchainServer + "/api/payment/order/cardpay/create",
                new Dictionary<string, object>
                {
                    ["productId"] = order.product.id,
                    ["currency"] = order.currency.Name,
                    //["bcId"] = order.payer.blockchain.id,
                    ["customer"] = order.payer.account,
                    ["email"] = order.payer.email,
                    ["baseClientUrl"] = baseClientUrl,
                });

            yield return download.RequestCoroutine();

            if (!download.success)
            {
                order.error = download.errorMessage;
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            order.id = download.responseDict["orderId"].ToString();

            Result checkoutResult = null;
#if UNITY_WEBGL && !UNITY_EDITOR
            var configData = new Dictionary<string, object>
            {
                ["gateway"] = "cardpay",
                ["redirectLink"] = download.responseDict["redirectUrl"],
                ["form"] = new Dictionary<string, object> { ["width"] = 1210, ["height"] = 840 },
            };

            CheckoutManager.Open(configData, result => checkoutResult = result);
            while (checkoutResult == null) yield return null;
#else
            yield return new WaitForSeconds(2f);

            checkoutResult = new Result().SetSuccess(new Dictionary<string, object> {
                { "status", "success" },
            });
#endif

            if (!checkoutResult.success)
            {
                order.error = checkoutResult.error;
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            if (checkoutResult.data.IsValue("status") && checkoutResult.data["status"].ToString() == "success")
                order.SetStatus(Order.Status.PAID);

            CheckoutManager.onCheckoutClosed?.Invoke(this, order);

            callback?.Invoke(order);
        }
    }
}
#endif