using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class TransferTransaction : Transaction
    {
        public const string TYPE = "TRANSFER";
        public const string TYPE_RECEIVE = "TRANSFER_GET";

        public const string PLACEHOLDER_ADDRESS = "0x0123456789012345678901234567890123456789";

        public TransferTransaction(Token token, Blockchain blockchain, string recipient = null, decimal amount = default, long? id = null, string from = null) :
            base(TYPE, payload: new DictSO { ["tokenName"] = token.Name })
        {
            var function = token.TransferFunction[blockchain];

            if (from.IsEmpty())
                from = blockchain.loginedAccount.Address;

            if (token.Standart == Token.Standarts.ERC20)
                SetFunction(function,
                    new DictSO
                    {
                        ["recipient"] = recipient ?? PLACEHOLDER_ADDRESS,
                        ["amount"] = token.ToMin(amount)
                    },
                    from);

            else if (token.Standart == Token.Standarts.ERC1155 && id.HasValue)
                SetFunction(function,
                    new DictSO
                    {
                        ["from"] = from,
                        ["recipient"] = recipient ?? PLACEHOLDER_ADDRESS,
                        ["id"] = id,
                        ["amount"] = (long)amount,
                        ["data"] = "0x"
                    },
                    from);

            else if (token.Standart == Token.Standarts.Native)
            {
                SetFunction(function, new DictSO(), from);
                gasEstimation = function.Fee;
                From = from;
                To = recipient;
                NativeTokenAmount = amount;
            }
        }
    }
}