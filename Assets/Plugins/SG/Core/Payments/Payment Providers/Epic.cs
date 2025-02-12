using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

#if EPIC_GAMES && !EOS_DISABLE && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UserInfo;
using Epic.OnlineServices.Ecom;
using PlayEveryWare.EpicOnlineServices;
using EpicResult = Epic.OnlineServices.Result;
#endif

namespace SG.Payments
{
    public class Epic : PaymentProvider
    {
        // SANDBOXES
        // Dev p-jrju54me6vz7f53hentqvzt4rbumsr
        // Stage p-khbc69jqexcfqyz4gs5lunpk66529n
        // Live 7b13795e0ec14de5a4b516bbb0f3a3f0

        // DEPLOYMENTS
        // Dev 08dacc4cbcd3450db3d028c7bd08da8c
        // Stage 45947ffd420a4852a94a1eef406dec72
        // Live ebd3dd99ce534a4e9dcf2e13b0adba11

        public string GameSlug;

        public EpicProduct[] Products;
        [Serializable]
        public class EpicProduct
        {
            public string Slug;
            public string OfferId;
            public string ItemId;

            [NonSerialized]
            public CurrencyValue Price;
        }

#if EPIC_GAMES && !EOS_DISABLE && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
        private static EOSManager.EOSSingleton _manager => EOSManager.Instance;
        private static EpicAccountId _userId => _manager.GetLocalUserId();
        private static EcomInterface _ecom => _manager.GetEOSEcomInterface();

        private static PlatformInterface _platform => _manager.GetEOSPlatformInterface();
        private static AuthInterface _platformAuth => _platform.GetAuthInterface();
        private static UserInfoInterface _platformUserInfo => _platform.GetUserInfoInterface();

        public override void Setup()
        {
            Name = Names.EPIC;
            name = "Epic Games Store";

            if (Products.Length != Configurator.Instance.appInfo.products.Count)
                Log.Error("Number of EpicProducts should the same as Configurator.Instance.appInfo.products.Count");
        }

        private bool _isInit = false;
        public override bool IsInit() => _isInit;

        public override void Init(Action<Result> callback = null)
        {
            if (IsInit())
            {
                Log.Info("Epic - Already inited");
                callback?.Invoke(Result.Success());
                return;
            }

            base.Init();

            gameObject.AddComponent<EOSManager>();
            _isInit = true;

            Log.Info("Epic - Inited");

            Login(force: false, callback);
        }

        public override bool IsLogin() => _userId != null && _userId.IsValid() && Products[0].Price != null;

        public override void Login(bool force = true, Action<Result> callback = null)
        {
            if (IsLogin())
            {
                Log.Info("Epic - Account already loginned");
                callback?.Invoke(Result.Success(new DictSO { ["userId"] = _userId }));
                return;
            }

            var type = ExternalCredentialType.Epic;
            var scopeFlags =
                AuthScopeFlags.BasicProfile |
                AuthScopeFlags.FriendsList | // TODO
                AuthScopeFlags.Presence | // TODO
                AuthScopeFlags.Country; // TODO

            Log.Info("Epic - StartLoginWithLoginOptions PersistentAuth...");
            _manager.StartLoginWithLoginOptions(
                new LoginOptions
                {
                    Credentials = new Credentials { Type = LoginCredentialType.PersistentAuth, ExternalType = type },
                    ScopeFlags = scopeFlags
                },
                persistentLoginInfo =>
                {
                    LogResult(persistentLoginInfo.ResultCode, "StartLoginWithLoginOptions PersistentAuth");

                    if (persistentLoginInfo.ResultCode == EpicResult.Success)
                    {
                        Connect(persistentLoginInfo);
                    }
                    else if (force)
                    {
                        Log.Info("Epic - StartLoginWithLoginOptions AccountPortal...");
                        _manager.StartLoginWithLoginOptions(
                            new LoginOptions
                            {
                                Credentials = new Credentials { Type = LoginCredentialType.AccountPortal, ExternalType = type },
                                ScopeFlags = scopeFlags
                            },
                            loginInfo =>
                            {
                                LogResult(loginInfo.ResultCode, "StartLoginWithLoginOptions AccountPortal");

                                if (loginInfo.ResultCode == EpicResult.NotFound)
                                {
                                    callback?.Invoke(Result.Error(Errors.USER_CANCELED));
                                    return;
                                }
                                else if (loginInfo.ResultCode != EpicResult.Success)
                                {
                                    callback?.Invoke(Result.Error($"Failed to log into Epic Games Store ({loginInfo.ResultCode})"));
                                    return;
                                }

                                Connect(loginInfo);
                            });
                    }
                    else
                    {
                        callback?.Invoke(force ? Result.Error($"Failed to log into Epic Games Store ({persistentLoginInfo.ResultCode})") : Result.Success());
                    }
                });

            void Connect(LoginCallbackInfo loginInfo)
            {
                Log.Info("Epic - StartConnectLoginWithEpicAccount...");
                _manager.StartConnectLoginWithEpicAccount(loginInfo.LocalUserId,
                    connectInfo =>
                    {
                        LogResult(connectInfo.ResultCode, "StartConnectLoginWithEpicAccount");

                        if (connectInfo.ResultCode == EpicResult.InvalidUser) // "CreateConnectUser" is needed
                        {
                            Log.Info("Epic - CreateConnectUserWithContinuanceToken... " + connectInfo.ContinuanceToken);
                            _manager.CreateConnectUserWithContinuanceToken(connectInfo.ContinuanceToken,
                                createUserInfo =>
                                {
                                    LogResult(createUserInfo.ResultCode, "CreateConnectUserWithContinuanceToken");

                                    if (createUserInfo.ResultCode != EpicResult.Success)
                                    {
                                        callback?.Invoke(Result.Error($"Failed to create user for Epic Games Store ({createUserInfo.ResultCode})"));
                                        return;
                                    }

                                    OnConnectSuccess();
                                });
                            return;
                        }
                        else if (connectInfo.ResultCode != EpicResult.Success)
                        {
                            callback?.Invoke(Result.Error($"Failed to connect into Epic Games Store ({connectInfo.ResultCode})"));
                            return;
                        }

                        OnConnectSuccess();
                    });

                void OnConnectSuccess()
                {
                    GetUserName();

                    QueryOffers();
                }
            }

            void QueryOffers()
            {
                if (Products[0].Price != null)
                {
                    Log.Info("Epic - Offers already downloaded");
                    callback?.Invoke(Result.Success());
                    return;
                }

                Log.Info("Epic - QueryOffers...");
                var queryOfferOptions = new QueryOffersOptions { LocalUserId = _userId, OverrideCatalogNamespace = null };
                _ecom.QueryOffers(ref queryOfferOptions, null,
                    (ref QueryOffersCallbackInfo queryOffersCallbackInfo) =>
                    {
                        LogResult(queryOffersCallbackInfo.ResultCode, "QueryOffers");

                        if (queryOffersCallbackInfo.ResultCode != EpicResult.Success)
                        {
                            callback?.Invoke(Result.Error($"Failed to download offers from Epic Games Store ({queryOffersCallbackInfo.ResultCode})"));
                            return;
                        }

                        var countOptions = new GetOfferCountOptions { LocalUserId = _userId };
                        var count = _ecom.GetOfferCount(ref countOptions);

                        for (int i = 0; i < count; ++i)
                        {
                            var indexOptions = new CopyOfferByIndexOptions { LocalUserId = _userId, OfferIndex = (uint) i };
                            if (_ecom.CopyOfferByIndex(ref indexOptions, out var offer) != EpicResult.Success) // Result.EcomCatalogOfferPriceInvalid Result.EcomCatalogOfferStale
                            {
                                Log.Error($"Epic - Offer {i}/{count - 1} invalid");
                                continue;
                            }

                            var price = new CurrencyValue(
                                offer.Value.CurrentPrice64 / MathF.Pow(10, offer.Value.DecimalPoint),
                                offer.Value.CurrencyCode);

                            Log.Info($"Epic - Offer {i}/{count - 1}: Id {offer?.Id}, Title {offer?.TitleText}, Current Price {price.ToString()} {offer?.CurrencyCode}");

                            if (Products.TryFind(p => p.OfferId == offer?.Id, out EpicProduct product))
                                product.Price = price;
                            else
                                Log.Warning($"Epic - Fail to find product for offer {offer?.TitleText} {offer?.Id}");
                        }

                        callback?.Invoke(count > 1 ? Result.Success() : Result.Error($"Failed to download offers from Epic Games Store (Count: {count})"));
                    });
            }

            void GetUserName()
            {
                Log.Info("Epic - QueryUserInfo...");
                var options = new QueryUserInfoOptions() { LocalUserId = _userId, TargetUserId = _userId };
                _platformUserInfo.QueryUserInfo(ref options, null,
                    (ref QueryUserInfoCallbackInfo data) =>
                    {
                        LogResult(data.ResultCode, "QueryUserInfo");

                        var options = new CopyUserInfoOptions() { LocalUserId = data.LocalUserId, TargetUserId = data.TargetUserId };
                        var result = _platformUserInfo.CopyUserInfo(ref options, out UserInfoData? userInfo);

                        if (result != EpicResult.Success)
                        {
                            Log.Error("Epic - QueryUserInfo " + result);
                            return;
                        }

                        // TODO
                        UI.UI.Instance.Notifications?.NotificationCreate("epicLogin",
                            $"Welcome, {userInfo?.DisplayName}",
                            autoHideTime: 5f);
                    });
            }
        }

        protected override IEnumerator OpenCheckoutCoroutine(Order order, Action<Result> callback = null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX
            var slug = string.Format(GameSlug, Products[order.product.Index].Slug + "-");
            UI.Helpers.OpenLink("https://store.epicgames.com/p/" + slug);
            callback?.Invoke(Result.Success(new DictSO { ["order"] = order }));
            yield break;
#endif

            Log.Info("Epic - Checkout...");
            var checkoutOptions = new CheckoutOptions
            {
                LocalUserId = _userId,
                Entries = new CheckoutEntry[]
                {
                    new CheckoutEntry { OfferId = Products[order.product.Index].OfferId }
                }
            };
            _ecom.Checkout(ref checkoutOptions, null,
                (ref CheckoutCallbackInfo checkoutInfo) =>
                {
                    LogResult(checkoutInfo.ResultCode, "Checkout");

                    if (checkoutInfo.ResultCode != EpicResult.Success)
                    {
                        callback?.Invoke(Result.Error($"Failed to make the purchase in Epic Games Store ({checkoutInfo.ResultCode})"));
                        return;
                    }

                    callback?.Invoke(Result.Success(new DictSO { ["order"] = order }));
                });

            yield break;
        }

        public override void Redeem(Action<List<Order>> callback = null)
        {
            if (!IsLogin())
            {
                Log.Info("Epic - Login first, then Redeem");
                callback?.Invoke(null);
                return;
            }

            var token = _manager.GetUserAuthTokenForAccountId(_userId);
            if (token == null)
            {
                Log.Error("Epic - Fail to get the token");
                callback?.Invoke(null);
                return;
            }


            QueryEntitlements(unredeemedEntitlements =>
            {
                if (unredeemedEntitlements.Count == 0)
                {
                    Log.Info("Epic - No entitlements to redeem");
                    callback?.Invoke(null);
                    return;
                }

                new Download(Configurator.ApiUrl + "/api/payment/order/epic/redeem",
                        new DictSO
                        {
                            ["accountId"] = _userId.ToString(),
                            ["accessToken"] = token?.AccessToken,
                        })
                    .SetCallback(download =>
                    {
                        if (!download.success ||
                            !download.responseDict.TryGetList("entitlements", out var entitlementIds) ||
                            entitlementIds.Count == 0)
                        {
                            callback?.Invoke(null);
                            return;
                        }

                        var orders = new List<Order>();
                        foreach (string entitlementId in entitlementIds)
                            if (unredeemedEntitlements.TryFind(e => e.EntitlementId == entitlementId, out Entitlement entitlement) &&
                                Products.TryFind(p => p.ItemId == entitlement.CatalogItemId, out EpicProduct epicProduct))
                            {
                                var productIndex = Products.IndexOf(epicProduct);

                                var order = new Order { product = Configurator.Instance.appInfo.products[productIndex] }
                                    .SetId(entitlementId);

                                Order.onDelivered?.Invoke(order);

                                orders.Add(order);
                            }

                        callback?.Invoke(orders);
                    })
                    .IgnoreError()
                    .SetPlayer(null)
                    .Run();
            });

            void QueryEntitlements(Action<List<Entitlement>> callback = null)
            {
                var unredeemedEntitlements = new List<Entitlement>();

                Log.Info("Epic - QueryEntitlements...");
                var entitlementsOptions = new QueryEntitlementsOptions
                {
                    LocalUserId = _userId,
                    IncludeRedeemed = !Configurator.production,
                };
                _ecom.QueryEntitlements(ref entitlementsOptions, null,
                    (ref QueryEntitlementsCallbackInfo data) =>
                    {
                        LogResult(data.ResultCode, "QueryEntitlements");

                        if (data.ResultCode != EpicResult.Success)
                        {
                            callback?.Invoke(null);
                            return;
                        }

                        var countOptions = new GetEntitlementsCountOptions { LocalUserId = _userId };
                        var count = _ecom.GetEntitlementsCount(ref countOptions);

                        for (int i = 0; i < count; ++i)
                        {
                            var indexOptions = new CopyEntitlementByIndexOptions { LocalUserId = _userId, EntitlementIndex = (uint) i };
                            if (_ecom.CopyEntitlementByIndex(ref indexOptions, out var entitlement) != EpicResult.Success)
                            {
                                Log.Error($"Epic - Entitlement {i}/{count - 1} invalid");
                                continue;
                            }

                            Log.Info($"Epic - Entitlement {i}/{count - 1}: Id {entitlement?.EntitlementId}, CatalogId {entitlement?.CatalogItemId}, ServerIndex {entitlement?.ServerIndex}, Redeemed {entitlement?.Redeemed}");

                            if (entitlement?.Redeemed == false)
                                unredeemedEntitlements.Add(entitlement.Value);
                        }

                        callback?.Invoke(unredeemedEntitlements);
                    });
            }

            //void RedeemEntitlements(Utf8String[] entitlementIds, Action<string> callback = null)
            //{
            //    var redeemEntitlementsOptions =
            //        new RedeemEntitlementsOptions { LocalUserId = _userId, EntitlementIds = entitlementIds };
            //    _ecom.RedeemEntitlements(ref redeemEntitlementsOptions, null,
            //        (ref RedeemEntitlementsCallbackInfo redeemEntitlementsInfo) =>
            //        {
            //            Log.Info("Epic - RedeemEntitlements " + redeemEntitlementsInfo.ResultCode);
            //            callback?.Invoke(redeemEntitlementsInfo.ResultCode.ToString());
            //        });
            //}
        }

        public override void Clean()
        {
            if (!IsInit())
                return;

            var deleteTokenOptions = new DeletePersistentAuthOptions();
            _platformAuth.DeletePersistentAuth(ref deleteTokenOptions, null,
                (ref DeletePersistentAuthCallbackInfo deleteTokeninfo) =>
                {
                    Log.Info("Epic - DeletePersistentAuth " + deleteTokeninfo.ResultCode);
                });

            if (IsLogin())
                _manager.StartLogout(_userId,
                    (ref LogoutCallbackInfo logoutInfo) =>
                    {
                        Log.Info("Epic - StartLogout " + logoutInfo.ResultCode);
                    });
        }

        public override decimal GetFee(Product product) => 0.03m; // TODO

        public override CurrencyValue GetPrice(Product product) => Products[product.Index].Price;

        private static void LogResult(EpicResult result, string functionName)
        {
            if (result == EpicResult.Success)
                Log.Info($"Epic - {functionName}: {result}");
            else
                Log.Error($"Epic - {functionName}: {result}");
        }
#endif
    }
}