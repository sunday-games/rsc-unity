using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Text = TMPro.TextMeshProUGUI;
using DG.Tweening;

namespace SG.UI
{
    public class Slider : MonoBehaviour,
        // IPointerUpHandler, IPointerDownHandler,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool interactable = true;
        public float value;
        public float step = 0.2f;

        public SliderImage sliderImage;
        [Serializable]
        public class SliderImage
        {
            public Image backImage;
            public Image fullImage;
            [Space(5)]
            public bool changeColor = false;
            public Color normalColor = Color.white;
            public Color highlightedColor = Color.white;

            public void OnPointerEnter()
            {
                if (backImage && changeColor)
                    backImage.color = highlightedColor;
                if (fullImage && changeColor)
                    fullImage.color = highlightedColor;
            }

            public void OnPointerExit()
            {
                if (backImage && changeColor)
                    backImage.color = normalColor;
                if (fullImage && changeColor)
                    fullImage.color = normalColor;
            }

            public void SetValue(float value)
            {
                if (fullImage && fullImage.ActivateIf(value > 0f))
                    fullImage.rectTransform.anchorMax = new Vector2(value, 1f);
            }
        }

        public SliderText sliderText;
        [Serializable]
        public class SliderText
        {
            public Text mainText;
            [Space(5)]
            public bool changeColor = false;
            public Color highlightedColor = Color.white;
            public Color normalColor = Color.white;

            public void OnPointerEnter()
            {
                if (mainText && changeColor)
                    mainText.color = highlightedColor;
            }

            public void OnPointerExit()
            {
                if (mainText && changeColor)
                    mainText.color = normalColor;
            }
        }

        public Transparency transparency;
        [Serializable]
        public class Transparency
        {
            public CanvasGroup canvasGroup;
            public float interactableAlpha = 1f;
            public float nonInteractableAlpha = 0.5f;

            public void OnSetInteractable(bool value)
            {
                if (canvasGroup)
                    canvasGroup.alpha = value ? interactableAlpha : nonInteractableAlpha;
            }
        }

        [Space(10)]
        public SliderEvent onValueChanged;
        [Serializable]
        public class SliderEvent : UnityEvent<float> { }

        public void SetValue(float value)
        {
            SetValueWithoutNotify(value);

            onValueChanged?.Invoke(value);
        }
        public void SetValueWithoutNotify(float value)
        {
            this.value = value;

            sliderImage.SetValue(value);

            sliderText.mainText?.SetText(Mathf.RoundToInt(value * 100) + "%");
        }

        public void SetInteractable(bool value)
        {
            interactable = value;

            transparency.OnSetInteractable(value);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
                return;

            if (value >= 1f)
                SetValue(0f);
            else if (value + step >1f)
                SetValue(1f);
            else
                SetValue(value + step);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable)
                return;

            sliderImage.OnPointerEnter();
            sliderText.OnPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable)
                return;

            sliderImage.OnPointerExit();
            sliderText.OnPointerExit();
        }

        //bool handle;
        //public void OnPointerDown(PointerEventData eventData)
        //{
        //    if (!interactable) return;

        //    handle = true;
        //}

        //public void OnPointerUp(PointerEventData eventData)
        //{
        //    if (!interactable) return;

        //    handle = false;
        //}
    }
}