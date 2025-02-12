using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class NeoX : Ethereum
    {
        public override void Setup()
        {
            name = Names.NeoX;

            NativeTokenSetup(
                new Token("GAS", Token.Standarts.Native, decimals: 18, nameMin: "WEI"),
                new SmartContract() { Blockchain = this },
                new SmartContract.Function() { Name = "transfer", Payable = true, Fee = 30000 });

            GasToken = new Token("GAS", Token.Standarts.Native, decimals: 0, nameMin: "GAS");
            gasMax = 6700000;
            gasMultiplier = 1.2m;

            FeeToken = new Token("GWEI", Token.Standarts.Native, decimals: 9, nameMin: "WEI");
            feePriceMin = 40;
            feePriceWarning = 100;

            // https://docs.banelabs.org/development/development-environment-information

            // TODO nonFungibleTokenUrl: MainnetExplorer + "tokens/{0}/instance/{1}/",
            network = Configurator.production ?
                new Network("NeoX Mainnet",
                    id: "47763",
                    blockCreationTime: TimeSpan.FromSeconds(11),
                    requiredBlockConfirmations: 2,
                    nodeUrls: new string[] { "https://mainnet-1.rpc.banelabs.org" },
                    explorer: new Explorer("https://xexplorer.neo.org/"))
                :
                new Network("NeoX Testnet4",
                    id: "12227332",
                    blockCreationTime: TimeSpan.FromSeconds(11),
                    requiredBlockConfirmations: 2,
                    getCurrencyUrl: "https://neoxwish.ngd.network/",
                    nodeUrls: new string[] { "https://neoxt4seed1.ngd.network" },
                    explorer: new Explorer("https://xt4scan.ngd.network/"));
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

            var download = new Download(network.explorer.Url + "api/v2/stats").IgnoreError();

            if (showLoading)
                download.SetLoadingName("feePriceUpdating".Localize());

            yield return download.RequestCoroutine();

            if (!download.success || download.responseDict == null)
            {
                result?.SetError(download.errorMessage);
                yield break;
            }

            if (!download.responseDict.TryGetDict("gas_prices", out DictSO data))
            {
                Log.Error("GetFeePrice - Unexpected response: " + download.responseText);
                yield break;
            }

            // NativeToken.UsdRate = data.GetDecimal("coin_price");

            decimal GetDecimal(string key) => data[key].ToDecimal();
            decimal Round(decimal value) => Math.Ceiling(value * 10) / 10m;

            var slow = Round(GetDecimal("slow"));
            var average = Round(GetDecimal("average"));
            var fast = Round(GetDecimal("fast"));

            Log.Info($"GetFeePrice - safeLow={slow}, average={average}, fastest={fast}");

            feeBase = slow;
            feePrioritySuggestion = new List<decimal> { slow - slow };
            if (average > slow)
                feePrioritySuggestion.Add(average - slow);
            if (fast > average)
                feePrioritySuggestion.Add(fast - slow);


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