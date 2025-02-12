using System;
using System.Collections.Generic;
using UnityEngine;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class AccountImportWindow : Window
    {
        [Space(10)]
        public InputField nameInputField;
        public InputField keyInputField;

        string defaultName;

        protected override void Awake()
        {
            base.Awake();

            mainButton.SetCallback(
                () =>
                {
                    var name = !nameInputField.text.IsEmpty() ? nameInputField.text : defaultName;

                    var account = Accounts.AccountCreate(Blockchain.current, name, keyInputField.text);
                    if (!account)
                        return;

                    if (!Accounts.AccountAdd(account))
                        ui.windows.error.Open("errorDuplicateAccount".Localize(account.Address));

                    if (ui.mode == UI.Mode.AccountChoose)
                    {
                        ui.selectedAccount = account;
                        ui.mode = UI.Mode.None;
                        _previous = null;
                    }

                    Close();
                });

            nameInputField.onValueChanged.AddListener(
                value => MainButton_SetInteractable());

            keyInputField.onValueChanged.AddListener(
                value => MainButton_SetInteractable());
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            if (data.IsValue("defaultName"))
                defaultName = data["defaultName"] as string;

            nameInputField.text = defaultName;

            keyInputField.text = string.Empty;

            MainButton_SetInteractable();
        }

        private void MainButton_SetInteractable()
        {
            mainButton.SetInteractable(
               !nameInputField.text.IsEmpty() &&
               Blockchain.current.IsValidPrivateKey(keyInputField.text));

        }
    }
}