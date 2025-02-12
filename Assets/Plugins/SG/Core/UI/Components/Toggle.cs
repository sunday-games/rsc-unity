using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

namespace SG.UI
{
    public class Toggle : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        public bool interactable = true;
        public bool value;

        [HideInInspector]
        public ToggleGroup group;

        public HighlightImage highlightImage;
        [Serializable]
        public class HighlightImage
        {
            public Image image;
            public Sprite normalSprite;
            public Sprite highlightedSprite;
            [Space(5)]
            public bool changeColor = false;
            public Color highlightedColor = Color.white;
            public Color normalColor = Color.white;

            public void OnPointerEnter()
            {
                if (!image)
                    return;

                if (highlightedSprite || normalSprite)
                {
                    image.gameObject.SetActive(highlightedSprite);
                    image.sprite = highlightedSprite;
                }

                if (changeColor)
                    image.color = highlightedColor;
            }

            public void OnPointerExit()
            {
                if (!image)
                    return;

                if (highlightedSprite || normalSprite)
                {
                    image.gameObject.SetActive(normalSprite);
                    image.sprite = normalSprite;
                }

                if (changeColor)
                    image.color = normalColor;
            }
        }

        public ToggleImage toggleImage;
        [Serializable]
        public class ToggleImage
        {
            public Image image;
            public Sprite offSprite;
            public Sprite onSprite;
            [Space(5)]
            public bool changeColor = false;
            public Color onColor = Color.white;
            public Color offColor = Color.white;
            [Space(5)]
            public Transform onPosition;
            public Transform offPosition;
            [Space(5)]
            public Vector3 onRotation;
            public Vector3 offRotation;

            public void SetValue(bool value, bool visible)
            {
                if (!image)
                    return;

                if (onSprite || offSprite)
                {
                    image.gameObject.SetActive(value ? onSprite : offSprite);
                    image.sprite = value ? onSprite : offSprite;
                }

                if (changeColor)
                    image.color = value ? onColor : offColor;

                if (onPosition)
                {
                    if (visible)
                    {
                        image.transform.DOKill();
                        if (onPosition.position != offPosition.position)
                            image.transform.DOMove(value ? onPosition.position : offPosition.position, 0.2f);
                        if (onPosition.rotation != offPosition.rotation)
                            image.transform.DORotate((value ? onPosition.rotation : offPosition.rotation).eulerAngles, 0.2f);
                    }
                    else
                    {
                        if (onPosition.position != offPosition.position)
                            image.transform.position = value ? onPosition.position : offPosition.position;
                        if (onPosition.rotation != offPosition.rotation)
                            image.transform.rotation = value ? onPosition.rotation : offPosition.rotation;
                    }
                }

                if (onRotation != default || offRotation != default)
                {
                    if (visible)
                    {
                        image.transform.DOKill();
                        image.transform.DORotate(value ? onRotation : offRotation, 0.2f);
                    }
                    else
                    {
                        image.transform.eulerAngles = value ? onRotation : offRotation;
                    }
                }
            }
        }

        public ToggleText buttonText;
        [Serializable]
        public class ToggleText
        {
            public TMPro.TextMeshProUGUI mainText;
            [Space(5)]
            public bool changeColor = false;
            public Color highlightedColor = Color.white;
            public Color normalColor = Color.white;
            [Space(5)]
            public CanvasGroup canvasGroup;
            public float interactableAlpha = 1f;
            public float nonInteractableAlpha = 0.5f;

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

            public void SetValue(bool value)
            {
                if (mainText && changeColor)
                    mainText.color = value ? highlightedColor : normalColor;

                if (mainText && canvasGroup)
                    canvasGroup.alpha = value ? interactableAlpha : nonInteractableAlpha;
            }
        }

        public Scale scale;
        [Serializable]
        public class Scale
        {
            public Transform mainTransform;
            public float animationTime = 0.2f;
            public Vector3 highlighted = new Vector3(1.05f, 1.05f, 1f);
            public Vector3 normalOn = new Vector3(1f, 1f, 1f);
            public Vector3 normalOff = new Vector3(1f, 1f, 1f);
            public Vector3 pressed = new Vector3(0.9f, 0.9f, 1f);

            Tweener tweener;

            public void OnPointerDown() => Set(pressed);
            public void OnPointerUp(bool on) => Set(on ? normalOn : normalOff);
            public void OnPointerEnter() => Set(highlighted);
            public void OnPointerExit(bool on) => Set(on ? normalOn : normalOff);
            public void OnDestroy() => tweener?.Kill();

            void Set(Vector3 targetScale)
            {
                if (!mainTransform)
                    return;

                if (animationTime > 0)
                {
                    tweener?.Kill();
                    tweener = mainTransform.DOScale(targetScale, animationTime);
                }
                else
                {
                    mainTransform.localScale = targetScale;
                }
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
        public ToggleEvent onValueChanged;
        [Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        public void SetValue(bool value, bool invokeEvent = true)
        {
            this.value = value;

            highlightImage.OnPointerExit();

            toggleImage.SetValue(value, gameObject.activeInHierarchy);

            scale.OnPointerExit(value);

            buttonText.SetValue(value);

            if (value && group)
            {
                group.Current = this;

                foreach (var toggle in group.Toggles)
                    if (toggle == this)
                    {
                        toggle.interactable = false;
                    }
                    else
                    {
                        toggle.interactable = true;
                        toggle.SetValue(false, invokeEvent: false);
                    }
            }

            if (invokeEvent)
                onValueChanged?.Invoke(value);
        }

        public void SetInteractable(bool value)
        {
            interactable = value;

            transparency.OnSetInteractable(value);
        }

        public Toggle SetText(string value)
        {
            buttonText.mainText.text = value;
            return this;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
                return;

            SetValue(!value);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable)
                return;

            scale.OnPointerDown();

            //if (!invokeEventOnClick) SetValue(!value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable)
                return;

            scale.OnPointerUp(value);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable)
                return;

            scale.OnPointerEnter();
            highlightImage.OnPointerEnter();
            buttonText.OnPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable)
                return;

            scale.OnPointerExit(value);
            highlightImage.OnPointerExit();
            buttonText.OnPointerExit();
        }

        [ContextMenu("Change")]
        public void Change() => SetValue(!value);

        public Toggle SetCallback(Action<bool> callback)
        {
            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener(callback.Invoke);
            return this;
        }
    }
}