using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatOrion : CatSuper
{
    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateOrion)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateOrion))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateOrion, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        if (isRiki)
        {
            Invoke("MakeBlackHole", 0.4f);
            Destroy(gameObject, 2f);
        }
        else
        {
            shape.enabled = false;
            MakeBlackHole();
            ShakeAndDestroy();
        }
    }

    void MakeBlackHole()
    {
        if (isRiki)
        {
            shape.enabled = false;
            Destroy(rb);
        }

        BlackHole.Create(t.position, item.power);
    }
}
