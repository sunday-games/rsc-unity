using UnityEngine;

namespace SG.UI
{
    [AddComponentMenu("Sunday/UI/InputField", 30)]
    public class InputField : TMPro.TMP_InputField
    {
        public void SetStyle(Style style)
        {
            if (transition == Transition.ColorTint)
            {
                colors = new UnityEngine.UI.ColorBlock()
                {
                    colorMultiplier = 1f,
                    fadeDuration = 0.1f,
                    normalColor = style.normalColor,
                    pressedColor = style.pressedColor,
                    disabledColor = style.disabledColor,
                    highlightedColor = style.highlightedColor,
                    selectedColor = style.selectedColor,
                };

                targetGraphic.color = style.normalColor;
            }

            if (placeholder)  placeholder.color = style.Text.disabledColor;

            selectionColor = style.Text.disabledColor;
        }
    }
}
