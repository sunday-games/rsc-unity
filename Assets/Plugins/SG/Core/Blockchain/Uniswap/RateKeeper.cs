using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Blockchain/" + nameof(RateKeeper), fileName = nameof(RateKeeper), order = 0)]
    public class RateKeeper : ScriptableObject
    {
        public Blockchain.Names BlockchainName;
        protected Blockchain Blockchain => BlockchainName.ToBlockchain();

        [Space(20)]
        public TokenView TargetTokenView;
        public string TargetTokenRate;
        [Space]
        public float FrequencyMinutes = 5f;
        public Vector2 FrequencyRandom = new Vector2(0.5f, 1.5f);
        [Space]
        public float UsdBuySize = 50f;
        public Vector2 UsdBuyRandom = new Vector2(0.5f, 1.5f);
        [Space]
        public int FeePriceGWEI = 5;

#if SG_BLOCKCHAIN
        [Space]
        public List<AccountView> Accounts;
        [UI.Button("SetupAccounts_OnClick")] public bool SetupAccounts;
        public void SetupAccounts_OnClick()
        {
            var blockchain = Blockchain;

            if (!blockchain || !blockchain.loginedAccount || !(blockchain.wallet is SundayWallet sundayWallet))
            {
                Log.Blockchain.Error("Please login first");
                return;
            }

            var router = Uniswap.RouterContracts[blockchain].Address;
            var targetToken = TargetTokenView.Token;
            var usdToken = targetToken.UniswapPairContracts[blockchain].First().Key;

            Approver().Start();
            IEnumerator Approver()
            {
                for (int i = 0; i < Accounts.Count; i++)
                {
                    var address = Accounts[i].Address;
                    Log.Info($"Approver {i + 1}/{Accounts.Count}: {address}");

                    var targetTokenApproved = false;
                    yield return targetToken.GetIsApproved(blockchain, address, router, targetToken.TotalSupplyLimit / 2,
                        aprooved => targetTokenApproved = aprooved);
                    if (!targetTokenApproved)
                    {
                        yield return sundayWallet.TransactionSignAndSend_AutoConfirm(
                            new Token.ApproveTransaction(targetToken, blockchain, router, targetToken.TotalSupplyLimit) { From = address },
                            FeePriceGWEI);
                        yield return new WaitForSecondsRealtime(5f);
                    }

                    var usdTokenApproved = false;
                    yield return usdToken.GetIsApproved(blockchain, address, router, usdToken.TotalSupplyLimit / 2,
                        aprooved => usdTokenApproved = aprooved);
                    if (!usdTokenApproved)
                    {
                        yield return sundayWallet.TransactionSignAndSend_AutoConfirm(
                            new Token.ApproveTransaction(usdToken, blockchain, router, usdToken.TotalSupplyLimit) { From = address },
                            FeePriceGWEI);
                        yield return new WaitForSecondsRealtime(5f);
                    }
                }
            }
        }

        [Space(20)]
        [UI.Button("RunButton_OnClick")] public bool Run;
        [Space]
        [ReadOnly] public string TimeLeftUntilNextTx;
        public void RunButton_OnClick()
        {
            var blockchain = Blockchain;

            if (!blockchain || !blockchain.loginedAccount || !(blockchain.wallet is SundayWallet sundayWallet))
            {
                Log.Blockchain.Error("Please login first");
                return;
            }

            decimal targetRate = TargetTokenRate.ToDecimal();
            decimal currentRate = default;
            var targetToken = TargetTokenView.Token;
            var usdToken = targetToken.UniswapPairContracts[blockchain].First().Key;

            RateKeeper().Start();
            IEnumerator RateKeeper()
            {
                while (true)
                {
                    var recipient = Accounts.GetRandom().Address;

                    yield return targetToken.GetUniswapRate(blockchain, usdToken, rate => currentRate = rate);
                    var needBuy = targetRate > currentRate;

                    decimal usd = (UsdBuySize * UnityEngine.Random.Range(UsdBuyRandom.x, UsdBuyRandom.y)).ToDecimal();

                    Token sellToken = needBuy ? usdToken : targetToken;
                    decimal sellAmount = needBuy ? usd : usd / currentRate;

                    Token buyToken = !needBuy ? usdToken : targetToken;
                    decimal buyAmount = !needBuy ? usd : usd / currentRate;

                    Log.Info($"RateKeeper - Current rate is {currentRate.ToStringFormated3()} so I will {(needBuy ? "buy" : "sell")} {targetToken.Name}." +
                        $" {sellAmount.ToStringFormated2(sellToken)} >> {buyAmount.ToStringFormated2(buyToken)}");

                    var tx = new Uniswap.SwapTransaction(
                        Blockchain, sellToken, sellAmount, buyToken, buyAmount, recipient)
                    {
                        From = recipient
                    };

                    yield return sundayWallet.TransactionSignAndSend_AutoConfirm(tx, FeePriceGWEI);

                    if (tx.error.IsNotEmpty())
                    {
                        yield return new WaitForSecondsRealtime(5f);
                        continue;
                    }

                    var timeLeft = FrequencyMinutes * 60 * UnityEngine.Random.Range(FrequencyRandom.x, FrequencyRandom.y);
                    while (timeLeft > 0)
                    {
                        TimeLeftUntilNextTx = TimeSpan.FromSeconds(timeLeft.ToInt()).ToString();
                        yield return new WaitForSecondsRealtime(1f);
                        timeLeft -= 1f;
                    }
                }
            }
        }
#endif
    }
}