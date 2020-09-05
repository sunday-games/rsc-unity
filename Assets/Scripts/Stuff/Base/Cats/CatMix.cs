using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatMix : CatSuper
{
    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateMix)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateMix))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateMix, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        shape.enabled = false;

        ShakeAndDestroy();

        Mix();
    }

    void Mix()
    {
        var stuff1 = Factory.LIVE_STUFF.ToArray();

        var stuff2 = Factory.LIVE_STUFF.ToArray();
        SG_Utils.Shuffle<Stuff>(stuff2);

        for (int i = 0; i < stuff1.Length; i++)
        {
            Factory.LIVE_STUFF.Remove(stuff1[i]);

            stuff1[i].StartCoroutine(stuff1[i].MoveTo(stuff2[i].t.position, Random.Range(0.5f, 2f), s => { Factory.LIVE_STUFF.Add(s); }));
        }
    }
}
