using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CatZen : CatSuper
{
    public Mover timer;

    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateZen)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateZen))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateZen, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        StartCoroutine(FlyingTimers((int)item.power));

        iTween.PunchScale(ui.game.timeText.gameObject, new Vector3(0.6f, 0.6f, 0), 1);

        if (isRiki)
        {
            Destroy(gameObject, 2f);
        }
        else
        {
            shape.enabled = false;
            ShakeAndDestroy();
        }
    }

    IEnumerator FlyingTimers(int count)
    {
        if (isRiki)
        {
            shape.enabled = false;
            Destroy(rb);
        }

        for (int i = 0; i < count; i++)
        {
            Mover.Create(timer, ui.canvas[3].transform, t.position, ui.game.timeImage, UnityEngine.Random.Range(-0.5f, 0.5f), target => { gameplay.seconds++; });
            yield return new WaitForEndOfFrame();
        }
    }
}
