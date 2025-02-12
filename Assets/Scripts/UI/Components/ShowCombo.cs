using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SG.RSC
{
    public class ShowCombo : Core
    {
        public Text mainText;

        Vector2 upPath = new Vector2(0f, 64f);

        RectTransform t;

        void Awake()
        {
            t = transform as RectTransform;
        }

        public void Show(Vector2 position, int chain)
        {
            gameObject.SetActive(true);

            mainText.text = chain.ToString();
            t.localScale = Vector3.zero;
            t.anchoredPosition = position;

            t.DOKill();
            t.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            t.DOAnchorPos(t.anchoredPosition + upPath, 0.2f).SetEase(Ease.OutQuad);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}