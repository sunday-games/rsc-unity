namespace SG
{
    public class IAPFacebookCanvas : IAPStore
    {
        public string urlPrefix;

        public override void Init()
        {
            iapManager.isInitialized = true;

            foreach (var iap in IAP.IAPs)
            {
                iap.currencyCode = IAP.defaultCurrency;
                iap.price = iap.priceUSD;
            }
        }

        IAP iap = null;

        public override void Purchase(IAP iap)
        {
            if (string.IsNullOrEmpty(urlPrefix))
            {
                LogError("IAP - Purchase has failed. Facebook Url dont setup");
                iapManager.PurchaseFailed();
                return;
            }

            this.iap = iap;

#if FACEBOOK
            Facebook.Unity.FB.Canvas.Pay(product: server.links.hosting + urlPrefix + iap.name, callback: result =>
            {
                //{ payment_id: 848929916459082, quantity: "2", status: "completed", signed_request: "7QYHzKqKByA7fjiqJUh2bxFvEdqdvn0n_y1zYiyN6tg.eyJhbGCJxdWFudGl0eSI6IjEiLCJzdGF0dXMiOiJjb21wbGV0ZWQifQ" }

                // {"code":400,"body":{"error":{"message":"An active access token must be used to query information about the current user.","type":"OAuthException","code":2500}}}

                if (result == null)
                {
                    LogError("IAP - Purchase Failed");
                    iapManager.PurchaseFailed();
                }
                else if (!string.IsNullOrEmpty(result.Error))
                {
                    LogError("IAP - Purchase Failed: " + result.Error);
                    iapManager.PurchaseFailed();
                }
                else if (result.Cancelled)
                {
                    Log("IAP - Purchase Failed: " + result.RawResult);
                    iapManager.PurchaseFailed();
                }
                else
                {
                    LogDebug("IAP - Purchase Success: " + result.RawResult);

                    iapManager.PurchaseSucceed(new PurchaseData(
                        iap,
                        (string)result.ResultDictionary["payment_id"],
                        (string)result.ResultDictionary["signed_request"]));
                }
            });
#endif
        }
    }
}