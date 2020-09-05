using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatKing : CatSuper
{
    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateKing)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateKing))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateKing, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        if (isRiki)
        {
            Invoke("SpreadCoins", 0.5f);
            Destroy(gameObject, 2f);
        }
        else
        {
            shape.enabled = false;
            SpreadCoins();
            ShakeAndDestroy();
        }
    }

    void SpreadCoins()
    {
        if (isRiki)
        {
            shape.enabled = false;
            Destroy(rb);
        }

        SG_Utils.Shuffle<Stuff>(Factory.LIVE_STUFF);

        int goldfishs = (int)item.power;
        foreach (Stuff stuff in Factory.LIVE_STUFF)
        {
            if (stuff != null && stuff is CatBasic && (stuff as CatBasic).isCanHoldCoin)
            {
                Mover.CreateCoinForCat(t.position, stuff as CatBasic);
                goldfishs--;
            }
            if (goldfishs < 1)
                break;
        }
    }
}
