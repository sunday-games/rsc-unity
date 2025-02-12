using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class AccountCreateWindow : Window
    {
        [Space(10)]
        public InputField nameInputField;

        string defaultName;

        protected override void Awake()
        {
            base.Awake();

            mainButton.SetCallback(
                () =>
                {
                    var name = !nameInputField.text.IsEmpty() ? nameInputField.text : defaultName;

                    var account = Accounts.AccountCreate(Blockchain.current, name);
                    if (account)
                    {
                        if (!Accounts.AccountAdd(account))
                            ui.windows.error.Open("errorDuplicateAccount".Localize(account.Address));

                        ui.windows.account.Open(new Dictionary<string, object> { ["account"] = account, ["justCreated"] = true });
                    }
                });

            nameInputField.onValueChanged.AddListener(
                value => MainButton_SetInteractable());
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            if (data.IsValue("defaultName"))
                defaultName = data["defaultName"] as string;

            nameInputField.text = defaultName;
        }

        private void MainButton_SetInteractable()
        {
            mainButton.SetInteractable(!nameInputField.text.IsEmpty());
        }
    }
}