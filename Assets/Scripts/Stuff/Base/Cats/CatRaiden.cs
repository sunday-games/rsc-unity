using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class CatRaiden : CatSuper
    {
        public float radius = 2.5f;

        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateRaiden)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateRaiden))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateRaiden, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            StartCoroutine(ChainReaction());
        }

        IEnumerator ChainReaction()
        {
            Stuff raiden = null;
            float distanceMin = float.MaxValue;
            foreach (var stuff in Factory.LIVE_STUFF)
                if (stuff != null && stuff is CatRaiden)
                {
                    float distance = DistanceTo(stuff);

                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        raiden = stuff;
                    }
                }

            if (raiden != null)
            {
                Factory.LIVE_STUFF.Remove(raiden);

                Lightning.Create(raiden, this);
            }

            yield return new WaitForSeconds(0.1f);
            shape.enabled = false;
            //ShakeAndDestroy();
            Destroy(rb);
            iTween.ScaleTo(gameObject, iTween.Hash("x", 0, "y", 0, "easeType", "easeInBack", "time", 0.3f));
            Destroy(gameObject, 1f);
            Bomb(radius);

            if (raiden != null)
            {
                yield return new WaitForSeconds(0.3f);

                raiden.Activate(Vector2.zero);
            }
        }

        //IEnumerator ChainReaction()
        //{
        //    var raidens = new List<Stuff>();
        //    foreach (var stuff in Factory.LIVE_STUFF)
        //        if (stuff != null && stuff is CatRaiden) raidens.Add(stuff);

        //    foreach (var raiden in raidens) Factory.LIVE_STUFF.Remove(raiden);

        //    foreach (var raiden in raidens)
        //    {
        //        Lightning.Create(raiden, this);

        //        yield return new WaitForSeconds(0.3f);

        //        raiden.shape.enabled = false;
        //        raiden.Bomb(radius);
        //        raiden.ShakeAndDestroy();
        //    }

        //    shape.enabled = false;
        //    ShakeAndDestroy();
        //    Bomb(radius);
        //}
    }
}