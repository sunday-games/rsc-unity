using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangingColor : Core
{
    public Image image;
    public Color[] colors;
    public float speed = 0.1f;
    public float freezeTime = 1.5f;

    public CatType catType
    {
        get
        {
            if (isWait) return gameplay.basicCats[color];
            else if (color == 0) return gameplay.basicCats[colors.Length - 1];
            else return gameplay.basicCats[color - 1];
        }
    }
    int color;

    bool isWait = false;

    void OnEnable() { StartCoroutine(ChangeColor()); }
    IEnumerator ChangeColor()
    {
        int startColor = Random.Range(0, colors.Length);

        image.color = colors[startColor];

        startColor = startColor + 1 == colors.Length ? 0 : startColor + 1;

        while (true)
        {
            for (color = startColor; color < colors.Length; color++)
            {
                float t = 0;

                while (Mathf.Abs(image.color.r - colors[color].r) > 0.05f ||
                    Mathf.Abs(image.color.g - colors[color].g) > 0.05f ||
                    Mathf.Abs(image.color.b - colors[color].b) > 0.05f)
                {
                    image.color = new Color(
                        Mathf.Lerp(image.color.r, colors[color].r, t),
                        Mathf.Lerp(image.color.g, colors[color].g, t),
                        Mathf.Lerp(image.color.b, colors[color].b, t));

                    t += speed * Time.deltaTime;

                    yield return new WaitForEndOfFrame();
                }

                isWait = true;
                yield return new WaitForSeconds(freezeTime);
                isWait = false;
            }
            startColor = 0;
        }
    }
}
