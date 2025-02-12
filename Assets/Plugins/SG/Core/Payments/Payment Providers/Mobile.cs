#if SG_PAYMENTS && UNITY_PURCHASING
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SG.Payments
{
    public class Mobile : PaymentProvider, IStoreListener
    {
        public DebugData debug;
        [Serializable]
        public class DebugData
        {
            public static Dictionary<string, object> receiptAppStore = new Dictionary<string, object>
            {
                { "Store", "AppleAppStore" },
                { "TransactionID", "1000000566193414" },
                { "Payload", "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" },
            };

            public static Dictionary<string, object> receiptGooglePlay = new Dictionary<string, object>
            {
                { "Store", "GooglePlay" },
                { "TransactionID", "GPA.3377-9879-5734-44000" },
                { "Payload", "{\"json\":\"{\\\"orderId\\\":\\\"GPA.3377-9879-5734-44000\\\",\\\"packageName\\\":\\\"com.oxgames.oxuniverse\\\",\\\"productId\\\":\\\"com.oxgames.oxuniverse.usd25\\\",\\\"purchaseTime\\\":1566459064775,\\\"purchaseState\\\":0,\\\"developerPayload\\\":\\\"{\\\\\\\"developerPayload\\\\\\\":\\\\\\\"eyJiY0lkIjoyLCJiY0FjY291bnQiOiIweDRkZjZmMThjNTgzZmU0NWYxODY4YWVjYTIwMDQ3ZjFi\\\\\\\\nNDE3MTJjNDUifQ==\\\\\\\\n\\\\\\\",\\\\\\\"is_free_trial\\\\\\\":false,\\\\\\\"has_introductory_price_trial\\\\\\\":false,\\\\\\\"is_updated\\\\\\\":false,\\\\\\\"accountId\\\\\\\":\\\\\\\"\\\\\\\"}\\\",\\\"purchaseToken\\\":\\\"emhaeaiehmamikoohmffhamm.AO-J1OykK4yJsnCN1OQW_nsm70BD4HGhkNAjCVt8MV1bDLf4ftiFUaWNjnwPaszBmloJeU9br-VMttma3dEQ2_NNbWERipjUalZ6Dpc-V3-l1Dfll8M4mW64cA0MxW8l1MKOtSQqluec8HxEEtPdOhHk2Ut1VQiYYg\\\"}\",\"signature\":\"O5aaT83C+66NWmkCKLGdBfIcDn2xJ72jsDal+q\\/F0xqfofHYd4NP07FwxnbA86GpY1IEHlyd4\\/+YIYVFXzKByYx368qju0IYo3542iNfSlMTQg3kXlQkEuLfkyEw9GdK94PBY7DnOcnZ78I8IQb5j9Qhlw3P3FdNP\\/+zaU4iKhEFwE2THDvXTF\\/fhowk5tpECGbE\\/dAnfd0aJzHXjv889DWRnk2FsVTiB0UDijI++3BigOvVLKPQBlDCT6+zLq\\/K8DJSiDI5s09wot2Vd2D7c4dOErcxZ5EzFC6kPvGclG7QhuZxJ46VoB7zl4uZlKKhIy7esgBkMGuOgMOq3HbdjA==\",\"skuDetails\":\"{\\\"skuDetailsToken\\\":\\\"AEuhp4LtEHAJyuLp33jdJtgSWF7QR80Zzl-Qxbhnlsm5ltrkRVTPzZY9GT0ywHovJg==\\\",\\\"productId\\\":\\\"com.oxgames.oxuniverse.usd25\\\",\\\"type\\\":\\\"inapp\\\",\\\"price\\\":\\\"RUB 1,999.00\\\",\\\"price_amount_micros\\\":1999000000,\\\"price_currency_code\\\":\\\"RUB\\\",\\\"title\\\":\\\"PLANET PACK 1 (0xUniverse)\\\",\\\"description\\\":\\\"PLANET PACK 1\\\"}\",\"isPurchaseHistorySupported\":true}" },
            };
        }

        IStoreController controller;
        IAppleExtensions appleExtensions;
        IGooglePlayStoreExtensions googleExtensions;
        ITransactionHistoryExtensions txHistoryExtensions;
        Order order;
        List<UnityEngine.Purchasing.Product> pendingProduct = new List<UnityEngine.Purchasing.Product>();

        public override void Init()
        {
            Name = Names.Mobile;

            if (Utils.IsPlatform(Platform.iOS))
                name = "App Store";
            else if (Utils.IsPlatform(Platform.Android))
                name = "Google Play";

            yield return (provider as Mobile).Setup(
                result => { if (result.success) provider.Setup(); });

            base.Init();
        }

        public override bool IsCanBuy(Product product)
        {
            return Utils.IsPlatform(Platform.iOS) && product.price <= 100.0 ||
                Utils.IsPlatform(Platform.Android) && product.price <= 400.0;
        }

        public override double GetFee(Product product) { return 0.30; }

        SG.Result setupResult;
        public IEnumerator Setup(Action<SG.Result> callback = null)
        {
            OrderManager.onOrdersLoaded += SubmitProducts;

            var module = StandardPurchasingModule.Instance();

            if (Utils.IsPlatform(Platform.Editor))
                module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            var builder = ConfigurationBuilder.Instance(module);

            foreach (var p in config.appInfo.products)
                if (!p.appleId.IsEmpty() && !p.googleId.IsEmpty())
                    builder.AddProduct(p.id, ProductType.Consumable, new IDs { { p.appleId, AppleAppStore.Name }, { p.googleId, GooglePlay.Name }, });

            setupResult = null;
            UnityPurchasing.Initialize(this, builder); // Then wait for call OnInitialized or OnInitializeFailed
            while (setupResult == null) yield return null;

            callback?.Invoke(setupResult);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.controller = controller;

            if (Utils.IsPlatform(Platform.iOS))
            {
                appleExtensions = extensions.GetExtension<IAppleExtensions>();

                appleExtensions.RegisterPurchaseDeferredListener(
                    product =>
                    {
                        /// iOS Specific. This is called as part of Apple's 'Ask to buy' functionality,
                        /// when a purchase is requested by a minor and referred to a parent for approval.
                        /// When the purchase is approved or rejected, the normal purchase events will fire.
                        Log.Info("MobileStores - Purchase deferred: " + product.definition.id);
                    });

                // var product_details = appleExtensions.GetProductDetails();
            }
            else if (Utils.IsPlatform(Platform.Android))
            {
                googleExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();

                // var sku_to_SKUDetailsJson = googleExtensions.GetProductJSONDictionary();

                // Sample code for manually finish a transaction (consume a product on GooglePlay store)
                // googleExtensions.FinishAdditionalTransaction(productId, transactionId); 
            }

            txHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

            foreach (var product in controller.products.all)
            {
                if (!product.availableToPurchase)
                {
                    Log.Warning($"MobileStores - OnInitialized - Product '{product.definition.id}' is unavailable to purchase");
                    continue;
                }

                Log.Info($"MobileStores - OnInitialized - Product title: {product.metadata.localizedTitle}, Description: {product.metadata.localizedDescription}, Price: {product.metadata.localizedPrice.ToString()} {product.metadata.isoCurrencyCode} '{product.metadata.localizedPriceString}'");

                var p = config.appInfo.GetProduct(product.definition.id);
                if (p == null)
                {
                    Log.Error($"MobileStores - OnInitialized - Can't find product '{product.definition.id}' in Configurator.AppInfo.products");
                    continue;
                }

                p.storePrice = (product.metadata.localizedPrice.ToDouble(), product.metadata.localizedPriceString, product.metadata.isoCurrencyCode);
            }

            setupResult = new SG.Result().SetSuccess();
        }

        public void OnInitializeFailed(InitializationFailureReason failureReason)
        {
            Log.Error($"MobileStores - Failed to initialize because '{failureReason}'");
            if (failureReason == InitializationFailureReason.AppNotKnown)
                Log.Error("App not known. Is your app correctly uploaded on the relevant publisher console?");
            else if (failureReason == InitializationFailureReason.PurchasingUnavailable)
                Log.Error("Billing disabled. If you turned it off in the device settings, then please enable again");
            else if (failureReason == InitializationFailureReason.NoProductsAvailable)
                Log.Error("No products available for purchase. Developer configuration error; check product metadata");

            setupResult = new SG.Result().SetError(failureReason.ToString());
        }

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

            if (controller == null)
            {
                order.error = "MobileStores is not initialized";
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            if (this.order != null)
            {
                order.error = "Another order is in progress";
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }
            this.order = order;

            var product = controller.products.WithID(order.product.id);
            if (product == null)
            {
                order.error = "No product has such id";
                order.SetStatus(Order.Status.FAILED);
                CheckoutManager.onCheckoutClosed?.Invoke(this, order);
                callback?.Invoke(order);
                yield break;
            }

            var payload = new Dictionary<string, object> {
                    //{ "accountId", order.payer.blockchain.name + "_" + order.payer.account },
                    { "developerPayload", new Dictionary<string, object> {
                            //{ "bcId", order.payer.blockchain.id },
                            { "customer", order.payer.account },
                        } },
                };

            controller.InitiatePurchase(product, Json.Serialize(payload));
            // Then wait for call ProcessPurchase or OnPurchaseFailed

            while (order.status == Order.Status.CREATED || order.status == Order.Status.PAID)
                yield return null;

            this.order = null;

            CheckoutManager.onCheckoutClosed?.Invoke(this, order);

            callback?.Invoke(order);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var product = e.purchasedProduct;

            Log.Info($"MobileStores - Order {product.definition.id} is success" + Const.lineBreak +
                $"Transaction ID: {product.transactionID}" + Const.lineBreak +
                $"Receipt: {product.receipt}");

            if (this.order)
            {
                this.order.id = product.transactionID;
                if (Utils.IsPlatform(Platform.Editor)) this.order.id = DebugData.receiptGooglePlay["TransactionID"].ToString();

                this.order.SetStatus(Order.Status.PAID);
            }

            pendingProduct.Add(product);

            SubmitProducts();

            return PurchaseProcessingResult.Pending;
        }

        void SubmitProducts()
        {
            foreach (var product in pendingProduct)
            {
                var receiptData = Json.Deserialize(product.receipt) as Dictionary<string, object>;
                if (Utils.IsPlatform(Platform.Editor)) receiptData = DebugData.receiptGooglePlay;

                var transactionID = receiptData["TransactionID"].ToString();
                var store = receiptData["Store"].ToString();
                var payload = receiptData["Payload"].ToString();

                var order = OrderManager.GetOrder(transactionID);
                if (!order)
                {
                    Log.Warning($"MobileStores - Failed to restore order data for '{transactionID}'");

                    var payer = new Order.Payer();

                    if (store == AppleAppStore.Name)
                    {
                        // We don’t know how to get an account
                    }
                    else if (store == GooglePlay.Name)
                    {
                        try
                        {
                            var json = (Json.Deserialize(payload) as Dictionary<string, object>)["json"] as string;
                            var developerPayload = (Json.Deserialize(json) as Dictionary<string, object>)["developerPayload"] as string;
                            var accountId = (Json.Deserialize(developerPayload) as Dictionary<string, object>)["accountId"] as string;

                            //if (accountId.Contains("_"))
                            //    payer = new Order.Payer(
                            //        BlockchainPlugin.Blockchain.Deserialize(accountId.Split('_')[0]),
                            //        accountId.Split('_')[1]);
                            //else // TODO Пока это тут временно для совместимости
                            //    payer = new Order.Payer(
                            //        BlockchainPlugin.Blockchain.IdentifyAddress(accountId),
                            //        accountId);
                        }
                        catch
                        {
                            Log.Warning($"MobileStores - GooglePlay - Failed to get accountId form receipt");
                        }
                    }

                    order = new Order(config.appInfo.GetProduct(product.definition.id), payer);
                    order.paymentProvider = this;
                    order.id = transactionID;
                    order.SetStatus(Order.Status.PAID);
                };

                new Download(config.appInfo.blockchainServer + (store == AppleAppStore.Name ? "/api/payment/order/apple/create" : "/api/payment/order/google/create"),
                        new Dictionary<string, object> {
                            //{ "bcId", order.payer.blockchain ? order.payer.blockchain.id : 0 },
                            { "customer", order.payer.account },
                            { "receipt", payload },
                        })
                    .SetCallback(download =>
                    {
                        if (!download.success && download.errorMessage != Download.Errors.orderAlreadyExists)
                        {
                            order.error = download.errorMessage;
                            order.SetStatus(Order.Status.FAILED);

                            UI.Helpers.SendEmail(
"feedback@sunday.games",
"Fail to submit order " + order.id,
$@"Hello Sunday Games Support Team,

I paid for the order, but I can’t receive it. Please help me!

Game: {config.appInfo.name}
Account: {(!order.payer.account.IsEmpty() ? order.payer.account : "unknown")}

Order ID: {order.id}
Store: {store}
Receipt: {payload}

Server error: {download.errorMessage}");

                            return;
                        }

                        controller.ConfirmPendingPurchase(product);

                        pendingProduct.Remove(product);

                        order.SetStatus(Order.Status.PROCESSING);
                    })
                    .Run(this);
            }
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
        {
            Log.Info(
                $"MobileStores - Order {product.definition.id} is failed. Failure reason: {failureReason}" +
                $"Store specific error code: {txHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode()}" +
                (txHistoryExtensions.GetLastPurchaseFailureDescription() == null ? "" :
                    $"Store specific error code: {txHistoryExtensions.GetLastPurchaseFailureDescription().message}"));

            if (failureReason == PurchaseFailureReason.UserCancelled)
            {
                order.error = Errors.OperationCanceled;
                order.SetStatus(Order.Status.CANCELLED);
            }
            else
            {
                if (failureReason == PurchaseFailureReason.ExistingPurchasePending)
                    SubmitProducts();

                order.error = failureReason.ToString();
                order.SetStatus(Order.Status.FAILED);
            }
        }


        public void RestoreButtonClick(Action<bool> callback)
        {
            // Call it from UI
            if (Utils.IsPlatform(Platform.iOS))
                appleExtensions.RestoreTransactions(callback);
            else if (Utils.IsPlatform(Platform.Android))
                googleExtensions.RestoreTransactions(callback);
        }
    }
}
#endif