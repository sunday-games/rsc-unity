using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SG.RSC
{
#if UNITY_PURCHASING && IAP_PURCHASES
    public class IAPMobile : IAPStore, IStoreListener
    {
        IStoreController controller;

        public override void Init()
        {
            if (controller != null) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var iap in IAP.IAPs)
                builder.AddProduct(iap.sku, ProductType.Consumable, new IDs() { { iap.sku, AppleAppStore.Name }, { iap.sku, GooglePlay.Name } });

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
           Log.Info(IAP - Initialized");

            this.controller = controller;

            extensions.GetExtension<IAppleExtensions>().RegisterPurchaseDeferredListener(OnDeferred);

            foreach (var iap in IAP.IAPs)
            {
                var product = controller.products.WithID(iap.sku);

                if (product != null)
                {
                    iap.priceFormated = product.metadata.localizedPriceString;
                    iap.currencyCode = product.metadata.isoCurrencyCode;
                    iap.price = Convert.ToDouble(product.metadata.localizedPrice);

                    Log.Debug("IAP - Initialized. " + iap.name + ": Price Formated " + iap.priceFormated + ", Currency Code " + iap.currencyCode + ",  Price " + iap.price);
                }
                else
                {
                    iap.currencyCode = IAP.defaultCurrency;
                    iap.price = iap.priceUSD;
                }
            }

            iapManager.isInitialized = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Error("IAP - Initialize Failed. Reason: " + error);
        }

        public override void Purchase(IAP iap)
        {
            try
            {
                if (!iapManager.isInitialized || controller == null)
                {
                    Log.Error("IAP - Purchase Failed. Not initialized");
                    iapManager.PurchaseFailed();
                    return;
                }

                var product = controller.products.WithID(iap.sku);

                if (product == null || !product.availableToPurchase)
                {
                   Log.Info(IAP - Purchase Failed. Not purchasing product, either is not found or is not available for purchase");
                    iapManager.PurchaseFailed();
                    return;
                }

                controller.InitiatePurchase(product);
            }
            catch (Exception e)
            {
               Log.Info(IAP - Purchase Failed. Exception: " + e);
                iapManager.PurchaseFailed();
                return;
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var iap = IAP.FromSKU(args.purchasedProduct.definition.id);
            if (iap == null)
            {
               Log.Info(IAP - Purchase Failed. Unrecognized product: " + args.purchasedProduct.definition.id);
                iapManager.PurchaseFailed();
                return PurchaseProcessingResult.Complete;
            }

           Log.Info$"IAP - Purchase Success: {args.purchasedProduct.definition.id}, Purchase Receipt: {args.purchasedProduct.receipt}");
            // {"Store":"GooglePlay","TransactionID":"GPA.1365-0610-5320-80591","Payload":"{\"json\":\"{\\\"orderId\\\":\\\"GPA.1365-0610-5320-80591\\\",\\\"packageName\\\":\\\"com.sundaygames.rhymes\\\",\\\"productId\\\":\\\"com.sundaygames.rhymes.coins2\\\",\\\"purchaseTime\\\":1457679428830,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"plfjeknfmgggeeoeplhcpccb.AO-J1Oyr-sBdnVXmUwD9JWdvYMXYsjQVVUbJ11HCIf5R6f6E0smmAVj4Lq_LoR79it8MBIc3j7FTN5i97f56UoViJ95k-R6ddJ-OjAxjoYybOsXzBoGGXuHbRBglLf1bJURFTNhYvBME8IbH0YS5Iags61ojxu1oIw\\\"}\",\"signature\":\"1GWVO1alYCTdu0Xxv+bNBNWtXiXX2XVyR+P6ItxWVPI0w7+a2c68AzpGpAVTRg6yXRTPdXX1NPt8pUlQW0vxhNEc2yRX\\/agEGx1CorFMBtNj1xJxiYyoZC85BD6+RU\\/2QnfbJXyPy\\/GzXARsdbFJMwGWmVw7ZzRMLFSEghIAjVCVHcammxgugpkzZcwEaipX6rb9G6ZsETvlBa3EosX+WukzGxiL0w1V4H0mb\\/VTcNqtejdD6akqrsbR\\/UvHcjETJQm1MKqv0K2UEog22HX4CxGCD1saFzxU0fhUTTjxYturLp8z312Qu3FmwTff+6QlPv9QwgPLRsiXC+0UmS9KvA==\"}"}

            var purchaseData = new PurchaseData(iap, transaction: args.purchasedProduct.transactionID);
            var payload = (Json.Deserialize(args.purchasedProduct.receipt) as Dictionary<string, object>)["Payload"] as string;
            if (platform == Platform.iOS || platform == Platform.tvOS)
            {
                purchaseData.receipt = payload;
            }
            else if (platform == Platform.Android)
            {
                purchaseData.receipt = (Json.Deserialize(payload) as Dictionary<string, object>)["json"] as string;
                purchaseData.signature = (Json.Deserialize(payload) as Dictionary<string, object>)["signature"] as string;
            }
            else if (platform == Platform.WindowsPhone)
            {
                // Payload is an XML string as specified by Microsoft https://msdn.microsoft.com/en-US/library/windows/apps/windows.applicationmodel.store.currentapp.getappreceiptasync.aspx
            }

            if (build.serverPurchaseVerification)
            {
                server.VerifyPurchase(purchaseData, result =>
                {
                    if (result == true)
                        iapManager.PurchaseSucceed(purchaseData);
                    else if (result == false)
                        iapManager.PurchaseFailed();
                    else
                    {
                        if (build.localPurchaseVerification)
                            LocalPurchaseVerification.Validate(args.purchasedProduct.receipt, purchaseData,
                                success => { if (success) iapManager.PurchaseSucceed(purchaseData); else iapManager.PurchaseFailed(); });
                        else
                            iapManager.PurchaseFailed();
                    }
                });
                return PurchaseProcessingResult.Complete;
            }

            if (build.localPurchaseVerification)
            {
                LocalPurchaseVerification.Validate(args.purchasedProduct.receipt, purchaseData,
                    success => { if (success) iapManager.PurchaseSucceed(purchaseData); else iapManager.PurchaseFailed(); });
                return PurchaseProcessingResult.Complete;
            }

            iapManager.PurchaseSucceed(purchaseData);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (failureReason == PurchaseFailureReason.UserCancelled)
               Log.Info$"IAP - Purchase Failed. Product: {product.definition.id}, PurchaseFailureReason: {failureReason}");
            else
                Log.Error($"IAP - Purchase Failed. Product: {product.definition.id}, PurchaseFailureReason: {failureReason}");

            iapManager.PurchaseFailed();
        }

        void OnDeferred(Product item)
        {
            // iOS Specific. This is called as part of Apple's 'Ask to buy' functionality, when a purchase is requested by a minor and referred to a parent for approval.
           Log.Info(Purchase deferred: {0}", item.definition.id);
        }

        void OnTransactionsRestored(bool success)
        {
            // This will be called after a call to IAppleExtensions.RestoreTransactions()
           Log.Info(OnTransactionsRestored: {0}", success);
        }

        //public void RestorePurchases()
        //{
        //    if (!isInitialized)
        //    {
        //       Log.Info(RestorePurchases FAIL. Not initialized");
        //        return;
        //    }

        //    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        //    {
        //       Log.Info(RestorePurchases started ...");

        //        var apple = extensions.GetExtension<IAppleExtensions>();
        //        apple.RestoreTransactions((result) =>
        //        {
        //           Log.Info(RestorePurchases continuing: {0}. If no further messages, no purchases available to restore", result);
        //        });
        //    }
        //    elseLog.Info(RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        //}
    }
#else
    public class IAPMobile : MonoBehaviour { }
#endif
}