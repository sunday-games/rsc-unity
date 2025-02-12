using System;
using System.Collections.Generic;
using UnityEngine;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using InputField = TMPro.TMP_InputField;
using Dropdown = SG.UI.Dropdown;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class AccountWindow : Window
    {
        [Header("Edit")]
        public InputField addressInputField;
        public InputField nameInputField;
        public InputField keyInputField;
        public Dropdown tokenDropdown;

        public Vector2[] positions = new Vector2[4];
        [Space(10)]
        public Button depositButton;
        public Button sendButton;
        public Button deleteButton;
        [Space(10)]
        public Button chooseButton;
        public Vector2 privateKeyWarningPosition;

        private byte _deleteClicked;
        private const byte _deleteClickTimes = 5;

        [Header("Transactions")]
        public GameObject transactionWindow;
        public TransactionView transactionViewPrefab;
        private List<TransactionView> _transactionViews = new List<TransactionView>();

        private Account _account;
        private Token _token;

        protected override void Awake()
        {
            base.Awake();

            transactionViewPrefab.gameObject.SetActive(false);

            TransactionManager.onUpdated += UpdateTransactionList;

            depositButton.onClick.AddListener(
                () => SG.UI.Helpers.OpenLink(_account.Blockchain.network.GetCurrencyUrl));

            tokenDropdown.onShow += items =>
            {
                for (int i = 0; i < tokenDropdown.options.Count; i++)
                {
                    var option = tokenDropdown.options[i];
                    var item = items[i];

                    var parsed = option.text.Split(' ');
                    if (parsed.Length == 1)
                    {
                        var token = BlockchainManager.GetToken(parsed[0]);

                        option.text = item.text.text = "... " + token.Name;
                        tokenDropdown.RefreshShownValue();
                        token.GetBalance(_account.Blockchain, _account.Address,
                                callback: balance =>
                                {
                                    option.text = item.text.text = balance.HasValue ? balance.Value.ToString(token, decimals: 6, sign: false) : "???";
                                    tokenDropdown.RefreshShownValue();

                                    if(tokenDropdown.value == tokenDropdown.options.IndexOf(option))
                                        sendButton.SetInteractable(balance.HasValue && balance > 0);
                                })
                            .Start();
                    }
                }
            };

            tokenDropdown.onValueChanged.AddListener(index =>
            {
                var parsed = tokenDropdown.options[index].text.Split(' ');
                if (parsed.Length == 1)
                {
                    _token = BlockchainManager.GetToken(parsed[0]);

                    tokenDropdown.options[index].text = "... " + _token.Name;
                    tokenDropdown.RefreshShownValue();
                    sendButton.SetInteractable(false);
                    _token.GetBalance(_account.Blockchain, _account.Address,
                            callback: balance =>
                            {
                                tokenDropdown.options[index].text = balance.HasValue ? balance.Value.ToString(_token, decimals: 6, sign: false) : "???";
                                tokenDropdown.RefreshShownValue();
                                sendButton.SetInteractable(balance.HasValue && balance > 0);
                            })
                        .Start();
                }
                else
                {
                    _token = BlockchainManager.GetToken(parsed[parsed.Length - 1]);
                    sendButton.SetInteractable(decimal.Parse(parsed[0]) > 0);
                }
            });

            sendButton.onClick.AddListener(
                () =>
                {
                    var tx = new TransferTransaction(_token, _account.Blockchain, from: _account.Address);
                    wallet.TransactionSignAndSend(tx).Start();
                });

            deleteButton.onClick.AddListener(
                () =>
                    {
                        _deleteClicked++;
                        UpdateDeleteButton();

                        if (_deleteClicked >= _deleteClickTimes)
                        {
                            Accounts.AccountRemove(_account);
                            base.Close(new Dictionary<string, object>
                            {
                                ["account"] = _account
                            });
                        }
                    });

            chooseButton.onClick.AddListener(
                () =>
                {
                    ui.selectedAccount = _account;
                    ui.mode = UI.Mode.None;
                    _previous = null;
                    Close();
                });

            keyInputField.onSelect.AddListener(value =>
            {
                var privateKey = _account.GetPrivateKey();
                if (privateKey.IsEmpty())
                {
                    ui.windows.pinEnter.Open();
                    return;
                }
                keyInputField.text = _account.GetPrivateKey();
            });
        }

        private void OnDestroy()
        {
            TransactionManager.onUpdated -= UpdateTransactionList;
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            if (data.IsValue("account"))
                _account = data["account"] as Account;

            var justCreated = data.GetBool("justCreated", false);
            tokenDropdown.transform.parent.gameObject.SetActive(!justCreated);
            if (justCreated)
            {
                ui.warning.Show("privateKeyBackupWarning".Localize(), privateKeyWarningPosition, "gotIt".Localize());
                _previous = ui.windows.accounts;
            }

            if (ui.mode == UI.Mode.AccountChoose)
            {
                depositButton.gameObject.SetActive(false);
                sendButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);

                chooseButton.gameObject.SetActive(true);
            }
            else
            {
                // TODO depositButton.gameObject.SetActive(true);
                sendButton.gameObject.SetActive(true);
                deleteButton.gameObject.SetActive(true);

                chooseButton.gameObject.SetActive(false);

                _deleteClicked = 0;
                UpdateDeleteButton();
            }

            addressInputField.text = _account.Address;
            nameInputField.text = _account.Name;

            UpdateTransactionList();

            keyInputField.text = string.Empty;

            tokenDropdown.ClearOptions();
            tokenDropdown.options.Add(new Dropdown.OptionData(_account.Blockchain.NativeToken.Name));
            foreach (var tokenView in BlockchainManager.TokenViews)
                if (tokenView is ERC20View && tokenView.Token.Contracts.ContainsKey(_account.Blockchain))
                    tokenDropdown.options.Add(new Dropdown.OptionData(tokenView.name));
            tokenDropdown.value = 0;
            tokenDropdown.onValueChanged.Invoke(tokenDropdown.value);
            tokenDropdown.RefreshShownValue();
        }

        public override void Close(Dictionary<string, object> data = null)
        {
            if (_account.Name != nameInputField.text && !nameInputField.text.IsEmpty())
                Accounts.AccountChangeName(_account, nameInputField.text);

            ui.warning.Hide();

            base.Close();
        }

        private void UpdateDeleteButton()
        {
            deleteButton.buttonText.mainText.text = "delete".Localize();
            if (_deleteClicked > 0)
                deleteButton.buttonText.mainText.text += Const.lineBreak + "<size=50%>" + "clickMoreTimes".Localize(_deleteClickTimes - _deleteClicked);
        }

        private void UpdateTransactionList()
        {
            if (!gameObject.activeSelf)
                return;

            _transactionViews.DestroyAndClear();
            foreach (var tx in TransactionManager.transactions)
                if (tx.isPending && _account.Address.IsEqualIgnoreCase(tx.From))
                {
                    if (tx.progress > 0f)
                        _transactionViews.Add(transactionViewPrefab.Copy().Setup(tx));
                    else
                        _transactionViews.Add(transactionViewPrefab.Copy().Setup(tx, TransactionView_OnClick));

                    void TransactionView_OnClick(Transaction tx)
                    {
                        if (tx.isPending && tx.progress == 0f)
                            BlockchainManager.Instance.StartCoroutine(wallet.TransactionSignAndReplace(tx));
                    }
                }

            transactionWindow.SetActive(_transactionViews.Count > 0);

            window.anchoredPosition = positions[
                _transactionViews.Count < positions.Length - 1 ? _transactionViews.Count : positions.Length - 1];
        }
    }
}