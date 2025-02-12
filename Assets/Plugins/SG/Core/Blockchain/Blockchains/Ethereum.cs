using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using Newtonsoft.Json;

namespace SG.BlockchainPlugin
{
    public class Ethereum : Blockchain
    {
        public class Explorer : Network.Explorer
        {
            public Explorer(string url) : base(url) { }
            public override string GetTxURL(string tx) => $"{Url}tx/{tx}";
            public override string GetAddressURL(string address) => $"{Url}address/{address}";
            public override string GetNonFungibleTokenURL(string address, int id) => $"{Url}token/{address}?a={id}";
        }

        public override void Setup()
        {
            name = Names.Ethereum;

            NativeTokenSetup(
                new Token("ETH", Token.Standarts.Native, decimals: 18, sign: "Ξ", nameMin: "WEI"),
                new SmartContract() { Blockchain = this },
                new SmartContract.Function() { Name = "transfer", Payable = true, Fee = 21000 });

            GasToken = new Token("GAS", Token.Standarts.Native, decimals: 0, nameMin: "GAS");
            gasMax = 6700000;
            gasMultiplier = 1.2m;

            FeeToken = new Token("GWEI", Token.Standarts.Native, decimals: 9, nameMin: "WEI");
            feePriceMin = 1;
            feePriceWarning = 50;

            var mainnet = new Network("Ethereum Mainnet",
                id: "1",
                blockCreationTime: TimeSpan.FromSeconds(20),
                nodeUrls: new string[]
                {
                    "https://mainnet.infura.io/v3/d5e92759c70f41b0a2ca3c2f39ce4c75", // admin@sunday.games
                    "https://eth.public-rpc.com",
                    "https://rpc.ankr.com/eth",
                    "https://mainnet.infura.io/v3/33ee31f2e19e4d98a447d34c54203c4d", // admin@0x.games
                    "https://mainnet.infura.io/v3/510c2b6aeeee4af7992a9a4e8e701fec", // goodthingscomeafterbad@gmail.com
                },
                explorer: new Explorer("https://etherscan.io/"));

            var sepolia = new Network("Ethereum Sepolia Testnet",
                id: "11155111",
                blockCreationTime: TimeSpan.FromSeconds(10),
                getCurrencyUrl: "https://sepoliafaucet.net/",
                nodeUrls: new string[]
                {
                    "https://sepolia.infura.io/v3/d5e92759c70f41b0a2ca3c2f39ce4c75", // admin@sunday.games
                    "https://rpc.sepolia.org",
                    "https://rpc.sepolia.dev",
                    "https://www.sepoliarpc.space",
                    "https://rpc-sepolia.rockx.com",
                    "https://rpc.bordel.wtf/sepolia",
                },
                explorer: new Explorer("https://sepolia.etherscan.io/"));

            network = Configurator.production ? mainnet : sepolia;
        }

        public override DictSO GetTxDefaultExecutionFormat(Transaction tx)
        {
            var parameters = new DictSO
            {
                ["from"] = tx.From,
                ["to"] = tx.To,
                ["value"] = CurrencyToTxFormat(tx.NativeTokenAmount),
#if SG_BLOCKCHAIN
                ["data"] = NethereumManager.GetData(tx),
#endif
            };
            if (tx.gasLimit > 0)
                parameters["gas"] = tx.gasLimit;

            if (tx.feeMaxPriority > 0)
            {
                parameters["maxFeePerGas"] = FeeToken.ToMin(tx.feeMax);
                parameters["maxPriorityFeePerGas"] = FeeToken.ToMin(tx.feeMaxPriority);
            }
            else
            {
                parameters["gasPrice"] = FeeToken.ToMin(tx.feeMax);
            }

            return parameters;
        }

        const string prefix = "0x";
        public override bool IsValidAddress(string address) =>
            address.IsNotEmpty() && address.Length == 40 + prefix.Length && address.Substring(0, prefix.Length) == prefix;
        public override string RandomAddress() => prefix + Utils.RandomString(40);
        public override string RandomTxHash() => prefix + base.RandomTxHash();

        [Serializable]
        private class EtherscanGasStation
        {
            public Result result;

            [Serializable]
            public class Result
            {
                public decimal SafeGasPrice;
                public decimal ProposeGasPrice;
                public decimal FastGasPrice;
                public decimal suggestBaseFee;
            }
        }

        public override IEnumerator GetFeePrice(bool showLoading = false, Result result = null)
        {
            if (feeInfoLastUpdated != default && (DateTime.UtcNow - feeInfoLastUpdated).TotalSeconds < 5.0)
            {
                Log.Warning($"GetFeePrice - Update is too frequent. Last update was {(int) (DateTime.UtcNow - feeInfoLastUpdated).TotalSeconds} seconds ago");
                result?.SetSuccess(new DictSO
                {
                    ["feeBase"] = feeBase,
                    ["feePrioritySuggestion"] = feePrioritySuggestion,
                });
                yield break;
            }

            var download = new Download("https://api.etherscan.io/api?module=gastracker&action=gasoracle&apikey=5HIPSC83HVBAEBCPP82KKHFCR5W2T2WZI7")
                .IgnoreError();

            if (showLoading)
                download.SetLoadingName("feePriceUpdating".Localize());

            yield return download.RequestCoroutine();

            if (!download.success || download.responseDict == null)
            {
                result?.SetError(download.errorMessage);
                yield break;
            }

            var data = JsonConvert.DeserializeObject<EtherscanGasStation>(download.responseText);
            if (data == null)
            {
                Log.Error("GetFeePrice - Unexpected response: " + download.responseText);
                result?.SetError(download.responseText);
                yield break;
            }

            Log.Info($"GetFeePrice - Safe={data.result.SafeGasPrice}, Propose={data.result.ProposeGasPrice}, Fast={data.result.FastGasPrice}");

            decimal Round(decimal value) => Math.Ceiling(value * 10) / 10m;

            feeBase = Round(data.result.suggestBaseFee);

            feePrioritySuggestion = new List<decimal> { Round(data.result.SafeGasPrice - feeBase) };
            if (data.result.ProposeGasPrice > data.result.SafeGasPrice)
                feePrioritySuggestion.Add(Round(data.result.ProposeGasPrice - feeBase));
            if (data.result.FastGasPrice > data.result.ProposeGasPrice)
                feePrioritySuggestion.Add(Round(data.result.FastGasPrice - feeBase));


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