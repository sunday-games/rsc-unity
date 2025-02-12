using UnityEngine;
using System.Collections.Generic;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class ErrorWindow : Window
    {
        public static List<string> ignoredErrors = new List<string>
        {
            Wallet.Errors.OperationCanceled,
           // Wallet.Errors.TooManyRequests,
        };

        [Space(10)]
        public Text mainText;

        public void Open(string error)
        {
            foreach (var ignoredError in ignoredErrors)
                if (error.Contains(ignoredError))
                    return;

            if (!gameObject.activeSelf)
                base.Open();

            if (Localization.IsKey(error))
                mainText.text = error.Localize();
            else
                mainText.text = error;
        }
    }
}