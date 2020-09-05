using UnityEngine;

public class CatJokerRiki : CatJoker
{
    [Space(10)]
    public Transform animParent;
    public GameObject rikiImage;

    [Space(10)]
    public BasicCatAnimation[] animations;

    [HideInInspector]
    public BasicCatAnimation anim;

    public override void Setup()
    {
        radiusNormal = (shape as CircleCollider2D).radius;

        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateJoker)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateJoker))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateJoker, new Transform[] { t });
    }

    public override void Activate(Vector2 sourse)
    {
        t.SetParent(ui.game.stuffFrontFront, false);

        isPicked = false;

        shape.enabled = false;
#if GAF
        if (anim != null) anim.clip.play();
#endif
        if (gameplay.isPlaying)
        {
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

    public override void Pick()
    {
        isPicked = true;

        (shape as CircleCollider2D).radius *= 1.2f;

        rikiImage.SetActive(false);

        if (anim == null)
        {
            anim = Instantiate(animations[CHAIN.Count > 0 ? CHAIN[CHAIN.Count - 1].type.id : 0]) as BasicCatAnimation;
            anim.transform.SetParent(animParent, false);
#if GAF
            anim.clip.addTrigger(clip =>
            {
                if (isPicked) clip.pause();
            }, anim.frameHalf);
            anim.clip.addTrigger(clip =>
            {
                if (anim != null) Destroy(anim.gameObject);
                rikiImage.SetActive(true);
            }, anim.frameEnd);
#endif
        }

        if (sound.ON && onClickSound != null) onClickSound.Play();

        if (CHAIN.Count > 0) SetColor(CHAIN[CHAIN.Count - 1].type);
    }
    public override void Unpick()
    {
        isPicked = false;

        (shape as CircleCollider2D).radius = radiusNormal;
#if GAF
        anim.clip.play();
#endif
        RunColors();
    }
}
