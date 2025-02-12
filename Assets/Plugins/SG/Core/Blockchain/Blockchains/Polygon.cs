using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using Newtonsoft.Json;

namespace SG.BlockchainPlugin
{
    public class Polygon : Ethereum
    {
        public override void Setup()
        {
            name = Names.Polygon;

            NativeTokenSetup(
                new Token("MATIC", Token.Standarts.Native, decimals: 18, nameMin: "WEI"),
                new SmartContract() { Blockchain = this },
                new SmartContract.Function() { Name = "transfer", Payable = true, Fee = 30000 });

            GasToken = new Token("GAS", Token.Standarts.Native, decimals: 0, nameMin: "GAS");
            gasMax = 6700000;
            gasMultiplier = 1.2m;

            FeeToken = new Token("GWEI", Token.Standarts.Native, decimals: 9, nameMin: "WEI");
            feePriceMin = 1;
            feePriceWarning = 300;

            var mainnet = new Network("Polygon Mainnet",
                id: "137",
                blockCreationTime: TimeSpan.FromSeconds(5),
                requiredBlockConfirmations: 64,
                nodeUrls: new string[] {
                    "https://polygon-mainnet.infura.io/v3/d5e92759c70f41b0a2ca3c2f39ce4c75", // admin@sunday.games
                    "https://polygon-rpc.com",
                    "https://rpc.ankr.com/polygon",
                    "https://polygon-mainnet.infura.io/v3/510c2b6aeeee4af7992a9a4e8e701fec", // goodthingscomeafterbad@gmail.com
                    "https://polygon-mainnet.infura.io/v3/2a3023ee410245b98df8e7e747f6f75a", // admin@0x.games Matic
                },
                explorer: new Explorer("https://polygonscan.com/"));

            //var mumbai = new Network("Polygon Mumbai Testnet",
            //    id: "80001",
            //    blockCreationTime: TimeSpan.FromSeconds(5),
            //    requiredBlockConfirmations: 10,
            //    getCurrencyUrl: "https://faucet.matic.network",
            //    nodeUrls: new string[] {
            //        "https://polygon-mumbai.infura.io/v3/d5e92759c70f41b0a2ca3c2f39ce4c75", // admin@sunday.games
            //        "https://rpc.ankr.com/polygon_mumbai",
            //        "https://polygon-mumbai.infura.io/v3/510c2b6aeeee4af7992a9a4e8e701fec", // goodthingscomeafterbad@gmail.com
            //        "https://polygon-mumbai.infura.io/v3/2a3023ee410245b98df8e7e747f6f75a", // admin@0x.games Matic
            //    },
            //    explorer: new Explorer("https://mumbai.polygonscan.com/"));

            var amoy = new Network("Polygon Amoy Testnet",
                id: "80002",
                blockCreationTime: TimeSpan.FromSeconds(5),
                requiredBlockConfirmations: 10,
                getCurrencyUrl: "https://faucet.polygon.technology",
                nodeUrls: new string[] {
                    "https://polygon-amoy.infura.io/v3/d5e92759c70f41b0a2ca3c2f39ce4c75", // admin@sunday.games
                    "https://rpc-amoy.polygon.technology",
                },
                explorer: new Explorer("https://amoy.polygonscan.com/"));

            // https://docs.matic.network/docs/develop/network-details/network/
            network = Configurator.production ? mainnet : amoy;
        }

        [Serializable]
        private class PolygonGasStation
        {
            public Fee safeLow;
            public Fee standard;
            public Fee fast;
            public decimal estimatedBaseFee;

            [Serializable]
            public class Fee
            {
                public decimal maxPriorityFee;
                public decimal maxFee;
            }
        }

        public override IEnumerator GetFeePrice(bool showLoading = false, Result result = null)
        {
            if (feeInfoLastUpdated != default && (DateTime.UtcNow - feeInfoLastUpdated).TotalSeconds < 5.0)
            {
                Log.Warning($"GetFeePrice - Update is too frequent. Last update was {(int)(DateTime.UtcNow - feeInfoLastUpdated).TotalSeconds} seconds ago");
                result?.SetSuccess(new DictSO
                {
                    ["feeBase"] = feeBase,
                    ["feePrioritySuggestion"] = feePrioritySuggestion,
                });
                yield break;
            }

            var download = new Download("https://gasstation.polygon.technology/v2")
                      .IgnoreError();

            if (showLoading)
                download.SetLoadingName("feePriceUpdating".Localize());

            yield return download.RequestCoroutine();

            if (!download.success || download.responseDict == null)
            {
                result?.SetError(download.errorMessage);
                yield break;
            }

            var data = JsonConvert.DeserializeObject<PolygonGasStation>(download.responseText);
            if (data == null)
            {
                Log.Error("GetFeePrice - Unexpected response: " + download.responseText);
                result?.SetError(download.responseText);
                yield break;
            }

            Log.Info($"GetFeePrice - SafeLow={data.safeLow.maxFee}, Standard={data.standard.maxFee}, Fast={data.fast.maxFee}");

            decimal Round(decimal value) => Math.Ceiling(value * 10) / 10m;

            feeBase = Round(data.estimatedBaseFee);

            feePrioritySuggestion = new List<decimal> { Round(data.safeLow.maxPriorityFee) };
            if (data.standard.maxPriorityFee > data.safeLow.maxPriorityFee)
                feePrioritySuggestion.Add(Round(data.standard.maxPriorityFee));
            if (data.fast.maxPriorityFee > data.standard.maxPriorityFee)
                feePrioritySuggestion.Add(Round(data.fast.maxPriorityFee));

            feeInfoLastUpdated = DateTime.UtcNow;

            onFeeInfoUpdated?.Invoke(this);

            result?.SetSuccess(new DictSO
            {
                ["feeBase"] = feeBase,
                ["feePrioritySuggestion"] = feePrioritySuggestion,
            });
        }
    }
}