using System;
using UnityEngine;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class Warning : MonoBehaviour
    {
        public RectTransform window;
        public Text mainText;
        public Button mainButton;

        public void Show(string mainText, Vector2 position, string buttonText = null, Action callback = null)
        {
            window.anchoredPosition = position;
            this.mainText.text = mainText;

            if (mainButton.ActivateIf(!buttonText.IsEmpty()))
            {
                mainButton.buttonText.mainText.text = buttonText;
                mainButton.SetCallback(() =>
                {
                    Hide();
                    callback?.Invoke();
                });
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}