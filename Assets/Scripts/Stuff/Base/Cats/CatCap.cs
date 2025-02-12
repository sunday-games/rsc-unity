using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class CatCap : CatSuper
    {
        static float[] capPower = new float[] { 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f, 2.75f, 3f, 3.25f, 3.5f, 3.75f, 4f };

        public override void Setup()
        {
            t.localScale = type.scale * capPower[item.level - 1];
            if (smallScreen) t.localScale *= 1.1f;

            if (ui.current == ui.game && !user.IsTutorialShown(Tutorial.Part.CatUseActivateCap)) Invoke("TutorialCatUseActivate", 2);
        }

        void TutorialCatUseActivate()
        {
            if (!user.IsTutorialShown(Tutorial.Part.CatUseActivateCap))
                ui.tutorial.Show(Tutorial.Part.CatUseActivateCap, new Transform[] { t });
        }

        public override void ActivatePower()
        {
            Destroy(rb);
            shape.enabled = false;

            StartCoroutine(Fly());
            StartCoroutine(PushOutCatsOnPath());
        }

        IEnumerator Fly()
        {
            if (isRiki)
            {
                t.transform.Rotate(0, 0, -t.transform.rotation.eulerAngles.z);
                yield return new WaitForSeconds(0.5f);
            }
            else
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
            float power = 0.9f * capPower[item.level - 1] * (smallScreen ? 1.1f : 1f);

            var stuffToPushOut = new List<Stuff>();
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
    }
}