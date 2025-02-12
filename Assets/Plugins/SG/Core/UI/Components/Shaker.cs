using UnityEngine;
using DG.Tweening;

namespace SG.UI
{
    public class Shaker : MonoBehaviour
    {
        public float Duration = 0.6f;
        public float Strength = 50f;

        private Tween _tween;
        private Vector2 _startPosition;

        private void Start()
        {
            _startPosition = this.RectTransform().anchoredPosition;

            _tween = this.RectTransform()
                .DOShakeAnchorPos(Duration, Strength, randomness: 0)
                .OnComplete(() => Destroy(this));
        }

        private void OnDisable()
        {
            Destroy(this);
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            this.RectTransform().anchoredPosition = _startPosition;
        }
    }
}