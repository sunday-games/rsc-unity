using UnityEngine;
using System;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Blockchain/" + nameof(UniswapView), fileName = nameof(UniswapView), order = 0)]
    public class UniswapView : ScriptableObject
    {
        public Blockchain.Names BlockchainName;
        protected Blockchain Blockchain => BlockchainName.ToBlockchain();
        public TextAsset FactoryABI;

        public TokenView SellTokenView;
        public string SellAmount;
        public TokenView BuyTokenView;

        [Space(20)]
        [UI.Button("UpdateUniswapRateButton_OnClick")] public bool UpdateUniswapRate;
        public string Rate;
        public void UpdateUniswapRateButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning("Please login first");
                return;
            }

            UpdateRate(BuyTokenView.Token);
        }
        private void UpdateRate(Token token, Action<decimal> callback = null)
        {
            token.GetUniswapRate(
                    callback: rate =>
                    {
                        Rate = rate.ToString();
                        callback?.Invoke(rate);
                    })
                .Start();
        }

        [Space(20)]
        [UI.Button("GetPairAddressButton_OnClick")] public bool GetPairAddressButton;
        public string PairAddress;
        public void GetPairAddressButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Error("Please login first");
                return;
            }

            Uniswap.GetFactoryAddress(Blockchain, factoryAddress =>
            {
                new SmartContract { Blockchain = Blockchain, Address = factoryAddress, ABI = FactoryABI.text }
                    .Functions["getPair"].Read<string>(
                        new DictSO
                        {
                            ["tokenIn"] = SellTokenView.Token.GetAddress(Blockchain),
                            ["tokenOut"] = BuyTokenView.Token.GetAddress(Blockchain),
                        },
                        pairAddress => PairAddress = pairAddress.Value);
            });
        }

        [Space(20)]
        [UI.Button("ExecuteButton_OnClick")] public bool Execute;
        public void ExecuteButton_OnClick()
        {
            var blockchain = Blockchain;

            if (!blockchain || !blockchain.loginedAccount)
            {
                Log.Blockchain.Error("Please login first");
                return;
            }

            var router = Uniswap.RouterContracts[blockchain].Address;
            var recipient = Blockchain.loginedAccount.Address;

            SellTokenView.Token.GetIsApproved(Blockchain, recipient, router, SellTokenView.Token.TotalSupplyLimit / 2,
                    sellTokenAprooved =>
                    {
                        if (!sellTokenAprooved)
                        {
                            SellTokenView.Token.Approve(Blockchain, router, SellTokenView.Token.TotalSupplyLimit).Start();
                            return;
                        }

                        var transaction = new Uniswap.SwapTransaction(
                            Blockchain, SellTokenView.Token, SellAmount.ToDecimal(), BuyTokenView.Token, default, recipient);

                        Blockchain.wallet.TransactionSignAndSend(transaction,
                                tx =>
                                {
                                    if (tx.Hash.IsNotEmpty())
                                        UpdateRate(BuyTokenView.Token);
                                })
                            .Start();
                    })
                .Start();
        }
    }
}