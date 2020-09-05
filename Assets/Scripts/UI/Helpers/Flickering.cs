using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class Flickering : MonoBehaviour
{
    public Image flickeringImage;
    public Ease ease = Ease.InOutQuad;
    public float speed = 1f;

    void OnEnable()
    {
        flickeringImage.color = new Color(flickeringImage.color.r, flickeringImage.color.g, flickeringImage.color.b, 0f);
        flickeringImage.DOColor(new Color(flickeringImage.color.r, flickeringImage.color.g, flickeringImage.color.b, 1f), speed).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable()
    {
        flickeringImage.DOKill();
    }
}
