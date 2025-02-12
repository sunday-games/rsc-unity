using UnityEngine;

namespace SG.RSC
{
    public class CatLoki : CatSuper
    {
        public static byte[,] bomb = new byte[14, 4]
        {
            {1,0,0,0},
            {2,0,0,0},
            {1,1,0,0},
            {2,1,0,0},
            {2,2,0,0},
            {1,1,1,0},
            {2,1,1,0},
            {2,2,1,0},
            {2,2,2,0},
            {1,1,1,1},
            {2,1,1,1},
            {2,2,1,1},
            {2,2,2,1},
            {2,2,2,2}
        };

        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateLoki)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateLoki))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateLoki, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            if (isRiki)
            {
                Invoke("SpreadFireworks", 1.8f);
                Destroy(gameObject, 2f);
            }
            else
            {
                shape.enabled = false;
                SpreadFireworks();
                ShakeAndDestroy();
            }
        }

        void SpreadFireworks()
        {
            if (isRiki)
            {
                shape.enabled = false;
                Destroy(rb);
            }

            for (byte i = 0; i < bomb[item.level - 1, 0]; i++)
                factory.CreateFireworkAndMove(factory.fireworkPrefabs.boomSmall, t.position, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
            for (byte i = 0; i < bomb[item.level - 1, 1]; i++)
                factory.CreateFireworkAndMove(factory.fireworkPrefabs.rocket, t.position, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
            for (byte i = 0; i < bomb[item.level - 1, 2]; i++)
                factory.CreateFireworkAndMove(factory.fireworkPrefabs.boomBig, t.position, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
            for (byte i = 0; i < bomb[item.level - 1, 3]; i++)
                factory.CreateFireworkAndMove(factory.fireworkPrefabs.color, t.position, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
        }
    }
}