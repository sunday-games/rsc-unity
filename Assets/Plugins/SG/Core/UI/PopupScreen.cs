using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public class PopupScreen : Screen
    {
        [Space(10)]
        public Image MainImage;
        public Text MainText;
        public TMPro.TMP_Dropdown MainDropdown;
        public Button SecondaryButton;

        public void Show(string title,
            string text = null,
            Sprite sprite = null,
            DropdownData dropdownData = null,
            ButtonData mainButton = null,
            ButtonData secondaryButton = null,
            Screen previous = null) =>
            Show(new Dictionary<string, object>
            {
                ["titleText"] = title,
                ["mainText"] = text,
                ["mainImage"] = sprite,
                ["dropdownData"] = dropdownData,
                ["mainButton"] = mainButton,
                ["secondaryButton"] = secondaryButton,
            }, previous);

        public override void Open()
        {
            if (Params == null || !Params.TryGetString("titleText", out string titleText))
            {
                Log.Error("ScreenPopup - param cant be null");
                return;
            }

            settings.TitleText.text = titleText;

            if (MainText.ActivateIf(Params.TryGetString("mainText", out string mainText)))
                MainText.text = mainText;

            if (MainImage.ActivateIf(Params.TryGet("mainImage", out Sprite sprite)))
                MainImage.sprite = sprite;

            if (MainDropdown.ActivateIf(Params.TryGet("dropdownData", out DropdownData dropdownData)))
            {
                MainDropdown.ClearOptions();
                MainDropdown.AddOptions(dropdownData.Options);
                MainDropdown.onValueChanged.RemoveAllListeners();
                MainDropdown.onValueChanged.AddListener(index => dropdownData.OnValueChanged(index));
            }

            if (settings.MainButton.ActivateIf(Params.TryGet("mainButton", out ButtonData mainButton)))
            {
                settings.MainButton.buttonText.mainText.text = mainButton.Text;
                settings.MainButton.SetCallback(mainButton.Action);
            }

            if (SecondaryButton.ActivateIf(Params.TryGet("secondaryButton", out ButtonData secondaryButton)))
            {
                SecondaryButton.buttonText.mainText.text = secondaryButton.Text;
                SecondaryButton.SetCallback(secondaryButton.Action);
            }

            settings.MainButton.transform.parent.gameObject.SetActive(settings.MainButton.gameObject.activeSelf || SecondaryButton.gameObject.activeSelf);

            settings.CloseButton.gameObject.SetActive(previous != null);
        }

        public override void OnEscapeKey()
        {
            if (previous != null)
                base.OnEscapeKey();
        }
    }

    public class ButtonData
    {
        public string Text;
        public Action Action;
        public bool CloseWindow = true;
        public bool HideButton = true;
        public ButtonData() { }
        public ButtonData(string text, Action action) { Text = text; Action = action; }
    }

    public class DropdownData
    {
        public List<string> Options;
        public Action<int> OnValueChanged;
        public DropdownData(List<string> options, Action<int> onValueChanged) { Options = options; OnValueChanged = onValueChanged; }
    }
}