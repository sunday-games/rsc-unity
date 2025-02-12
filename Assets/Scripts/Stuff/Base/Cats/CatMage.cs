using UnityEngine;

namespace SG.RSC
{
    public class CatMage : CatSuper
    {
        public ChangingColor changingColor;

        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateMage)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateMage))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateMage, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            if (isRiki)
            {
                Invoke("Magic", 0.6f);
                Destroy(gameObject, 2f);
            }
            else
            {
                shape.enabled = false;
                Magic();
                ShakeAndDestroy();
            }
        }

        void Magic()
        {
            if (isRiki)
            {
                shape.enabled = false;
                Destroy(rb);
            }

            SG_Utils.Shuffle<Stuff>(Factory.LIVE_STUFF);

            int count = (int)item.power;

            Stuff[] sortStuff = Factory.LIVE_STUFF.ToArray();

            System.Array.Sort(sortStuff, delegate (Stuff s1, Stuff s2)
            {
                if (s1 == null || s2 == null) return 0;
                else return DistanceTo(s1).CompareTo(DistanceTo(s2));
            });

            foreach (Stuff stuff in sortStuff)
                if (stuff != null && stuff is CatBasic && !(stuff is CatJoker) && (stuff as CatBasic).type != changingColor.catType)
                {
                    (stuff as CatBasic).ChangeType(changingColor.catType);

                    stuff.Punch();

                    if (--count <= 0) break;
                }
        }
    }
}