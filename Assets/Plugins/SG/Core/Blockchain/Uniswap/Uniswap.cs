using System;
using System.Numerics;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

#if SG_BLOCKCHAIN
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
#endif

namespace SG.BlockchainPlugin
{
    public static class Uniswap
    {
        public static Dictionary<Blockchain, SmartContract> RouterContracts;

        static Uniswap()
        {
            var addresses = new BlockchainAddressDictionary
            {
                [Blockchain.Names.BSC] = Configurator.production ? "0x10ed43c718714eb63d5aa57b78b54704e256024e" : "0x9Ac64Cc6e4415144C455BD8E4837Fea55603e5c3",
            };

            RouterContracts = addresses.ToContracts("UniswapRouter");
        }

        public class SwapTransaction : Transaction
        {
            public const string TYPE = "SWAP";
            public static string[] FunctionNames = new string[]
            {
                "swapExactETHForTokens",
                "swapExactTokensForTokens",
            };

            public static SmartContract.Function GetFunction(Blockchain blockchain, string sellToken) =>
                RouterContracts[blockchain]
                    .Functions[FunctionNames[sellToken == blockchain.NativeToken.Name ? 0 : 1]];

            private const double TIMEOUT = 60; // minutes

            public SwapTransaction(Blockchain blockchain, Token sellToken, decimal sellAmount, Token buyToken, decimal buyAmount, string recipient = null, Token middleToken = null) :
                base(GetFunction(blockchain, sellToken.Name), TYPE)
            {
                // The minimum amount of output tokens that must be received for the transaction not to revert
                var amountOutMin = 0;

                // address[] calldata - An array of token addresses. path.length must be >= 2
                var sellTokenAddress = (sellToken == blockchain.NativeToken ? blockchain.NativeWrappedToken : sellToken).GetAddress(Blockchain);
                var path = middleToken != null ?
                    new string[] { sellTokenAddress, middleToken.GetAddress(Blockchain), buyToken.GetAddress(Blockchain) } :
                    new string[] { sellTokenAddress, buyToken.GetAddress(Blockchain) };

                var to = recipient.IsNotEmpty() ? recipient : blockchain.loginedAccount.Address;

                // uint - Unix timestamp after which the transaction will revert
                var deadline = DateTimeOffset.UtcNow.AddMinutes(TIMEOUT).ToUnixTimeSeconds();

                if (sellToken == blockchain.NativeToken)
                {
                    NativeTokenAmount = sellAmount;

                    Content = new DictSO
                    {
                        ["amountOutMin"] = amountOutMin,
                        ["path"] = path,
                        ["to"] = to,
                        ["deadline"] = deadline,
                    };
                }
                else
                {
                    Content = new DictSO
                    {
                        ["amount"] = sellToken.ToMin(sellAmount),
                        ["amountOutMin"] = amountOutMin,
                        ["path"] = path,
                        ["to"] = to,
                        ["deadline"] = deadline,
                    };
                }

                Payload = new DictSO
                {
                    ["tokenName"] = sellToken.Name,
                    ["tokenAmount"] = sellToken.ToMin(sellAmount),
                    ["targetTokenName"] = buyToken.Name,
                    ["targetTokenAmount"] = buyToken.ToMin(buyAmount),
                };
            }
        }
#if SG_BLOCKCHAIN
        public class GetReserves
        {
            [FunctionOutput]
            public class Output : FunctionOutputDTO
            {
                [Parameter("uint112", "_reserve0", 1)]
                public virtual BigInteger ReserveA { get; set; }
                [Parameter("uint112", "_reserve1", 2)]
                public virtual BigInteger ReserveB { get; set; }
                [Parameter("uint32", "_blockTimestampLast", 3)]
                public virtual uint BlockTimestampLast { get; set; }
            }
        }
#endif
        public static void GetFactoryAddress(Blockchain blockchain, Action<string> callback) =>
            RouterContracts[blockchain].Functions["factory"].Read<string>(
                new DictSO(),
                result => callback.Invoke(result.Value));
    }
}