using UnityEngine;
using System.Collections.Generic;

namespace SG.RSC
{
    public class FireworkColor : Firework
    {
        public ChangingColor changingColor;
        public Mover shardExplosion;

        public override void Boom()
        {
            HashSet<Stuff> stuffToBoom = new HashSet<Stuff>();
            foreach (Stuff stuff in Factory.LIVE_STUFF)
                if (stuff != null && stuff is CatBasic && (stuff as CatBasic).type == changingColor.catType)
                    stuffToBoom.Add(stuff);

            foreach (Stuff stuff in stuffToBoom)
                Factory.LIVE_STUFF.Remove(stuff);

            Vector2 position = t.anchoredPosition;
            foreach (Stuff stuff in stuffToBoom)
                Mover.Create(shardExplosion, ui.canvas[3].transform, t.position, stuff, 0.4f,
                    target =>
                    {
                        (target as CatBasic).Activate(position);
                        iTween.PunchScale(target.gameObject, new Vector3(0.5f, 0.5f, 0), 0.7f);
                    });

            gameplay.GetScores(t.anchoredPosition, countCats: stuffToBoom.Count);

            if (sound.ON && audioClip != null) AudioSource.PlayClipAtPoint(audioClip, t.position);
            Destroy(gameObject);
        }
    }
}