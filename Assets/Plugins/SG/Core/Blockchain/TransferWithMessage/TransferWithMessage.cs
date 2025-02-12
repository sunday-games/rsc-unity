using System;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public static class TransferWithMessage
    {
        public static Dictionary<Blockchain, SmartContract> Contracts;

        static TransferWithMessage()
        {
            var addresses = Configurator.production ?
                new BlockchainAddressDictionary
                {
                    [Blockchain.Names.BSC] = "0x1e3546b9ec8bc93a8f426d692f1f317ef962e88d",
                    [Blockchain.Names.NeoX] = "0xd35eA08f0f53810D1fA198854B5Ff9f60216c529",
                } :
                new BlockchainAddressDictionary
                {
                    [Blockchain.Names.BSC] = "0x630ce09bf59ef0c3560263fb29d5edbada0291df",
                    [Blockchain.Names.NeoX] = "0x48a4eA55fdB4D165c8Eb1Dc697b7a3ba11e591C4",
                };

            Contracts = addresses.ToContracts("TransferWithMessage");
        }

        public class TransferToken : Transaction
        {
            public const string TYPE = "TRANSFER";

            public const string FUNCTION_NAME = "transferToken";
            public static SmartContract.Function GetFunction(Blockchain blockchain) =>
                Contracts[blockchain].Functions[FUNCTION_NAME];

            public TransferToken(Token token, Blockchain blockchain, string recipient, decimal amount, string message, string from = null) :
                base(TYPE, payload: new DictSO { ["tokenName"] = token.Name, ["tokenAmount"] = token.ToMin(amount) })
            {
                SetFunction(GetFunction(blockchain),
                    new DictSO
                    {
                        ["to"] = recipient,
                        ["message"] = message,
                        ["token"] = token.GetAddress(blockchain),
                        ["amount"] = token.ToMin(amount),
                    },
                    from);
            }
        }
    }
}