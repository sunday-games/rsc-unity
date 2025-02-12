using System;
using System.Collections.Generic;
using System.Linq;
using DictSS = System.Collections.Generic.Dictionary<string, string>;

namespace SG.BlockchainPlugin
{
    public class WAX : EOSIO
    {
        public override void Setup()
        {
            name = Names.WAX;

            NativeTokenSetup(
                new Token("WAX", Token.Standarts.Native, decimals: 8),
                new SmartContract() { Blockchain = this, Address = "eosio.token", Name = "EOSIO Token" },
                new SmartContract.Function() // https://developers.eos.io/eosio-cleos/reference#cleos-transfer
                {
                    Name = "transfer",
                    Payable = true,
                    Inputs = new DictSS
                    {
                        ["sender"] = "TEXT",
                        ["recipient"] = "TEXT",
                        ["amount"] = "UINT",
                        ["memo"] = "TEXT",
                    }
                });

            var system = new SmartContract() { Blockchain = this, Address = "eosio", Name = "EOSIO" };

            stakeFunction = new SmartContract.Function()
            {
                Name = "delegatebw",
                Contract = system,
                Payable = true,
                Inputs = new DictSS
                {
                    ["from"] = "TEXT",
                    ["receiver"] = "TEXT",
                    ["stake_net_quantity"] = "TEXT",
                    ["stake_cpu_quantity"] = "TEXT",
                }
            };

            unstakeFunction = new SmartContract.Function()
            {
                Name = "undelegatebw",
                Contract = system,
                Payable = true,
                Inputs = new DictSS
                {
                    ["from"] = "TEXT",
                    ["receiver"] = "TEXT",
                    ["unstake_net_quantity"] = "TEXT",
                    ["unstake_cpu_quantity"] = "TEXT",
                }
            };

            buyramFunction = new SmartContract.Function()
            {
                Name = "buyram",
                Contract = system,
                Payable = true,
                Inputs = new DictSS { ["payer"] = "TEXT", ["receiver"] = "TEXT", ["tokens"] = "TEXT" }
            };

            sellramFunction = new SmartContract.Function()
            {
                Name = "sellram",
                Contract = system,
                Payable = true,
                Inputs = new DictSS { ["account"] = "TEXT", ["bytes"] = "UINT" }
            };

            network = Configurator.production ?
                new Network("WAX Mainnet",
                    id: "1064487b3cd1a897ce03ae5b6a865651747e2e152090f99c1d19d44e01aea5a4",
                    blockCreationTime: TimeSpan.FromSeconds(5),
                    nodeUrls: new string[] {
                        "https://chain.wax.io:443",
                        // "https://wax.greymass.com:443",
                    },
                    explorer: new Explorer("https://wax.bloks.io/"))
                :
                new Network("WAX Testnet",
                    id: "f16b1833c747c43682f4386fca9cbb327929334a762755ebec17f6f23c9b8a12",
                    blockCreationTime: TimeSpan.FromSeconds(5),
                    getCurrencyUrl: "https://waxsweden.org/testnet",
                    nodeUrls: new string[] {
                        "https://api.waxsweden.org:443",
                        "https://wax-test.eosdac.io:443",
                    },
                    explorer: new Explorer("https://wax-test.bloks.io/"));
        }
    }
}