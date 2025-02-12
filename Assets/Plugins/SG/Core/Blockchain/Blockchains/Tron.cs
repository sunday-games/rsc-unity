using System;
using System.Linq;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    using DictSO = Dictionary<string, object>;
    using ListO = List<object>;

    public class Tron : Blockchain
    {
        public override void Setup()
        {
            name = Names.Tron;

            NativeToken = new Token("TRX", Token.Standarts.Native, decimals: 6, nameMin: "SUN");

            GasToken = new Token("ENERGY", Token.Standarts.Native, decimals: 0, nameMin: "SUN");
            gasMax = 1000000000;
            // 1 TRX = 100000 ENERGY ( according to https://tronstation.io/calculator )

            network = Configurator.production ?
                new Network("Tron Mainnet",
                    id: "1",
                    blockCreationTime: TimeSpan.FromSeconds(10),
                    explorer: new Network.Explorer("https://tronscan.org/#/"))
                :
                new Network("Tron Shasta Testnet",
                    id: "2",
                    blockCreationTime: TimeSpan.FromSeconds(10),
                    explorer: new Network.Explorer("https://shasta.tronscan.org/#/"));
        }

        public override DictSO GetTxDefaultExecutionFormat(Transaction tx)
        {
            var i = 0;
            var inputTypes = tx.Function.Inputs.Values.ToArray();
            var parameters = new ListO();
            foreach (var param in tx.Content)
                parameters.Add(new DictSO { ["type"] = inputTypes[i++], ["value"] = param.Value });

            return new DictSO
            {
                ["issuerAddress"] = tx.From,
                ["contractAddress"] = tx.To,
                ["functionSelector"] = tx.Function.ToString(),
                ["callValue"] = CurrencyToTxFormat(tx.NativeTokenAmount).ToString(),
                ["parameters"] = parameters,
                ["feeLimit"] = gasMax, // TODO
            };
        }
        const string prefix = "T";
        public override bool IsValidAddress(string address) =>
            !address.IsEmpty() && address.Length == 33 + prefix.Length && address.Substring(0, prefix.Length) == prefix;
        public override string RandomAddress() => prefix + Utils.RandomString(33);
    }
}