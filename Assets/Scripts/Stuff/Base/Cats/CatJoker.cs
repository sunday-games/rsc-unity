using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CatJoker : CatBasic
{
    public float speed;
    public Color[] colors;

    public void SetColor(CatType catType)
    {
        if (catType.id > colors.Length - 1) return;

        StopCoroutine("ChangeColor");

        image.color = colors[catType.id];
    }

    public void RunColors()
    {
        StopCoroutine("ChangeColor");
        StartCoroutine("ChangeColor");
    }

    void Start()
    {
        RunColors();
    }
    IEnumerator ChangeColor()
    {
        while (true)
            for (int i = 0; i < colors.Length; i++)
            {
                float t = 0;

                while (Mathf.Abs(image.color.r - colors[i].r) > 0.05f ||
                    Mathf.Abs(image.color.g - colors[i].g) > 0.05f ||
                    Mathf.Abs(image.color.b - colors[i].b) > 0.05f)
                {
                    image.color = new Color(
                        Mathf.Lerp(image.color.r, colors[i].r, t),
                        Mathf.Lerp(image.color.g, colors[i].g, t),
                        Mathf.Lerp(image.color.b, colors[i].b, t));

                    t += speed * Time.deltaTime;

                    yield return new WaitForEndOfFrame();
                }
            }
    }

    public CatItem item = null;
    public override void Setup()
    {
        radiusNormal = (shape as CircleCollider2D).radius;

        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateJoker)) Invoke("TutorialCatUseActivate", 2);
    }
    public override void Activate(Vector2 sourse)
    {
        t.SetParent(ui.game.stuffFrontFront, false);

        isPicked = false;

        shape.enabled = false;

        if (gameplay.isPlaying)
        {
            outlineImage.gameObject.SetActive(false);

            Vector2 force = (t.anchoredPosition - new Vector2(sourse.x * Random.Range(0.8f, 1.2f), sourse.y * Random.Range(0.8f, 1.2f))).normalized;
            rb.AddForce(force * 500);
            rb.gravityScale *= 1.5f;

            Missions.OnUseCats(type);

            Destroy(gameObject, 2f);
        }
        else
        {
            shape.enabled = false;
            ShakeAndDestroy();
            gameplay.GetScores(t.anchoredPosition, countCats: Mathf.CeilToInt(item.level * 1.5f));
        }
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateJoker))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateJoker, new Transform[] { t });
    }

    public override void Pick()
    {
        isPicked = true;

        (shape as CircleCollider2D).radius *= 1.1f;
        Punch(thirdVector3);

        outlineImage.color = Color.white;
        outlineImage.gameObject.SetActive(true);

        if (sound.ON && onClickSound != null) onClickSound.Play();

        if (CHAIN.Count > 0) SetColor(CHAIN[CHAIN.Count - 1].type);
    }
    public override void Unpick()
    {
        isPicked = false;

        (shape as CircleCollider2D).radius = radiusNormal;

        outlineImage.gameObject.SetActive(false);

        RunColors();
    }
}
