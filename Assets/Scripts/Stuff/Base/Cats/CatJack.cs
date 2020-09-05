using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatJack : CatSuper
{
    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateJack)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateJack))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateJack, new Transform[] { t });
    }

    void Start()
    {
        StartCoroutine(PumpkinNearCats());
    }
    IEnumerator PumpkinNearCats()
    {
        yield return new WaitForSeconds(0.5f);

        float radius = 3f * (smallScreen ? 1.1f : 1f);
        var catsToMakePumpkin = new List<CatBasic>();

        while (t != null)
        {
            foreach (var stuff in Factory.LIVE_STUFF)
                if (stuff != null && stuff is CatBasic && DistanceTo(stuff) < radius) catsToMakePumpkin.Add(stuff as CatBasic);

            foreach (var cat in catsToMakePumpkin) Factory.LIVE_STUFF.Remove(cat);

            foreach (var cat in catsToMakePumpkin)
            {
                Pumpkin.Create(cat.t.position, cat.t.rotation);
                cat.Reset();
            }

            catsToMakePumpkin.Clear();

            yield return new WaitForEndOfFrame();
        }
    }

    public override void ActivatePower()
    {
        StopAllCoroutines();
        shape.enabled = false;

        if (isRiki)
        {
            Invoke("ActivatePumpkins", 0.1f);
            Destroy(rb);
            Destroy(gameObject, 2f);
        }
        else
        {
            ActivatePumpkins();
            ShakeAndDestroy();
        }
    }

    void ActivatePumpkins()
    {
        if (Pumpkin.pumpkins.Count <= 0) return;

        var pumpkinsToActivate = new HashSet<Stuff>();
        foreach (var pumpkin in Pumpkin.pumpkins)
            if (pumpkin != null && Factory.LIVE_STUFF.Contains(pumpkin)) pumpkinsToActivate.Add(pumpkin);

        foreach (var pumpkin in pumpkinsToActivate) Factory.LIVE_STUFF.Remove(pumpkin);

        foreach (var pumpkin in pumpkinsToActivate) pumpkin.Activate(t.anchoredPosition);

        gameplay.GetScores(t.anchoredPosition, countPumpkins: pumpkinsToActivate.Count);

        Pumpkin.pumpkins.Clear();

        var jacks = new List<CatJack>();
        foreach (Stuff stuff in Factory.LIVE_STUFF)
            if (stuff != null && stuff is CatJack) jacks.Add(stuff as CatJack);

        foreach (var catJack in jacks)
        {
            Factory.LIVE_STUFF.Remove(catJack);

            catJack.StopAllCoroutines();
            catJack.shape.enabled = false;
            catJack.ShakeAndDestroy();
        }
    }
}
