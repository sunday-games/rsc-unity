#if SG_PAYMENTS && PAYPAL
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.Payments
{
    public class PayPal : PaymentProvider
    {
        public string clientIdSandbox = "AdfF708hTkrWdhRyExO6iUTv3EZcAMO_7aM7FM90OqpIHl0rTXdNaXzVKh-S9ZleSm6t6nbTBadCyTZ-";
        public string clientIdProduction = "AUoNJYEaMKsW8WAY7nYLuB2FtXZOnE5j4Ni18-UKN29zoItrEvTXNGqp2SiknKkslcUVRziSU7HVwozg";

        public override void Setup()
        {
            Name = Names.PayPal;
            name = Name.ToString();

            base.Setup();
        }

        public override void Init()
        {
            transform.parent.CreateComponent<CheckoutManager>(name: CheckoutManager.goName);

            base.Init();
        }

        public override decimal GetFee(Product product) => (product.price * 0.029M + 0.30M) / product.price;

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

            var settings = new Dictionary<string, object> {
                { "gateway", "paypal" },
                { "env", Configurator.production ? "production" : "sandbox" },
                { "client", new Dictionary<string, object> { { "sandbox", clientIdSandbox }, { "production", clientIdProduction } } },
                { "createLink", config.appInfo.blockchainServer + "/api/payment/order/paypal/create" },
                { "captureLink", config.appInfo.blockchainServer + "/api/payment/order/paypal/capture" },
                { "payload", new Dictionary<string, object> {
                        { "productId", order.product.id },
                        { "currency", order.currency.Name },
                        //{ "bcId", order.payer.blockchain.id },
                        { "customer", order.payer.account },
                    }
                },
                { "form", new Dictionary<string, object> {
                        { "width", 520 },
                        { "height", 640 },
                        { "name", order.product.name.Localize() },
                        { "description", order.product.description.Localize() },
                        { "imgLink", order.product.imageUrl },
                        { "price", order.priceToView },
                    }
                },
            };

            Result checkoutResult = null;
#if UNITY_WEBGL && !UNITY_EDITOR
            CheckoutManager.Open(settings, result => checkoutResult = result);
            while (checkoutResult == null) yield return null;
#else
            yield return new WaitForSeconds(2f);

            // { "success":true,"data":{
            //      "payload":"{\"productId\":\"PLANET_PACK_1\",\"currency\":\"EUR\",\"bcId\":0,\"customer\":\"0x36d6279b6f403db3bcbeb5c4a7cdfa55f4a3e2bf\"}",
            //      "order":{ "orderID":"21141463A74236804","payerID":"FM5NEKZQ2NX6W"},
            //      "details":{
            //          "purchase_units":[{"reference_id":"PLANET_PACK_1","payments":{"captures":[{"amount":{"value":"1.00","currency_code":"EUR"},"seller_protection":{"dispute_categories":["ITEM_NOT_RECEIVED","UNAUTHORIZED_TRANSACTION"],"status":"ELIGIBLE"},"update_time":"2019-04-19T13:52:18Z","create_time":"2019-04-19T13:52:18Z","final_capture":true,"links":[{"method":"GET","rel":"self","href":"https://api.sandbox.paypal.com/v2/payments/captures/87E85345G2486551R"},{"method":"POST","rel":"refund","href":"https://api.sandbox.paypal.com/v2/payments/captures/87E85345G2486551R/refund"},{"method":"GET","rel":"up","href":"https://api.sandbox.paypal.com/v2/checkout/orders/21141463A74236804"}],"id":"87E85345G2486551R","status_details":{"reason":"RECEIVING_PREFERENCE_MANDATES_MANUAL_ACTION"},"status":"PENDING"}]}}],
            //          "links":[{"method":"GET","rel":"self","href":"https://api.sandbox.paypal.com/v2/checkout/orders/21141463A74236804"}],
            //          "id":"21141463A74236804",
            //          "payer":{"address":{"country_code":"US"},"email_address":"test@sunday.games","phone":{},"name":{"surname":"Lebedev","given_name":"Pavel"},"payer_id":"FM5NEKZQ2NX6W"},
            //          "status":"COMPLETED"}}}

            checkoutResult = new Result().SetSuccess(new Dictionary<string, object> {
                { "order", new Dictionary<string, object> {
                        { "orderID", "9D656119NV337725A" },
                    }
                },
                { "details", new Dictionary<string, object> {
                        { "status", "COMPLETED" },
                    }
                },
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

            if (checkoutResult.data.IsValue("order"))
            {
                var orderData = checkoutResult.data["order"] as Dictionary<string, object>;

                order.id = orderData["orderID"].ToString();
            }

            if (checkoutResult.data.IsValue("details"))
            {
                var details = checkoutResult.data["details"] as Dictionary<string, object>;

                if (details.IsValue("payer"))
                {
                    var payerData = details["payer"] as Dictionary<string, object>;

                    order.payer.id = payerData["payer_id"].ToString();
                    order.payer.email = payerData["email_address"].ToString();

                    if (payerData.IsValue("address"))
                    {
                        var addressData = payerData["address"] as Dictionary<string, object>;
                        order.payer.countryCode = addressData["country_code"].ToString();
                    }

                    if (payerData.IsValue("name"))
                    {
                        var nameData = payerData["name"] as Dictionary<string, object>;
                        order.payer.surname = nameData["surname"].ToString();
                        order.payer.name = nameData["given_name"].ToString();
                    }
                }

                if (details.IsValue("status"))
                {
                    if (details["status"].ToString() == "COMPLETED")
                        order.SetStatus(Order.Status.PAID);
                }
            }

            CheckoutManager.onCheckoutClosed?.Invoke(this, order);

            callback?.Invoke(order);
        }
    }
}
#endif