using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class ImmutableEVM : Ethereum
    {
        public override void Setup()
        {
            name = Names.Immutable;

            NativeTokenSetup(
                new Token("IMX", Token.Standarts.Native, decimals: 18, nameMin: "WEI"),
                new SmartContract() { Blockchain = this },
                new SmartContract.Function() { Name = "transfer", Payable = true, Fee = 30000 });

            GasToken = new Token("GAS", Token.Standarts.Native, decimals: 0, nameMin: "GAS");
            gasMax = 6700000;
            gasMultiplier = 1.2m;

            FeeToken = new Token("GWEI", Token.Standarts.Native, decimals: 9, nameMin: "WEI");
            feePriceMin = 10;

            network = Configurator.production ?
                new Network("Immutable zkEVM Mainnet",
                    id: "13371",
                    blockCreationTime: TimeSpan.FromSeconds(3),
                    requiredBlockConfirmations: 10,
                    nodeUrls: new string[]
                    {
                        "https://rpc.immutable.com",
                    },
                    explorer: new Explorer("https://explorer.immutable.com/"))
                :
                new Network("Immutable zkEVM Testnet",
                    id: "13473",
                    blockCreationTime: TimeSpan.FromSeconds(3),
                    requiredBlockConfirmations: 10,
                    getCurrencyUrl: "https://testnet.binance.org/faucet-smart",
                    nodeUrls: new string[] {
                        "https://rpc.testnet.immutable.com",
                    },
                    explorer: new Explorer("https://explorer.testnet.immutable.com/"));
        }

        public override IEnumerator GetFeePrice(bool showLoading = false, Result result = null)
        {
            feeBase = 10;
            feePrioritySuggestion = new List<decimal> { 0 };

            feeInfoLastUpdated = DateTime.UtcNow;

            onFeeInfoUpdated?.Invoke(this);

            result?.SetSuccess(new DictSO
            {
                ["feeBase"] = feeBase,
                ["feePrioritySuggestion"] = feePrioritySuggestion,
            });

            yield break;
        }
    }
}