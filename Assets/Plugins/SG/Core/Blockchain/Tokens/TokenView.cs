using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    public class TokenView : ScriptableObject
    {
        public static List<(string address, decimal amount)> ParseRecipientAmount(string csv)
        {
            var recipients = new List<(string address, decimal amount)>();
            foreach (var line in csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.Contains(","))
                    return null;

                var pair = line.Split(',');
                if (pair.Length != 2)
                    return null;

                var amount = pair[1].ToDecimal();
                if (amount == default)
                    return null;

                recipients.Add((pair[0], amount));
            }
            return recipients;
        }

        public static List<string> ParseRecipient(string csv)
        {
            var recipients = new List<string>();
            foreach (var line in csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                if (Blockchain.current.IsValidAddress(line))
                    recipients.Add(line);
            return recipients;
        }

        public Token.Standarts Standart;
        public int Decimals;
        public double TotalSupplyLimit;
        public double UsdRate;
        public string Sign;
        public string NameMin;
        [Space]
        public string OwnerAddress;
        public string OwnerAddressDebug;
        [Space]
        public string AuthorityAddress;
        public string AuthorityAddressDebug;
        [Space]
        [SerializeField] private BlockchainAddressDictionary _addresses, _addressesDebug;
        public BlockchainAddressDictionary Addresses => Configurator.production ? _addresses : _addressesDebug;

        [Space(20)]
        public Blockchain.Names BlockchainName;
        protected Blockchain Blockchain => BlockchainName.ToBlockchain();

        public Token Token;
        public virtual Token SetupToken()
        {
            Token = new Token(name, Standart, Decimals, Sign)
            {
                OwnerAddress = Configurator.production ? OwnerAddress : OwnerAddressDebug,
                AuthorityAddress = Configurator.production ? AuthorityAddress : AuthorityAddressDebug,
                TotalSupplyLimit = TotalSupplyLimit.ToDecimal(),
                UsdRate = UsdRate.ToDecimal(),
                NameMin = NameMin,
            };

#if SG_BLOCKCHAIN
            Token.SetContracts(Addresses);
#endif

            return Token;
        }

        [Space(10)]
        public float Time = 1f;
        public int GasPriceGWEI = 5;

        [Space]
        [TextArea(10, 100)] public string Result;

        [Space(20)]
        [UI.Button("OpenExplorerButton_OnClick")] public bool OpenExplorer;
        public virtual void OpenExplorerButton_OnClick() =>
            UI.Helpers.OpenLink(Blockchain.network.explorer.GetAddressURL(Token.GetAddress(Blockchain)));

        public BalanceData Balance;
        [Serializable]
        public class BalanceData
        {
            public int TokenId;
            [Tooltip("address1,<enter>address2<enter>address3...")] [TextArea(10, 100)] public string Recipients;

            [UI.Button("GetBalanceButton_OnClick")] public bool GetBalance;
        }
        public virtual void GetBalanceButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Please login first");
                return;
            }

            var recipients = ParseRecipient(Balance.Recipients);

            Routine().Start();
            IEnumerator Routine()
            {
                Result = string.Empty;
                for (int i = 0; i < recipients.Count; i++)
                {
                    yield return Token.GetBalance(Blockchain, recipients[i], callback: result =>
                    {
                        Log.Blockchain.Info($"Balance {i + 1} / {recipients.Count}. {recipients[i]}: {result}");

                        if (Result.IsNotEmpty())
                            Result += Const.lineBreak;

                        Result += recipients[i] + "," + result;
                    });
                }
            }
        }

        [Space(20)]
        [UI.Button("GetTotalSupplyButton_OnClick")] public bool GetTotalSupply;
        public virtual void GetTotalSupplyButton_OnClick()
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

        public MintData Mint;
        [Serializable]
        public class MintData
        {
            public int TokenId;
            public string Recipient;
            public string Amount;
            [Tooltip("Ignored for ERC1155")] public string Reason;

            [UI.Button("MintButton_OnClick")] public bool Mint;
        }
        public virtual void MintButton_OnClick() { }

        public BurnData Burn;
        [Serializable]
        public class BurnData
        {
            public int TokenId;
            public string Amount;
            [Tooltip("Ignored for ERC1155")] public string Reason;

            [UI.Button("BurnButton_OnClick")] public bool Burn;
        }
        public virtual void BurnButton_OnClick() { }

        public TransferData Transfer;
        [Serializable]
        public class TransferData
        {
            public int TokenId;
            [Tooltip("address1,amount1<enter>address2,amount2<enter>address3...")] [TextArea(10, 100)] public string Recipients;
            [ReadOnly] public string Preview;

            [Header("WARNING: Auto Confirm")]
            [UI.Button("TransferButton_OnClick")] public bool Transfer;
        }
        public virtual void TransferButton_OnClick()
        {
            if (Transfer.Preview == "ERROR")
                return;

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
                    if (!Blockchain.IsValidAddress(recipients[i].address))
                    {
                        Log.Blockchain.Error($"{recipients[i].address} is not valid {Blockchain.name} address");
                        continue;
                    }

                    yield return Token.Transfer_AutoConfirm(Blockchain, recipients[i].address, recipients[i].amount, GasPriceGWEI);

                    Log.Blockchain.Info($"Transfer {i + 1} / {recipients.Count}");

                    yield return new WaitForSeconds(Time);
                }
            }
        }

        public ApproveData Approve;
        [Serializable]
        public class ApproveData
        {
            public string Operator;
            public string Amount;

            [UI.Button("ApproveButton_OnClick")] public bool Approve;
            [UI.Button("IsApprovedButton_OnClick")] public bool IsApproved;
            [UI.Button("RevokeApprovalButton_OnClick")] public bool RevokeApproval;
        }
        public virtual void ApproveButton_OnClick() =>
            Token.Approve(Blockchain, Approve.Operator, Approve.Amount.ToDecimal()).Start();

        public virtual void IsApprovedButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Please login first");
                return;
            }

            Token.GetIsApproved(Blockchain, Blockchain.loginedAccount.Address, Approve.Operator, Approve.Amount.ToDecimal(),
                approved => Result = $"{Approve.Operator} {(approved ? "have" : "DONT have")} approval for {Approve.Amount} {Token.Name}")
            .Start();
        }

        public virtual void RevokeApprovalButton_OnClick() =>
            Token.Approve(Blockchain, Approve.Operator, 0)
            .Start();

        private void OnValidate()
        {
            if (Transfer.Recipients.IsNotEmpty())
            {
                var recipients = ParseRecipientAmount(Transfer.Recipients);
                Transfer.Preview = recipients == null ?
                    "ERROR" :
                    $"{recipients.Count} recipients, {recipients.Sum(p => p.amount)} tokens";
            }
        }
    }
}