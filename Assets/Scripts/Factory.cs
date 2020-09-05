using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SG;

public class Factory : Core
{
    public static List<Stuff> STUFF = new List<Stuff>(200);
    public static List<Stuff> LIVE_STUFF = new List<Stuff>(150);

    public static void ClearStuff()
    {
        var temp = STUFF.ToArray();

        foreach (Stuff stuff in temp)
        {
            if (stuff == null)
            {
                STUFF.Remove(stuff);
            }
            else if (!(stuff is CatBasic) || stuff is CatJoker)
            {
                Destroy(stuff.gameObject);
            }
            else if (!POOL_CATS.Contains(stuff as CatBasic))
            {
                (stuff as CatBasic).Reset();
            }
        }

        LIVE_STUFF.Clear();
    }

    Vector3 randomPosition
    { get { return new Vector3(Random.Range(ui.game.spawnPointLeft.position.x, ui.game.spawnPointRight.position.x), Random.Range(ui.game.spawnPointLeft.position.y, ui.game.spawnPointRight.position.y), 0); } }

    int LAST_ID;

    public void Init()
    {
        CreateCatsBasicToPool(110);
    }

    #region CAT
    public CatBasic catBasicPrefab;
    public GameObject catLevelUpFXPrefab;
    public static List<CatBasic> POOL_CATS = new List<CatBasic>(200);

    public void CreateCatBasic(CatType catType, bool isMultiplier)
    {
        CatBasic cat;

        if (POOL_CATS.Count > 0 && POOL_CATS[0] != null)
        {
            cat = POOL_CATS[0];
            POOL_CATS.Remove(cat);
            LIVE_STUFF.Add(cat);
            cat.gameObject.SetActive(true);
        }
        else
        {
            cat = Instantiate(catBasicPrefab, randomPosition, Quaternion.identity) as CatBasic;
            cat.t.SetParent(ui.game.stuffBack, true);
            cat.t.Rotate(0f, 0f, Random.Range(0f, 360f));
        }

        cat.type = catType;
        cat.name = "Cat " + cat.type.name + " " + ++LAST_ID;
        cat.Setup(isMultiplier);
    }
    public void RemoveCatBasic(CatBasic cat)
    {
        cat.t.SetParent(ui.game.stuffBack, true);
        cat.t.position = randomPosition;
        cat.gameObject.SetActive(false);

        if (!POOL_CATS.Contains(cat)) POOL_CATS.Add(cat);
    }

    public void CreateCatSuper(CatItem catItem, int count)
    {
        for (int i = 0; i < count; i++) CreateCatSuper(catItem);
    }
    public void CreateCatSuper(CatItem catItem)
    {
        Stuff cat;
        if (catItem.type == CatType.GetCatType(Cats.Joker))
        {
            cat = Instantiate(catItem.type.gamePrefab, randomPosition, Quaternion.identity) as CatJoker;
            (cat as CatJoker).item = catItem;
            (cat as CatJoker).type = catItem.type;
        }
        else
        {
            cat = Instantiate(catItem.type.gamePrefab, randomPosition, Quaternion.identity) as CatSuper;
            (cat as CatSuper).item = catItem;
            (cat as CatSuper).type = catItem.type;
        }

        cat.t.SetParent(ui.game.stuffFront, true);
        cat.t.Rotate(0, 0, Random.Range(0, 360));
        cat.name = "Cat " + catItem.type.name + " " + ++LAST_ID;

        cat.Setup();
    }

    public void CreateCatsBasic(int n)
    {
        StartCoroutine(CreatingCatsBasic(n));
    }
    IEnumerator CreatingCatsBasic(int n)
    {
        for (int i = 0; i < n; ++i)
        {
            if (gameplay.boosts.multiplier.ON && i == n / 2)
                CreateCatRandomBasic(isMultiplier: true);
            else
                CreateCatRandomBasic();
            yield return new WaitForEndOfFrame();
        }
    }
    public void CreateCatRandomBasic(bool isMultiplier = false)
    {
        CreateCatBasic(gameplay.basicCats[Random.Range(0, gameplay.basicCats.Length)], isMultiplier);
    }

    public void CreateCatsBasicToPool(int n)
    {
        for (int i = 0; i < n; ++i)
        {
            var cat = Instantiate(catBasicPrefab, randomPosition, Quaternion.identity) as CatBasic;
            cat.t.SetParent(ui.game.stuffBack, true);
            cat.t.Rotate(0f, 0f, Random.Range(0f, 360f));
            cat.gameObject.SetActive(false);

            POOL_CATS.Add(cat);
            LIVE_STUFF.Remove(cat);
        }
    }
    #endregion

    [Space(10)]

    #region FIREWORK
    public FireworkPrefabs fireworkPrefabs;
    [System.Serializable]
    public class FireworkPrefabs
    {
        public Firework boomSmall;
        public Firework rocket;
        public Firework boomBig;
        public Firework color;
    }

    public void CreateFirework(Firework prefab, Vector3 position)
    {
        var firework = Instantiate(prefab, position, Quaternion.identity) as Firework;
        firework.t.SetParent(ui.game.stuffFront, true);
        firework.name = "Firework " + ++LAST_ID;

        firework.t.localScale = halfScale;
        firework.t.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (firework is FireworkRocket) (firework as FireworkRocket).RotateToCenter(position);
    }
    public void CreateFireworkAndMove(Firework prefab, Vector3 from, Vector3 to)
    {
        var firework = Instantiate(prefab, from, Quaternion.identity) as Firework;
        firework.t.SetParent(ui.game.stuffFrontFront, true);
        firework.name = "Firework " + ++LAST_ID;

        firework.t.localScale = Vector3.zero;
        firework.t.DOScale(Vector3.one, 1.5f).SetEase(Ease.OutBack);

        if (firework is FireworkRocket) (firework as FireworkRocket).RotateToCenter(to);

        firework.Spread(to);
    }
    #endregion

    [Space(10)]

    #region SHOW TEXT
    public ShowText showTextPrefab;
    public static List<ShowText> POOL_SHOW_TEXTS = new List<ShowText>(20);
    public void CreateShowText(Vector2 position, int score, int chain = 0)
    {
        string msg = string.Empty;
        if (chain > 0 && !CatBasic.IS_LOOP) msg = Localization.Get("comboX", chain);
        else if (chain > 0 && CatBasic.IS_LOOP) msg = Localization.Get("comboLoopX", chain);

        if (POOL_SHOW_TEXTS.Count > 0)
        {
            POOL_SHOW_TEXTS[0].gameObject.SetActive(true);
            POOL_SHOW_TEXTS[0].Setup(position, score.SpaceFormat(), msg);
            POOL_SHOW_TEXTS.Remove(POOL_SHOW_TEXTS[0]);
        }
        else
        {
            ShowText showText = Instantiate(showTextPrefab) as ShowText;
            showText.name = "Show Text " + ++LAST_ID;
            showText.t = showText.transform as RectTransform;
            showText.t.SetParent(ui.showTextParent, false);
            showText.Setup(position, score.SpaceFormat(), msg);
        }
    }
    public void RemoveShowText(ShowText showText)
    {
        showText.gameObject.SetActive(false);
        POOL_SHOW_TEXTS.Add(showText);
    }
    #endregion

    [Space(10)]
    public Medal medalPrefab;
    public MedalSprites medalIcons;
    [System.Serializable]
    public class MedalSprites
    {
        public Sprite friends;
        public Sprite star;
        public Sprite timer;
        public Sprite sausage;
        public Sprite fish;
        public Sprite combo;
        public Sprite championship;
        public Sprite catLevelUp;
        public Sprite cat;
        public Sprite tournament;
    }

    public Sprite[] medalSprites;
    public Color[] medalColors;

    [Space(10)]
    public Pumpkin pumpkinPrefab;

    [Space(10)]
    public BlackHole blackHolePrefab;

    [Space(10)]
    public Lightning lightningPrefab;

    [Space(10)]
    public Mover moverMultiplier;

    [Space(10)]
    public TutorialBubble tutorialBubble;

    [Space(10)]
    public Sprites sprites;
    [System.Serializable]
    public class Sprites
    {
        public Sprite hat;
        public Sprite heart;
        public Sprite bat;
        public Sprite goldfish;
        public Sprite catbox;
        public Sprite sausage;
        public Sprite aquariumSmall;
        public Sprite catSlot;
        public Sprite championship;
        public Sprite tournament;
        public Sprite luckyWheel;
        public Sprite fever;
        public Sprite fireworkBoomSmall;
        public Sprite fireworkBoomBig;
        public Sprite fireworkRocket;
        public Sprite fireworkColor;
        public Sprite multiplier;
        public Sprite boosterbox;
        public Texture2D emptyUserPic;
        [Space(10)]
        public Sprite[] multipliers;
        [Space(10)]
        public Sprite[] crowns;
        public Color[] crownColors;
        [Space(10)]
        public Characters characters;
        [System.Serializable]
        public class Characters
        {
            public Sprite vasia;
            public Sprite bobilich;
            public Sprite liolia;
            public Sprite palna;
            public Sprite vasiaMirror;
            public Sprite bobilichMirror;
            public Sprite lioliaMirror;
            public Sprite palnaMirror;
            public Sprite gingerCat;
        }
        [Space(10)]
        public Buttons buttons;
        [System.Serializable]
        public class Buttons
        {
            public Sprite green;
            public Sprite orange;
        }
    }

    [Space(10)]
    public ExceptionView exceptionView;
}


