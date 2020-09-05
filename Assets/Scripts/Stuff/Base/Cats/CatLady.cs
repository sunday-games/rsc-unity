using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CatLady : CatSuper
{
    public ChangingColor changingColor;

    public override void Setup()
    {
        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateLady)) Invoke("TutorialCatUseActivate", 2);
    }

    void TutorialCatUseActivate()
    {
        if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateLady))
            ui.tutorial.Show(Tutorial.Part.CatUseActivateLady, new Transform[] { t });
    }

    public override void ActivatePower()
    {
        StartCoroutine(Magneting());
    }

    IEnumerator Magneting()
    {
        if (isRiki)
        {
            Destroy(gameObject, 2f);
            yield return new WaitForSeconds(1.6f);
            Destroy(rb);
        }
        else
        {
            iTween.ScaleAdd(gameObject,
                iTween.Hash("x", -0.3f, "y", -0.2f, "easeType", "easeInOutQuad", "loopType", "pingPong", "time", 0.2f));
        }

        var catsToPushMagnet = new List<CatBasic>();
        foreach (var stuff in Factory.LIVE_STUFF)
            if (stuff != null && stuff is CatBasic && (stuff as CatBasic).type == changingColor.catType)
            {
                catsToPushMagnet.Add(stuff as CatBasic);
                if (catsToPushMagnet.Count >= item.power) break;
            }

        var catDone = new List<Stuff>();
        foreach (var cat in catsToPushMagnet)
        {
            Factory.LIVE_STUFF.Remove(cat);
            cat.StartCoroutine(cat.MoveTo(t, 3f, s =>
            {
                catDone.Add(s);
                Factory.LIVE_STUFF.Add(s);
            }));
        }

        while (catDone.Count < catsToPushMagnet.Count) yield return new WaitForEndOfFrame();

        if (isRiki)
        {
        }
        else
        {
            var fx = Instantiate(type.onEndFX, t.position, t.rotation) as GameObject;
            fx.transform.SetParent(t, true);
            fx.layer = 11;

            iTween.Stop(gameObject);
            iTween.ScaleTo(gameObject, iTween.Hash("x", 0, "y", 0, "easeType", "easeInBack", "time", 0.6f));
            Destroy(gameObject, 1f);
        }
    }
}
