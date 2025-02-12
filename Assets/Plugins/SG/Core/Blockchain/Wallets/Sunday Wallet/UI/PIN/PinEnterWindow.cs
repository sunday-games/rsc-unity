using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class PinEnterWindow : Window
    {
        [Space(10)]
        public InputField pinInputField;

        private bool _isValidPin => pinInputField.text.Length == 4;

        protected override void Awake()
        {
            base.Awake();

            pinInputField.onValueChanged.AddListener(
                text => mainButton.SetInteractable(_isValidPin));

            mainButton.onClick.AddListener(
             () =>
             {
                 if (!Accounts.IsCorrectPIN(pinInputField.text))
                 {
                     ui.windows.error.Open("wrongPin".Localize());
                     return;
                 }

                 Accounts.PIN = pinInputField.text;

                 CloseButton_OnClick();
             });
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            mainButton.SetInteractable(false);

            pinInputField.text = string.Empty;
            pinInputField.Select();
        }

        public override void CloseButton_OnClick()
        {
            if (ui.mode == UI.Mode.EnterPIN)
                ui.mode = UI.Mode.None;

            base.CloseButton_OnClick();
        }
    }
}