using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class CatSanta : CatSuper
    {
        public Mover timer;

        public override void Setup()
        {
            t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateSanta)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateSanta))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateSanta, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            var types = new CatType[] {
            CatType.GetCatType (Cats.Snow), CatType.GetCatType (Cats.Loki), CatType.GetCatType (Cats.Orion),
            CatType.GetCatType (Cats.Zen), CatType.GetCatType (Cats.Disco), CatType.GetCatType (Cats.Boom),
            CatType.GetCatType (Cats.Cap), CatType.GetCatType (Cats.King), CatType.GetCatType (Cats.Flint) };

            type = types[Random.Range(0, types.Length)];
            item = new CatItem(type, item.level, 0);

            if (type == CatType.GetCatType(Cats.Zen))
            {
                shape.enabled = false;

                StartCoroutine(FlyingTimers((int)item.power));

                iTween.PunchScale(ui.game.timeText.gameObject, new Vector3(0.6f, 0.6f, 0), 1);

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Snow))
            {
                shape.enabled = false;

                gameplay.FreezeSeconds(item.power);

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Disco))
            {
                shape.enabled = false;

                gameplay.StartFever(item.power);

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Boom))
            {
                shape.enabled = false;

                Bomb((type.levelPower[0] + (type.levelPower[item.level - 1] - type.levelPower[0]) * 0.4f));

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Cap))
            {
                Destroy(rb);
                shape.enabled = false;

                float scale = type.scale.x * (smallScreen ? 1.1f : 1f) * 0.6f * (type.levelPower[item.level - 1] / type.levelPower[0]);
                iTween.ScaleTo(gameObject, iTween.Hash("x", scale, "y", scale, "easeType", "easeOutBack", "time", 0.6f));

                StartCoroutine(Fly());
                StartCoroutine(PushOutCatsOnPath());
            }
            else if (type == CatType.GetCatType(Cats.King))
            {
                shape.enabled = false;

                SG_Utils.Shuffle<Stuff>(Factory.LIVE_STUFF);

                int goldfishs = (int)item.power;
                foreach (Stuff stuff in Factory.LIVE_STUFF)
                {
                    if (stuff != null && stuff is CatBasic && (stuff as CatBasic).isCanHoldCoin)
                    {
                        Mover.CreateCoinForCat(t.position, stuff as CatBasic);
                        goldfishs--;
                    }
                    if (goldfishs < 1)
                        break;
                }

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Flint))
            {
                shape.enabled = false;

                SG_Utils.Shuffle<Stuff>(Factory.LIVE_STUFF);

                int goldfishs = (int)item.power;
                foreach (Stuff stuff in Factory.LIVE_STUFF)
                    if (stuff != null && stuff is CatBasic && (stuff as CatBasic).isCoin)
                    {
                        stuff.Punch();

                        (stuff as CatBasic).FreeCoin();

                        if (--goldfishs <= 0)
                            break;
                    }

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Loki))
            {
                shape.enabled = false;

                for (byte i = 0; i < CatLoki.bomb[item.level - 1, 0]; i++)
                    factory.CreateFirework(factory.fireworkPrefabs.boomSmall, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
                for (byte i = 0; i < CatLoki.bomb[item.level - 1, 1]; i++)
                    factory.CreateFirework(factory.fireworkPrefabs.rocket, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
                for (byte i = 0; i < CatLoki.bomb[item.level - 1, 2]; i++)
                    factory.CreateFirework(factory.fireworkPrefabs.boomBig, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));
                for (byte i = 0; i < CatLoki.bomb[item.level - 1, 3]; i++)
                    factory.CreateFirework(factory.fireworkPrefabs.color, new Vector3(Random.Range(-2f, 2f), Random.Range(-6f, 6f), 0));

                if (!isRiki) ShakeAndDestroy();
            }
            else if (type == CatType.GetCatType(Cats.Orion))
            {
                shape.enabled = false;

                BlackHole.Create(t.position, item.power);

                if (!isRiki) ShakeAndDestroy();
            }
        }

        IEnumerator Fly()
        {
            t.transform.Rotate(0, 0, 180 - t.transform.rotation.eulerAngles.z);

            Vector3 direction = new Vector3(0, -1, 0);
            float speed = 10;

            while (true)
            {
                t.position += direction * speed * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        IEnumerator PushOutCatsOnPath()
        {
            float power = 0.3f * type.levelPower[item.level - 1] * (smallScreen ? 1.1f : 1f);

            List<Stuff> stuffToPushOut = new List<Stuff>();
            int pushedStuff = 0;
            int pumpkins = 0;
            float powerSquare = power * power;
            while (t != null && t.position.y > -7)
            {
                stuffToPushOut.Clear();
                foreach (Stuff stuff in Factory.LIVE_STUFF)
                    if (stuff != null && DistanceTo(stuff) < powerSquare) stuffToPushOut.Add(stuff);

                pushedStuff += stuffToPushOut.Count;

                foreach (Stuff stuff in stuffToPushOut) Factory.LIVE_STUFF.Remove(stuff);

                foreach (Stuff stuff in stuffToPushOut)
                {
                    if (stuff is Pumpkin) ++pumpkins;

                    stuff.Activate(t.anchoredPosition);
                }

                yield return new WaitForEndOfFrame();
            }

            gameplay.GetScores(t.anchoredPosition, countCats: pushedStuff - pumpkins, countPumpkins: pumpkins);

            Destroy(gameObject, 1f);
        }

        IEnumerator FlyingTimers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Mover.Create(timer, ui.canvas[3].transform, t.position, ui.game.timeImage, UnityEngine.Random.Range(-0.5f, 0.5f), target =>
                {
                    gameplay.seconds++;
                });
                yield return new WaitForEndOfFrame();
            }
        }
    }
}