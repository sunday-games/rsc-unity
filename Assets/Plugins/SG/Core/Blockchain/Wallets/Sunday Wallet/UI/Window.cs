using UnityEngine;
using System.Collections.Generic;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using InputField = TMPro.TMP_InputField;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class Window : MonoBehaviour
    {
        public static Window current;
        public static Wallet wallet;
        public static UI ui;

        public Text titleText;
        public RectTransform window;
        public Button closeButton;
        public Button mainButton;

        protected Window _previous;

        protected virtual void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseButton_OnClick);
        }

        protected bool isOpened => gameObject.activeSelf;

        public void Open() => Open(data: null);

        public void Open(string openParamName, object openParamValue) =>
            Open(new Dictionary<string, object> { [openParamName] = openParamValue });

        public virtual void Open(Dictionary<string, object> data)
        {
            if (current) _previous = current;

            current = this;

            _previous?.Close();

            gameObject.SetActive(true);
        }

        public virtual void Close(Dictionary<string, object> data = null)
        {
            gameObject.SetActive(false);

            if (current == this)
            {
                current = null;

                if (_previous)
                {
                    _previous.Open(data);
                    _previous = null;
                }
            }
        }

        public virtual void CloseButton_OnClick()
        {
            Close();
        }

        public virtual void OnTabKey() { }

        private void Update()
        {
            Keyboard();
        }

        private void Keyboard()
        {
            if (SG.UI.IsKeyDown.esc)
                CloseButton_OnClick();
            else if (SG.UI.IsKeyDown.enter && mainButton.interactable)
                mainButton.onClick?.Invoke();
            else if (SG.UI.IsKeyDown.tab)
                OnTabKey();

#if UNITY_WEBGL && !UNITY_EDITOR
            else if (SG.UI.IsKeyDown.copy)
            {
                var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (selected) Copy(selected.GetComponent<InputField>());
            }
#endif
        }

        private void Copy(InputField inputField)
        {
            if (inputField)
                ClipboardPlugin.Clipboard.Copy(inputField.text);
        }
    }
}