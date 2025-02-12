using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace SG.RSC
{
    public class BlackHole : Core
    {
        public static BlackHole Create(Vector3 position, float time)
        {
            var hole = Instantiate(factory.blackHolePrefab, position, Quaternion.identity) as BlackHole;
            hole.t = hole.transform as RectTransform;
            hole.t.SetParent(ui.game.stuffBack, true);
            hole.t.localScale = Vector3.zero;
            hole.StartCoroutine(hole.Absorb(time));

            return hole;
        }

        RectTransform t;

        IEnumerator Absorb(float time)
        {
            t.DOScale(Vector3.one, 0.9f).SetEase(Ease.OutBack);

            var stuffToAbsorb = new List<Stuff>();
            int absorbedStuff = 0;
            int pumpkins = 0;

            float startTime = Time.time;
            while (Time.time - startTime < time && gameplay.isPlaying)
            {
                float startPauseTime = Time.time;
                while (gameplay.isPause) yield return null;
                startTime += Time.time - startPauseTime;

                stuffToAbsorb.Clear();
                foreach (var stuff in Factory.LIVE_STUFF)
                    if (stuff != null && stuff.DistanceTo(t.position) < 3) stuffToAbsorb.Add(stuff);

                absorbedStuff += stuffToAbsorb.Count;

                foreach (var stuff in stuffToAbsorb)
                {
                    if (stuff is Pumpkin) ++pumpkins;

                    Factory.LIVE_STUFF.Remove(stuff);

                    StartCoroutine(stuff.MoveToBlackHole(t));
                }

                yield return null;
            }

            gameplay.GetScores(t.anchoredPosition, countCats: absorbedStuff - pumpkins, countPumpkins: pumpkins);

            t.DOScale(Vector3.zero, 0.9f).SetEase(Ease.InBack);
            Destroy(gameObject, 1f);
        }
    }
}