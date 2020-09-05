using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Blinds : MonoBehaviour
{
    public Image image;
    public float speed = 1f;

    bool isAnimating = false;

    public void Show(float targetTransparent)
    {
        gameObject.SetActive(true);
        StartCoroutine(Showing(targetTransparent));
    }
    IEnumerator Showing(float targetTransparent)
    {
        isAnimating = true;
        while (image.color.a < targetTransparent)
        {
            float t = image.color.a + speed * Time.deltaTime;
            image.color = new Color(1, 1, 1, t < targetTransparent ? t : targetTransparent);
            yield return new WaitForEndOfFrame();
        }
        isAnimating = false;
    }

    public void Hide()
    {
        if (gameObject.activeSelf) StartCoroutine(Hiding());
    }

    IEnumerator Hiding()
    {
        isAnimating = true;
        while (image.color.a > 0)
        {
            image.color = new Color(1, 1, 1, image.color.a - speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
        isAnimating = false;
    }

    public void TryClose()
    {
        if (Game.ui.current.blindsCloseByTap && !isAnimating) Game.ui.PopupClose();
    }
}
