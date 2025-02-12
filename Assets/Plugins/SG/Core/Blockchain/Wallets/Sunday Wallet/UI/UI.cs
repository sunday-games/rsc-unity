using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SG.UI;
using Image = UnityEngine.UI.Image;
using ScrollRect = UnityEngine.UI.ScrollRect;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class UI : MonoBehaviour
    {
        public static UI Setup()
        {
            var instance = FindObjectOfType<UI>();
            if (instance == null)
            {
                const string canvasName = "SundayWallet Canvas";
                var prefab = Resources.Load<UI>(canvasName);
                if (prefab == null)
                {
                    Log.Error($"Fail to find UI prefab with name '{canvasName}' in Resources folder");
                    return null;
                }

                instance = Instantiate(prefab);
                instance.name = prefab.name;
#if SG_BLOCKCHAIN
                SundayWallet.UI = Window.ui = instance;
                Window.wallet = Wallet.GetWallet<SundayWallet>();
#endif
            }

            return instance;
        }

        public Loading loader;
        public Warning warning;
        public Windows windows;
        [Serializable]
        public class Windows
        {
            public AccountsWindow accounts;
            public AccountWindow account;
            public AccountCreateWindow accountCreate;
            public AccountImportWindow accountImport;

            public PinSetWindow pinSet;
            public PinEnterWindow pinEnter;

            public ErrorWindow error;
            public TransactionWindow transaction;
            public SettingsWindow settings;
        }

        [Header("Style")]
        public Color interactable;
        public Color nonInteractable;
        public Color nonInteractableText;
        public Color nonInteractableHalfText;
        public Button[] buttons;
        public Toggle[] toggles;
        public ScrollRect[] scrollRects;
        public Image[] interactableImages;
        public Image[] nonInteractableImages;
        public TMP_InputField[] inputFields;
        public TextMeshProUGUI[] nonInteractableHalfTexts;

        public bool autoUpdate;
        void OnValidate() { if (autoUpdate) UpdateStyleAll(); }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        [Button("UpdateStyleAll")] public bool updateStyleAll;
        public void UpdateStyleAll()
        {
            foreach (var item in buttons)
                UpdateStyle(item);

            foreach (var item in toggles)
                UpdateStyle(item);

            foreach (var item in inputFields)
                UpdateStyle(item);

            foreach (var item in scrollRects)
                UpdateStyle(item);

            foreach (var item in interactableImages)
                UpdateStyle(item, true);

            foreach (var item in nonInteractableImages)
                UpdateStyle(item, false);

            foreach (var item in nonInteractableHalfTexts)
                if (item)
                    item.color = nonInteractableHalfText;
        }

        public void UpdateStyle(Button button)
        {
            if (!button)
                return;

            var interactableHalf = interactable.SetAlpha(0.5f);

            if (button.buttonImage.mainImage)
                button.buttonImage.mainImage.color = button.buttonImage.changeColor ? interactableHalf : interactable;

            //if (button.buttonImage.changeColor)
            //{
            //    button.buttonImage.normalColor = interactableHalf;
            //    button.buttonImage.highlightedColor = interactable;
            //}

            if (button.iconImage.mainImage)
                button.iconImage.mainImage.color = button.iconImage.changeColor ? interactableHalf : interactable;

            //if (button.iconImage.changeColor)
            //{
            //    button.iconImage.normalColor = interactableHalf;
            //    button.iconImage.highlightedColor = interactable;
            //}

            //if (button.buttonText.mainText && button.buttonText.changeColor)
            //{
            //    button.buttonText.mainText.color = interactableHalf;
            //    button.buttonText.normalColor = interactableHalf;
            //    button.buttonText.highlightedColor = interactable;
            //}
        }

        public void UpdateStyle(Toggle toggle)
        {
            if (!toggle)
                return;

            var interactableHalf = interactable.SetAlpha(0.5f);

            if (toggle.highlightImage.image)
                toggle.highlightImage.image.color = toggle.highlightImage.changeColor ? interactableHalf : interactable;

            if (toggle.highlightImage.changeColor)
            {
                toggle.highlightImage.normalColor = interactableHalf;
                toggle.highlightImage.highlightedColor = interactable;
                toggle.toggleImage.onColor = interactable;
                toggle.toggleImage.offColor = interactableHalf;
            }

            if (toggle.buttonText.mainText)
                toggle.buttonText.mainText.color = interactableHalf;

            if (toggle.buttonText.changeColor)
            {
                toggle.buttonText.normalColor = interactableHalf;
                toggle.buttonText.highlightedColor = interactable;
            }
        }

        public void UpdateStyle(TMP_Dropdown inputField)
        {
            if (!inputField)
                return;

            var image = inputField.GetComponent<Image>();
            if (image)
                image.color = inputField.interactable ? interactable : nonInteractable;

            foreach (Transform child in inputField.transform)
                if (child.name == "Right")
                {
                    image = child.GetComponent<Image>();
                    if (image)
                        image.color = inputField.interactable ? interactable : nonInteractable;
                }
        }

        public void UpdateStyle(TMP_InputField inputField)
        {
            if (!inputField)
                return;

            var interactableHalf = interactable.SetAlpha(0.5f);

            if (inputField.transition == UnityEngine.UI.Selectable.Transition.ColorTint)
                inputField.colors = new UnityEngine.UI.ColorBlock()
                {
                    colorMultiplier = 1f,
                    fadeDuration = 0.1f,
                    normalColor = !inputField.readOnly ? interactableHalf : nonInteractable,
                    pressedColor = !inputField.readOnly ? interactableHalf : nonInteractable,
                    disabledColor = !inputField.readOnly ? interactableHalf : nonInteractable,
                    highlightedColor = !inputField.readOnly ? interactable : nonInteractable,
                    selectedColor = !inputField.readOnly ? interactable : nonInteractable,
                };

            if (inputField.targetGraphic && inputField.targetGraphic is Image)
                (inputField.targetGraphic as Image).color = !inputField.readOnly ? interactable : nonInteractable;

            if (inputField.textComponent)
                inputField.textComponent.color = !inputField.readOnly ? interactable : nonInteractableText;

            if (inputField.placeholder && inputField.placeholder is TextMeshProUGUI)
                (inputField.placeholder as TextMeshProUGUI).color = nonInteractableHalfText;

            inputField.selectionColor = interactableHalf;
        }

        public void UpdateStyle(ScrollRect scrollRect)
        {
            if (!scrollRect)
                return;

            var interactableHalf = interactable.SetAlpha(0.5f);

            if (scrollRect.verticalScrollbar)
                scrollRect.verticalScrollbar.handleRect.GetComponent<Image>().color = interactableHalf;
        }

        public void UpdateStyle(TextMeshProUGUI text, bool isInteractable)
        {
            if (!text)
                return;

            text.color = isInteractable ? interactable : nonInteractableText;
        }

        public void UpdateStyle(Image image, bool isInteractable)
        {
            if (!image)
                return;

            image.color = isInteractable ? interactable : nonInteractable;
        }


        public enum Mode { None, AccountChoose, TransactionConfirm, EnterPIN }
        [HideInInspector] public Mode mode = Mode.None;

        public Account selectedAccount;
        public IEnumerator AccountChoose(Blockchain blockchain, Action<Account> callback = null)
        {
            mode = Mode.AccountChoose;
            windows.accounts.Open("blockchain", blockchain);

            selectedAccount = null;
            while (mode == Mode.AccountChoose)
                yield return null;

            callback?.Invoke(selectedAccount);
        }

        public Dictionary<string, object> transactionOutput;
        public IEnumerator TransactionConfirm(Dictionary<string, object> input, Action<Dictionary<string, object>> callback = null)
        {
            mode = Mode.TransactionConfirm;
            windows.transaction.Open(input);

            transactionOutput = null;
            while (mode == Mode.TransactionConfirm)
                yield return null; // while (!isTransactionConfirm && isOpened) yield return null;

            callback?.Invoke(transactionOutput);
        }

        public IEnumerator GetPrivateKey(Account account, Result result)
        {
            var privateKey = account.GetPrivateKey();
            if (privateKey.IsEmpty())
            {
                mode = Mode.EnterPIN;
                windows.pinEnter.Open();

                while (mode == Mode.EnterPIN)
                    yield return null;
            }

            privateKey = account.GetPrivateKey();

            if (privateKey.IsEmpty())
                result.SetError(Wallet.Errors.OperationCanceled);
            else
                result.SetSuccess(new Dictionary<string, object> { ["privateKey"] = privateKey });
        }
    }
}