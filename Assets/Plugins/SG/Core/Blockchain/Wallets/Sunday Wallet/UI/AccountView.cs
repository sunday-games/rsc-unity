using System;
using System.Collections;
using UnityEngine;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using Image = UnityEngine.UI.Image;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class AccountView : MonoBehaviour
    {
        public Button mainButton;
        public Button openUrlButton;
        public Button refreshButton;
        public Image iconImage;
        public Image highlightImage;
        public Text titleText;
        public Text nameText;
        public Text addressText;
        public Text balanceText;

        private Account _account;

        public AccountView Setup(Account account, bool highlight = false, Action<Account> onClick = null)
        {
            _account = account;

            nameText.text = account.Name;
            addressText.text = account.AddressShort;

            SetHighlight(highlight);

            if (mainButton)
                mainButton.onClick.AddListener(() => onClick?.Invoke(account));

            if (openUrlButton)
                openUrlButton.onClick.AddListener(account.OpenUrl);

            if (refreshButton)
                refreshButton.onClick.AddListener(
                    () =>
                    {
                        if (balanceText == null)
                            return;

                        balanceText.text = "...";

                        StartCoroutine(RefreshCoroutine());
                        IEnumerator RefreshCoroutine()
                        {
                            const float updateRate = 10f;

                            refreshButton.gameObject.SetActive(false);

                            Result<decimal> refreshResult = null;
                            yield return account.Update(updateRate, r => refreshResult = r);

                            balanceText.text = account.Balance.ToString(account.Blockchain.NativeToken, 3);

                            if (refreshResult.Success)
                                yield return new WaitForSeconds(updateRate);

                            refreshButton.gameObject.SetActive(true);
                        }
                    });

            if (iconImage)
                iconImage.sprite = Configurator.Instance.GetIconWhite(account.Blockchain.ToString());

            if (balanceText)
                balanceText.text = account.Balance.ToString(account.Blockchain.NativeToken, 3);

            return this;
        }

        public void CopyAddress() => ClipboardPlugin.Clipboard.Copy(_account.Address);

        public void SetHighlight(bool highlight)
        {
            highlightImage?.gameObject.SetActive(highlight);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
