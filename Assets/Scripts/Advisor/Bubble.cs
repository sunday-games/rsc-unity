using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Bubble : Core
{
    public Text bubbleText;
    public Image bubbleContentImage;
    public Image bubbleImage;

    [Space(10)]
    public float animationTime = 0.5f;

    [Space(10)]
    public int bubbleTextfontSizeMIN = 16;
    public int bubbleTextfontSizeDefault = 24;
    public int bubbleTextfontSizeMAX = 28;

    public void Show(string text, Vector2 bubbleImageHeight)
    {
        this.bubbleImageHeight = bubbleImageHeight;

        bubbleText.fontSize = bubbleTextfontSizeDefault;
        bubbleText.text = text;

        transform.DOScale(Vector3.one, animationTime).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;

        if (bubbleContentImage) bubbleContentImage.sprite = null;
    }

    Vector2 bubbleImageHeight;
    public void Update()
    {
        if (bubbleImage.rectTransform.rect.height < bubbleImageHeight.x && bubbleText.fontSize < bubbleTextfontSizeMAX)
            ++bubbleText.fontSize;
        else if (bubbleImage.rectTransform.rect.height > bubbleImageHeight.y && bubbleText.fontSize > bubbleTextfontSizeMIN)
            --bubbleText.fontSize;
    }
}
