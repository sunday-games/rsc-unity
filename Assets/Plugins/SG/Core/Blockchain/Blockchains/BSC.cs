using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class BSC : Ethereum
    {
        public override void Setup()
        {
            name = Names.BSC;

            NativeTokenSetup(
                new Token("BNB", Token.Standarts.Native, decimals: 18, nameMin: "WEI"),
                new SmartContract() { Blockchain = this },
                new SmartContract.Function() { Name = "transfer", Payable = true, Fee = 30000 });

            GasToken = new Token("GAS", Token.Standarts.Native, decimals: 0, nameMin: "GAS");
            gasMax = 6700000;
            gasMultiplier = 1.2m;

            FeeToken = new Token("GWEI", Token.Standarts.Native, decimals: 9, nameMin: "WEI");
            feePriceMin = 3;
            feePriceWarning = 10;

            // https://docs.binance.org/smart-chain/developer/rpc.html

            // TODO nonFungibleTokenUrl: MainnetExplorer + "tokens/{0}/instance/{1}/",
            network = Configurator.production ?
                new Network("BNB Smart Chain Mainnet",
                    id: "56",
                    blockCreationTime: TimeSpan.FromSeconds(3),
                    requiredBlockConfirmations: 10,
                    nodeUrls: new string[]
                    {
                        "https://bsc-dataseed.binance.org",
                        "https://bsc-dataseed1.defibit.io",
                        "https://bsc-dataseed1.ninicoin.io",
                        // "https://bsc-mainnet.infura.io/v3/a4272357ad12430c9b2884f6f682b7de", sergey.kopov@gmail.com
                    },
                    explorer: new Explorer("https://bscscan.com/"))
                :
                new Network("BNB Smart Chain Testnet",
                    id: "97",
                    blockCreationTime: TimeSpan.FromSeconds(3),
                    requiredBlockConfirmations: 10,
                    getCurrencyUrl: "https://testnet.binance.org/faucet-smart",
                    nodeUrls: new string[] {
                        "https://data-seed-prebsc-1-s3.binance.org:8545",
                        "https://data-seed-prebsc-2-s3.binance.org:8545",
                        "https://data-seed-prebsc-1-s1.binance.org:8545",
                        "https://data-seed-prebsc-2-s1.binance.org:8545",
                        "https://data-seed-prebsc-1-s2.binance.org:8545",
                        "https://data-seed-prebsc-2-s2.binance.org:8545",
                    },
                    explorer: new Explorer("https://testnet.bscscan.com/"));
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

            if (!Configurator.production)
            {
                feeBase = 10;
                feePrioritySuggestion = new List<decimal> { 0 };
            }
            else
            {
                var download = new Download("https://api.bscscan.com/api?module=gastracker&action=gasoracle&apikey=HMAVHI94YGZMNZP7VI6RYSZC42S9HI8RX7")
                               .IgnoreError();

                if (showLoading)
                    download.SetLoadingName("feePriceUpdating".Localize());

                yield return download.RequestCoroutine();

                if (!download.success || download.responseDict == null)
                {
                    result?.SetError(download.errorMessage);
                    yield break;
                }

                if (download.responseDict["status"].ToInt() != 1 ||
                    !download.responseDict.TryGetDict("result", out DictSO data))
                {
                    Log.Error("GetFeePrice - Unexpected response: " + download.responseText);
                    yield break;
                }

                NativeToken.UsdRate = data.GetDecimal("UsdPrice");

                decimal GetDecimal(string key) => data[key].ToDecimal();
                decimal Round(decimal value) => Math.Ceiling(value * 10) / 10m;

                var safe = Round(GetDecimal("SafeGasPrice"));
                var propose = Round(GetDecimal("ProposeGasPrice"));
                var fast = Round(GetDecimal("FastGasPrice"));

                Log.Info($"GetFeePrice - safeLow={safe}, average={propose}, fastest={fast}");

                feeBase = safe;
                feePrioritySuggestion = new List<decimal> { safe - safe };
                if (propose > safe)
                    feePrioritySuggestion.Add(propose - safe);
                if (fast > propose)
                    feePrioritySuggestion.Add(fast - safe);
            }

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