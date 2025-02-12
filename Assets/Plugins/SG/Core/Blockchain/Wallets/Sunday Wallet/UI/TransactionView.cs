using System;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class TransactionView : MonoBehaviour
    {
        public Text nameText;
        public Text hashText;
        public Text dateText;
        public Button mainButton;
        public Button openUrlButton;
        public Image bgImage;
        public Image progressImage;

       private Transaction _tx;

        public TransactionView Setup(Transaction tx, Action<Transaction> onClick = null)
        {
            _tx = tx;

            nameText.text = tx.FunctionToString();
            if (nameText.text.Length > 50) nameText.text = nameText.text.Substring(0, 45) + "...";

            hashText.text = tx.hashShort;

            dateText.text = tx.Date.ToLocalTime().ToString(Const.dateTimeFormat_DMYHM);
            if (tx.nonce.HasValue) dateText.text = "#" + tx.nonce + " - " + dateText.text;

            if (mainButton)
            {
                mainButton.SetInteractable(onClick != null);

                if (mainButton.interactable)
                    mainButton.onClick.AddListener(() => onClick(tx));
            }

            if (openUrlButton)
                openUrlButton.onClick.AddListener(_tx.OpenUrl);

            SetProgress(tx.progress);

            return this;
        }

        public void CopyHash()
        {
            ClipboardPlugin.Clipboard.Copy(_tx.Hash);
        }

        const float maxProgress = 0.95f;
        void SetProgress(float progress)
        {
            if (!progressImage) return;

            progressImage.gameObject.SetActive(progress > 0f);

            if (progressImage.gameObject.activeSelf)
            {
                if (progress > maxProgress) progress = maxProgress;
                progressImage.rectTransform.sizeDelta = new Vector2(bgImage.rectTransform.sizeDelta.x * progress, bgImage.rectTransform.sizeDelta.y);
            }
        }
    }
}
