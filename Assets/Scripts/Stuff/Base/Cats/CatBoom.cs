using UnityEngine;

namespace SG.RSC
{
    public class CatBoom : CatSuper
    {
        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateBoom)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateBoom))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateBoom, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            if (isRiki)
            {
                Invoke("MakeBoom", 0.4f);
                Destroy(gameObject, 2f);
            }
            else
            {
                shape.enabled = false;
                MakeBoom();
                ShakeAndDestroy();
            }
        }

        void MakeBoom()
        {
            if (isRiki)
            {
                shape.enabled = false;
                Destroy(rb);
            }

            Bomb((type.levelPower[0] + (type.levelPower[item.level - 1] - type.levelPower[0]) * 0.4f));
        }
    }
}