using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class PinSetWindow : Window
    {
        [Space(10)]
        public InputField pin1InputField;
        public InputField pin2InputField;

        private bool _isValidPin =>
            pin1InputField.text.Length == 4 &&
            pin1InputField.text == pin2InputField.text;

        protected override void Awake()
        {
            base.Awake();

            pin1InputField.onValueChanged.AddListener(
                text => mainButton.SetInteractable(_isValidPin));

            pin2InputField.onValueChanged.AddListener(
                text => mainButton.SetInteractable(_isValidPin));

            mainButton.onClick.AddListener(
             () =>
             {
                 if (_isValidPin)
                 {
                     Accounts.SetPIN(pin1InputField.text);

                     CloseButton_OnClick();
                 }
             });
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            mainButton.SetInteractable(false);

            pin1InputField.text = pin2InputField.text = string.Empty;
            pin1InputField.Select();
        }

        public override void OnTabKey()
        {
            if(SG.UI.UI.selected == pin1InputField.gameObject)
                pin2InputField.Select();
            if (SG.UI.UI.selected == pin2InputField.gameObject)
                pin1InputField.Select();
        }
    }
}