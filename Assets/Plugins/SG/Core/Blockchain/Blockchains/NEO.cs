using System;

namespace SG.BlockchainPlugin
{
    public class NEO : Blockchain
    {
        // public static ERC20 NEO = new ERC20(code: "NEO", codeMin: "NEO", decimals: 0);

        public override void Setup()
        {
            name = Names.NEO;

            NativeTokenSetup(
                new Token("GAS", Token.Standarts.Native, decimals: 8),
                new SmartContract() { Blockchain = this },
                // https://docs.neo.org/docs/en-us/reference/rpc/latest-version/api/sendfrom.html
                new SmartContract.Function() { Name = "sendfrom", Payable = true });

            gasMax = 10; // TODO

            network = Configurator.production ?
                new Network("NEO Mainnet",
                    id: "1",
                    blockCreationTime: TimeSpan.FromSeconds(10),
                    explorer: new Network.Explorer("https://neoscan.io/"))
                :
                new Network("NEO Testnet",
                    id: "2",
                    blockCreationTime: TimeSpan.FromSeconds(10),
                    explorer: new Network.Explorer("https://neoscan-testnet.io/"));
        }

        const string prefix = "A";
        public override bool IsValidAddress(string address) =>
            !address.IsEmpty() && address.Length == 33 + prefix.Length && address.Substring(0, prefix.Length) == prefix;
        public override string RandomAddress() => prefix + Utils.RandomString(33);
    }
}