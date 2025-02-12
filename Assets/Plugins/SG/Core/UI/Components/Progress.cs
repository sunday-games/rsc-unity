using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Image = UnityEngine.UI.Image;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public class Progress : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Text TitleText;
        public Text ValueText;

        public float Width;

        [SerializeField] private Image _valueImage;
        [SerializeField] private Image _handleImage;

        public float Value { private set; get; }

        public void SetValueIfNoActive(float value)
        {
            if (_tween != null)
                return;

            Value = value;
        }

        public void SetValue(float value)
        {
            Value = value;
            
            _valueImage.rectTransform.sizeDelta = new Vector2(Width * value, _valueImage.rectTransform.rect.height);
        }

        private Tween _tween;
        public void SetValueWithAnimation(float value, float duration, float delay = default, TweenCallback callback = null)
        {
            if (!gameObject.activeSelf)
            {
                SetValue(value);
                return;
            }

            _tween?.Kill();
            _tween = DOTween.To(
                getter: () => Value,
                setter: SetValue,
                endValue: value,
                duration);

            if (delay != default)
                _tween.SetDelay(delay);

            if (callback != null)
                _tween.OnComplete(callback);
        }

        public void HandleSetVisible(bool visible) => _handleImage.gameObject.SetActive(visible);

        public void SetColor(Color color) => _valueImage.color = color;

        [SerializeField] private string _hoverTitle;
        private string _mainTitle;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hoverTitle.IsEmpty())
                return;

            _mainTitle = TitleText.text;
            TitleText.text = _hoverTitle.Localize();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hoverTitle.IsEmpty())
                return;

            TitleText.text = _mainTitle;
        }
    }
}