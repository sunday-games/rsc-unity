using System;
using System.Security.Cryptography;
using System.Text;

namespace SG
{
    public static class MoonpayUtil
    {
        public static string ApiKey => Configurator.production ?
            "pk_live_8CD0m2QzYjpU8QZ5sD1gMdCDHKwMoah" :
            "pk_test_KxHmftcF2suY31Qc6pvgqRdFiQ1b9l5a";

        public static string SkKey => Configurator.production ?
            "sk_live_MbkmfxIKgoDkHOt6EMPuiiRszDFuf" :
            "sk_test_lrNFqn2AjzkVwrqdI07OfclLT0ae2seM";

        public static string BaseUri => Configurator.production ?
            "https://buy.moonpay.com/" :
            "https://buy-sandbox.moonpay.com/";

        public static string GenerateUrl(string email, string walletAddress, string currencyCode)
        {
            var urlParams = new StringBuilder("?apiKey=").Append(ApiKey);
            if (email.IsNotEmpty())
                urlParams.Append("&email=").Append(Uri.EscapeDataString(email));

            if (walletAddress.IsNotEmpty())
                urlParams.Append("&walletAddress=").Append(Uri.EscapeDataString(walletAddress));

            if (currencyCode.IsNotEmpty())
                urlParams.Append("&currencyCode=").Append(Uri.EscapeDataString(currencyCode));

            string signature;
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(SkKey)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(urlParams.ToString()));
                signature = Uri.EscapeDataString(Convert.ToBase64String(hash));
            }

            urlParams.Append("&signature=").Append(signature);

            Log.Info(BaseUri + urlParams);

            return BaseUri + urlParams;
        }
    }
}