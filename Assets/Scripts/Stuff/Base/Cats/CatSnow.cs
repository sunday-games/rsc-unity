using UnityEngine;

namespace SG.RSC
{
    public class CatSnow : CatSuper
    {
        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateSnow)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateSnow))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateSnow, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            if (isRiki)
            {
                Invoke("Freeze", 0.5f);
                Destroy(gameObject, 2f);
            }
            else
            {
                shape.enabled = false;
                Freeze();
                ShakeAndDestroy();
            }
        }

        void Freeze()
        {
            if (isRiki)
            {
                shape.enabled = false;
                Destroy(rb);
            }

            gameplay.FreezeSeconds(item.power);
        }
    }
}