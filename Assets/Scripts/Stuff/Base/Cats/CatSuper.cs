using UnityEngine;

namespace SG.RSC
{
    public enum Cats { Ginger, Purple, Whitey, Lime, Snow, Zen, Disco, Boom, Cap, Joker, King, Flint, Mage, Loki, Santa, Lady, Jack, Orion, Raiden, Mix }

    public abstract class CatSuper : Stuff
    {
        public CatItem item = null;
        [HideInInspector]
        public CatType type = null;

        public override void Activate(Vector2 sourse)
        {
            if (isActivated) return;
            else isActivated = true;

            t.SetParent(ui.game.stuffFrontFront, false);

            if (gameplay.isPlaying)
            {
                ActivatePower();
                Missions.OnUseCats(type);

                if (type.onFreeAnimation != null)
                {
                    image.gameObject.SetActive(false);
                    var anim = Instantiate(type.onFreeAnimation) as BasicCatAnimation;
                    anim.transform.SetParent(t, false);
#if GAF
                anim.clip.addTrigger(clip => { Destroy(anim.gameObject); }, anim.frameEnd);
#endif
                }
            }
            else
            {
                shape.enabled = false;
                ShakeAndDestroy();
                gameplay.GetScores(t.anchoredPosition, countCats: Mathf.CeilToInt(item.level * 1.5f));
            }

            if (type.onFreeFX != null)
            {
                var fx = Instantiate(type.onFreeFX, t.position, t.rotation) as GameObject;
                fx.transform.SetParent(t, true);
                fx.layer = 11;
            }

            if (sound.ON && type.onFreeFXSound != null) AudioSource.PlayClipAtPoint(type.onFreeFXSound, t.position);
        }

        public abstract void ActivatePower();

        public void InputClick()
        {
            Factory.LIVE_STUFF.Remove(this);

            Activate(Vector2.zero);
        }
    }
}