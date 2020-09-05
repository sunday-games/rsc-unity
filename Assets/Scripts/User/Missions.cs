using UnityEngine;
using System;

public class Missions : Core
{
    public static int MAX_LEVEL;
    public static Level[] LEVELS;
    public static void UNLOCK(int level)
    {
        for (int i = 2; i <= level; i++)
            if (LEVELS[i - 1].unlock != null) LEVELS[i - 1].unlock();
    }


    public static int maxMultiplier = 1;
    public static int maxCatSlot = 0;
    public static bool isChampionship = false;
    public static bool isGoldfishes = false;
    public static bool isTournament = false;
    public static bool isLuckyWheel = false;
    public static bool isFever = false;

    public static bool isFireworkBoomSmall = false;
    public static bool isFireworkBoomBig = false;
    public static bool isFireworkRocket = false;
    public static bool isFireworkColor = false;

    public static bool isBoostMultiplier = false;
    public static bool isBoostExperience = false;
    public static bool isBoostTime = false;
    public static bool isBoostFirework = false;
    public static bool isBoosts { get { return isBoostMultiplier || isBoostExperience || isBoostTime || isBoostFirework; } }

    static int _сhainSquence = 0;
    static bool isChainSquenceInGame;
    static bool isMultiplierUsed;
    static bool isLimeUsed;
    static bool isGingerUsed;

    public static void LockAll()
    {
        maxMultiplier = 1;
        maxCatSlot = 0;
        isChampionship = false;
        isGoldfishes = false;
        isTournament = false;
        isLuckyWheel = false;
        isFever = false;

        isFireworkBoomSmall = false;
        isFireworkBoomBig = false;
        isFireworkRocket = false;
        isFireworkColor = false;

        isBoostMultiplier = false;
        isBoostExperience = false;
        isBoostTime = false;
        isBoostFirework = false;
    }

    public static void UnlockAll()
    {
        maxMultiplier = 9;
        maxCatSlot = 4;
        isChampionship = true;
        isGoldfishes = true;
        isTournament = true;
        isLuckyWheel = true;
        isFever = true;

        isFireworkBoomSmall = true;
        isFireworkBoomBig = true;
        isFireworkRocket = true;
        isFireworkColor = true;

        isBoostMultiplier = true;
        isBoostExperience = true;
        isBoostTime = true;
        isBoostFirework = true;
    }

    public static bool IsMissionActive<T>()
    {
        if (!user.isLevelOK) return false;

        foreach (Mission mission in LEVELS[user.level].missions)
            if (mission is T) return true;

        return false;
    }

    public static void AtOneGameMissionsClear()
    {
        if (user.isLevelOK)
            foreach (Mission mission in Missions.LEVELS[user.level].missions)
                if (!mission.isDone && mission.atOneGame && mission.clear != null) mission.clear();
    }

    #region EVENTS
    public static void OnGameStart()
    {
        _сhainSquence = 0;
        isMultiplierUsed = false;
        isLimeUsed = false;
        isGingerUsed = false;
    }

    public static void OnGameEnd()
    {
        user.gameSessions++;
    }

    public static void OnUseFirework(Firework firework)
    {
        if (IsMissionActive<UseFireworkBoomSmall>() && firework is FireworkBoom && (firework as FireworkBoom).radius < 3)
            user.useFireworkBoomSmall++;
        else if (IsMissionActive<UseFireworkBoomBig>() && firework is FireworkBoom && (firework as FireworkBoom).radius > 3)
            user.useFireworkBoomBig++;
        else if (IsMissionActive<UseFireworkRocket>() && firework is FireworkRocket)
            user.useFireworkRocket++;
        else if (IsMissionActive<UseFireworkColor>() && firework is FireworkColor)
            user.useFireworkColor++;

        if (IsMissionActive<UseFireworksAtOneGame>())
            user.useFireworkAtOneGame++;
    }

    public static void OnUseBoost(Boost boost)
    {
        if (IsMissionActive<UseBoost>()) user.useBoosts++;
    }

    public static void OnUseSausage()
    {
        if (IsMissionActive<UseSausage>()) user.useSausages++;

        user.useSausagesHistory++;
    }

    public static void OnGetScore(int currentScore)
    {
        if (IsMissionActive<GetScore>() && user.recordStat < currentScore)
            user.recordStat = currentScore;

        if (IsMissionActive<GetScoreWithoutMultipliers>() && !isMultiplierUsed && user.recordWithoutMultipliers < currentScore)
            user.recordWithoutMultipliers = currentScore;

        if (IsMissionActive<GetScoreWithoutLime>() && !isLimeUsed && user.recordWithoutLime < currentScore)
            user.recordWithoutLime = currentScore;

        if (IsMissionActive<GetScoreWithoutGinger>() && !isGingerUsed && user.recordWithoutGinger < currentScore)
            user.recordWithoutGinger = currentScore;
    }

    public static void OnGetGoldfishes(int count)
    {
        if (IsMissionActive<GetGoldfishes>())
            user.getGoldfishes += count;

        if (IsMissionActive<GetGoldfishesAtOneGame>())
            user.getGoldfishesAtOneGame += count;
    }

    public static void OnChainDone(int count, CatType type, bool isLoop)
    {
        OnGetCats(count);

        if (count > user.chainLengthHistory)
        {
            user.chainLengthHistory = count;

            achievements.OnChainDone();
        }

        if (type == CatType.GetCatType(Cats.Lime))
            isLimeUsed = true;

        if (type == CatType.GetCatType(Cats.Ginger))
            isGingerUsed = true;

        if (IsMissionActive<ChainLength>() && count > user.chainLength)
            user.chainLength = count;

        if (IsMissionActive<LoopLength>() && isLoop && count > user.loopLength)
            user.loopLength = count;

        if (IsMissionActive<ChainSequence>())
        {
            isChainSquenceInGame = count >= 6;
            if (isChainSquenceInGame) _сhainSquence++;
            if (_сhainSquence > user.chainSequence) user.chainSequence = _сhainSquence;
            if (!isChainSquenceInGame) _сhainSquence = 0;
        }

        if (IsMissionActive<GetLoops>() && isLoop)
            user.loops++;

        if (IsMissionActive<GetLoopsAtOneGame>() && isLoop)
            user.loopsAtOneGame++;

        // if (!isLoop && UnityEngine.Random.value < 0.1f) Analytic.Event("Game", "Combo", count.ToString());
        if (isLoop) Analytic.EventProperties("Game", "ComboLoop", count.ToString());
    }

    public static void OnGetCats(int count)
    {
        if (IsMissionActive<GetCats>())
            user.getCats += count;

        if (IsMissionActive<GetCatsAtOneGame>())
            user.getCatsAtOneGame += count;

        user.getCatsHistory += count;
    }

    public static void OnGetFever()
    {
        if (IsMissionActive<GetFever>())
            ++user.getFever;

        if (IsMissionActive<GetFeverAtOneGame>())
            ++user.getFeverAtOneGame;
    }

    public static void OnGetMultiplier(int multiplier)
    {
        isMultiplierUsed = true;

        if (IsMissionActive<GetMultiplier>() && multiplier > user.getMultiplier)
            user.getMultiplier = multiplier;
    }

    public static void OnUseCats(CatType catType)
    {
        if (IsMissionActive<UseCats>())
            ++user.useCats;

        if (IsMissionActive<UseCatsAtOneGame>())
            ++user.useCatsAtOneGame;
    }
    #endregion

    ///////////////////////////////////////////////////////////
    // Если что то меняешь, не забудь проверить этот хардкод //
    ///////////////////////////////////////////////////////////
    public static int[] BOX_UNLOCK_LEVELS = new int[4] { 4, 13, 24, 36 };
    public static int COMBO_LOOP_LEVEL = 10;
    public static int FIREWORK_BOOM_SMALL_LEVEL = 11;
    public static int FIREWORK_ROCKET_LEVEL = 18;
    public static int FIREWORK_BOOM_BIG_LEVEL = 28;
    public static int FIREWORK_COLOR_LEVEL = 39;
    public static int MULTIPLIER_X5_LEVEL = 21;

    public static void Init()
    {
        MAX_LEVEL = 100;
        LEVELS = new Level[MAX_LEVEL];
        for (int i = 0; i < MAX_LEVEL; i++) LEVELS[i] = new Level();

        if (isRiki) SetupMissions();
        else SetupMissionsWithBonusBox();
    }

    static void SetupMissionsWithBonusBox()
    {
        Mission[] missionList = new Mission[]
        {
new GameSessions (1),
new GameSessions (1),
new GameSessions (1),
// 1 - Missions
new GetScore (5000),
new ChainLength (6),
new GetCats (30),
// 2 - Championship
new ChainSequence (2),
new GameSessions (3),
new GetCatsAtOneGame (30),
// 3 - Goldfishes
new GetGoldfishes (100), // 3
new GetCats (90),
new GetScore (8000),
// 4 - Catbox + CatSlot 1
new ChainLength (8),
new UseCats (3), // 4 - Tutorial.Part.CatUse, Tutorial.Part.CatBox
new GameSessions (8),
// 5 - Multiplier x2
new GetMultiplier (2), // 5 
new GetGoldfishesAtOneGame (50),
new GetScoreWithoutLime (7000),
// 6 - AquariumSmall
new GetCatsAtOneGame (60),
new ChainSequence (3),
new GetScore (20000),
// 7 - LuckyWheel
new UseCatsAtOneGame (2),
new UseSausage (1),
new GetScoreWithoutMultipliers (9000),
// 8 - Tournament
new GetCats (250),
new ChainLength (10),
new GetGoldfishes (250),
// 9 - Multiplier x3
new GetMultiplier (3), // 9
new LevelUpCat (2),
new GetScore (30000),
// 10 - Catbox
new LoopLength (6), // 10 - Tutorial.Part.GameComboLoop
new GetGoldfishesAtOneGame (80),
new GameSessions (20),
// 11 - FireworkBoomSmall
new ChainSequence (4),
new GetLoops (5),
new UseFireworkBoomSmall (5), // 11 - Tutorial.Part.FireworkBoomSmall
// 12 - Sausage
new GetScoreWithoutGinger (15000),
new UseCats (10),
new UseFireworksAtOneGame (4),
// 13 - CatSlot 2
new GetGoldfishes (500),
new GetCatsAtOneGame (90),
new ChainLength (12),
// 14 - Multiplier x4
new GetMultiplier (4), // 14
new GetCats (500),
new GetLoopsAtOneGame (2),
// 15 - Catbox
new GetScore (70000),
new LevelUpCat (3),
new UseFireworkBoomSmall (20),
// 16 - Fever
new GetGoldfishesAtOneGame (200),
new GetScoreWithoutMultipliers (20000),
new GetFever (3), // 16

new LoopLength (8),
new GetFeverAtOneGame (2),
new ChainSequence (5),

new GetLoops (10),
new UseCatsAtOneGame (4),
new UseFireworkRocket (10), // 18

new GetGoldfishes (1000),
new ChainLength (14),
new GetCatsAtOneGame (150),

new UseCats (20),
new GetScore (150000),
new GetCats (750),
// 21
new GetMultiplier (5), // 21
new UseFireworksAtOneGame (6),
new GameSessions (50),

new GetFever (10),
new GetLoopsAtOneGame (3),
new GetScoreWithoutLime (70000),

new GetGoldfishesAtOneGame (250),
new LoopLength (10),
new LevelUpCat (4),

new GetFeverAtOneGame (3),
new UseFireworkRocket (20),
new ChainLength (16),

new UseCatsAtOneGame (6),
new GetLoops (20),
new ChainSequence (6),
// 26
new GetScore (300000),
new GetCatsAtOneGame (200),
new GetCats (1000),

new GetFever (20),
new GetGoldfishes (2000),
new GetScoreWithoutGinger (100000),

new GetLoopsAtOneGame (4),
new UseFireworkBoomBig (10), // 28
new GameSessions (100),

new LoopLength (12),
new GetGoldfishesAtOneGame (300),
new UseCats (50),

new GetFeverAtOneGame (4),
new GetLoops (30),
new ChainLength (18),
// 31
new UseCatsAtOneGame (8),
new GetCats (1500),
new LevelUpCat (5),

new GetMultiplier (6), // 32
new GetCatsAtOneGame (250),
new GetScore (600000),

new GetFever (40),
new UseFireworksAtOneGame (8),
new ChainSequence (7),

new GetGoldfishes (3000),
new GetLoopsAtOneGame (5),
new UseCats (80),

new LoopLength (14),
new GetFeverAtOneGame (5),
new GameSessions (150),
// 36
new GetScoreWithoutMultipliers (80000),
new UseFireworkBoomBig (20),
new UseCatsAtOneGame (10),

new ChainLength (20),
new GetCats (2000),
new GetGoldfishesAtOneGame (400),

new GetLoops (40),
new GetScore (900000),
new LevelUpCat (6),

new UseFireworkColor (10), // 39
new GetCatsAtOneGame (350),
new GetFever (60),

new GetGoldfishes (4000),
new UseCats (120),
new ChainSequence (8),
// 41
new ChainLength (22),
new GameSessions (200),
new GetGoldfishesAtOneGame (500),

new GetCats (3000),
new LoopLength (16),
new UseFireworksAtOneGame (10),

new GetScore (1200000),
new UseCatsAtOneGame (12),
new GetMultiplier (7), // 43

new LevelUpCat (7),
new GetLoopsAtOneGame (6),
new GetGoldfishes (5000),

new UseCats (140),
new GetCatsAtOneGame (400),
new GetLoops (50),
// 46
new UseFireworkColor (20),
new GetFever (80),
new GetGoldfishesAtOneGame (600),

new ChainLength (24),
new GetMultiplier (8), // 47
new GameSessions (300),

new GetCats (4000),
new GetFeverAtOneGame (6),
new LevelUpCat (8),

new GetScore (2400000),
new UseFireworkBoomSmall (50),
new LoopLength (18),

new UseCatsAtOneGame (14),
new ChainSequence (9),
new GetScoreWithoutGinger (400000),

// 51
new GetMultiplier (9), // 51
new GetGoldfishes (6000),
new UseCats (160),

new GetCatsAtOneGame (450),
new GameSessions (400),
new ChainLength (26),

new UseFireworkRocket (30),
new GetLoops (60),
new GetScoreWithoutMultipliers (100000),

new GetFever (100),
new GetGoldfishesAtOneGame (700),
new GetCats (5000),

new LevelUpCat (9),
new UseCatsAtOneGame (16),
new LoopLength (20),

// 56
new GetScore (3500000),
new UseFireworksAtOneGame (12),
new GetGoldfishes (7000),

new GetFeverAtOneGame (7),
new UseCats (180),
new GameSessions (500),

new GetScoreWithoutLime (600000),
new ChainLength (28),
new GetCatsAtOneGame (500),

new LevelUpCat (10),
new UseFireworkBoomBig (30),
new GetFever (120),

new LoopLength (22),
new UseCatsAtOneGame (18),
new ChainSequence (10),
// 61
new GetCats (6000),
new GetScore (6000000),
new GetLoops (70),

new GetGoldfishesAtOneGame (800),
new UseFireworkColor (30),
new UseBoost (5),

new GetGoldfishes (8000),
new UseCats (200),
new LoopLength (24),

new UseSausage(3),
new LevelUpCat (11),
new UseCatsAtOneGame (20),

new GetScore (9000000),
new UseFireworksAtOneGame (14),
new GameSessions (600),
// 66
new GetCatsAtOneGame (600),
new ChainLength (30),
new ChainSequence (11),

new GetLoops(80),
new GetCats (7000),
new GetFeverAtOneGame (8),

new GetGoldfishes (9000),
new UseCats (220),
new UseFireworkRocket (40),

new GetScore (15000000),
new GetFever (140),
new LoopLength (26),

new GameSessions (700),
new UseCatsAtOneGame (22),
new UseSausage(5),
// 71
new LevelUpCat (12),
new UseFireworkColor (40),
new GetGoldfishesAtOneGame (900),

new GetLoops(90),
new GetScore (24000000),
new GetCats (8000),

new ChainSequence (12),
new GetGoldfishes (10000),
new UseCats (250),

new GetScoreWithoutGinger (800000),
new UseFireworksAtOneGame (16),
new GetFever (160),

new GetScoreWithoutMultipliers (200000),
new GameSessions (800),
new ChainLength (32),
// 76
new UseBoost (10),
new GetCatsAtOneGame (700),
new UseCatsAtOneGame (24),

new GetLoops(100),
new UseFireworkBoomSmall (100),
new LoopLength (28),

new UseCats (300),
new GetScore (36000000),
new GetGoldfishes (12000),

new GetCats (9000),
new GetFever (180),
new ChainSequence (13),

new GetScoreWithoutLime (1000000),
new UseFireworkBoomBig (60),
new GameSessions (900),
// 81
new UseBoost (20),
new GetGoldfishesAtOneGame (1000),
new GetLoops(110),

new LevelUpCat (13),
new GetFeverAtOneGame (9),
new GetScore (48000000),

new GetGoldfishes (14000),
new UseCats (350),
new ChainLength (34),

new UseFireworksAtOneGame (18),
new GetCats (10000),
new UseSausage(8),

new GameSessions (1000),
new ChainSequence (14),
new GetFever (200),
// 86
new LoopLength (30),
new UseCatsAtOneGame (26),
new GetCatsAtOneGame (800),

new GetLoops(120),
new UseFireworkRocket (50),
new GetScore (72000000),

new GetCats (11000),
new GetGoldfishes (16000),
new UseCats (400),

new ChainLength (36),
new ChainSequence (15),
new GetScoreWithoutLime (1200000),

new UseBoost (30),
new GameSessions (1500),
new UseFireworkColor (50),
// 91
new LevelUpCat (14),
new GetFever (220),
new LoopLength (32),

new GetCats (12000),
new GetGoldfishes (18000),
new GetScore (120000000),

new UseCats (450),
new ChainLength (38),
new UseCatsAtOneGame (28),

new UseFireworksAtOneGame (20),
new ChainSequence (16),
new GetLoops(130),

new GetScoreWithoutGinger (1500000),
new GetGoldfishesAtOneGame (1200),
new GameSessions (2000),
// 95
new GetCatsAtOneGame (900),
new LoopLength (34),
new GetFever (240),

new ChainLength (40),
new GetScore (240000000),
new UseFireworkBoomBig(80),

new ChainSequence (17),
new UseCats (500),
new GetCats (13000),

new GetGoldfishes (20000),
new UseBoost (40),
new GetFeverAtOneGame (10),

new GameSessions (3000),
new UseFireworkBoomSmall (200),
new GetScoreWithoutMultipliers (500000),
        };

        int j = 0;
        for (int i = 0; i < MAX_LEVEL; i++)
            LEVELS[i].missions = new Mission[3] { missionList[j++], missionList[j++], missionList[j++] };

        int n = 0;
        AddGift(n, Gifts.aquariumSmall);
        // 1
        AddGift(++n, () => isChampionship = true, pics.championship, () => Localization.Get("unlockChampionship"));
        AddGift(++n, () => isGoldfishes = true, pics.goldfish, () => Localization.Get("unlockGoldfishes"));
        AddGift(++n, Gifts.catbox, 1, () => maxCatSlot = 1);
        AddGift(++n, () => maxMultiplier = 2, pics.multiplier, () => Localization.Get("unlockMultiplier", 2));
        AddGift(++n, Gifts.aquariumSmall, 1);
        // 6        
        AddGift(++n, () => isLuckyWheel = true, pics.luckyWheel, () => Localization.Get("unlockLuckyWheel"));
        if (build.facebook) AddGift(++n, () => isTournament = true, pics.tournament, () => Localization.Get("unlockTournament"));
        else AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxMultiplier = 3, pics.multiplier, () => Localization.Get("unlockMultiplier", 3));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isFireworkBoomSmall = true, pics.fireworkBoomSmall, () => Localization.Get("unlockFireworkBoomSmall"));
        // 11
        AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxCatSlot = 2, pics.catSlot, () => Localization.Get("unlockCatSlot", 2));
        AddGift(++n, () => maxMultiplier = 4, pics.multiplier, () => Localization.Get("unlockMultiplier", 4));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isFever = true, pics.fever, () => Localization.Get("unlockFever"));
        // 16
        AddGift(++n, Gifts.aquariumSmall, 2);
        AddGift(++n, () => isFireworkRocket = true, pics.fireworkRocket, () => Localization.Get("unlockFireworkRocket"));
        AddGift(++n, () => isBoostTime = true, gameplay.boosts.time.sprite, () => Localization.Get("unlockBoostTime")); // AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxMultiplier = 5, pics.multiplier, () => Localization.Get("unlockMultiplier", 5));
        // 21
        AddGift(++n, Gifts.aquariumSmall, 3);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxCatSlot = 3, pics.catSlot, () => Localization.Get("unlockCatSlot", 3));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.aquariumSmall, 4);
        // 26
        AddGift(++n, () => isBoostExperience = true, gameplay.boosts.experience.sprite, () => Localization.Get("unlockBoostExperience")); //AddGift(++n, Gifts.sausage);
        AddGift(++n, () => isFireworkBoomBig = true, pics.fireworkBoomBig, () => Localization.Get("unlockFireworkBoomBig"));
        AddGift(++n, Gifts.aquariumSmall, 5);
        AddGift(++n, Gifts.catbox); // Тут сидит Царь
        AddGift(++n, Gifts.boosterbox);
        // 31
        AddGift(++n, () => maxMultiplier = 6, pics.multiplier, () => Localization.Get("unlockMultiplier", 6));
        AddGift(++n, () => isBoostMultiplier = true, gameplay.boosts.multiplier.sprite, () => Localization.Get("unlockBoostMultiplier")); //AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 6);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxCatSlot = 4, pics.catSlot, () => Localization.Get("unlockCatSlot", 4));
        // 36
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 7);
        AddGift(++n, () => isFireworkColor = true, pics.fireworkColor, () => Localization.Get("unlockFireworkColor"));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isBoostFirework = true, gameplay.boosts.firework.sprite, () => Localization.Get("unlockBoostFirework")); //AddGift(++n, Gifts.sausage);
        // 41
        AddGift(++n, Gifts.aquariumSmall, 8);
        AddGift(++n, () => maxMultiplier = 7, pics.multiplier, () => Localization.Get("unlockMultiplier", 7));
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.aquariumSmall, 9);
        // 46
        AddGift(++n, () => maxMultiplier = 8, pics.multiplier, () => Localization.Get("unlockMultiplier", 8));
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 10);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxMultiplier = 9, pics.multiplier, () => Localization.Get("unlockMultiplier", 9));
        // 51
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 11);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        // 56
        AddGift(++n, Gifts.aquariumSmall, 12);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 13);
        // 61
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 14);
        AddGift(++n, Gifts.boosterbox);
        // 66
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 15);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        // 71
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 16);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        // 76
        AddGift(++n, Gifts.aquariumSmall, 17);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 18);
        // 81
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 19);
        AddGift(++n, Gifts.boosterbox);
        // 86
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 20);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        // 91
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 21);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        // 96
        AddGift(++n, Gifts.aquariumSmall, 22);
        AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
    }

    static void SetupMissions()
    {
        Mission[] missionList = new Mission[]
        {
new GameSessions (1),
new GameSessions (1),
new GameSessions (1),
// 1 - Missions
new GetScore (5000),
new ChainLength (6),
new GetCats (30),
// 2 - Championship
new ChainSequence (2),
new GameSessions (3),
new GetCatsAtOneGame (30),
// 3 - Goldfishes
new GetGoldfishes (100), // 3
new GetCats (90),
new GetScore (8000),
// 4 - Catbox + CatSlot 1
new ChainLength (8),
new UseCats (3), // 4 - Tutorial.Part.CatUse, Tutorial.Part.CatBox
new GameSessions (8),
// 5 - Multiplier x2
new GetMultiplier (2), // 5 
new GetGoldfishesAtOneGame (50),
new GetScoreWithoutLime (7000),
// 6 - AquariumSmall
new GetCatsAtOneGame (60),
new ChainSequence (3),
new GetScore (20000),
// 7 - LuckyWheel
new UseCatsAtOneGame (2),
new UseSausage (1),
new GetScoreWithoutMultipliers (9000),
// 8 - Tournament
new GetCats (250),
new ChainLength (10),
new GetGoldfishes (250),
// 9 - Multiplier x3
new GetMultiplier (3), // 9
new LevelUpCat (2),
new GetScore (30000),
// 10 - Catbox
new LoopLength (6), // 10 - Tutorial.Part.GameComboLoop
new GetGoldfishesAtOneGame (80),
new GameSessions (20),
// 11 - FireworkBoomSmall
new ChainSequence (4),
new GetLoops (5),
new UseFireworkBoomSmall (5), // 11 - Tutorial.Part.FireworkBoomSmall
// 12 - Sausage
new GetScoreWithoutGinger (15000),
new UseCats (10),
new UseFireworksAtOneGame (4),
// 13 - CatSlot 2
new GetGoldfishes (500),
new GetCatsAtOneGame (90),
new ChainLength (12),
// 14 - Multiplier x4
new GetMultiplier (4), // 14
new GetCats (500),
new GetLoopsAtOneGame (2),
// 15 - Catbox
new GetScore (70000),
new LevelUpCat (3),
new UseFireworkBoomSmall (20),
// 16 - Fever
new GetGoldfishesAtOneGame (200),
new GetScoreWithoutMultipliers (20000),
new GetFever (3), // 16

new LoopLength (8),
new GetFeverAtOneGame (2),
new ChainSequence (5),

new GetLoops (10),
new UseCatsAtOneGame (4),
new UseFireworkRocket (10), // 18

new GetGoldfishes (1000),
new ChainLength (14),
new GetCatsAtOneGame (150),

new UseCats (20),
new GetScore (150000),
new GetCats (750),
// 21
new GetMultiplier (5), // 21
new UseFireworksAtOneGame (6),
new GameSessions (50),

new GetFever (10),
new GetLoopsAtOneGame (3),
new GetScoreWithoutLime (70000),

new GetGoldfishesAtOneGame (250),
new LoopLength (10),
new LevelUpCat (4),

new GetFeverAtOneGame (3),
new UseFireworkRocket (20),
new ChainLength (16),

new UseCatsAtOneGame (6),
new GetLoops (20),
new ChainSequence (6),
// 26
new GetScore (300000),
new GetCatsAtOneGame (200),
new GetCats (1000),

new GetFever (20),
new GetGoldfishes (2000),
new GetScoreWithoutGinger (100000),

new GetLoopsAtOneGame (4),
new UseFireworkBoomBig (10), // 28
new GameSessions (100),

new LoopLength (12),
new GetGoldfishesAtOneGame (300),
new UseCats (50),

new GetFeverAtOneGame (4),
new GetLoops (30),
new ChainLength (18),
// 31
new UseCatsAtOneGame (8),
new GetCats (1500),
new LevelUpCat (5),

new GetMultiplier (6), // 32
new GetCatsAtOneGame (250),
new GetScore (600000),

new GetFever (40),
new UseFireworksAtOneGame (8),
new ChainSequence (7),

new GetGoldfishes (3000),
new GetLoopsAtOneGame (5),
new UseCats (80),

new LoopLength (14),
new GetFeverAtOneGame (5),
new GameSessions (150),
// 36
new GetScoreWithoutMultipliers (80000),
new UseFireworkBoomBig (20),
new UseCatsAtOneGame (10),

new ChainLength (20),
new GetCats (2000),
new GetGoldfishesAtOneGame (400),

new GetLoops (40),
new GetScore (900000),
new LevelUpCat (6),

new UseFireworkColor (10), // 39
new GetCatsAtOneGame (350),
new GetFever (60),

new GetGoldfishes (4000),
new UseCats (120),
new ChainSequence (8),
// 41
new ChainLength (22),
new GameSessions (200),
new GetGoldfishesAtOneGame (500),

new GetCats (3000),
new LoopLength (16),
new UseFireworksAtOneGame (10),

new GetScore (1200000),
new UseCatsAtOneGame (12),
new GetMultiplier (7), // 43

new LevelUpCat (7),
new GetLoopsAtOneGame (6),
new GetGoldfishes (5000),

new UseCats (140),
new GetCatsAtOneGame (400),
new GetLoops (50),
// 46
new UseFireworkColor (20),
new GetFever (80),
new GetGoldfishesAtOneGame (600),

new ChainLength (24),
new GetMultiplier (8), // 47
new GameSessions (300),

new GetCats (4000),
new GetFeverAtOneGame (6),
new LevelUpCat (8),

new GetScore (2400000),
new UseFireworkBoomSmall (50),
new LoopLength (18),

new UseCatsAtOneGame (14),
new ChainSequence (9),
new GetScoreWithoutGinger (400000),

// 51
new GetMultiplier (9), // 51
new GetGoldfishes (6000),
new UseCats (160),

new GetCatsAtOneGame (450),
new GameSessions (400),
new ChainLength (26),

new UseFireworkRocket (30),
new GetLoops (60),
new GetScoreWithoutMultipliers (100000),

new GetFever (100),
new GetGoldfishesAtOneGame (700),
new GetCats (5000),

new LevelUpCat (9),
new UseCatsAtOneGame (16),
new LoopLength (20),

// 56
new GetScore (3500000),
new UseFireworksAtOneGame (12),
new GetGoldfishes (7000),

new GetFeverAtOneGame (7),
new UseCats (180),
new GameSessions (500),

new GetScoreWithoutLime (600000),
new ChainLength (28),
new GetCatsAtOneGame (500),

new LevelUpCat (10),
new UseFireworkBoomBig (30),
new GetFever (120),

new LoopLength (22),
new UseCatsAtOneGame (18),
new ChainSequence (10),
// 61
new GetCats (6000),
new GetScore (6000000),
new GetLoops (70),

new GetGoldfishesAtOneGame (800),
new UseFireworkColor (30),
new UseBoost (5),

new GetGoldfishes (8000),
new UseCats (200),
new LoopLength (24),

new UseSausage(3),
new LevelUpCat (11),
new UseCatsAtOneGame (20),

new GetScore (9000000),
new UseFireworksAtOneGame (14),
new GameSessions (600),
// 66
new GetCatsAtOneGame (600),
new ChainLength (30),
new ChainSequence (11),

new GetLoops(80),
new GetCats (7000),
new GetFeverAtOneGame (8),

new GetGoldfishes (9000),
new UseCats (220),
new UseFireworkRocket (40),

new GetScore (15000000),
new GetFever (140),
new LoopLength (26),

new GameSessions (700),
new UseCatsAtOneGame (22),
new UseSausage(5),
// 71
new LevelUpCat (12),
new UseFireworkColor (40),
new GetGoldfishesAtOneGame (900),

new GetLoops(90),
new GetScore (24000000),
new GetCats (8000),

new ChainSequence (12),
new GetGoldfishes (10000),
new UseCats (250),

new GetScoreWithoutGinger (800000),
new UseFireworksAtOneGame (16),
new GetFever (160),

new GetScoreWithoutMultipliers (200000),
new GameSessions (800),
new ChainLength (32),
// 76
new UseBoost (10),
new GetCatsAtOneGame (700),
new UseCatsAtOneGame (24),

new GetLoops(100),
new UseFireworkBoomSmall (100),
new LoopLength (28),

new UseCats (300),
new GetScore (36000000),
new GetGoldfishes (12000),

new GetCats (9000),
new GetFever (180),
new ChainSequence (13),

new GetScoreWithoutLime (1000000),
new UseFireworkBoomBig (60),
new GameSessions (900),
// 81
new UseBoost (20),
new GetGoldfishesAtOneGame (1000),
new GetLoops(110),

new LevelUpCat (13),
new GetFeverAtOneGame (9),
new GetScore (48000000),

new GetGoldfishes (14000),
new UseCats (350),
new ChainLength (34),

new UseFireworksAtOneGame (18),
new GetCats (10000),
new UseSausage(8),

new GameSessions (1000),
new ChainSequence (14),
new GetFever (200),
// 86
new LoopLength (30),
new UseCatsAtOneGame (26),
new GetCatsAtOneGame (800),

new GetLoops(120),
new UseFireworkRocket (50),
new GetScore (72000000),

new GetCats (11000),
new GetGoldfishes (16000),
new UseCats (400),

new ChainLength (36),
new ChainSequence (15),
new GetScoreWithoutLime (1200000),

new UseBoost (30),
new GameSessions (1500),
new UseFireworkColor (50),
// 91
new LevelUpCat (14),
new GetFever (220),
new LoopLength (32),

new GetCats (12000),
new GetGoldfishes (18000),
new GetScore (120000000),

new UseCats (450),
new ChainLength (38),
new UseCatsAtOneGame (28),

new UseFireworksAtOneGame (20),
new ChainSequence (16),
new GetLoops(130),

new GetScoreWithoutGinger (1500000),
new GetGoldfishesAtOneGame (1200),
new GameSessions (2000),
// 95
new GetCatsAtOneGame (900),
new LoopLength (34),
new GetFever (240),

new ChainLength (40),
new GetScore (240000000),
new UseFireworkBoomBig(80),

new ChainSequence (17),
new UseCats (500),
new GetCats (13000),

new GetGoldfishes (20000),
new UseBoost (40),
new GetFeverAtOneGame (10),

new GameSessions (3000),
new UseFireworkBoomSmall (200),
new GetScoreWithoutMultipliers (500000),
        };

        int j = 0;
        for (int i = 0; i < MAX_LEVEL; i++)
            LEVELS[i].missions = new Mission[3] { missionList[j++], missionList[j++], missionList[j++] };

        int n = 0;
        AddGift(n, Gifts.aquariumSmall);
        // 1
        AddGift(++n, () => isChampionship = true, pics.championship, () => Localization.Get("unlockChampionship"));
        AddGift(++n, () => isGoldfishes = true, pics.goldfish, () => Localization.Get("unlockGoldfishes"));
        AddGift(++n, Gifts.catbox, 1, () => maxCatSlot = 1);
        AddGift(++n, () => maxMultiplier = 2, pics.multiplier, () => Localization.Get("unlockMultiplier", 2));
        AddGift(++n, Gifts.aquariumSmall, 1);
        // 6        
        AddGift(++n, () => isLuckyWheel = true, pics.luckyWheel, () => Localization.Get("unlockLuckyWheel"));
        if (build.facebook) AddGift(++n, () => isTournament = true, pics.tournament, () => Localization.Get("unlockTournament"));
        else AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxMultiplier = 3, pics.multiplier, () => Localization.Get("unlockMultiplier", 3));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isFireworkBoomSmall = true, pics.fireworkBoomSmall, () => Localization.Get("unlockFireworkBoomSmall"));
        // 11
        AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxCatSlot = 2, pics.catSlot, () => Localization.Get("unlockCatSlot", 2));
        AddGift(++n, () => maxMultiplier = 4, pics.multiplier, () => Localization.Get("unlockMultiplier", 4));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isFever = true, pics.fever, () => Localization.Get("unlockFever"));
        // 16
        AddGift(++n, Gifts.aquariumSmall, 2);
        AddGift(++n, () => isFireworkRocket = true, pics.fireworkRocket, () => Localization.Get("unlockFireworkRocket"));
        AddGift(++n, () => isBoostTime = true, gameplay.boosts.time.sprite, () => Localization.Get("unlockBoostTime")); // AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxMultiplier = 5, pics.multiplier, () => Localization.Get("unlockMultiplier", 5));
        // 21
        AddGift(++n, Gifts.aquariumSmall, 3);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, () => maxCatSlot = 3, pics.catSlot, () => Localization.Get("unlockCatSlot", 3));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.aquariumSmall, 4);
        // 26
        AddGift(++n, () => isBoostExperience = true, gameplay.boosts.experience.sprite, () => Localization.Get("unlockBoostExperience")); //AddGift(++n, Gifts.sausage);
        AddGift(++n, () => isFireworkBoomBig = true, pics.fireworkBoomBig, () => Localization.Get("unlockFireworkBoomBig"));
        AddGift(++n, Gifts.aquariumSmall, 5);
        AddGift(++n, Gifts.catbox); // Тут сидит Царь
        AddGift(++n, Gifts.sausage); //AddGift(++n, Gifts.boosterbox);
        // 31
        AddGift(++n, () => maxMultiplier = 6, pics.multiplier, () => Localization.Get("unlockMultiplier", 6));
        AddGift(++n, () => isBoostMultiplier = true, gameplay.boosts.multiplier.sprite, () => Localization.Get("unlockBoostMultiplier")); //AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 6);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxCatSlot = 4, pics.catSlot, () => Localization.Get("unlockCatSlot", 4));
        // 36
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 7);
        AddGift(++n, () => isFireworkColor = true, pics.fireworkColor, () => Localization.Get("unlockFireworkColor"));
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => isBoostFirework = true, gameplay.boosts.firework.sprite, () => Localization.Get("unlockBoostFirework")); //AddGift(++n, Gifts.sausage);
        // 41
        AddGift(++n, Gifts.aquariumSmall, 8);
        AddGift(++n, () => maxMultiplier = 7, pics.multiplier, () => Localization.Get("unlockMultiplier", 7));
        AddGift(++n, Gifts.sausage); //AddGift(++n, Gifts.boosterbox);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.aquariumSmall, 9);
        // 46
        AddGift(++n, () => maxMultiplier = 8, pics.multiplier, () => Localization.Get("unlockMultiplier", 8));
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 10);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, () => maxMultiplier = 9, pics.multiplier, () => Localization.Get("unlockMultiplier", 9));
        // 51
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 11);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 12);

        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 13);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);

        AddGift(++n, Gifts.aquariumSmall, 14);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 15);
        AddGift(++n, Gifts.catbox);

        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 16);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 17);

        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 18);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);

        AddGift(++n, Gifts.aquariumSmall, 19);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 20);
        AddGift(++n, Gifts.catbox);

        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 21);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 22);

        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 23);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);

        AddGift(++n, Gifts.aquariumSmall, 24);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 25);
        AddGift(++n, Gifts.catbox);

        AddGift(++n, Gifts.sausage);
        AddGift(++n, Gifts.aquariumSmall, 26);
        AddGift(++n, Gifts.catbox);
        AddGift(++n, Gifts.sausage);

        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 11);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //// 56
        //AddGift(++n, Gifts.aquariumSmall, 12);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 13);
        //// 61
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 14);
        //AddGift(++n, Gifts.boosterbox);
        //// 66
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 15);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //// 71
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 16);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //// 76
        //AddGift(++n, Gifts.aquariumSmall, 17);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 18);
        //// 81
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 19);
        //AddGift(++n, Gifts.boosterbox);
        //// 86
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 20);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //// 91
        //AddGift(++n, Gifts.sausage);
        //AddGift(++n, Gifts.aquariumSmall, 21);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
        //// 96
        //AddGift(++n, Gifts.aquariumSmall, 22);
        //AddGift(++n, Gifts.boosterbox);
        //AddGift(++n, Gifts.catbox);
        //AddGift(++n, Gifts.sausage);
    }

    public static void AddGift(int n, Gifts gift, int count = 1, Action unlock = null)
    {
        LEVELS[n].gift = gift;
        LEVELS[n].unlock = unlock;

        if (gift == Gifts.catbox)
        {
            LEVELS[n].giftSprite = pics.catbox;
            LEVELS[n].giftDescription = () => Localization.Get(gift.ToString());
        }
        else if (gift == Gifts.sausage)
        {
            LEVELS[n].giftSprite = pics.sausage;
            LEVELS[n].giftCount = count * balance.reward.sausageForLevelUp;
            LEVELS[n].giftDescription = () => Localization.Get(gift.ToString());
        }
        else if (gift == Gifts.aquariumSmall)
        {
            LEVELS[n].giftSprite = pics.aquariumSmall;
            LEVELS[n].giftCount = count * balance.reward.coinsForLevelUp;
            LEVELS[n].giftDescription = () => Localization.Get(gift.ToString(), LEVELS[n].giftCount);
        }
        else if (gift == Gifts.boosterbox)
        {
            LEVELS[n].giftSprite = pics.boosterbox;
            LEVELS[n].giftDescription = () => Localization.Get(gift.ToString());
        }
    }
    public static void AddGift(int n, Action unlock, Sprite sprite, Func<string> description)
    {
        LEVELS[n].unlock = unlock;
        LEVELS[n].giftSprite = sprite;
        LEVELS[n].giftDescription = description;
    }
}

public enum Gifts { catbox, sausage, aquariumSmall, boosterbox, empty }

public class Level
{
    public Action unlock;
    public Gifts gift = Gifts.empty;
    public int giftCount;
    public Func<string> giftDescription;
    public Sprite giftSprite;
    public Mission[] missions = null;

    public bool isDone { get { return missions[0].isDone && missions[1].isDone && missions[2].isDone; } }

    public Level()
    {
    }
    public Level(Mission[] missions, Action unlock, Gifts gift)
    {
        this.missions = missions;
        this.unlock = unlock;
        this.gift = gift;
    }
}

public class Mission
{
    public string name { get { return this.GetType().Name; } }
    public Func<bool> check = null;
    public int target = 0;
    public Func<int> current = null;
    public Action clear = null;
    public Tutorial.Part tipTutorial = null;
    public bool isDone
    {
        get
        {
            if (check != null) return check();
            else return current() >= target;
        }
    }
    public bool isProgress { get { return target != 0; } }
    public string description
    {
        get
        {
            string key = "mission" + name;
            if (key == "missionInviteFriends" && !Core.user.isId) key = "missionInviteFriendsLogout";
            return target > 0 ? Localization.Get(key, target.SpaceFormat()) : Localization.Get(key);
        }
    }

    public bool[] status;
    public bool atOneGame = false;
}

public class ChainLength : Mission
{
    public ChainLength(int targetValue)
    {
        this.current = () => Core.user.chainLength;
        this.target = targetValue;
        this.clear = () => Core.user.chainLength = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.ChainLengthTip;
    }
}
public class ChainSequence : Mission
{
    public ChainSequence(int targetValue)
    {
        this.current = () => Core.user.chainSequence;
        this.target = targetValue;
        this.clear = () => Core.user.chainSequence = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.ChainSequenceTip;
    }
}
public class LoopLength : Mission
{
    public LoopLength(int targetValue)
    {
        this.current = () => Core.user.loopLength;
        this.target = targetValue;
        this.clear = () => Core.user.loopLength = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.LoopLengthTip;
    }
}
public class GetLoops : Mission
{
    public GetLoops(int targetValue)
    {
        this.current = () => Core.user.loops;
        this.target = targetValue;
        this.clear = () => Core.user.loops = 0;
        this.tipTutorial = Tutorial.Part.GetLoopsTip;
    }
}
public class GetLoopsAtOneGame : Mission
{
    public GetLoopsAtOneGame(int targetValue)
    {
        this.current = () => Core.user.loopsAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.loopsAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetLoopsTip;
    }
}
public class GetCats : Mission
{
    public GetCats(int targetValue)
    {
        this.current = () => Core.user.getCats;
        this.target = targetValue;
        this.clear = () => Core.user.getCats = 0;
        this.tipTutorial = Tutorial.Part.GetCatsTip;
    }
}
public class GetCatsAtOneGame : Mission
{
    public GetCatsAtOneGame(int targetValue)
    {
        this.current = () => Core.user.getCatsAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.getCatsAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetCatsTip;
    }
}
public class GetScore : Mission
{
    public GetScore(int targetValue)
    {
        this.current = () => Core.user.recordStat;
        this.target = targetValue;
        this.clear = () => Core.user.recordStat = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetScoreTip;
    }
}
public class GetScoreWithoutMultipliers : Mission
{
    public GetScoreWithoutMultipliers(int targetValue)
    {
        this.current = () => Core.user.recordWithoutMultipliers;
        this.target = targetValue;
        this.clear = () => Core.user.recordWithoutMultipliers = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetScoreWithoutMultipliersTip;
    }
}
public class GetScoreWithoutLime : Mission
{
    public GetScoreWithoutLime(int targetValue)
    {
        this.current = () => Core.user.recordWithoutLime;
        this.target = targetValue;
        this.clear = () => Core.user.recordWithoutLime = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetScoreWithoutLimeTip;
    }
}
public class GetScoreWithoutGinger : Mission
{
    public GetScoreWithoutGinger(int targetValue)
    {
        this.current = () => Core.user.recordWithoutGinger;
        this.target = targetValue;
        this.clear = () => Core.user.recordWithoutGinger = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetScoreWithoutGingerTip;
    }
}
public class GameSessions : Mission
{
    public GameSessions(int targetValue)
    {
        this.current = () => Core.user.gameSessions;
        this.target = targetValue;
        this.tipTutorial = Tutorial.Part.GameSessionsTip;
    }
}
public class GetFever : Mission
{
    public GetFever(int targetValue)
    {
        this.current = () => Core.user.getFever;
        this.target = targetValue;
        this.clear = () => Core.user.getFever = 0;
        this.tipTutorial = Tutorial.Part.GetFeverTip;
    }
}
public class GetFeverAtOneGame : Mission
{
    public GetFeverAtOneGame(int targetValue)
    {
        this.current = () => Core.user.getFeverAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.getFeverAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetFeverTip;
    }
}
public class GetGoldfishes : Mission
{
    public GetGoldfishes(int targetValue)
    {
        this.current = () => Core.user.getGoldfishes;
        this.target = targetValue;
        this.clear = () => Core.user.getGoldfishes = 0;
        this.tipTutorial = Tutorial.Part.GetGoldfishesTip;
    }
}
public class GetGoldfishesAtOneGame : Mission
{
    public GetGoldfishesAtOneGame(int targetValue)
    {
        this.current = () => Core.user.getGoldfishesAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.getGoldfishesAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetGoldfishesTip;
    }
}
public class GetMultiplier : Mission
{
    public GetMultiplier(int targetValue)
    {
        this.current = () => Core.user.getMultiplier;
        this.target = targetValue;
        this.clear = () => Core.user.getMultiplier = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.GetMultiplierTip;
    }
}
public class UseCats : Mission
{
    public UseCats(int targetValue)
    {
        this.current = () => Core.user.useCats;
        this.target = targetValue;
        this.clear = () => Core.user.useCats = 0;
        this.tipTutorial = Tutorial.Part.UseCatsTip;
    }
}
public class UseCatsAtOneGame : Mission
{
    public UseCatsAtOneGame(int targetValue)
    {
        this.current = () => Core.user.useCatsAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.useCatsAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.UseCatsTip;
    }
}
public class LevelUpCat : Mission
{
    public LevelUpCat(int targetValue)
    {
        this.current = () => Core.user.maxCatLevel;
        this.target = targetValue;
        this.tipTutorial = Tutorial.Part.LevelUpCatTip;
    }
}
public class UseFireworksAtOneGame : Mission
{
    public UseFireworksAtOneGame(int targetValue)
    {
        this.current = () => Core.user.useFireworkAtOneGame;
        this.target = targetValue;
        this.clear = () => Core.user.useFireworkAtOneGame = 0;
        this.atOneGame = true;
        this.tipTutorial = Tutorial.Part.UseFireworksAtOneGameTip;
    }
}
public class UseFireworkBoomSmall : Mission
{
    public UseFireworkBoomSmall(int targetValue)
    {
        this.current = () => Core.user.useFireworkBoomSmall;
        this.target = targetValue;
        this.clear = () => Core.user.useFireworkBoomSmall = 0;
        this.tipTutorial = Tutorial.Part.UseFireworkBoomSmallTip;
    }
}
public class UseFireworkBoomBig : Mission
{
    public UseFireworkBoomBig(int targetValue)
    {
        this.current = () => Core.user.useFireworkBoomBig;
        this.target = targetValue;
        this.clear = () => Core.user.useFireworkBoomBig = 0;
        this.tipTutorial = Tutorial.Part.UseFireworkBoomBigTip;
    }
}
public class UseFireworkRocket : Mission
{
    public UseFireworkRocket(int targetValue)
    {
        this.current = () => Core.user.useFireworkRocket;
        this.target = targetValue;
        this.clear = () => Core.user.useFireworkRocket = 0;
        this.tipTutorial = Tutorial.Part.UseFireworkRocketTip;
    }
}
public class UseFireworkColor : Mission
{
    public UseFireworkColor(int targetValue)
    {
        this.current = () => Core.user.useFireworkColor;
        this.target = targetValue;
        this.clear = () => Core.user.useFireworkColor = 0;
        this.tipTutorial = Tutorial.Part.UseFireworkColorTip;
    }
}
public class UseSausage : Mission
{
    public UseSausage(int targetValue)
    {
        this.current = () => Core.user.useSausages;
        this.target = targetValue;
        this.clear = () => Core.user.useSausages = 0;
        this.tipTutorial = Tutorial.Part.UseSausageTip;
    }
}
public class UseBoost : Mission
{
    public UseBoost(int targetValue)
    {
        this.current = () => Core.user.useBoosts;
        this.target = targetValue;
        this.clear = () => Core.user.useBoosts = 0;
        this.tipTutorial = Tutorial.Part.UseBoostTip;
    }
}

//public class WinTournament : Mission
//{
//    public WinTournament(int targetValue)
//    {
//        this.current = () => Core.user.maxWonFriends;
//        this.target = targetValue;
//        this.tipTutorial = Tutorial.Part.WinTournamentTip;
//    }
//}
//public class InviteFriends : Mission
//{
//    public InviteFriends(int targetValue)
//    {
//        this.current = () => Core.user.invitedFriends.Count;
//        this.target = targetValue;
//        this.tipTutorial = Tutorial.Part.InviteFriendsTip;
//    }
//}