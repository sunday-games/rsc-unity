using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum TutorialView { Top, Bottom, Left, Right }
public class Tutorial : Core
{
    public RectTransform targetsParent;
    public Image blinds;
    [Space(10)]
    public GameObject gameBasics;
    [Space(10)]
    public GameObject gameComboLoop;
    [Space(10)]
    public GameObject gameCombo;
    public Text combo3;
    public Text combo6;
    public Text combo9;
    [Space(10)]
    public GameObject feverDemo;
    [Space(10)]
    public GameObject catUse;
    public CatSlot catUseSlot;
    [Space(10)]
    public GameObject missions;
    public Image giftImage;
    public Image multiplierImage;
    public Text multiplierText;
    [Space(10)]
    public GameObject skipMission;
    public GameObject priceSkipMission;
    public Text priceSkipMissionText;
    [Space(10)]
    public GameObject fireworkBoomSmall;
    public GameObject fireworkRocket;
    public GameObject fireworkBoomBig;
    public GameObject fireworkColor;
    [Space(10)]
    public GameObject catBox;

    Part part;
    List<Part> parts = new List<Part>();
    Dictionary<Transform, Transform> highlights = new Dictionary<Transform, Transform>();

    [HideInInspector]
    public TutorialBubble currentBubble;

    Transform[] targets;
    public void Show(List<Part> parts, Transform[] targets = null)
    {
        this.part = parts[0];
        parts.Remove(part);
        this.parts = parts;
        this.targets = targets;

        Show(part, targets);
    }

    public void Show(Part part, Transform[] targets = null, string param = null)
    {
        if (gameObject.activeSelf) return;

        gameObject.SetActive(true);

        ui.Block();

        user.TutorialShown(part);

        this.part = part;

        if (highlights.Count == 0)
        {
            if (targets != null)
            {
                foreach (Transform t in targets)
                    if (t != null && !highlights.ContainsKey(t))
                    {
                        highlights.Add(t, t.parent);
                        t.SetParent(ui.tutorial.targetsParent, true);
                    }
            }
            else if (part.targets != null)
            {
                foreach (Transform t in part.targets)
                    if (t != null)
                    {
                        highlights.Add(t, t.parent);
                        t.SetParent(ui.tutorial.targetsParent, true);
                    }
            }
        }

        if (part == Part.GameBasics || part == Part.ChainLengthTip || part == Part.ChainSequenceTip) gameBasics.SetActive(true);
        else if (part == Part.GameComboLoop || part == Part.LoopLengthTip || part == Part.GetLoopsTip) gameComboLoop.SetActive(true);
        else if (part == Part.Fever || part == Part.GetFeverTip) feverDemo.SetActive(true);
        else if (part == Part.FireworkBoomSmall || part == Part.UseFireworkBoomSmallTip) fireworkBoomSmall.SetActive(true);
        else if (part == Part.FireworkRocket || part == Part.UseFireworkRocketTip) fireworkRocket.SetActive(true);
        else if (part == Part.FireworkBoomBig || part == Part.FireworkBoomBig) fireworkBoomBig.SetActive(true);
        else if (part == Part.FireworkColor || part == Part.UseFireworkColorTip) fireworkColor.SetActive(true);
        else if (part == Part.CatBox) catBox.SetActive(true);
        else if (part == Part.BuySkipMission)
        {
            buySkipMission = false;
            skipMission.SetActive(true);
            priceSkipMission.SetActive(!build.premium);
            priceSkipMissionText.text = iapManager.skipMissions.priceLocalized;
        }
        else if (part == Part.GameCombo || part == Part.GetScoreTip)
        {
            gameCombo.SetActive(true);
            combo3.text = Localization.Get("comboX", 3);
            combo6.text = Localization.Get("comboX", 6);
            combo9.text = Localization.Get("comboX", 9);
        }
        else if (part == Part.CatUse || part == Part.UseCatsTip)
        {
            catUse.SetActive(true);
            catUseSlot.Init(ui.prepare.catSlots[0].catItem != null ? ui.prepare.catSlots[0].catItem : user.collection[0]);
        }
        else if ((part == Part.Missions || part == Part.Missions8) && Missions.LEVELS[user.level].giftSprite != null)
        {
            missions.SetActive(true);
            giftImage.sprite = Missions.LEVELS[user.level].giftSprite;

            if (Missions.LEVELS[user.level].giftSprite == pics.catbox && !user.isCanGetSimpleBox)
                giftImage.sprite = pics.aquariumSmall;

            if (giftImage.sprite == pics.multiplier)
            {
                if (isRiki)
                {
                    multiplierText.gameObject.SetActive(true);
                    multiplierText.text = "x" + (Missions.maxMultiplier + 1);
                }
                else
                {
                    multiplierImage.gameObject.SetActive(true);
                    multiplierImage.sprite = pics.multipliers[Missions.maxMultiplier - 2 + 1];
                }
            }
            else
            {
                if (isRiki) multiplierText.gameObject.SetActive(false);
                else multiplierImage.gameObject.SetActive(false);
            }
        }

        if (ui.current == ui.game) gameplay.isPause = true;

        currentBubble = Instantiate(factory.tutorialBubble) as TutorialBubble;
        currentBubble.transform.SetParent(transform, false);
        currentBubble.Show(part, param);

        blinds.gameObject.SetActive(true);
        blinds.DOColor(new Color(0f, 0f, 0f, part.background), 0.5f);
    }

    public void Hide()
    {
        gameBasics.SetActive(false);
        gameComboLoop.SetActive(false);
        gameCombo.SetActive(false);
        feverDemo.SetActive(false);
        catUse.SetActive(false);
        if (catUseSlot.catItem != null) catUseSlot.Clear();
        skipMission.SetActive(false);
        missions.SetActive(false);
        fireworkBoomSmall.SetActive(false);
        fireworkRocket.SetActive(false);
        fireworkBoomBig.SetActive(false);
        fireworkColor.SetActive(false);
        catBox.SetActive(false);

        if (parts.Count > 0)
        {
            if (part.targets != parts[0].targets)
            {
                foreach (var pair in highlights)
                    if (pair.Key != null && pair.Value != null) pair.Key.SetParent(pair.Value, true);
                highlights.Clear();
            }

            gameObject.SetActive(false);

            part = parts[0];
            parts.Remove(part);

            Show(part, targets);
        }
        else
        {
            blinds.DOKill();
            blinds.DOColor(Color.clear, 0.5f);
        }
    }

    public void TutorialBubbleEnd(TutorialBubble bubble)
    {
        Destroy(bubble.gameObject);

        if (currentBubble == bubble)
        {
            foreach (var pair in highlights)
                if (pair.Key != null && pair.Value != null) pair.Key.SetParent(pair.Value, true);
            highlights.Clear();

            ui.Unblock();

            gameObject.SetActive(false);

            blinds.gameObject.SetActive(false);

            if (ui.current == ui.game) gameplay.isPause = false;

            if (part == Part.BuySkipMission)
            {
                if (buySkipMission) ui.PopupShow(ui.levelUp);
                else ui.prepare.missionList.skipButton.SetActive(true);
            }
        }
    }

    bool buySkipMission = false;
    public void BuySkipMission()
    {
        if (build.premium)
        {
            user.LevelUp();
            buySkipMission = true;
            currentBubble.Hide();
            MissionList.lastSkipMission = DateTime.Now;
        }
        else
            iapManager.Purchase(iapManager.skipMissions,
                purchaseSuccess =>
                {
                    if (purchaseSuccess)
                    {
                        buySkipMission = true;
                        currentBubble.Hide();
                    }
                    else buySkipMission = false;
                }
            );
    }

    public class Part
    {
        public static Part
            GameBasics,
            GameCombo,
            GameClosedBox,
            Missions,
            PrepareBox,
            Tournament,
            TournamentTime,
            TournamentPrizeFund,
            TournamentInviteToCompite, TournamentBetterResults, TournamentInviteToMoreGoldfish,
            Championship,
            Multiplier, MultiplierX5,
            Goldfishes,
            LuckyWheel,
            Fever,
            Collection,
            CatLevelUp,
            CatBox,
            CatGoldfishes,
            CatUse,
            CatUseActivateSnow, CatUseActivateZen, CatUseActivateDisco, CatUseActivateBoom, CatUseActivateCap, CatUseActivateFlint,
            CatUseActivateKing, CatUseActivateMage, CatUseActivateLoki, CatUseActivateLady, CatUseActivateSanta, CatUseActivateJoker,
            CatUseActivateJack, CatUseActivateOrion, CatUseActivateMix, CatUseActivateRaiden,
            BuySausages, BuyCatbox,
            BuySkipMission,
            CatExperience,
            GameComboLoop,
            CollectionSanta, CollectionLady, CollectionJack, CollectionMix,
            FireworkBoomSmall, FireworkRocket, FireworkBoomBig, FireworkColor,
            Login,
            Medals,
            VideoRewarded,
            LeagueUp,
            BoostMultiplier, BoostFirework, BoostExperience, BoostTime,
            ChainLengthTip, ChainSequenceTip, GameSessionsTip, GetCatsTip, GetFeverTip, GetGoldfishesTip, GetLoopsTip,
            GetMultiplierTip, GetScoreTip, GetScoreWithoutGingerTip, GetScoreWithoutLimeTip, GetScoreWithoutMultipliersTip,
            InviteFriendsTip, InviteFriendsLogoutTip, LevelUpCatTip, LoopLengthTip, UseBoostTip, UseCatsTip,
            UseFireworkBoomBigTip, UseFireworkBoomSmallTip, UseFireworkColorTip, UseFireworkRocketTip,
            UseFireworksAtOneGameTip, UseSausageTip, WinTournamentTip;

        public static Part
            GameBasics1, GameBasics2, GameBasics3, GameBasics4, GameBasics5, GameBasics6,
            Missions1, Missions2, Missions3, Missions4, Missions5, Missions6, Missions7, Missions8,
            Championship1, Championship2,
            Collection1, Collection2,
            Goldfishes1,
            LuckyWheel1,
            LeagueUp1,
            BuyCatbox1,
            Fever1,
            Login1,
            Tournament1, Tournament2;

        static Part()
        {
            GameBasics = new Part("GameBasics");
            GameCombo = new Part("GameCombo");
            GameClosedBox = new Part("GameClosedBox", TutorialView.Bottom, -5.7f, fullscreenClosing: true);
            Missions = new Part("Missions", TutorialView.Top, 4.2f, fullscreenClosing: true);
            PrepareBox = new Part("PrepareBox", TutorialView.Bottom, -5.7f, fullscreenClosing: true);
            Tournament = new Part("Tournament", TutorialView.Bottom, -7.7f);
            TournamentTime = new Part("TournamentTime", TutorialView.Bottom, -2.7f, fullscreenClosing: true);
            TournamentPrizeFund = new Part("TournamentPrizeFund", TutorialView.Bottom, -2.7f, fullscreenClosing: true);
            TournamentInviteToCompite = new Part("TournamentInviteToCompite");
            TournamentBetterResults = new Part("TournamentBetterResults");
            TournamentInviteToMoreGoldfish = new Part("TournamentInviteToMoreGoldfish");
            Championship = new Part("Championship", TutorialView.Bottom, -7.7f);
            Multiplier = new Part("Multiplier", TutorialView.Bottom, -5.7f);
            MultiplierX5 = new Part("MultiplierX5", TutorialView.Bottom, -7.7f);
            Goldfishes = new Part("Goldfishes", TutorialView.Bottom, -7.7f);
            LuckyWheel = new Part("LuckyWheel");
            Fever = new Part("Fever");
            Collection = new Part("Collection", TutorialView.Top, 4f);
            CatBox = new Part("CatBox", TutorialView.Bottom, -4.7f);
            CatGoldfishes = new Part("CatGoldfishes", TutorialView.Bottom, -4.7f);
            CatUse = new Part("CatUse");

            CatUseActivateSnow = new Part("CatUseActivateSnow", TutorialView.Bottom, -6.7f);
            CatUseActivateZen = new Part("CatUseActivateZen", TutorialView.Bottom, -6.7f);
            CatUseActivateDisco = new Part("CatUseActivateDisco", TutorialView.Bottom, -6.7f);
            CatUseActivateBoom = new Part("CatUseActivateBoom", TutorialView.Bottom, -6.7f);
            CatUseActivateCap = new Part("CatUseActivateCap", TutorialView.Bottom, -6.7f);
            CatUseActivateFlint = new Part("CatUseActivateFlint", TutorialView.Bottom, -6.7f);
            CatUseActivateKing = new Part("CatUseActivateKing", TutorialView.Bottom, -6.7f);
            CatUseActivateMage = new Part("CatUseActivateMage", TutorialView.Bottom, -6.7f);
            CatUseActivateLoki = new Part("CatUseActivateLoki", TutorialView.Bottom, -6.7f);
            CatUseActivateLady = new Part("CatUseActivateLady", TutorialView.Bottom, -6.7f);
            CatUseActivateSanta = new Part("CatUseActivateSanta", TutorialView.Bottom, -6.7f);
            CatUseActivateJoker = new Part("CatUseActivateJoker", TutorialView.Bottom, -6.7f);
            CatUseActivateJack = new Part("CatUseActivateJack", TutorialView.Bottom, -6.7f);
            CatUseActivateOrion = new Part("CatUseActivateOrion", TutorialView.Bottom, -6.7f);
            CatUseActivateRaiden = new Part("CatUseActivateRaiden", TutorialView.Bottom, -6.7f);
            CatUseActivateMix = new Part("CatUseActivateMix", TutorialView.Bottom, -6.7f);

            BuySausages = new Part("BuySausages", TutorialView.Bottom, -4.7f);
            BuyCatbox = new Part("BuyCatbox", TutorialView.Bottom, -4.7f);
            BuySkipMission = new Part("BuySkipMission", TutorialView.Top, 2.7f);
            CatExperience = new Part("CatExperience", TutorialView.Bottom, -7.7f);
            CatLevelUp = new Part("CatLevelUp", TutorialView.Bottom, -7.7f);
            GameComboLoop = new Part("GameComboLoop");

            CollectionSanta = new Part("CollectionSanta", TutorialView.Top, 0f, fullscreenClosing: true);
            CollectionLady = new Part("CollectionLady", TutorialView.Bottom, -6f, fullscreenClosing: true);
            CollectionJack = new Part("CollectionJack", TutorialView.Bottom, -6f, fullscreenClosing: true);
            CollectionMix = new Part("CollectionMix", TutorialView.Bottom, -3f, fullscreenClosing: true);

            FireworkBoomSmall = new Part("FireworkBoomSmall");
            FireworkRocket = new Part("FireworkRocket");
            FireworkBoomBig = new Part("FireworkBoomBig");
            FireworkColor = new Part("FireworkColor");

            Login = new Part("Login", TutorialView.Bottom, -4.7f);
            Medals = new Part("Medals", TutorialView.Top, 3.7f);
            VideoRewarded = new Part("VideoRewarded", TutorialView.Bottom, -0.7f);
            LeagueUp = new Part("LeagueUp", TutorialView.Bottom, -7.5f);

            BoostMultiplier = new Part("BoostMultiplier", TutorialView.Top, 1f, fullscreenClosing: true);
            BoostFirework = new Part("BoostFirework", TutorialView.Top, 1f, fullscreenClosing: true);
            BoostExperience = new Part("BoostExperience", TutorialView.Bottom, -6f, fullscreenClosing: true);
            BoostTime = new Part("BoostTime", TutorialView.Bottom, -6f, fullscreenClosing: true);

            ChainLengthTip = new Part("ChainLengthTip", fullscreenClosing: true);
            ChainSequenceTip = new Part("ChainSequenceTip", fullscreenClosing: true);
            GameSessionsTip = new Part("GameSessionsTip", fullscreenClosing: true);
            GetCatsTip = new Part("GetCatsTip", fullscreenClosing: true);
            GetFeverTip = new Part("GetFeverTip", fullscreenClosing: true);
            GetGoldfishesTip = new Part("GetGoldfishesTip", fullscreenClosing: true);
            GetLoopsTip = new Part("GetLoopsTip", fullscreenClosing: true);
            GetMultiplierTip = new Part("GetMultiplierTip", fullscreenClosing: true);
            GetScoreTip = new Part("GetScoreTip", fullscreenClosing: true);
            GetScoreWithoutGingerTip = new Part("GetScoreWithoutGingerTip", fullscreenClosing: true);
            GetScoreWithoutLimeTip = new Part("GetScoreWithoutLimeTip", fullscreenClosing: true);
            GetScoreWithoutMultipliersTip = new Part("GetScoreWithoutMultipliersTip", fullscreenClosing: true);
            InviteFriendsTip = new Part("InviteFriendsTip", fullscreenClosing: true);
            InviteFriendsLogoutTip = new Part("InviteFriendsLogoutTip", fullscreenClosing: true);
            LevelUpCatTip = new Part("LevelUpCatTip", fullscreenClosing: true);
            LoopLengthTip = new Part("LoopLengthTip", fullscreenClosing: true);
            UseBoostTip = new Part("UseBoostTip", fullscreenClosing: true);
            UseCatsTip = new Part("UseCatsTip", fullscreenClosing: true);
            UseFireworkBoomBigTip = new Part("UseFireworkBoomBigTip", fullscreenClosing: true);
            UseFireworkBoomSmallTip = new Part("UseFireworkBoomSmallTip", fullscreenClosing: true);
            UseFireworkColorTip = new Part("UseFireworkColorTip", fullscreenClosing: true);
            UseFireworkRocketTip = new Part("UseFireworkRocketTip", fullscreenClosing: true);
            UseFireworksAtOneGameTip = new Part("UseFireworksAtOneGameTip", fullscreenClosing: true);
            UseSausageTip = new Part("UseSausageTip", fullscreenClosing: true);
            WinTournamentTip = new Part("WinTournamentTip", fullscreenClosing: true);

            if (isTNT)
            {
                GameBasics1 = new Part("GameBasics1", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true, background: 0.1f);
                GameBasics2 = new Part("GameBasics2", TutorialView.Right, -4f, pics.characters.vasia, fullscreenClosing: true, background: 0.1f);
                GameBasics3 = new Part("GameBasics3", TutorialView.Left, -7.6f, pics.characters.bobilich, fullscreenClosing: true, background: 0.1f);
                GameBasics4 = new Part("GameBasics4", TutorialView.Right, -4f, pics.characters.palna, fullscreenClosing: true, background: 0.1f);
                GameBasics5 = new Part("GameBasics5", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true, background: 0.1f);
                GameBasics6 = new Part("GameBasics6", TutorialView.Right, -4f, pics.characters.palna, fullscreenClosing: true, background: 0.1f);
                GameBasics = new Part("GameBasics", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true);

                GameCombo = new Part("GameCombo", TutorialView.Bottom, -7.7f, pics.characters.palna, fullscreenClosing: true);

                GameClosedBox = new Part("GameClosedBox", TutorialView.Bottom, -5.7f, fullscreenClosing: true);

                Missions = new Part("Missions", TutorialView.Top, 4.2f, fullscreenClosing: true);
                Missions1 = new Part("Missions1", TutorialView.Right, 0f, pics.characters.vasia, fullscreenClosing: true);
                Missions2 = new Part("Missions2", TutorialView.Left, 4.2f, pics.characters.liolia, fullscreenClosing: true);
                Missions3 = new Part("Missions3", TutorialView.Right, 0f, pics.characters.bobilich, fullscreenClosing: true);
                Missions4 = new Part("Missions4", TutorialView.Left, 4.2f, pics.characters.liolia, fullscreenClosing: true);
                Missions5 = new Part("Missions5", TutorialView.Right, 0f, pics.characters.bobilich, fullscreenClosing: true);
                Missions6 = new Part("Missions6", TutorialView.Left, 4.2f, pics.characters.vasia, fullscreenClosing: true);
                Missions7 = new Part("Missions7", TutorialView.Right, 0f, pics.characters.palna, fullscreenClosing: true);
                Missions8 = new Part("Missions8", TutorialView.Left, 4.2f, pics.characters.bobilich, fullscreenClosing: true, targets: new Transform[] { ui.prepare.missionList.transform });

                PrepareBox = new Part("PrepareBox", TutorialView.Bottom, -5.7f, fullscreenClosing: true);

                Tournament = new Part("Tournament", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                Tournament1 = new Part("Tournament1", TutorialView.Left, -7.7f, pics.characters.vasia, fullscreenClosing: true);
                Tournament2 = new Part("Tournament2", TutorialView.Right, -7.7f, pics.characters.bobilich, fullscreenClosing: true);

                TournamentTime = new Part("TournamentTime", TutorialView.Bottom, -2.7f, fullscreenClosing: true);
                TournamentPrizeFund = new Part("TournamentPrizeFund", TutorialView.Bottom, -2.7f, fullscreenClosing: true);

                TournamentInviteToCompite = new Part("TournamentInviteToCompite", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                TournamentBetterResults = new Part("TournamentBetterResults", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                TournamentInviteToMoreGoldfish = new Part("TournamentInviteToMoreGoldfish", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                Championship = new Part("Championship", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                Championship1 = new Part("Championship1", TutorialView.Left, -7.7f, pics.characters.vasia, fullscreenClosing: true);
                Championship2 = new Part("Championship2", TutorialView.Right, -7.7f, pics.characters.liolia, fullscreenClosing: true, param: () => user.league.score.SpaceFormat());

                Multiplier = new Part("Multiplier", TutorialView.Bottom, -5.7f, fullscreenClosing: true);
                MultiplierX5 = new Part("MultiplierX5", TutorialView.Bottom, -6.7f, fullscreenClosing: true);

                Goldfishes1 = new Part("Goldfishes1", TutorialView.Left, -7.7f, pics.characters.vasia, fullscreenClosing: true);
                Goldfishes = new Part("Goldfishes", TutorialView.Right, -7.7f, pics.characters.palna, fullscreenClosing: true);

                LuckyWheel1 = new Part("LuckyWheel1", TutorialView.Left, -7.5f, pics.characters.liolia, fullscreenClosing: true);
                LuckyWheel = new Part("LuckyWheel", TutorialView.Right, -7.5f, pics.characters.bobilich, fullscreenClosing: true);

                Fever1 = new Part("Fever1", TutorialView.Left, -7.7f, pics.characters.bobilich, fullscreenClosing: true);
                Fever = new Part("Fever", TutorialView.Right, -7.7f, pics.characters.palna, fullscreenClosing: true);

                Collection1 = new Part("Collection1", TutorialView.Left, 4.2f, pics.characters.vasia, fullscreenClosing: true);
                Collection2 = new Part("Collection2", TutorialView.Right, 4.2f, pics.characters.palna, fullscreenClosing: true);
                Collection = new Part("Collection", TutorialView.Left, 4.2f, pics.characters.bobilich, fullscreenClosing: true);

                CatBox = new Part("CatBox", TutorialView.Bottom, -4.7f, fullscreenClosing: true);

                CatGoldfishes = new Part("CatGoldfishes", TutorialView.Bottom, -4.7f, fullscreenClosing: true);

                CatUse = new Part("CatUse", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                CatUseActivateSnow = new Part("CatUseActivateSnow", TutorialView.Bottom, -5.7f, pics.characters.palna, fullscreenClosing: true);
                CatUseActivateZen = new Part("CatUseActivateZen", TutorialView.Bottom, -5.7f, pics.characters.bobilich, fullscreenClosing: true);
                CatUseActivateDisco = new Part("CatUseActivateDisco", TutorialView.Bottom, -5.7f, pics.characters.liolia, fullscreenClosing: true);
                CatUseActivateBoom = new Part("CatUseActivateBoom", TutorialView.Bottom, -5.7f, pics.characters.vasia, fullscreenClosing: true);
                CatUseActivateCap = new Part("CatUseActivateCap", TutorialView.Bottom, -5.7f, pics.characters.bobilich, fullscreenClosing: true);
                CatUseActivateFlint = new Part("CatUseActivateFlint", TutorialView.Bottom, -5.7f, pics.characters.vasia, fullscreenClosing: true);
                CatUseActivateKing = new Part("CatUseActivateKing", TutorialView.Bottom, -5.7f, pics.characters.liolia, fullscreenClosing: true);
                CatUseActivateMage = new Part("CatUseActivateMage", TutorialView.Bottom, -5.7f, pics.characters.palna, fullscreenClosing: true);
                CatUseActivateLoki = new Part("CatUseActivateLoki", TutorialView.Bottom, -5.7f, pics.characters.vasia, fullscreenClosing: true);
                CatUseActivateLady = new Part("CatUseActivateLady", TutorialView.Bottom, -5.7f, pics.characters.liolia, fullscreenClosing: true);
                CatUseActivateSanta = new Part("CatUseActivateSanta", TutorialView.Bottom, -5.7f, pics.characters.palna, fullscreenClosing: true);
                CatUseActivateJoker = new Part("CatUseActivateJoker", TutorialView.Bottom, -5.7f, pics.characters.bobilich, fullscreenClosing: true);
                CatUseActivateJack = new Part("CatUseActivateJack", TutorialView.Bottom, -5.7f, pics.characters.palna, fullscreenClosing: true);
                CatUseActivateRaiden = new Part("CatUseActivateRaiden", TutorialView.Bottom, -5.7f, pics.characters.bobilich, fullscreenClosing: true);
                CatUseActivateMix = new Part("CatUseActivateMix", TutorialView.Bottom, -5.7f, pics.characters.palna, fullscreenClosing: true);

                BuySausages = new Part("BuySausages", TutorialView.Bottom, -4.7f, fullscreenClosing: true);

                BuyCatbox1 = new Part("BuyCatbox1", TutorialView.Left, -4.7f, pics.characters.bobilich, fullscreenClosing: true);
                BuyCatbox = new Part("BuyCatbox", TutorialView.Right, -4.7f, pics.characters.liolia, fullscreenClosing: true);

                BuySkipMission = new Part("BuySkipMission", TutorialView.Top, 2.7f);

                CatExperience = new Part("CatExperience", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                CatLevelUp = new Part("CatLevelUp", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                GameComboLoop = new Part("GameComboLoop", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                CollectionSanta = new Part("CollectionSanta", TutorialView.Top, 1f, fullscreenClosing: true);
                CollectionLady = new Part("CollectionLady", TutorialView.Bottom, -6f, fullscreenClosing: true);
                CollectionJack = new Part("CollectionJack", TutorialView.Bottom, -6f, fullscreenClosing: true);
                CollectionMix = new Part("CollectionMix", TutorialView.Bottom, -3f, fullscreenClosing: true);

                FireworkBoomSmall = new Part("FireworkBoomSmall", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                FireworkRocket = new Part("FireworkRocket", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                FireworkBoomBig = new Part("FireworkBoomBig", TutorialView.Bottom, -7.7f, fullscreenClosing: true);
                FireworkColor = new Part("FireworkColor", TutorialView.Bottom, -7.7f, fullscreenClosing: true);

                Login1 = new Part("Login1", TutorialView.Left, -4f, pics.characters.palna, fullscreenClosing: true);
                Login = new Part("Login", TutorialView.Right, -4f, pics.characters.bobilich, fullscreenClosing: true);

                Medals = new Part("Medals", TutorialView.Top, 3.7f, sprite: pics.characters.bobilich, fullscreenClosing: true);

                VideoRewarded = new Part("VideoRewarded", TutorialView.Bottom, -0.7f, fullscreenClosing: true);

                LeagueUp1 = new Part("LeagueUp1", TutorialView.Left, -7.5f, pics.characters.liolia, fullscreenClosing: true);
                LeagueUp = new Part("LeagueUp", TutorialView.Right, -7.5f, pics.characters.bobilich, fullscreenClosing: true);

                BoostMultiplier = new Part("BoostMultiplier", TutorialView.Top, 2f, pics.characters.liolia, fullscreenClosing: true);
                BoostFirework = new Part("BoostFirework", TutorialView.Top, 2f, pics.characters.palna, fullscreenClosing: true);
                BoostExperience = new Part("BoostExperience", TutorialView.Bottom, -6f, pics.characters.bobilich, fullscreenClosing: true);
                BoostTime = new Part("BoostTime", TutorialView.Bottom, -6f, pics.characters.vasia, fullscreenClosing: true);

                ChainLengthTip = new Part("ChainLengthTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                ChainSequenceTip = new Part("ChainSequenceTip", TutorialView.Left, -7.6f, pics.characters.bobilich, fullscreenClosing: true);
                GameSessionsTip = new Part("GameSessionsTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                GetCatsTip = new Part("GetCatsTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                GetFeverTip = new Part("GetFeverTip", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true);
                GetGoldfishesTip = new Part("GetGoldfishesTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                GetLoopsTip = new Part("GetLoopsTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                GetMultiplierTip = new Part("GetMultiplierTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                GetScoreTip = new Part("GetScoreTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                GetScoreWithoutGingerTip = new Part("GetScoreWithoutGingerTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                GetScoreWithoutLimeTip = new Part("GetScoreWithoutLimeTip", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true);
                GetScoreWithoutMultipliersTip = new Part("GetScoreWithoutMultipliersTip", TutorialView.Left, -7.6f, pics.characters.bobilich, fullscreenClosing: true);
                InviteFriendsTip = new Part("InviteFriendsTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                InviteFriendsLogoutTip = new Part("InviteFriendsLogoutTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                LevelUpCatTip = new Part("LevelUpCatTip", TutorialView.Left, -7.6f, pics.characters.bobilich, fullscreenClosing: true);
                LoopLengthTip = new Part("LoopLengthTip", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true);
                UseBoostTip = new Part("UseBoostTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                UseCatsTip = new Part("UseCatsTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                UseFireworkBoomBigTip = new Part("UseFireworkBoomBigTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                UseFireworkBoomSmallTip = new Part("UseFireworkBoomSmallTip", TutorialView.Left, -7.6f, pics.characters.vasia, fullscreenClosing: true);
                UseFireworkColorTip = new Part("UseFireworkColorTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                UseFireworkRocketTip = new Part("UseFireworkRocketTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                UseFireworksAtOneGameTip = new Part("UseFireworksAtOneGameTip", TutorialView.Left, -7.6f, pics.characters.liolia, fullscreenClosing: true);
                UseSausageTip = new Part("UseSausageTip", TutorialView.Left, -7.6f, pics.characters.palna, fullscreenClosing: true);
                WinTournamentTip = new Part("WinTournamentTip", TutorialView.Left, -7.6f, pics.characters.bobilich, fullscreenClosing: true);
            }
        }

        public string name;
        public float offset;
        public Sprite sprite;
        public TutorialView view;
        public bool fullscreenClosing;
        public bool mirror;
        public float background;
        public Transform[] targets;
        public Func<string> param;

        public Part(string name, TutorialView view = TutorialView.Bottom, float offset = -8.2f, Sprite sprite = null, bool fullscreenClosing = false, bool mirror = false, float background = 0.7f, Transform[] targets = null, Func<string> param = null)
        {
            this.name = name;
            this.view = view;
            this.offset = offset * 51.2f;
            this.sprite = sprite != null ? sprite : (isTNT ? pics.characters.vasia : pics.characters.gingerCat);
            this.fullscreenClosing = fullscreenClosing;
            this.mirror = mirror;
            if (view == TutorialView.Right) this.mirror = true;
            this.targets = targets;
            this.background = background;
            this.param = param;
        }
    }
}