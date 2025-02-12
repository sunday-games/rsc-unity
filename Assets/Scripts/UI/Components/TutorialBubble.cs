using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SG.RSC
{
    public class TutorialBubble : Core
    {
        public Image bubble;
        public Shadow bubbleShadow;
        public Image character;
        public Shadow characterShadow;
        public Text tipText;
        public Button closeButton;
        public Button fullscreenCloseButton;

        public Vector2 l = new Vector2(50f, 350f);
        public Vector2 h = new Vector2(220f, 480f);

        RectTransform t;

        Tutorial.Part part;

        Vector2 positionUp = new Vector2(0f, 650f);
        Vector2 positionBottom = new Vector2(0f, -650f);

        void Awake()
        {
            t = transform as RectTransform;
        }

        public void Show(Tutorial.Part part, string param = null)
        {
            this.part = part;

            var textKey = "tutorial" + part.name;
            if (Localization.IsKey(textKey + "Premium")) textKey = textKey + "Premium";

            if (!string.IsNullOrEmpty(param)) tipText.text = Localization.Get(textKey, param);
            else if (part.param != null) tipText.text = Localization.Get(textKey, part.param());
            else tipText.text = Localization.Get(textKey);

            bubble.rectTransform.sizeDelta = new Vector2(bubble.rectTransform.sizeDelta.x, (Mathf.Clamp(tipText.text.Length, l.x, l.y) - l.x) / (l.y - l.x) * (h.y - h.x) + h.x);

            closeButton.gameObject.SetActive(!part.fullscreenClosing);
            fullscreenCloseButton.gameObject.SetActive(part.fullscreenClosing);

            if (part.sprite != null) character.sprite = part.sprite;
            else character.sprite = isTNT ? pics.characters.vasia : pics.characters.gingerCat;

            if (part.mirror)
            {
                t.localRotation = Quaternion.Euler(0f, 180f, 0f);
                tipText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                closeButton.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                fullscreenCloseButton.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                if (characterShadow != null) characterShadow.effectDistance = new Vector2(-10f, -13f);
                if (bubbleShadow != null) bubbleShadow.effectDistance = new Vector2(-10f, -13f);

                if (character.sprite == pics.characters.liolia || character.sprite == pics.characters.palna || character.sprite == pics.characters.bobilich || character.sprite == pics.characters.vasia)
                {
                    if (character.sprite == pics.characters.bobilich) character.sprite = pics.characters.bobilichMirror;
                    else if (character.sprite == pics.characters.vasia) character.sprite = pics.characters.vasiaMirror;
                    else if (character.sprite == pics.characters.palna) character.sprite = pics.characters.palnaMirror;
                    else if (character.sprite == pics.characters.liolia) character.sprite = pics.characters.lioliaMirror;

                    character.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }

            bubble.transform.localScale = Vector3.zero;

            if (part.view == TutorialView.Top)
            {
                t.anchoredPosition = positionUp;
                t.DOAnchorPos(new Vector2(0f, part.offset), 0.5f).SetEase(Ease.OutBack);
                bubble.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }
            else if (part.view == TutorialView.Bottom)
            {
                t.anchoredPosition = positionBottom;
                t.DOAnchorPos(new Vector2(0f, part.offset), 0.5f).SetEase(Ease.OutBack);
                bubble.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }
            else if (part.view == TutorialView.Left)
            {
                t.anchoredPosition = new Vector2(-1024f * SG_Utils.aspectRatio, part.offset);
                t.DOAnchorPos(new Vector2(0f, t.anchoredPosition.y), 0.6f).SetEase(Ease.OutBack);
                bubble.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetDelay(0.25f);
            }
            else if (part.view == TutorialView.Right)
            {
                t.anchoredPosition = new Vector2(1024f * SG_Utils.aspectRatio, part.offset);
                t.DOAnchorPos(new Vector2(0f, t.anchoredPosition.y), 0.6f).SetEase(Ease.OutBack);
                bubble.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetDelay(0.25f);
            }
        }

        public void Hide()
        {
            fullscreenCloseButton.interactable = false;
            closeButton.interactable = false;

            bubble.transform.DOKill();
            bubble.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuad);

            t.DOKill();
            if (part.view == TutorialView.Top)
                t.DOAnchorPos(positionUp, 0.45f).SetEase(Ease.InBack).SetDelay(0.2f).OnComplete(() => ui.tutorial.TutorialBubbleEnd(this));
            else if (part.view == TutorialView.Bottom)
                t.DOAnchorPos(positionBottom, 0.45f).SetEase(Ease.InBack).SetDelay(0.2f).OnComplete(() => ui.tutorial.TutorialBubbleEnd(this));
            else if (part.view == TutorialView.Left)
                t.DOAnchorPos(new Vector2(-1024f * SG_Utils.aspectRatio, t.anchoredPosition.y), 0.4f).SetEase(Ease.InBack).SetDelay(0.15f).OnComplete(() => ui.tutorial.TutorialBubbleEnd(this));
            else if (part.view == TutorialView.Right)
                t.DOAnchorPos(new Vector2(1024f * SG_Utils.aspectRatio, t.anchoredPosition.y), 0.4f).SetEase(Ease.InBack).SetDelay(0.15f).OnComplete(() => ui.tutorial.TutorialBubbleEnd(this));

            ui.tutorial.Hide();
        }
    }
}