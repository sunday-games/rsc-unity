using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class AccountsWindow : Window
    {
        [Space(10)]
        public Button createButton;
        public Button importButton;
        public Button settingsButton;
        public Dropdown blockchainDropdown;
        public Text networkText;
        [Space(10)]
        public AccountView accountViewPrefab;
        private List<AccountView> accountViews = new List<AccountView>();

        protected override void Awake()
        {
            base.Awake();

            accountViewPrefab.gameObject.SetActive(false);

            blockchainDropdown.ClearOptions();
            foreach (var blockchain in wallet.supportedBlockchains)
                if (blockchain)
                    blockchainDropdown.options.Add(new Dropdown.OptionData(blockchain.ToString()));

            createButton.onClick.AddListener(
                () => ui.windows.accountCreate.Open("defaultName", "Account " + Accounts.List.Count));

            importButton.onClick.AddListener(
                () => ui.windows.accountImport.Open("defaultName", "Account " + Accounts.List.Count));

            settingsButton.onClick.AddListener(ui.windows.settings.Open);

            blockchainDropdown.onValueChanged.AddListener(
                value => SetBlockchain(wallet.supportedBlockchains[value]));
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            UpdateAccountList();

            blockchainDropdown.interactable = ui.mode == UI.Mode.None && blockchainDropdown.options.Count > 1;
            blockchainDropdown.value = wallet.supportedBlockchains.IndexOf(Blockchain.current);
            blockchainDropdown.RefreshShownValue();
            ui.UpdateStyle(blockchainDropdown);

            networkText.text = Configurator.production ? "MAINNET" : "TESTNET";

            if (ui.mode == UI.Mode.AccountChoose)
                titleText.text = "choiceAccount".Localize().ToUpper();
            else
                titleText.text = "Sunday Wallet";
        }

        public override void CloseButton_OnClick()
        {
            if (ui.mode == UI.Mode.AccountChoose)
                ui.mode = UI.Mode.None;

            base.CloseButton_OnClick();
        }

       private void SetBlockchain(Blockchain blockchain)
        {
            Blockchain.current = blockchain;

            blockchain.SetWallet(wallet);

            UpdateAccountList();
        }

        private void UpdateAccountList()
        {
            accountViews.DestroyAndClear();
            foreach (var account in Accounts.List)
                if (account.Blockchain == Blockchain.current)
                    accountViews.Add(accountViewPrefab.Copy().Setup(
                        account,
                        highlight: Blockchain.current.loginedAccount == account,
                        onClick: AccountView_OnClick));

            void AccountView_OnClick(Account account)
            {
                if (ui.mode == UI.Mode.AccountChoose)
                {
                    ui.selectedAccount = account;
                    CloseButton_OnClick();
                }
                else
                    ui.windows.account.Open("account", account);
            }
        }
    }
}