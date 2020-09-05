using UnityEngine;
using System.Collections;

public class CatFlint : CatSuper
{
    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateFlint)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateFlint))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateFlint, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        if (isRiki)
        {
            Invoke("GetGoldfish", 0.5f);
            Destroy(gameObject, 2f);
        }
        else
        {
            shape.enabled = false;
            GetGoldfish();
            ShakeAndDestroy();
        }
    }

    void GetGoldfish()
    {
        if (isRiki)
        {
            shape.enabled = false;
            Destroy(rb);
        }

        SG_Utils.Shuffle<Stuff>(Factory.LIVE_STUFF);

        int goldfishs = (int)item.power;
        foreach (Stuff stuff in Factory.LIVE_STUFF)
            if (stuff != null && stuff is CatBasic && (stuff as CatBasic).isCoin)
            {
                stuff.Punch();

                (stuff as CatBasic).FreeCoin();

                if (--goldfishs <= 0) break;
            }
    }
}