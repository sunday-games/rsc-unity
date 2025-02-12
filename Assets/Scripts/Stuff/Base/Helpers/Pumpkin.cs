using UnityEngine;
using System.Collections.Generic;

namespace SG.RSC
{
    public class Pumpkin : Stuff
    {
        public static List<Pumpkin> pumpkins = new List<Pumpkin>();
        static Vector3 punchScale = new Vector3(0.3f, 0.3f, 1f);

        public static Pumpkin Create(Vector3 position, Quaternion rotation)
        {
            var pumpkin = Instantiate(factory.pumpkinPrefab, position, rotation) as Pumpkin;
            pumpkin.t.SetParent(ui.game.stuffBack, true);
            pumpkin.t.localScale = Vector3.one;
            pumpkin.Punch(punchScale, 0.7f);
            if (sound.ON && pumpkin.onClickSound != null) pumpkin.onClickSound.Play();

            pumpkins.Add(pumpkin);

            return pumpkin;
        }

        public override void Activate(Vector2 sourse)
        {
            if (isActivated) return;
            else isActivated = true;

            t.SetParent(ui.game.stuffFrontFront, false);

            shape.enabled = false;

            Vector2 force = (t.anchoredPosition - new Vector2(sourse.x * Random.Range(0.8f, 1.2f), sourse.y * Random.Range(0.8f, 1.2f))).normalized;
            rb.AddForce(force * 500);
            rb.gravityScale *= 1.5f;

            if (gameplay.isPlaying) factory.CreateCatRandomBasic();

            Destroy(gameObject, 2f);
        }
    }
}