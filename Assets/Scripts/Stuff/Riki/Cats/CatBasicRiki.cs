using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class CatBasicRiki : CatBasic
    {
        public Transform animParent;

        public BasicCatRikiImage dokko;
        public BasicCatRikiImage olga;
        public BasicCatRikiImage krash;
        public BasicCatRikiImage rosa;
        BasicCatRikiImage rikiImage;

        [HideInInspector]
        public BasicCatAnimation anim;

        [HideInInspector]
        public CatTypeRiki typeRiki;

        public override void Setup(bool isMultiplier)
        {
            typeRiki = type as CatTypeRiki;
            radiusNormal = (shape as CircleCollider2D).radius;

            SetupType(typeRiki);

            if (isMultiplier)
            {
                SetMultiplier();
                gameplay.multiplierDroped++;
            }
            else if (gameplay.multiplierDroped < balance.multiplierGetting.Length &&
                gameplay.multiplierProgress > balance.multiplierGetting[gameplay.multiplierDroped] && Random.value < 0.2f)
            {
                SetMultiplier();
                gameplay.multiplierProgress -= balance.multiplierGetting[gameplay.multiplierDroped];
                gameplay.multiplierDroped++;
            }
            else if (Missions.isGoldfishes && Random.value > 1f - balance.reward.getCoinChance * achievements.moreGoldfishes)
            {
                SetCoin();
            }
            else if (Events.newYear.isActive && !Events.newYear.isHaveGift && !Events.newYear.isItemTryDrop && gameplay.seconds < balance.reward.timeDropEventItem)
            {
                Events.newYear.isItemTryDrop = true;

                if (Random.value > 0.4f)
                {
                    isHat = true;
                    hatImage.gameObject.SetActive(true);
                }
            }
            else if (Events.stValentin.isActive && !Events.stValentin.isHaveGift && !Events.stValentin.isItemTryDrop && gameplay.seconds < balance.reward.timeDropEventItem)
            {
                Events.stValentin.isItemTryDrop = true;

                if (Random.value > 0.4f)
                {
                    isHeart = true;
                    image.sprite = type.spriteHeart;
                }
            }
            else if (Events.halloween.isActive && !Events.halloween.isHaveGift && !Events.halloween.isItemTryDrop && gameplay.seconds < balance.reward.timeDropEventItem)
            {
                Events.halloween.isItemTryDrop = true;

                if (Random.value > 0.4f)
                {
                    isBat = true;
                    image.sprite = type.spriteBat;
                }
            }
            else
            {
                rikiImage.idle.SetActive(true);
            }
        }

        void SetupType(CatTypeRiki typeRiki)
        {
            t.localScale = smallScreen ? typeRiki.scale * 1.1f : typeRiki.scale;

            onClickSound.clip = typeRiki.onFreeFXSound;

            dokko.Hide();
            olga.Hide();
            krash.Hide();
            rosa.Hide();

            if (typeRiki == gameplay.basicCats[0])
            {
                rikiImage = dokko;
                dokko.gameObject.SetActive(true);
            }
            else if (typeRiki == gameplay.basicCats[1])
            {
                rikiImage = olga;
                olga.gameObject.SetActive(true);
            }
            else if (typeRiki == gameplay.basicCats[2])
            {
                rikiImage = krash;
                krash.gameObject.SetActive(true);
            }
            else if (typeRiki == gameplay.basicCats[3])
            {
                rikiImage = rosa;
                rosa.gameObject.SetActive(true);
            }
        }

        public override void ChangeType(CatType newType)
        {
            type = newType;
            typeRiki = type as CatTypeRiki;

            SetupType(typeRiki);

            if (isCoin)
            {
                rikiImage.candy.SetActive(true);
            }
            else if (isHeart)
            {
                // TODO
            }
            else if (isBat)
            {
                // TODO
            }
            else if (isMultiplier)
            {
                rikiImage.multiplier.SetActive(true);
                rikiImage.multiplierText.text = "x" + (gameplay.multiplier + 1);
            }
            else
            {
                rikiImage.idle.SetActive(true);
            }
        }

        public override void SetCoin()
        {
            isCoin = true;

            rikiImage.idle.SetActive(false);
            rikiImage.candy.SetActive(true);

            if (!user.IsTutorialShown(Tutorial.Part.Goldfishes)) Invoke("TutorialGoldfishes", 4);
        }
        public override void FreeCoin()
        {
            isCoin = false;

            rikiImage.idle.SetActive(true);
            rikiImage.candy.SetActive(false);        // TODO: Кто то обратно включает и получается, что idle не видно

            Mover.Create(ui.game.coinPrefab, ui.canvas[3].transform, t.position, gameplay.level.coinParent, 0.4f, target => { gameplay.GetCoin(); });
        }

        public override void SetMultiplier()
        {
            if (gameplay.multiplier < Missions.maxMultiplier)
            {
                isMultiplier = true;

                rikiImage.idle.SetActive(false);
                rikiImage.multiplier.SetActive(true);
                rikiImage.multiplierText.gameObject.SetActive(true);
                rikiImage.multiplierText.text = "x" + (gameplay.multiplier + 1);

                if (!user.IsTutorialShown(Tutorial.Part.Multiplier)) Invoke("TutorialMultiplier", 2);
            }
            else FreeMultiplier(false);
        }
        public override void FreeMultiplier(bool isIncrement)
        {
            isMultiplier = false;

            if (isIncrement)
            {
                var mover = Mover.Create(factory.moverMultiplier, ui.canvas[3].transform, t.position, ui.game.multiplierText, 0.5f, target =>
                {
                    if (gameplay.multiplier < Missions.maxMultiplier) gameplay.multiplier++;

                    foreach (Stuff cat in Factory.LIVE_STUFF)
                        if (cat != null && cat is CatBasic && (cat as CatBasic).isMultiplier)
                            (cat as CatBasic).SetMultiplier();
                });

                mover.multiplierText.text = rikiImage.multiplierText.text;
                mover.multiplierText.color = rikiImage.multiplierText.color;
            }

            rikiImage.idle.SetActive(true);
            rikiImage.multiplier.SetActive(false);
            rikiImage.multiplierText.gameObject.SetActive(false);
        }


        public override void Activate(Vector2 sourse)
        {
            if (isActivated)
            {
                Debug.LogWarning("CatBasic activated once more");
                return;
            }
            else isActivated = true;

            isPicked = false;

            t.SetParent(ui.game.stuffFrontFront, false);

            highlightImage.gameObject.SetActive(false);

            shape.enabled = false;

            if (isCoin) FreeCoin();

            if (isMultiplier) FreeMultiplier(true);

            if (isHat)
            {
                isHat = false;
                user.newYearHats++;
                Events.newYear.isItemGet = true;
            }

            if (isHeart)
            {
                isHeart = false;
                user.stValentinHearts++;
                Events.stValentin.isItemGet = true;
            }

            if (isBat)
            {
                isBat = false;
                user.halloweenBats++;
                Events.halloween.isItemGet = true;
            }

#if GAF
        if (anim != null) anim.clip.play();
#endif
            foreach (CatSlot catSlot in ui.game.catSlots)
                if (catSlot.catItem != null && catSlot.type == type)
                {
                    Mover mana = Mover.Create(ui.game.manaPrefab, ui.canvas[3].transform, t.position, catSlot, 0.2f, catSlot.AddMana);
                    mana.image.color = type.color;
                    break;
                }

            Vector2 force = (t.anchoredPosition - new Vector2(sourse.x * Random.Range(0.8f, 1.2f), sourse.y * Random.Range(0.8f, 1.2f))).normalized;
            rb.AddForce(force * 500);
            rb.gravityScale *= 1.5f;

            if (gameplay.isPlaying) factory.CreateCatRandomBasic();

            Invoke("Reset", 2f);
        }
        public override void Reset()
        {
            shape.enabled = true;
            (shape as CircleCollider2D).radius = radiusNormal;
            rb.gravityScale = 1.2f;

            if (anim != null) Destroy(anim.gameObject);

            highlightImage.gameObject.SetActive(false);

            isMultiplier = false;

            isCoin = false;

            //hatImage.gameObject.SetActive(false);
            isHat = false;

            isHeart = false;

            isBat = false;

            isActivated = false;

            factory.RemoveCatBasic(this);
        }

        public override void Pick()
        {
            isPicked = true;

            (shape as CircleCollider2D).radius *= 1.2f;

            if (anim != null) Destroy(anim.gameObject);

            if (isCoin)
            {
                rikiImage.candy.SetActive(false);
                anim = Instantiate(typeRiki.candyPicked) as BasicCatAnimation;
                anim.transform.SetParent(animParent, false);
#if GAF
            anim.clip.addTrigger(clip => { if (isPicked) clip.pause(); }, anim.frameHalf);
            anim.clip.addTrigger(clip => { Destroy(anim.gameObject); if (isCoin) rikiImage.candy.SetActive(true); }, anim.frameEnd);
#endif
            }
            else if (isMultiplier)
            {
                rikiImage.multiplier.SetActive(false);
                rikiImage.multiplierText.gameObject.SetActive(false);
                anim = Instantiate(typeRiki.multiplierPicked) as BasicCatAnimation;
                anim.transform.SetParent(animParent, false);
                anim.multiplierText.text = "x" + (gameplay.multiplier + 1);
#if GAF
            anim.clip.addTrigger(clip => { if (isPicked) clip.pause(); }, anim.frameHalf);
            anim.clip.addTrigger(clip =>
            {
                Destroy(anim.gameObject);
                if (isMultiplier)
                {
                    rikiImage.multiplier.SetActive(true);
                    rikiImage.multiplierText.gameObject.SetActive(true);
                }
            }, anim.frameEnd);
#endif
            }
            else if (isHeart)
            {
                // TODO
            }
            else if (isBat)
            {
                // TODO
            }
            else
            {
                rikiImage.idle.SetActive(false);
                anim = Instantiate(typeRiki.idlePicked) as BasicCatAnimation;
                anim.transform.SetParent(animParent, false);
#if GAF
            anim.clip.addTrigger(clip => { if (isPicked) clip.pause(); }, anim.frameHalf);
            anim.clip.addTrigger(clip => { Destroy(anim.gameObject); rikiImage.idle.SetActive(true); }, anim.frameEnd);
#endif

            }

            if (sound.ON && onClickSound != null) onClickSound.Play();
        }
        public override void Unpick()
        {
            isPicked = false;

            (shape as CircleCollider2D).radius = radiusNormal;
#if GAF
        anim.clip.play();
#endif

        }

        public bool isCanDrawAttention { get { return anim == null && !isMultiplier && !isHat && !isHeart && !isBat; } }
        public void DrawAttention()
        {
            if (isCoin)
            {
                rikiImage.candy.SetActive(false);
                anim = Instantiate(typeRiki.candy) as BasicCatAnimation;
                anim.transform.SetParent(animParent, false);
#if GAF
            anim.clip.addTrigger(clip => { Destroy(anim.gameObject); rikiImage.candy.SetActive(true); }, anim.frameEnd);
#endif

            }
            else
            {
                rikiImage.idle.SetActive(false);
                anim = Instantiate(typeRiki.idle) as BasicCatAnimation;
                anim.transform.SetParent(animParent, false);
#if GAF
            anim.clip.addTrigger(clip => { Destroy(anim.gameObject); rikiImage.idle.SetActive(true); }, anim.frameEnd);
#endif

            }
        }
    }
}