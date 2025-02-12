using System.Collections;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Token/NFT", fileName = "NFT", order = 0)]
    public class ERC1155View : TokenView
    {
        [Header("NFT")]
        public int OwnerFee;
        public string MetaUrl;
        public NFT[] Items;

        public override Token SetupToken()
        {
            base.SetupToken();

            Token.OwnerFee = OwnerFee;
            Token.MetaUrl = MetaUrl;

            foreach (var nft in Items)
                nft.Token = Token;

            return Token;
        }

        public override void GetBalanceButton_OnClick()
        {
            var blockchain = BlockchainName.ToBlockchain();

            if (!blockchain || !blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            var recipients = ParseRecipient(Balance.Recipients);

            Routine().Start();
            IEnumerator Routine()
            {
                Result = string.Empty;
                for (int i = 0; i < recipients.Count; i++)
                {
                    yield return Token.GetBalance(blockchain, recipients[i], Balance.TokenId, result =>
                    {
                        Log.Blockchain.Info($"Balance {i + 1} / {recipients.Count}. {recipients[i]}: {result}");

                        if (Result.IsNotEmpty())
                            Result += Const.lineBreak;

                        Result += recipients[i] + "," + result;
                    });
                }
            }
        }

        public override void GetTotalSupplyButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.GetTotalSupply(Blockchain,
                    callback: result =>
                    {
                        Result = result.ToString();
                    })
                .Start();
        }

        public override void TransferButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            var recipients = ParseRecipientAmount(Transfer.Recipients);

            Routine().Start();
            IEnumerator Routine()
            {
                for (int i = 0; i < recipients.Count; i++)
                {
                    yield return Token.Transfer_AutoConfirm(Blockchain, recipients[i].address, recipients[i].amount, GasPriceGWEI, Transfer.TokenId);

                    Log.Blockchain.Info($"Transfer {i + 1} / {recipients.Count}");

                    yield return new WaitForSeconds(Time);
                }
            }
        }

        public override void MintButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.Mint(Blockchain, Mint.Recipient, Mint.Amount.ToDecimal(), Mint.TokenId)
                .Start();
        }

        public override void BurnButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.Burn(Blockchain, Burn.Amount.ToDecimal(), Burn.TokenId)
                .Start();
        }
    }
}