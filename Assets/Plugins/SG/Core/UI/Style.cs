using System;
using UnityEngine;

namespace SG.UI
{
    using Text = TMPro.TextMeshProUGUI;
    using ScrollRect = UnityEngine.UI.ScrollRect;
    using Scrollbar = UnityEngine.UI.Scrollbar;

    [CreateAssetMenu(menuName = "Sunday/UI/Style")]
    public class Style : ScriptableObject
    {
        public TextStyle Text;
        [Serializable]
        public class TextStyle
        {
            public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            public Color highlightedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            public Color disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            public Color successColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
            public Color failedColor = new Color(0.8f, 0.0f, 0.0f, 1.0f);
        }

        public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        public Color highlightedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        public Color pressedColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        public Color selectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        public void Update()
        {
            foreach (var text in FindObjectsOfType<Text>())
            {
                text.color = text.raycastTarget ? Text.normalColor : Text.normalColor;
            }

            foreach (var button in FindObjectsOfType<Button>())
                button.SetStyle(this);

            foreach (var inputField in FindObjectsOfType<InputField>())
                inputField.SetStyle(this);

            //foreach (var item in toggles) Update(item);

            //foreach (var item in dropdowns) Update(item);

            foreach (var item in FindObjectsOfType<Scrollbar>()) SetStyle(item, this);

            Log.Info("Style Updated");
        }

        void SetStyle(Scrollbar scrollbar, Style style)
        {
            if (scrollbar.transition == UnityEngine.UI.Selectable.Transition.ColorTint)
            {
                scrollbar.colors = new UnityEngine.UI.ColorBlock()
                {
                    colorMultiplier = 1f,
                    fadeDuration = 0.1f,
                    normalColor = style.normalColor,
                    pressedColor = style.pressedColor,
                    disabledColor = style.disabledColor,
                    highlightedColor = style.highlightedColor,
                    selectedColor = style.selectedColor,
                };

                scrollbar.targetGraphic.color = style.normalColor;
            }
        }

        //void Update(Toggle toggle)
        //{
        //    if (!toggle) return;

        //    var interactableHalf = normalColor.SetAlpha(0.5f);

        //    if (toggle.buttonImage.mainImage)
        //        toggle.buttonImage.mainImage.color = toggle.buttonImage.changeColor ? interactableHalf : normalColor;

        //    if (toggle.buttonImage.changeColor)
        //    {
        //        toggle.buttonImage.normalColor = interactableHalf;
        //        toggle.buttonImage.highlightedColor = normalColor;
        //        toggle.buttonImage.onColor = normalColor;
        //        toggle.buttonImage.offColor = interactableHalf;
        //    }

        //    if (toggle.buttonText.mainText)
        //        toggle.buttonText.mainText.color = interactableHalf;

        //    if (toggle.buttonText.changeColor)
        //    {
        //        toggle.buttonText.normalColor = interactableHalf;
        //        toggle.buttonText.highlightedColor = normalColor;
        //    }
        //}

        //void Update(Dropdown dropdown)
        //{
        //    if (!dropdown) return;

        //    var image = dropdown.GetComponent<Image>();
        //    if (image) image.color = dropdown.interactable ? normalColor : disabledColor;

        //    foreach (Transform child in dropdown.transform)
        //        if (child.name == "Right")
        //        {
        //            image = child.GetComponent<Image>();
        //            if (image) image.color = dropdown.interactable ? normalColor : disabledColor;
        //        }
        //}

        //void Update(ScrollRect scrollRect)
        //{
        //    if (!scrollRect) return;

        //    var interactableHalf = normalColor.SetAlpha(0.5f);

        //    if (scrollRect.verticalScrollbar)
        //        scrollRect.verticalScrollbar.handleRect.GetComponent<Image>().color = interactableHalf;
        //}
    }
}