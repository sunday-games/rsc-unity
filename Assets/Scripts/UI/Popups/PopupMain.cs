using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class PopupMain : Popup
    {
        [Space(10)]
        public GameObject leaderboard;
        public GameObject loading;
        [Space(10)]
        public GameObject updateButton;
        [Space(10)]
        public GameObject advisorButton;

        public override void Init()
        {
            championship.SetActive(false);
            tournament.SetActive(false);

            isAfterInit = false;

            facebookLoginBonusText.text = balance.reward.coinsForFacebookLogin.ToString();

            if (Missions.isTournament) TournamentShow();
            else if (Missions.isChampionship) ChampionshipShow();

            LuckyWheelShow();

            var isEventButton =
                // (Events.halloween.isActive && !Events.halloween.isHaveGift) ||
                (Events.newYear.isActive && !Events.newYear.isHaveGift) ||
                (Events.stValentin.isActive && !Events.stValentin.isHaveGift);

            updateButton.SetActive(build.isUpdateNeeded && !isEventButton);

            if (advisorButton)
                advisorButton.SetActive(advisor && !isEventButton && !updateButton.activeSelf &&
                    !string.IsNullOrEmpty(advisor.GetProjectId(Localization.language)));
        }

        bool isAfterInit = false;
        public override void AfterInit()
        {
            isAfterInit = true;

            if (isTNT && championship.activeSelf && !user.IsTutorialShown(Tutorial.Part.Championship2))
            {
                ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Championship1, Tutorial.Part.Championship2 }, new Transform[] { championship.transform });
                user.TutorialShown(Tutorial.Part.Championship);
            }
            else if (isTNT && !user.IsTutorialShown(Tutorial.Part.Tournament) && tournament.activeSelf)
            {
                ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Tournament1, Tutorial.Part.Tournament2 }, new Transform[] { tournament.transform });
                user.TutorialShown(Tutorial.Part.Tournament);
            }
            else if (!user.IsTutorialShown(Tutorial.Part.Championship) && championship.activeSelf)
                TutorialChampionship();
            else if (!user.IsTutorialShown(Tutorial.Part.Tournament) && tournament.activeSelf)
                TutorialTournament();

            if (championship.activeSelf) foreach (GameObject go in user.league.mainFX) go.SetActive(true);

            if (championship.activeSelf && championshipSlots.Count > 0)
            {
                loading.SetActive(false);
                championshipGrid.gameObject.SetActive(true);
            }
            else if (tournament.activeSelf && tournamentSlots.Count > 0)
            {
                loading.SetActive(false);
                tournamentGrid.gameObject.SetActive(true);
            }
        }

        public override void PreReset()
        {
            if (championship.activeSelf) foreach (GameObject go in user.league.mainFX) go.SetActive(false);
        }
        public override void Reset()
        {
            StopAllCoroutines();
            foreach (GameObject go in leagueFX) go.SetActive(false);
        }

        public override void OnEscapeKey()
        {
            Application.Quit();
        }

        void SetGridSize(GridLayoutGroup grid, int count)
        {
            (grid.transform as RectTransform).sizeDelta = new Vector2((grid.transform as RectTransform).sizeDelta.x, count * grid.cellSize.y);
        }

        void FocusOnPlayer(RectTransform list, int playerPos, int maxPos)
        {
            list.anchoredPosition = new Vector2(0f, list.sizeDelta.y * (playerPos - 3) / maxPos);
        }

        #region TOURNAMENT
        [Space(10)]
        public GameObject tournament;
        public GameObject tournamentLeaderboard;
        public GameObject tournamentError;
        public GameObject facebookLogin;
        public GameObject loginBonus;
        public Text facebookLoginBonusText;
        public LeaderboardSlot tournamentSlotPrefab;
        public GridLayoutGroup tournamentGrid;
        public Text tournamentCoinsText;
        public Text tournamentTimerText;

        public GameObject tournamentTime;
        public GameObject tournamentPrizeFund;

        List<LeaderboardSlot> tournamentSlots = new List<LeaderboardSlot>();

        [HideInInspector]
        public bool isTournamentUpdateNeeded = true;
        DateTime lastTournamentUpdateDate = DateTime.MinValue;
        TimeSpan tournamentRefreshRate = new TimeSpan(0, 10, 0);
        DateTime tournamentEndDate;

        void TournamentShowError()
        {
            tournamentLeaderboard.SetActive(false);
            facebookLogin.SetActive(false);
            loading.SetActive(false);

            tournamentError.SetActive(true);
        }
        public void TournamentShow()
        {
            if (!Missions.isTournament) return;

            leaderboard.SetActive(true);

            championship.SetActive(false);
            tournament.SetActive(true);

            tournamentLeaderboard.SetActive(false);
            tournamentError.SetActive(false);
            facebookLogin.SetActive(false);

            loading.SetActive(!isAfterInit);
            tournamentGrid.gameObject.SetActive(isAfterInit);

            // Если игрок не залогинен
            if (!fb.isLogin)
            {
                facebookLogin.SetActive(true);
                loading.SetActive(false);
                loginBonus.SetActive(Missions.isGoldfishes && string.IsNullOrEmpty(user.facebookId));
                return;
            }

            if (lastTournamentUpdateDate + tournamentRefreshRate < DateTime.Now) isTournamentUpdateNeeded = true;

            // Если нет времени окончания турнира или временя истекло или просто надо обновить турнир
            if (user.recordTimestamp == 0 || tournamentEndDate <= DateTime.Now || isTournamentUpdateNeeded)
            {
                loading.SetActive(true);
                DestroyTournamentSlots();
                server.GetTournament(TournamentCallback);
                return;
            }

            StopCoroutine("TournamentTimer");
            if (gameObject.activeSelf) StartCoroutine("TournamentTimer");
            tournamentLeaderboard.SetActive(true);
        }

        void TournamentCallback(List<Rival> currentTournamentPlayers, long currentTournamentEndDate, List<Rival> tournamentPlayers, int tournamentCoins)
        {
            if (ui.current != ui.main) return;

            if (currentTournamentPlayers == null || currentTournamentEndDate == 0)
            {
                TournamentShowError();
                return;
            }

            DestroyTournamentSlots();

            if (user.recordTimestamp != currentTournamentEndDate) user.ResetRecord();

            user.recordTimestamp = currentTournamentEndDate;
            tournamentEndDate = currentTournamentEndDate.ToDateTime().ToLocalTime();
            if (tournamentEndDate > DateTime.Now) tournamentEndDate += new TimeSpan(0, 0, 30);
            else tournamentEndDate = DateTime.Now + new TimeSpan(0, 1, 0);

            Notifications.Create(Notifications.tournamentEnd, tournamentEndDate);

            CreateTournamentSlots(currentTournamentPlayers);

            if (tournamentPlayers != null && tournamentPlayers.Count > 0)
            {
                if (tournamentCoins > 0) user.UpdateCoins(tournamentCoins, false);

                ui.PopupShow(ui.tournamentResults);
                ui.tournamentResults.CreateTournamentSlots(tournamentPlayers, tournamentCoins);
            }
        }

        public IEnumerator TournamentTimer()
        {
            while (tournamentEndDate > DateTime.Now)
            {
                tournamentTimerText.text = (tournamentEndDate - DateTime.Now).Localize();
                yield return new WaitForSeconds(1f);
            }

            TournamentShow();
        }

        void CreateTournamentSlots(List<Rival> rivals)
        {
            SetGridSize(tournamentGrid, rivals.Count);

            int prizeFund = 0;
            int i = 0;
            int members = -1;
            long lastRecord = long.MaxValue;
            foreach (var rival in rivals)
            {
                var slot = Instantiate(tournamentSlotPrefab) as LeaderboardSlot;
                slot.transform.SetParent(tournamentGrid.transform, false);
                tournamentSlots.Add(slot);

                slot.Setup(rival);

                if (rival.record == 0)
                {
                    slot.placeText.text = "-";
                    if (!rival.isPlayer) slot.inviteButton.gameObject.SetActive(true);
                }
                else
                {
                    if (rival.record < lastRecord) slot.placeText.text = (++i).ToString();
                    else slot.placeText.text = i.ToString();

                    if (i > 0 && i < 4)
                    {
                        slot.crownImage.gameObject.SetActive(true);
                        slot.crownImage.sprite = pics.crowns[i - 1];
                        if (slot.frameImage != null) slot.frameImage.color = pics.crownColors[i - 1];
                    }

                    prizeFund += ++members;
                }
                lastRecord = rival.record;
            }

            tournamentCoinsText.text = prizeFund > 0 ? (prizeFund * 100).ToString() : "0";

            lastTournamentUpdateDate = DateTime.Now;
            isTournamentUpdateNeeded = false;
            TournamentShow();
        }
        void DestroyTournamentSlots()
        {
            foreach (var slot in tournamentSlots) Destroy(slot.gameObject);
            tournamentSlots.Clear();
        }

        public Transform InviteFriendsButton;
        public void InviteFriends()
        {
            ui.FacebookInviteFriends(InviteFriendsButton.position);
        }

        public void TutorialTournament() { ui.tutorial.Show(Tutorial.Part.Tournament, new Transform[] { tournament.transform }); }

        public void TutorialTournamentTime() { ui.tutorial.Show(Tutorial.Part.TournamentTime, new Transform[] { tournamentTime.transform }); }

        public void TutorialTournamentPrizeFund() { ui.tutorial.Show(Tutorial.Part.TournamentPrizeFund, new Transform[] { tournamentPrizeFund.transform }); }

        #endregion

        #region CHAMPIONSHIP
        [Space(10)]
        public GameObject championship;
        public GameObject tournamentButton;
        public GameObject tournamentButtonBack;
        public GameObject championshipError;
        public LeaderboardSlot championshipSlotPrefab;
        public GridLayoutGroup championshipGrid;

        public Text leagueNameText;
        public Text leagueAimText;
        public GameObject[] leagueFX;

        [HideInInspector]
        public bool isChampionshipUpdateNeeded = true;
        List<LeaderboardSlot> championshipSlots = new List<LeaderboardSlot>();

        public void ChampionshipShow()
        {
            if (!Missions.isChampionship) return;

            leaderboard.SetActive(true);

            if (Missions.isTournament) foreach (var go in user.league.mainFX) go.SetActive(true);
            leagueNameText.color = user.league.color;
            leagueNameText.text = Localization.Get("league" + user.league.name) + " " + Localization.Get("league");
            leagueAimText.text = user.league != gameplay.leagueLegendary ? Localization.Get("leagueAim", user.league.score.SpaceFormat()) : Localization.Get("leagueLegendaryDescription");

            tournamentButton.SetActive(Missions.isTournament);
            if (tournamentButtonBack != null) tournamentButtonBack.SetActive(Missions.isTournament);

            tournament.SetActive(false);
            championship.SetActive(true);

            championshipError.SetActive(false);

            loading.SetActive(!isAfterInit);
            championshipGrid.gameObject.SetActive(isAfterInit);

            if (isChampionshipUpdateNeeded)
            {
                loading.SetActive(true);
                DestroyChampionshipSlots();
                server.GetChampionship(CreateChampionshipSlots);
                return;
            }

            FocusOnPlayer(championshipGrid.transform as RectTransform, championshipUserPlace, championshipSlots.Count);
        }
        void DestroyChampionshipSlots()
        {
            foreach (var slot in championshipSlots) Destroy(slot.gameObject);
            championshipSlots.Clear();
        }

        public void TutorialChampionship() { ui.tutorial.Show(Tutorial.Part.Championship, new Transform[] { championship.transform }); }

        int championshipUserPlace = 0;
        void CreateChampionshipSlots(List<Rival> rivals)
        {
            if (rivals == null)
            {
                loading.SetActive(false);
                championshipError.SetActive(true);
                return;
            }

            DestroyChampionshipSlots();

            SetGridSize(championshipGrid, rivals.Count);

            int i = 0;
            foreach (var rival in rivals)
            {
                var slot = Instantiate(championshipSlotPrefab) as LeaderboardSlot;
                slot.transform.SetParent(championshipGrid.transform, false);
                championshipSlots.Add(slot);

                slot.Setup(rival);

                slot.placeText.text = (++i).ToString();

                if (rival.isPlayer)
                {
                    championshipUserPlace = i;

                    if (user.league == gameplay.leagueLegendary)
                    {
                        if (user.legendaryPlace > championshipUserPlace) user.legendaryPlace = championshipUserPlace;
                        achievements.OnLeagueLegendary();
                    }
                }

                if (rival.id == Rival.ID_ME && string.IsNullOrEmpty(user.socialName))
                    slot.profileButton.gameObject.SetActive(true);

                if (i > 0 && i < 4)
                {
                    slot.crownImage.gameObject.SetActive(true);
                    slot.crownImage.sprite = pics.crowns[i - 1];
                    if (slot.frameImage != null) slot.frameImage.color = pics.crownColors[i - 1];
                }
            }

            if (isAfterInit)
            {
                loading.SetActive(false);
                championshipGrid.gameObject.SetActive(true);
            }

            isChampionshipUpdateNeeded = false;

            FocusOnPlayer(championshipGrid.transform as RectTransform, championshipUserPlace, championshipSlots.Count);
        }
        #endregion

        #region LUCKY WHEEL
        [Space(10)]
        public GameObject luckyWheel;
        public GameObject luckyWheelParticles;
        public GameObject luckyWheelTimer;
        public Text luckyWheelTimerText;
        void LuckyWheelShow()
        {
            if (Missions.isLuckyWheel)
            {
                luckyWheel.SetActive(true);

                if (user.TotalSpins(System.DateTime.UtcNow) > 0)
                {
                    StopCoroutine("LuckyWheelTimer");
                    luckyWheelTimer.SetActive(false);
                    luckyWheelParticles.SetActive(true);
                }
                else StartCoroutine("LuckyWheelTimer");
            }
            else
            {
                luckyWheel.SetActive(false);
            }
        }
        IEnumerator LuckyWheelTimer()
        {
            luckyWheelTimer.SetActive(true);
            luckyWheelParticles.SetActive(false);

            while (!user.IsFreeSpin(System.DateTime.UtcNow))
            {
                luckyWheelTimerText.text = user.TimeToNextFreeSpin(DateTime.UtcNow).Localize();
                yield return new WaitForSeconds(1f);
            }
            luckyWheelTimerText.text = "";

            luckyWheelTimer.SetActive(false);
            luckyWheelParticles.SetActive(true);
        }
        #endregion
    }
}