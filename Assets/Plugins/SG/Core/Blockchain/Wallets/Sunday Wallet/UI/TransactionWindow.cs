using System;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using SG.UI;
using Text = TMPro.TextMeshProUGUI;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class TransactionWindow : Window
    {
        public Text functionText;

        [Header("Accounts")]
        public AccountView fromAccountView;
        private Account from;
        public AccountView toAccountView;
        private Account to;

        [Header("Recipient")]
        public InputField recipientInputField;
        private string _recipient;
        private bool _recipientNeeded;

        [Header("Asset")]
        public InputField assetAmountInputField;
        public Button assetAmountMaxButton;
        public Text assetAmountUSDText;
        public Text assetCodeText;
        private Token _asset;
        private decimal _assetAmount;
        private decimal _assetBalance;

        [Header("Fee")]
        public InputField feeInputField;
        public Text feeUSDText;
        public Text feeCurrencyCodeText;
        private decimal _fee => NativeToken.FromMin(_gasLimit * FeeToken.ToMin(_feePrice));

        [Header("Fee")]
        public GameObject FeeAdvanced;
        public ToggleGroup feePriceToggleGroup;
        public Text[] feePriceValueTexts;

        [Header("GAS Limit")]
        public InputField gasLimitInputField;
        public Text gasLimitCurrencyCodeText;
        public Text gasLimitEstimationText;
        private BigInteger _gasLimit;

        [Header("Fee Price")]
        public InputField feePriceInputField;
        public Text feePriceCurrencyCodeText;
        private decimal _feePrice;
        public Text feePriceBaseText;

        [Header("Warnings")]
        public GameObject insufficientFunds;
        public Button depositButton;
        [Space]
        public GameObject feeWarning;
        public Text failWarning;

        private Transaction tx;
        private Token NativeToken => tx.Blockchain.NativeToken;
        private Token FeeToken => tx.Blockchain.FeeToken;
        private bool _replacement;

        protected override void Awake()
        {
            base.Awake();

            recipientInputField.onValueChanged.AddListener(
                value =>
                {
                    _recipient = value;

                    if (tx.FunctionIsNativeTokenTransfer)
                    {
                        to.Address = value;
                        toAccountView.addressText.text = to.AddressShort;
                    }

                    MainButtonSetInteractable();
                });

            void SetAssetAmount(string text)
            {
                var value = text.IsNotEmpty() ? text.ToDecimal(_assetAmount) : default;

                if (value < 0)
                {
                    value = 0;
                }
                else
                {
                    if (_asset == null || tx.FunctionIsNativeTokenTransfer)
                    {
                        if (value > from.Balance - _fee)
                            value = from.Balance - _fee;
                    }
                    else
                    {
                        if (value > _assetBalance)
                            value = _assetBalance;
                    }
                }

                _assetAmount = value;
                AmountUpdate();
            }
            assetAmountInputField.onValueChanged.AddListener(SetAssetAmount);
            assetAmountInputField.onEndEdit.AddListener(
                text =>
                {
                    SetAssetAmount(text);
                    assetAmountInputField.SetTextWithoutNotify(_assetAmount.ToStringFormated());
                });

            void SetFeePrice(string text)
            {
                var value = text.IsNotEmpty() ? text.ToDecimal() : default;

                //if (tx.Blockchain.feeBase > 0 && value < tx.Blockchain.feeBase)
                //    value = tx.Blockchain.feeBase;
                if (value < 0)
                    value = 0;

                _feePrice = value;
                AmountUpdate();

                feeWarning.SetActive(_feePrice > tx.Blockchain.feePriceWarning && !insufficientFunds.activeSelf);
            }
            feePriceInputField.onValueChanged.AddListener(SetFeePrice);
            feePriceInputField.onEndEdit.AddListener(
                text =>
                {
                    SetFeePrice(text);
                    feePriceInputField.SetTextWithoutNotify(_feePrice.ToStringFormated());
                });

            feePriceToggleGroup.OnValueChanged +=
                index =>
                {
                    if (index < feePriceToggleGroup.Toggles.Count - 1)
                    {
                        FeeAdvanced.SetActive(false);
                        feePriceInputField.onEndEdit.Invoke(feePriceValueTexts[index].text.Split(' ')[0]);
                    }
                    else
                    {
                        FeeAdvanced.SetActive(true);
                    }
                };

            void SetGasLimit(string text)
            {
                var value = text.IsNotEmpty() ? text.ToBigInteger() : default;

                if (value < tx.gasEstimation)
                    value = tx.gasEstimation;
                else if (value > tx.Blockchain.gasMax)
                    value = tx.Blockchain.gasMax;

                _gasLimit = value;
                AmountUpdate();
            }
            gasLimitInputField.onValueChanged.AddListener(SetGasLimit);
            gasLimitInputField.onEndEdit.AddListener(
                text =>
                {
                    SetGasLimit(text);
                    gasLimitInputField.SetTextWithoutNotify(_gasLimit.ToString());
                });

            assetAmountMaxButton.onClick.AddListener(
                () =>
                {
                    var amount = _asset == tx.FunctionIsNativeTokenTransfer ?
                        (from.Balance > _fee ? from.Balance - _fee : 0) :
                        _assetBalance;

                    assetAmountInputField.onEndEdit.Invoke(amount.ToStringFormated());
                });


            depositButton.onClick.AddListener(
                () => Helpers.OpenLink(tx.Blockchain.network.GetCurrencyUrl));

            mainButton.onClick.AddListener(
                () =>
                {
                    ui.transactionOutput = new Dictionary<string, object>
                    {
                        ["feeMax"] = _feePrice,
                        ["feeMaxPriority"] = _feePrice > tx.Blockchain.feeBase ? _feePrice - tx.Blockchain.feeBase : 0,
                        ["gasLimit"] = _gasLimit,
                        ["recipient"] = _recipient,
                        ["assetAmount"] = _assetAmount,
                    };

                    CloseButton_OnClick();
                });
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            if (data.IsValue("tx"))
                tx = data["tx"] as Transaction;

            _recipient = string.Empty;
            _recipientNeeded = tx.FunctionIsNativeTokenTransfer && tx.To.IsEmpty();

            titleText.text = "transaction".Localize();
            if (tx.nonce.HasValue)
                titleText.text += Utils.TagSup("#" + tx.nonce);

            from = Accounts.List.Find(tx.Blockchain, tx.From);
            fromAccountView.titleText.text = "yourAccount".Localize();
            fromAccountView.Setup(from);

            if (tx.FunctionIsNativeTokenTransfer)
            {
                to = new Account(tx.Blockchain, tx.To, null, null, "recipient".Localize());
                toAccountView.titleText.text = "recipientAccount".Localize();
            }
            else
            {
                to = new Account(tx.Blockchain, tx.To, null, null, tx.Function ? tx.Function.Contract.Name : "unknown");
                toAccountView.titleText.text = "smartContract".Localize();
            }
            toAccountView.Setup(to);

            if (data.TryGet("token", out _asset))
            {
                _assetAmount = tx.FunctionIsNativeTokenTransfer ?
                    tx.NativeTokenAmount :
                    _asset.FromMin(tx.Content["amount"].ToBigInteger());

                _assetBalance = data.GetDecimal("tokenBalance");
                assetAmountInputField.readOnly = _assetAmount > 0;
                assetCodeText.text = _asset.Name;

                if (tx.Content.TryGetString("recipient", out string recipient) &&
                    recipient == TransferTransaction.PLACEHOLDER_ADDRESS)
                    _recipientNeeded = true;
            }
            else
            {
                _assetAmount = tx.NativeTokenAmount;

                assetAmountInputField.readOnly = true;
                assetCodeText.text = NativeToken.Name;
            }
            if (assetAmountMaxButton.ActivateIf(!assetAmountInputField.readOnly))
                assetAmountMaxButton.SetText(_assetBalance.ToStringFormated3() + " MAX");
            assetAmountInputField.onEndEdit.Invoke(_assetAmount.ToStringFormated());
            ui.UpdateStyle(assetAmountInputField);
            ui.UpdateStyle(assetCodeText, !assetAmountInputField.readOnly);

            feeCurrencyCodeText.text = NativeToken.Name;

            if (recipientInputField.ActivateIf(_recipientNeeded))
                recipientInputField.text = string.Empty;

            gasLimitEstimationText.text = tx.gasEstimation.ToString(tx.Blockchain.GasToken);
            gasLimitInputField.onEndEdit.Invoke((tx.gasLimit > 0 ?
                tx.gasLimit :
                (int) ((decimal) tx.gasEstimation * tx.Blockchain.gasMultiplier)).ToString());
            ui.UpdateStyle(gasLimitInputField);
            gasLimitCurrencyCodeText.text = tx.Blockchain.GasToken.Name;
            ui.UpdateStyle(gasLimitCurrencyCodeText, !gasLimitInputField.readOnly);

            _replacement = data.GetBool("replacement", false);

            mainButton.buttonText.mainText.text = (_replacement ? "replace" : "confirm").Localize();

            if (failWarning.transform.parent.parent.ActivateIf(tx.gasEstimation == default))
                failWarning.text = "txWillFailWarning".Localize(tx.error);

            SetupFees();

            functionText.text = _asset != null ? _asset.Name + " Transfer" : tx.FunctionToString();
        }

        private void SetupFees()
        {
            var feePriority = tx.Blockchain.feePrioritySuggestion;
            var feeBase = tx.Blockchain.feeBase;

            void SetOption(int index, decimal feePriority = -1)
            {
                if (feePriceToggleGroup.Toggles[index].ActivateIf(feePriority >= 0))
                    feePriceValueTexts[index].text = (feeBase + feePriority).ToString(FeeToken);
            }

            if (feePriority == null || feePriority.Count == 0)
            {
                SetOption(0);
                SetOption(1);
                SetOption(2);
            }
            else if (feePriority.Count == 1)
            {
                SetOption(0);
                SetOption(1, feePriority[0]);
                SetOption(2);
            }
            else if (feePriority.Count == 2)
            {
                SetOption(0);
                SetOption(1, feePriority[0]);
                SetOption(2, feePriority[1]);
            }
            else if (feePriority.Count == 3)
            {
                SetOption(0, feePriority[0]);
                SetOption(1, feePriority[1]);
                SetOption(2, feePriority[2]);
            }

            feePriceCurrencyCodeText.text = tx.Blockchain.FeeToken.Name;
            if (feePriority == null || feePriority.Count == 0 || tx.feeMax > 0)
            {
                feePriceToggleGroup.Toggles[feePriceToggleGroup.Toggles.Count - 1].SetValue(true, invokeEvent: false);

                feePriceInputField.onValueChanged.Invoke(tx.feeMax.ToString(FeeToken, 1));
            }
            else
            {
                foreach (var toggle in feePriceToggleGroup.Toggles)
                    if (toggle.gameObject.activeSelf)
                    {
                        toggle.SetValue(true, invokeEvent: true);
                        break;
                    }
            }

            feePriceBaseText.text = feeBase.ToString(FeeToken, 1);
        }

        public override void CloseButton_OnClick()
        {
            if (ui.mode == UI.Mode.TransactionConfirm)
                ui.mode = UI.Mode.None;

            base.CloseButton_OnClick();
        }

        private void AmountUpdate()
        {
            {
                decimal moneyUSD = default,
                    feeUSD = default;

                if (ExchangeRates.isRatesUpdated)
                {
                    moneyUSD = ExchangeRates.Convert(_assetAmount, _asset ?? NativeToken, Currency.USD);
                    feeUSD = ExchangeRates.Convert(_fee, NativeToken, Currency.USD);
                }
                else if (NativeToken.UsdRate != default)
                {
                    moneyUSD = _assetAmount * (_asset ?? NativeToken).UsdRate;
                    feeUSD = _fee * NativeToken.UsdRate;
                }

                if (assetAmountUSDText.ActivateIf(moneyUSD > 0))
                    assetAmountUSDText.text = moneyUSD.ToString(Currency.USD);

                if (feeUSDText.ActivateIf(feeUSD > 0))
                    feeUSDText.text = feeUSD.ToString(Currency.USD);
            }

            feeInputField.text = _fee.ToStringFormated();

            MainButtonSetInteractable();
        }

        private void MainButtonSetInteractable()
        {
            mainButton.SetInteractable(false);

            if (insufficientFunds.ActivateIf(
                _fee > from.Balance ||
                (_asset != null && !tx.FunctionIsNativeTokenTransfer && _assetAmount > _assetBalance) ||
                ((_asset == null || tx.FunctionIsNativeTokenTransfer) && _assetAmount + _fee > from.Balance)))
            {
                Log.Blockchain.Warning("Transaction is not ready: Insufficient funds");
                feeWarning.SetActive(false);
                return;
            }

            if (!tx.Blockchain.IsValidAddress(to.Address))
            {
                Log.Blockchain.Warning("Transaction is not ready: to.address invalid");
                return;
            }

            if (_recipientNeeded && !tx.Blockchain.IsValidAddress(_recipient))
            {
                Log.Blockchain.Warning("Transaction is not ready: recipient invalid");
                return;
            }

            if (from.Address.IsEqualIgnoreCase(to.Address))
            {
                Log.Blockchain.Warning("Transaction is not ready: from.address and to.address cannot be equal");
                return;
            }

            if (_asset != null && _assetAmount == default)
            {
                Log.Blockchain.Warning("Transaction is not ready: Amount must be greater than zero");
                return;
            }

            if (_replacement && _feePrice == tx.feeMax)
            {
                Log.Blockchain.Warning("Replacement Transaction is not ready: FeePrice must be increased");
                return;
            }

            if (_gasLimit == 0)
            {
                Log.Blockchain.Warning("Transaction will fail");
                return;
            }

            mainButton.SetInteractable(true);
        }
    }
}