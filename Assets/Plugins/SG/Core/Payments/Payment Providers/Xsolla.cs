using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

#if XSOLLA
using Xsolla.Core;
using Xsolla.Catalog;
using Xsolla.Auth;

namespace SG.Payments
{
    public class Xsolla : PaymentProvider
    {
        public static Xsolla Instance;

        public static ProviderSettings Settings => Configurator.production ? Instance.Prod : Instance.Test;

        public ProviderSettings Prod, Test;
        [Serializable]
        public class ProviderSettings
        {
            public string LoginId;
            public string StoreProjectId;
            public int OAuthClientId;
            public float AcceessTokenLifetime;
        }

        private VirtualCurrencyPackages _currencyPackages;
        private List<Product> _products => Configurator.Instance.appInfo.products;

        public override void Setup()
        {
            Name = Names.XSOLLA;
            name = "Xsolla";
        }

        public override bool IsInit() => XsollaToken.Keeper != null && _currencyPackages != null;

        public override void Init(Action<Result> callback = null)
        {
            if (IsInit())
            {
                Log.Info("Xsolla - Already inited");
                callback?.Invoke(Result.Success());
                return;
            }

            base.Init();

            Instance = this;

            if (XsollaToken.Keeper == null)
            {
                var id = SystemInfo.deviceUniqueIdentifier;
                var p = id + "dOe" + (552 * 1341) + "kDyVV" + (316 * 945) + "H4Fdd" + Utils.GetHash(id);
                XsollaToken.Keeper = new XsollaToken.TokenKeeper(
                    encrypt: data => Cryptography.Encrypt(data, p),
                    decrypt: encryptData => Cryptography.Decrypt(encryptData, p));

                XsollaSettings.StoreProjectId = Settings.StoreProjectId;
                XsollaSettings.LoginId = Settings.LoginId;
                XsollaSettings.OAuthClientId = Settings.OAuthClientId;
            }

            if (_currencyPackages == null)
            {
                XsollaCatalog.GetVirtualCurrencyPackagesList(
                    currencyPackages =>
                    {
                        _currencyPackages = currencyPackages;

                        if (_currencyPackages.items.Length != _products.Count)
                        {
                            Log.Info("Xsolla - Wrong currency packages count " + _currencyPackages.items.Length);
                            callback?.Invoke(Result.Error("Wrong currency packages count"));
                            return;
                        }

                        for (int i = 0; i < _products.Count; i++)
                        {
                            var item = _currencyPackages.items[i];
                            var product = _products[i];

                            Log.Info($"Xsolla - sku {item.sku}, price {item.price.amount} {item.price.currency}, currency {item.content[0].quantity} {item.content[0].sku}");

                            product.id = item.sku;
                            product.name = item.name;
                            product.description = item.description;
                            product.storePrice = new CurrencyValue(item.price.amount, item.price.currency);
                            product.reward.currency[0] = new CurrencyValue(item.content[0].quantity.ToDecimal(), item.content[0].sku);
                        }

                        Log.Info("Xsolla - Inited");
                        callback?.Invoke(Result.Success());
                    },
                    error =>
                    {
                        Log.Error($"Xsolla - GetVirtualCurrencyPackagesList: {error.errorCode} {error.errorMessage}");

                        callback?.Invoke(Result.Error(error.errorMessage));
                    }
                );

                return;
            }

            Log.Info("Xsolla - Inited");
            callback?.Invoke(Result.Success());
        }

        public override bool IsLogin() => XsollaToken.Exists;

        protected override IEnumerator OpenCheckoutCoroutine(Order order, Action<Result> callback = null)
        {
            // To test the payment process:
            // a) use the following URL: https://sandbox-secure.xsolla.com/paystation4/?access_token=ACCESS_TOKEN
            // b) use the following test bank card details:
            //   - Card number: 4111 1111 1111 1111
            //   - Exp. date: 12/40
            //   - CVV2: 123

            new Download(Configurator.ApiUrl + "/api/payment/order/xsolla/create",
                    new DictSO { ["productId"] = order.product.id, ["productQuantity"] = 1 })
                .SetCallback(download =>
                {
                    if (download.success &&
                        download.responseDict.TryGetString("redirectUrl", out var redirectUrl) &&
                        download.responseDict.TryGetString("orderId", out var orderId))
                    {
                        order.SetId(orderId);

                        UI.Helpers.OpenLink(redirectUrl);

                        callback?.Invoke(Result.Success(new DictSO { ["order"] = order }));
                    }
                    else
                    {
                        callback?.Invoke(Result.Error(download.errorMessage));
                    }
                })
                .SetPlayer(null)
                .Run();

            yield break;
        }

        // public override decimal GetFee(Product product) => 0.03M; // TODO

        public override CurrencyValue GetPrice(Product product)
        {
            if (_currencyPackages == null)
                return null;

            var price = _currencyPackages.items[product.Index].price;
            return new CurrencyValue(price.amount, price.currency);
        }
    }
}
#endif