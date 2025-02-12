#if SG_PAYMENTS && (PAYPAL || CARDPAY)
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.Payments
{
    public class DebugProvider : PaymentProvider
    {
        public override void Setup()
        {
            Name = Names.DebugProvider;

            base.Setup();
        }

        public override decimal GetFee(Product product) => 0.03M;

        protected override IEnumerator OpenCheckoutCoroutine(Order order, Action<Order> callback = null)
        {
            CheckoutManager.onCheckoutOpened?.Invoke(this);
            
            var download = new Download(config.appInfo.blockchainServer + "/api/payment/order/debug/create",
                new Dictionary<string, object> {
                    { "productId", order.product.id },
                    { "currency", order.currency.Name },
                    //{ "bcId", order.payer.blockchain.id },
                    { "customer", order.payer.account },
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

            order.SetStatus(Order.Status.PAID);

            CheckoutManager.onCheckoutClosed?.Invoke(this, order);

            callback?.Invoke(order);
        }
    }
}
#endif