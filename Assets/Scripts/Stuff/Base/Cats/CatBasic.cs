using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CatBasic : Stuff
{
    public static List<CatBasic> CHAIN = new List<CatBasic>();

    static bool _IS_LOOP = false;
    public static bool IS_LOOP
    {
        get { return _IS_LOOP; }
        set
        {
            _IS_LOOP = value;
            if (!isRiki) foreach (CatBasic cat in CHAIN) cat.outlineImage.color = _IS_LOOP ? Color.yellow : Color.white;
        }
    }

    protected Image outlineImage { get { return isCoin || isMultiplier || isHeart || isBat ? outline2Image : outline1Image; } }
    public Image outline1Image;
    public Image outline2Image;
    public Image multiplierImage;
    public Image multiplierLightImage;
    public Image highlightImage;
    public Image hatImage;

    [HideInInspector]
    public CatType type = null;
    [HideInInspector]
    public bool isHat = false;
    [HideInInspector]
    public bool isHeart = false;
    [HideInInspector]
    public bool isBat = false;
    [HideInInspector]
    public bool isCoin = false;
    [HideInInspector]
    public bool isMultiplier = false;

    [HideInInspector]
    public bool isPicked = false;

    [HideInInspector]
    public float radiusNormal;

    public virtual void Setup(bool isMultiplier)
    {
        radiusNormal = (shape as CircleCollider2D).radius;

        t.localScale = smallScreen ? type.scale * 1.1f : type.scale;

        image.sprite = type.spriteNormal;
        onClickSound.clip = type.onFreeFXSound;
        //onClickSound.pitch = Random.Range(1f, 1.1f);

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
    }

    public virtual void ChangeType(CatType newType)
    {
        type = newType;

        if (isCoin) image.sprite = type.spriteFish;
        else if (isHeart) image.sprite = type.spriteHeart;
        else if (isBat) image.sprite = type.spriteBat;
        else if (isMultiplier) image.sprite = type.spriteMultiplier;
        else image.sprite = type.spriteNormal;

        onClickSound.clip = type.onFreeFXSound;
    }

    public virtual void SetCoin()
    {
        isCoin = true;
        image.sprite = type.spriteFish;

        if (!user.IsTutorialShown(Tutorial.Part.Goldfishes)) Invoke("TutorialGoldfishes", 4);
    }
    public virtual void FreeCoin()
    {
        isCoin = false;
        image.sprite = type.spriteNormal;
        Mover.Create(ui.game.coinPrefab, ui.canvas[3].transform, t.position, gameplay.level.coinParent, 0.4f, target => { gameplay.GetCoin(); });
    }
    void TutorialGoldfishes()
    {
        if (!user.IsTutorialShown(Tutorial.Part.Goldfishes) && ui.game.gameObject.activeSelf)
        {
            var catWithGoldfishes = new List<Transform>();
            foreach (Stuff cat in Factory.LIVE_STUFF)
                if (cat != null && cat is CatBasic && (cat as CatBasic).isCoin) catWithGoldfishes.Add(cat.t);

            if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Goldfishes1, Tutorial.Part.Goldfishes }, catWithGoldfishes.ToArray());
            else ui.tutorial.Show(Tutorial.Part.Goldfishes, catWithGoldfishes.ToArray());
        }
    }
    public bool isCanHoldCoin { get { return (!(this is CatJoker) && !isCoin && !isMultiplier && !isHat && !isHeart && !isBat); } }

    public virtual void SetMultiplier()
    {
        if (gameplay.multiplier < Missions.maxMultiplier)
        {
            isMultiplier = true;
            image.sprite = type.spriteMultiplier;
            multiplierImage.sprite = pics.multipliers[(gameplay.multiplier + 1) - 2];
            multiplierImage.gameObject.SetActive(true);

            if (!user.IsTutorialShown(Tutorial.Part.Multiplier)) Invoke("TutorialMultiplier", 2);
        }
        else FreeMultiplier(false);
    }
    public virtual void FreeMultiplier(bool isIncrement)
    {
        isMultiplier = false;
        image.sprite = type.spriteNormal;
        multiplierImage.gameObject.SetActive(false);
        multiplierLightImage.gameObject.SetActive(false);

        if (isIncrement)
        {
            Mover mover = Mover.Create(Game.factory.moverMultiplier, ui.canvas[3].transform, t.position, ui.game.multiplierText, 0.5f, target =>
            {
                if (gameplay.multiplier < Missions.maxMultiplier) gameplay.multiplier++;

                foreach (Stuff cat in Factory.LIVE_STUFF)
                    if (cat != null && cat is CatBasic && (cat as CatBasic).isMultiplier)
                        (cat as CatBasic).SetMultiplier();
            });
            mover.multiplierImage.sprite = multiplierImage.sprite;
        }
    }
    void TutorialMultiplier()
    {
        if (!user.IsTutorialShown(Tutorial.Part.Multiplier)) ui.tutorial.Show(Tutorial.Part.Multiplier, new Transform[] { t });
    }

    IEnumerator FreeChain(CatBasic[] chain, Vector2 sourse)
    {
        // foreach (CatBasic cat in chain) Factory.LIVE_STUFF.Remove(cat);

        sound.PlayVoiceCombo(chain.Length);

        gameplay.multiplierProgress += chain.Length * chain.Length * (Random.Range(3, 30) < chain.Length ? 0.02f : 0.04f) * achievements.easyGetMultiplier;

        gameplay.GetFever(chain.Length);

        int score = 0;
        for (int i = 0; i < chain.Length; i++)
            if (chain[i] != null)
            {
                chain[i].Activate(sourse);

                if (chain[i] is CatJoker)
                    score += (int)(chain[i] as CatJoker).item.power;

                score += balance.baseScore * (i + 1) * (i + 1);

                yield return new WaitForSeconds(0.05f);
            }

        score *= gameplay.multiplier * (gameplay.isFever ? 3 : 1) * (IS_LOOP ? 3 : 1);

        Missions.OnChainDone(chain.Length, chain[chain.Length - 1].type, IS_LOOP);

        gameplay.score += score;

        factory.CreateShowText(t.anchoredPosition, score, chain.Length);

        if (Missions.isFireworkColor && chain.Length > balance.fireworkChain.color)
            factory.CreateFirework(factory.fireworkPrefabs.color, chain[chain.Length - 1].t.position);
        else if (Missions.isFireworkBoomBig && chain.Length > balance.fireworkChain.boomBig)
            factory.CreateFirework(factory.fireworkPrefabs.boomBig, chain[chain.Length - 1].t.position);
        else if (Missions.isFireworkRocket && chain.Length > balance.fireworkChain.rocket)
            factory.CreateFirework(factory.fireworkPrefabs.rocket, chain[chain.Length - 1].t.position);
        else if (Missions.isFireworkBoomSmall && chain.Length > balance.fireworkChain.boomSmall)
            factory.CreateFirework(factory.fireworkPrefabs.boomSmall, chain[chain.Length - 1].t.position);
    }
    public override void Activate(Vector2 sourse)
    {
        if (isActivated) return;
        else isActivated = true;

        isPicked = false;

        t.SetParent(ui.game.stuffFrontFront, false);

        outlineImage.gameObject.SetActive(false);

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
    public virtual void Reset()
    {
        shape.enabled = true;
        (shape as CircleCollider2D).radius = radiusNormal;
        rb.gravityScale = 1.2f;

        image.sprite = type.spriteNormal;

        isMultiplier = false;
        multiplierImage.gameObject.SetActive(false);
        multiplierLightImage.gameObject.SetActive(false);

        outline1Image.gameObject.SetActive(false);
        outline2Image.gameObject.SetActive(false);

        highlightImage.gameObject.SetActive(false);

        isCoin = false;

        hatImage.gameObject.SetActive(false);
        isHat = false;

        isHeart = false;

        isBat = false;

        isActivated = false;

        factory.RemoveCatBasic(this);
    }

    public virtual void Pick()
    {
        isPicked = true;

        t.SetParent(ui.game.stuffFront, true);

        (shape as CircleCollider2D).radius *= 1.1f;
        Punch(thirdVector3);

        outlineImage.color = Color.white;
        outlineImage.gameObject.SetActive(true);

        if (isCoin) image.sprite = type.spriteFishPicked;
        else if (isMultiplier) multiplierLightImage.gameObject.SetActive(true);
        else if (isHeart) image.sprite = type.spriteHeartPicked;
        else if (isBat) image.sprite = type.spriteBatPicked;
        else if (type.spritePicked != null) image.sprite = type.spritePicked;

        if (sound.ON && onClickSound != null) onClickSound.Play();
    }
    public virtual void Unpick()
    {
        isPicked = false;

        t.SetParent(ui.game.stuffBack, true);

        (shape as CircleCollider2D).radius = radiusNormal;

        outlineImage.gameObject.SetActive(false);

        if (isCoin) image.sprite = type.spriteFish;
        else if (isMultiplier) multiplierLightImage.gameObject.SetActive(false);
        else if (isHeart) image.sprite = type.spriteHeart;
        else if (isBat) image.sprite = type.spriteBat;
        else if (type.spritePicked != null) image.sprite = type.spriteNormal;
    }

    static int highlightCount = 0;
    public static void HighlightChain()
    {
        HighlightClear();

        if (CHAIN.Count > 0) // Где то тут возникал NPE, возможно здесь
            CHAIN[CHAIN.Count - 1].Highlight();

        if (CHAIN.Count + highlightCount < 3) HighlightClear();
    }
    public static void HighlightClear()
    {
        foreach (Stuff stuff in Factory.LIVE_STUFF)
            if (stuff != null && stuff is CatBasic && (stuff as CatBasic).highlightImage.gameObject.activeSelf)
                (stuff as CatBasic).highlightImage.gameObject.SetActive(false);
        highlightCount = 0;
    }
    public void Highlight()
    {
        HashSet<CatBasic> cats = new HashSet<CatBasic>();
        foreach (Stuff stuff in Factory.LIVE_STUFF)
            if (stuff is CatBasic && (stuff as CatBasic).type == type
                && !(stuff as CatBasic).highlightImage.gameObject.activeSelf && CHAIN.IndexOf(stuff as CatBasic) < 0 && isNear(stuff))
                cats.Add(stuff as CatBasic);

        foreach (CatBasic cat in cats)
        {
            cat.highlightImage.gameObject.SetActive(true);
            ++highlightCount;
            cat.Highlight();
        }
    }

    public void ChainStart()
    {
        ChainAdd();
    }
    public void ChainEnd()
    {
        if (CHAIN.Count > 2)
        {
            ui.game.showCombo.Hide();

            var chainCopy = CHAIN.ToArray();
            StartCoroutine(FreeChain(chainCopy, chainCopy[chainCopy.Length / 2].t.anchoredPosition));
        }
        else
        {
            var chainCopy = CHAIN.ToArray();
            foreach (var cat in chainCopy) if (cat != null) cat.ChainRemove();
        }

        CHAIN.Clear();
        HighlightClear();
    }

    public void ChainAdd()
    {
        Pick();

        CHAIN.Add(this);
        Factory.LIVE_STUFF.Remove(this);

        if (CHAIN.Count > 4) ui.game.showCombo.Show(t.anchoredPosition, CHAIN.Count);
        else ui.game.showCombo.Hide();

        HighlightChain();

        IS_LOOP = CHAIN.Count > 5 && CHAIN[0].isNear(CHAIN[CHAIN.Count - 1]);
    }
    public void ChainRemove()
    {
        Unpick();

        CHAIN.Remove(this);
        Factory.LIVE_STUFF.Add(this);

        if (CHAIN.Count > 4) ui.game.showCombo.Show(CHAIN[CHAIN.Count - 1].t.anchoredPosition, CHAIN.Count);
        else ui.game.showCombo.Hide();

        HighlightChain();

        IS_LOOP = CHAIN.Count > 5 && CHAIN[0].isNear(CHAIN[CHAIN.Count - 1]);
    }

    public void InputDown()
    {
        if (!gameplay.isPlaying || gameplay.isPause) return;

        if (CHAIN.Count == 0) ChainStart();
    }
    public void InputUp()
    {
        if (!gameplay.isPlaying || gameplay.isPause) return;

        if (CHAIN.Count > 0) CHAIN[CHAIN.Count - 1].ChainEnd();
    }
    public void InputEnter()
    {
        if (!gameplay.isPlaying || gameplay.isPause) return;

        if (CHAIN.Count > 0 && (CHAIN[CHAIN.Count - 1].type == type || CHAIN[CHAIN.Count - 1] is CatJoker ||
            this is CatJoker) && isNear(CHAIN[CHAIN.Count - 1]))
        {
            if (!CHAIN.Contains(this))
                ChainAdd();
            else if (CHAIN.Count > 1 && this == CHAIN[CHAIN.Count - 2])
                CHAIN[CHAIN.Count - 1].ChainRemove();
        }
    }

    bool isNear(Stuff stuff)
    {
        float radius = balance.maxDistanceToNextCat * achievements.increaseComboDistance * (smallScreen ? 1.1f : 1f);
        radius *= radius;
        return DistanceTo(stuff) < radius;
    }
}