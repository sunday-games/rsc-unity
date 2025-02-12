using UnityEngine;

namespace SG.RSC
{
    public class CatDisco : CatSuper
    {
        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateDisco)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateDisco))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateDisco, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            if (isRiki)
            {
                Invoke("Dance", 0.5f);
                Destroy(gameObject, 2f);
            }
            else
            {
                shape.enabled = false;
                Dance();
                ShakeAndDestroy();
            }
        }

        void Dance()
        {
            if (isRiki)
            {
                shape.enabled = false;
                Destroy(rb);
            }

            gameplay.StartFever(item.power);
        }
    }
}