using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SG.RSC
{
    public class ShowText : Core
    {
        public Text firstText;
        public Text secondText;
        public Shadow shadowText;

        [HideInInspector]
        public RectTransform t;

        Vector2 upPath = new Vector2(0f, 150f);

        public void Setup(Vector2 position, string first, string second)
        {
            t.localScale = Vector3.zero;
            t.anchoredPosition = position;

            firstText.text = first;
            secondText.text = second;

            t.DOKill();
            t.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            t.DOAnchorPos(t.anchoredPosition + upPath, 2f);
            t.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuad).SetDelay(1f).OnComplete(() => { factory.RemoveShowText(this); });
        }
    }
}