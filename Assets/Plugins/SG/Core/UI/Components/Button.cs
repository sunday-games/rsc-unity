using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Text = TMPro.TextMeshProUGUI;
using Image = UnityEngine.UI.Image;

namespace SG.UI
{
    [AddComponentMenu("Sunday/UI/Button", 30)]
    public class Button : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }
        public ButtonClickedEvent onClick;

        public ButtonImage buttonImage;
        [Serializable]
        public class ButtonImage
        {
            public Image mainImage;
            public Sprite normalSprite;
            public Sprite highlightedSprite;
            public bool changeColor = false;

            public void OnPointerEnter(Color highlightedColor)
            {
                if (mainImage && highlightedSprite)
                    mainImage.sprite = highlightedSprite;

                if (mainImage && changeColor)
                    mainImage.color = highlightedColor;
            }

            public void OnPointerExit(Color normalColor)
            {
                if (mainImage && normalSprite)
                    mainImage.sprite = normalSprite;

                if (mainImage && changeColor)
                    mainImage.color = normalColor;
            }
        }

        public IconImage iconImage;
        [Serializable]
        public class IconImage
        {
            public Image mainImage;
            public Sprite normalSprite;
            public Sprite highlightedSprite;
            public bool changeColor = false;

            public void OnPointerEnter(Color highlightedColor)
            {
                if (mainImage && highlightedSprite)
                {
                    mainImage.sprite = highlightedSprite;
                }

                if (mainImage && changeColor)
                {
                    mainImage.color = highlightedColor;
                }
            }

            public void OnPointerExit(Color normalColor)
            {
                if (mainImage && normalSprite)
                {
                    mainImage.sprite = normalSprite;
                }

                if (mainImage && changeColor)
                {
                    mainImage.color = normalColor;
                }
            }
        }

        public ButtonText buttonText;
        [Serializable]
        public class ButtonText
        {
            public Text mainText;
            public bool changeColor = false;

            public void OnPointerEnter(Color highlightedColor)
            {
                if (mainText && changeColor)
                {
                    mainText.color = highlightedColor;
                }
            }

            public void OnPointerExit(Color normalColor)
            {
                if (mainText && changeColor)
                {
                    mainText.color = normalColor;
                }
            }
        }

        public TipText tipText;
        [Serializable]
        public class TipText
        {
            public CanvasGroup mainText;

            Tweener tweener;

            public void OnPointerEnter()
            {
                if (!mainText)
                    return;

                mainText.gameObject.SetActive(true);

                tweener?.Kill();
                tweener = mainText.DOFade(1f, 0.3f);
            }

            public void OnPointerExit()
            {
                if (!mainText)
                    return;

                tweener?.Kill();
                tweener = mainText.DOFade(0f, 0.3f).OnComplete(() => mainText.gameObject.SetActive(false));
            }

            public void OnDestroy() => tweener?.Kill();

            public void OnEnable()
            {
                if (!mainText)
                    return;

                mainText.gameObject.SetActive(false);
            }
        }

        public Scale scale;
        [Serializable]
        public class Scale
        {
            public Transform mainTransform;
            public float animationTime = 0.2f;
            public Vector3 highlighted = new Vector3(1.05f, 1.05f, 1f);
            public Vector3 normal = new Vector3(1f, 1f, 1f);
            public Vector3 pressed = new Vector3(0.9f, 0.9f, 1f);

            Tweener tweener;

            public void OnPointerDown() => Set(pressed);
            public void OnPointerUp() => Set(normal);
            public void OnPointerEnter() => Set(highlighted);
            public void OnPointerExit() => Set(normal);
            public void OnDestroy() => tweener?.Kill();

            void Set(Vector3 targetScale)
            {
                if (!mainTransform)
                {
                    return;
                }

                tweener?.Kill();
                tweener = mainTransform.DOScale(targetScale, animationTime);
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
                {
                    canvasGroup.alpha = value ? interactableAlpha : nonInteractableAlpha;
                }
            }
        }

        protected Button() { }

        public void SetInteractable(bool value)
        {
            interactable = value;

            transparency.OnSetInteractable(value);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (!interactable)
            {
                return;
            }

            scale.OnPointerEnter();
            buttonImage.OnPointerEnter(colors.highlightedColor);
            iconImage.OnPointerEnter(colors.highlightedColor);
            buttonText.OnPointerEnter(colors.highlightedColor);
            tipText.OnPointerEnter();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            /*var parent = transform.parent;
            while (parent != null)
            {
                if (parent.TryGetComponent<IPointerExitHandler>(out var exitHandler))
                {
                    if (eventData.pointerCurrentRaycast.gameObject == null || !eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(parent))
                    {
                        exitHandler?.OnPointerExit(eventData);
                    }
                    break;
                }

                parent = parent.parent;
            }*/

            if (!interactable)
            {
                return;
            }

            scale.OnPointerExit();
            buttonImage.OnPointerExit(colors.normalColor);
            iconImage.OnPointerExit(colors.normalColor);
            buttonText.OnPointerExit(colors.normalColor);
            tipText.OnPointerExit();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) // if we get set disabled during the press don't run the coroutine
                return;

            Press();

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
            IEnumerator OnFinishSubmit()
            {
                var fadeTime = colors.fadeDuration;
                var elapsedTime = 0f;

                while (elapsedTime < fadeTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return null;
                }

                DoStateTransition(currentSelectionState, false);
            }
        }

        protected override void OnDestroy()
        {
            tipText.OnDestroy();
            scale.OnDestroy();

            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            tipText.OnEnable();

            OnPointerExit(null);

            base.OnEnable();
        }

        private void Press()
        {
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }

        public void SetStyle(Style style)
        {
            if (transition == Transition.ColorTint)
            {
                colors = new ColorBlock()
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

            if (buttonImage.mainImage && buttonImage.changeColor)
            {
                buttonImage.mainImage.color = style.normalColor;
            }

            if (iconImage.mainImage && iconImage.changeColor)
            {
                iconImage.mainImage.color = style.normalColor;
            }

            if (buttonText.mainText && buttonText.changeColor)
            {
                buttonText.mainText.color = style.Text.normalColor;
            }
        }

        public Button SetCallback(Action callback)
        {
            onClick.RemoveAllListeners();
            onClick.AddListener(callback.Invoke);
            return this;
        }

        public Button SetText(string text) { buttonText.mainText.text = text; return this; }
        public Button SetTextAndActivate(string text) { this.Activate(); return SetText(text); }

        public Button SetIcon(Sprite sprite) { iconImage.mainImage.sprite = sprite; return this; }
    }
}
