#if UNITY_EDITOR && SG_BLOCKCHAIN
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/" + nameof(SeaportView), fileName = nameof(SeaportView), order = 0)]
    public class SeaportView : ScriptableObject
    {
        public List<Seaport.Order> Orders = new List<Seaport.Order>();

        [Space(20)]
        public Blockchain.Names BlockchainName;
        public Blockchain blockchain => BlockchainName.ToBlockchain();
        [Space]
        public TokenView NFTView;
        public int NFTId;
        public int NFTAmount;
        [Space]
        public TokenView CoinView;
        public double CoinPrice;
        [Space]
        public int FeePercentage;
        public string FeeBeneficiary;

        [UI.Button("MakeOrderButton_OnClick")] public bool MakeOrder;
        public void MakeOrderButton_OnClick()
        {
            Coroutine().Start();
            IEnumerator Coroutine()
            {
                var seller = blockchain.loginedAccount.Address;

                int nftBalance = 0;
                yield return NFTView.Token.GetBalance(blockchain, seller, NFTId, result => nftBalance = result.ToInt());
                Log.Blockchain.Info($"The seller has {nftBalance}/{NFTAmount} {NFTView.Token.Name}");
                if (nftBalance < NFTAmount)
                {
                    Log.Blockchain.Error($"The seller has not enough {NFTView.Token.Name}");
                    yield break;
                }

                bool nftApproved = false;
                yield return NFTView.Token.GetIsApproved(blockchain, seller, Seaport.Addresses[blockchain.name], NFTAmount, result => nftApproved = result);
                if (!nftApproved)
                {
                    Log.Blockchain.Warning("The seller does not approve enough " + NFTAmount.ToString(NFTView.Token));

                    yield return NFTView.Token.Approve(blockchain, Seaport.Addresses[blockchain.name], NFTAmount);

                    yield break;
                }

                yield return Seaport.CreateOrderEnumerator(Seaport.Order.Type.SALE, blockchain, seller,
                    NFTView.Token, NFTId, NFTAmount,
                    CoinView.Token, (decimal) CoinPrice, FeePercentage, FeeBeneficiary,
                    order =>
                    {
                        Log.Blockchain.Info("New order: " + JsonUtility.ToJson(order));

                        Orders.Add(order);
                        UnityEditor.EditorUtility.SetDirty(this);
                    });
            }
        }


        [Space(20)]
        public int OrderIndex;
        [UI.Button("FulfillOrderButton_OnClick")] public bool FulfillOrder;
        public void FulfillOrderButton_OnClick()
        {
            Coroutine(Orders[OrderIndex]).Start();
            IEnumerator Coroutine(Seaport.Order order)
            {
                var buyer = blockchain.loginedAccount.Address;

                if (buyer == order.seller)
                {
                    Log.Blockchain.Error("The buyer and the seller cant be equal");
                    yield break;
                }

                decimal? coinBalance = null;
                yield return CoinView.Token.GetBalance(blockchain, buyer, callback: balance => coinBalance = balance);
                if (!coinBalance.HasValue)
                {
                    Log.Blockchain.Error($"Fail to update the buyer balance {CoinView.Token.Name}");
                    yield break;
                }
                Log.Blockchain.Info($"The buyer has {coinBalance}/{CoinPrice} {CoinView.Token.Name}");
                if (coinBalance < (decimal) CoinPrice)
                {
                    Log.Blockchain.Error($"The buyer has not enough {CoinView.Token.Name}");
                    yield break;
                }

                int nftBalance = 0;
                yield return NFTView.Token.GetBalance(blockchain, order.seller, NFTId, result => nftBalance = result.ToInt());
                Log.Blockchain.Info($"The seller has {nftBalance}/{NFTAmount} {NFTView.Token.Name}");
                if (nftBalance < NFTAmount)
                {
                    Log.Blockchain.Error($"The seller has not enough {NFTView.Token.Name}");
                    yield break;
                }

                bool nftApproved = false;
                yield return NFTView.Token.GetIsApproved(blockchain, order.seller, Seaport.Addresses[blockchain.name], NFTAmount, result => nftApproved = result);
                if (!nftApproved)
                {
                    Log.Blockchain.Warning("The seller does not approve enough " + NFTAmount.ToString(NFTView.Token));
                    yield break;
                }

                bool coinApproved = false;
                yield return CoinView.Token.GetIsApproved(blockchain, buyer, Seaport.Addresses[blockchain.name], (decimal) CoinPrice, result => coinApproved = result);
                if (!coinApproved)
                {
                    Log.Blockchain.Warning("The buyer does not approve enough " + CoinPrice.ToDecimal().ToStringFormated2(NFTView.Token));

                    yield return CoinView.Token.Approve(blockchain, Seaport.Addresses[blockchain.name], int.MaxValue);

                    yield break;
                }

                yield return Seaport.FulfillBasicOrderEnumerator(order,
                    tx =>
                    {
                        if (tx.Hash.IsNotEmpty())
                        {
                            Log.Blockchain.Info("Order fulfillment success. Tx hash: " + tx.Hash);
                            Orders.Remove(order);
                        }
                    });
            }
        }
    }
}
#endif