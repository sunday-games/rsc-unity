using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public abstract class Firework : Stuff
    {
        public GameObject activateFX;

        public void InputClick()
        {
            Factory.LIVE_STUFF.Remove(this);

            Activate(Vector2.zero);
        }

        public override void Activate(Vector2 sourse)
        {
            if (isActivated) return;
            else isActivated = true;

            if (gameplay.isPlaying) Missions.OnUseFirework(this);

            if (sound.ON && onClickSound != null) onClickSound.Play();

            Boom();
        }
        public virtual void Boom()
        {
        }

        public void Spread(Vector3 target)
        {
            shape.enabled = false;
            rb.gravityScale = 0;

            StartCoroutine(Fly(target));
        }
        IEnumerator Fly(Vector3 target)
        {
            Factory.LIVE_STUFF.Remove(this);

            float timer = 0f;
            float speed = 0.4f * Random.Range(0.9f, 1.1f);
            while (t != null && DistanceTo(target) > 0.25f)
            {
                timer += speed * Time.deltaTime;
                t.position = Vector3.Lerp(t.position, target, timer);

                yield return new WaitForEndOfFrame();
            }

            if (shape != null) shape.enabled = true;
            if (rb != null) rb.gravityScale = 1.2f;
            if (t != null) t.SetParent(ui.game.stuffFront, true);

            Factory.LIVE_STUFF.Add(this);
        }
    }
}