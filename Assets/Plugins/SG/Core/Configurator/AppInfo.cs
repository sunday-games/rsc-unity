using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    [CreateAssetMenu(menuName = "Sunday/App Info")]
    public class AppInfo : ScriptableObject
    {
        [Space]
        public string title;

        [TextArea(1, 10)]
        public string description;

        [Space]
        public string version;
        public int versionCode;

        public string unityOrg;
        public string unityId;

        public string appleAppId;
        public string appleBundleId;
        public string appleUrl => appleAppId.IsEmpty() ? defaultMobileRedirectUrl : "https://apps.apple.com/app/id" + appleAppId;

        public string androidPackageName;
        public string androidUrl
        {
            get
            {
                if (androidPackageName.IsEmpty())
                    return defaultMobileRedirectUrl;
#if UNITY_ANDROID
                return "market://details?id=" + androidPackageName;
#else
                return "https://play.google.com/store/apps/details?id=" + androidPackageName;
#endif
            }
        }

        public string defaultMobileRedirectUrl;

        public void OpenGameUrl()
        {
            UI.Helpers.OpenLink(
#if UNITY_IOS && !UNITY_EDITOR
                appleUrl);
#elif UNITY_ANDROID && !UNITY_EDITOR
                androidUrl);
#else
                deepLinkUrl);
#endif
        }

        [Space]
        public string requestReason;

        public string iconURL;

        public string UUID;

        [Space]
        public string blockchainServerProd;
        public string blockchainServerTest;
        public string blockchainServer => Configurator.production ? blockchainServerProd : blockchainServerTest;
        public string blockchainServerProjectId;

        [Space]
        public string deepLinkUrl;

        [Space]
        public string[] domains;

        [Space]
        [TextArea(1, 10)]
        public string messageToSign;
        public string GetSignature(string privateKey)
        {
#if SG_BLOCKCHAIN
            return BlockchainPlugin.NethereumManager.Sign(messageToSign, privateKey);
#else
            Log.Error("Fail to get Signature. Turn on blockchain feature!");
            return null;
#endif
        }

#if SG_PAYMENTS
        [Space(10)]
        public List<Payments.Product> products;
        public Payments.Product GetProduct(string id)
        {
            foreach (var product in products)
                if (product.id == id || product.appleId == id || product.googleId == id)
                    return product;
            return null;
        }
#endif // SG_PAYMENTS
    }
}