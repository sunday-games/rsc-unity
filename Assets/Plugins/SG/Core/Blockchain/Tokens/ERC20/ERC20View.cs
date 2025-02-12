using System;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Token/ERC20", fileName = "ERC20", order = 0)]
    public class ERC20View : TokenView
    {
        [Space(20)]
        public UniswapPairData[] UniswapPairs;
        [Serializable]
        public class UniswapPairData
        {
            public Blockchain.Names BlockchainName;
            public string Address;
            public TokenView TokenView;
        }

        [UI.Button("UpdateUniswapRateButton_OnClick")] public bool UpdateUniswapRate;
        public void UpdateUniswapRateButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.GetUniswapRate(callback: rate => Result = rate.ToString())
                .Start();
        }

        public override Token SetupToken()
        {
            base.SetupToken();

            Token.SetUniswapContracts(UniswapPairs);

            return Token;
        }

        public override void MintButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.Mint(Blockchain, Mint.Recipient, Mint.Amount.ToDecimal(), reason: Mint.Reason)
                .Start();
        }

        public override void BurnButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.Burn(Blockchain, Burn.Amount.ToDecimal(), reason: Burn.Reason)
                .Start();
        }
    }
}