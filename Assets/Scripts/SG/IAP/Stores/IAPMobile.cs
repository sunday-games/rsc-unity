using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

#if RECEIPT_VALIDATION
using UnityEngine.Purchasing.Security;
#endif

namespace SG
{
#if UNITY_PURCHASING
    public class IAPMobile : IAPStore, IStoreListener
    {
        IStoreController controller;
#if RECEIPT_VALIDATION
        CrossPlatformValidator validator;
#endif

        public override void Init()
        {
            if (controller != null) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var iap in IAP.IAPs)
                builder.AddProduct(iap.sku, ProductType.Consumable, new IDs() { { iap.sku, AppleAppStore.Name }, { iap.sku, GooglePlay.Name } });

#if RECEIPT_VALIDATION
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
#endif

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Log("IAP - Initialized");

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

                    LogDebug("IAP - Initialized. " + iap.name + ": Price Formated " + iap.priceFormated + ", Currency Code " + iap.currencyCode + ",  Price " + iap.price);
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
            LogError("IAP - Initialize Failed. Reason: " + error);
        }

        public override void Purchase(IAP iap)
        {
            try
            {
                if (!iapManager.isInitialized || controller == null)
                {
                    LogError("IAP - Purchase Failed. Not initialized");
                    iapManager.PurchaseFailed();
                    return;
                }

                var product = controller.products.WithID(iap.sku);

                if (product == null || !product.availableToPurchase)
                {
                    Log("IAP - Purchase Failed. Not purchasing product, either is not found or is not available for purchase");
                    iapManager.PurchaseFailed();
                    return;
                }

                controller.InitiatePurchase(product);
            }
            catch (Exception e)
            {
                Log("IAP - Purchase Failed. Exception: " + e);
                iapManager.PurchaseFailed();
                return;
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var iap = IAP.FromSKU(args.purchasedProduct.definition.id);
            if (iap == null)
            {
                Log("IAP - Purchase Failed. Unrecognized product: {0}", args.purchasedProduct.definition.id);
                iapManager.PurchaseFailed();
                return PurchaseProcessingResult.Complete;
            }

            Log("IAP - Purchase Success: {0}, Purchase Receipt: {1}", args.purchasedProduct.definition.id, args.purchasedProduct.receipt);
            // {"Store":"GooglePlay","TransactionID":"GPA.1365-0610-5320-80591","Payload":"{\"json\":\"{\\\"orderId\\\":\\\"GPA.1365-0610-5320-80591\\\",\\\"packageName\\\":\\\"com.sundaygames.rhymes\\\",\\\"productId\\\":\\\"com.sundaygames.rhymes.coins2\\\",\\\"purchaseTime\\\":1457679428830,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"plfjeknfmgggeeoeplhcpccb.AO-J1Oyr-sBdnVXmUwD9JWdvYMXYsjQVVUbJ11HCIf5R6f6E0smmAVj4Lq_LoR79it8MBIc3j7FTN5i97f56UoViJ95k-R6ddJ-OjAxjoYybOsXzBoGGXuHbRBglLf1bJURFTNhYvBME8IbH0YS5Iags61ojxu1oIw\\\"}\",\"signature\":\"1GWVO1alYCTdu0Xxv+bNBNWtXiXX2XVyR+P6ItxWVPI0w7+a2c68AzpGpAVTRg6yXRTPdXX1NPt8pUlQW0vxhNEc2yRX\\/agEGx1CorFMBtNj1xJxiYyoZC85BD6+RU\\/2QnfbJXyPy\\/GzXARsdbFJMwGWmVw7ZzRMLFSEghIAjVCVHcammxgugpkzZcwEaipX6rb9G6ZsETvlBa3EosX+WukzGxiL0w1V4H0mb\\/VTcNqtejdD6akqrsbR\\/UvHcjETJQm1MKqv0K2UEog22HX4CxGCD1saFzxU0fhUTTjxYturLp8z312Qu3FmwTff+6QlPv9QwgPLRsiXC+0UmS9KvA==\"}"}

            var purchaseData = new PurchaseData(iap, transaction: args.purchasedProduct.transactionID);
            var payload = (Json.Deserialize(args.purchasedProduct.receipt) as Dictionary<string, object>)["Payload"] as string;
            if (platform == Platform.AppStore || platform == Platform.tvOS)
            {
                purchaseData.receipt = payload;
            }
            else if (platform == Platform.GooglePlay)
            {
                purchaseData.receipt = (Json.Deserialize(payload) as Dictionary<string, object>)["json"] as string;
                purchaseData.signature = (Json.Deserialize(payload) as Dictionary<string, object>)["signature"] as string;
            }
            else if (platform == Platform.WindowsPhone)
            {
                // Payload is an XML string as specified by Microsoft https://msdn.microsoft.com/en-US/library/windows/apps/windows.applicationmodel.store.currentapp.getappreceiptasync.aspx
            }

#if RECEIPT_VALIDATION
            if (build.localPurchaseVerification)
            {
                try
                {
                    var result = validator.Validate(args.purchasedProduct.receipt);

                    foreach (IPurchaseReceipt productReceipt in result)
                    {
                        if (productReceipt.productID != iap.sku)
                        {
                            Log("IAP - Purchase Failed. productID {0}, purchaseDate {1}, transactionID {2}",
                                productReceipt.productID, productReceipt.purchaseDate, productReceipt.transactionID);

                            var google = productReceipt as GooglePlayReceipt;
                            if (google != null)
                            {
                                LogDebug("purchaseState " + google.purchaseState);
                                LogDebug("purchaseToken " + google.purchaseToken);
                            }

                            var apple = productReceipt as AppleInAppPurchaseReceipt;
                            if (apple != null)
                            {
                                LogDebug("originalTransactionIdentifier " + apple.originalTransactionIdentifier);
                                LogDebug("cancellationDate " + apple.cancellationDate);
                                LogDebug("quantity " + apple.quantity);
                            }

                            iapManager.PurchaseFailed();
                            return PurchaseProcessingResult.Complete;
                        }

                        Log("IAP - Purchase Verify Success");
                    }
                }
                catch (IAPSecurityException)
                {
                    Log("IAP - Purchase Failed. Invalid receipt");
                    iapManager.PurchaseFailed();
                    return PurchaseProcessingResult.Complete;
                }
            }
#endif

            if (build.serverPurchaseVerification)
            {
                server.VerifyPurchase(purchaseData);
                return PurchaseProcessingResult.Complete;
            }

            iapManager.PurchaseSucceed(purchaseData);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (failureReason == PurchaseFailureReason.UserCancelled)
                Log($"IAP - Purchase Failed. Product: {product.definition.id}, PurchaseFailureReason: {failureReason}");
            else
                LogError($"IAP - Purchase Failed. Product: {product.definition.id}, PurchaseFailureReason: {failureReason}");

            iapManager.PurchaseFailed();
        }

        [Space(10)]
        public string applePublicKey; // 02153ec69f26451b9ddc4fa3886f5667
        public string appleVerifyServer; // https://buy.itunes.apple.com/verifyReceipt
        public string appleVerifyServerSandbox; // https://sandbox.itunes.apple.com/verifyReceipt
        [Space(10)]
        public string googlePublicKey; // MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA18ZT9CIbIgXHTtelfdNgBQe1w40nolaFJgJUZwJOsWWawqPKsf/2Z5HKThIjcOBS3DrPljYhaMbnY0MxAvXts70K4n2X4/m34qYeWGy2ZIon/r9I+trE1ixaWRRjV5nOQyv2Vqc0RI2BnaFxku2VjdaGXSLXsUPfgcffGAI8CbDMaT8H/IuEtpckCNWfURIwMUmYH7sfH6bm39wwmRF8onisxHlL2TJS5hrquxvB5ir4K/TqafBzTGgbtMoTtCnl6SzzEf94+v5WDY0vPAEBvOoamJcKCK/+mfsNrGaT3N9elqTUh4MGx+Ejlcx12sx86lpVQvmawIF73aXSq6SAIwIDAQAB

        //public override void Verify(PurchaseData data, Action<bool> callback)
        //{
        //    if (platform == Platform.AppStore || platform == Platform.tvOS)
        //        server.RequestWWW("IAP - Apple", new WWW(isDebug ? appleVerifyServerSandbox : appleVerifyServer,
        //            System.Text.Encoding.UTF8.GetBytes("{\"receipt-data\":\"" + data.receipt + "\"}")), download =>
        //        {
        //            if (download.isSuccess)
        //            {
        //                LogDebug("IAP - Apple - Server response: " + download.www.text);
        //                // {"receipt":{"original_purchase_date_pst":"2015-11-21 02:33:11 America/Los_Angeles", "purchase_date_ms":"1448101991792", "unique_identifier":"d8ad32d9d96612840fb64effe6ad45d5e6481cd6", "original_transaction_id":"1000000181287068", "bvrs":"1.3.5", "transaction_id":"1000000181287068", "quantity":"1", "unique_vendor_identifier":"B506F928-29A0-4D86-A5B4-F8B47B7FE02E", "item_id":"924907289", "product_id":"com.sgmw.rsc.luckywheel", "purchase_date":"2015-11-21 10:33:11 Etc/GMT", "original_purchase_date":"2015-11-21 10:33:11 Etc/GMT", "purchase_date_pst":"2015-11-21 02:33:11 America/Los_Angeles", "bid":"com.suchgamesmuchwow.readysetcat", "original_purchase_date_ms":"1448101991792"}, "status":0}

        //                var response = Json.Deserialize(download.www.text) as Dictionary<string, object>;

        //                if (response != null && response.ContainsKey("status") && Convert.ToInt32(response["status"]) == 0 &&
        //                    (string)((Dictionary<string, object>)response["receipt"])["product_id"] == data.iap.sku)
        //                {
        //                    Log("IAP - Apple - Verifyed");
        //                    callback(true);
        //                }
        //                else
        //                {
        //                    LogError("IAP - Apple - Failure to verify. Product ID: {0}, Apple Response: {1}", data.iap.sku, download.www.text);
        //                    callback(false);
        //                }
        //            }
        //            else callback(false);
        //        });
        //    else if (platform == Platform.GooglePlay)
        //    {
        //        try
        //        {
        //            var receiptBytes = System.Text.Encoding.ASCII.GetBytes(data.receipt);
        //            var signBytes = Convert.FromBase64String((data.signature).Replace("\\", ""));

        //            using (var rsa = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(googlePublicKey))
        //            {
        //                if (rsa.VerifyData(receiptBytes, "SHA1", signBytes))
        //                {
        //                    Log("IAP - Google - Verify Succeed");
        //                    callback(true);
        //                }
        //                else
        //                {
        //                    LogError("IAP - Google - Verify Failure");
        //                    callback(false);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogError("IAP - Google - Verify Exception: " + e.StackTrace);
        //            callback(false);
        //        }
        //    }
        //}

        void OnDeferred(Product item)
        {
            // iOS Specific. This is called as part of Apple's 'Ask to buy' functionality, when a purchase is requested by a minor and referred to a parent for approval.
            Log("Purchase deferred: {0}", item.definition.id);
        }

        void OnTransactionsRestored(bool success)
        {
            // This will be called after a call to IAppleExtensions.RestoreTransactions()
            Log("OnTransactionsRestored: {0}", success);
        }

        //public void RestorePurchases()
        //{
        //    if (!isInitialized)
        //    {
        //        Log("RestorePurchases FAIL. Not initialized");
        //        return;
        //    }

        //    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        //    {
        //        Log("RestorePurchases started ...");

        //        var apple = extensions.GetExtension<IAppleExtensions>();
        //        apple.RestoreTransactions((result) =>
        //        {
        //            Log("RestorePurchases continuing: {0}. If no further messages, no purchases available to restore", result);
        //        });
        //    }
        //    else Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        //}
    }
#endif
}