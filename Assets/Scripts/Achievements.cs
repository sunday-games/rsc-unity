using System;
using UnityEngine;

namespace SG.RSC
{
    public class Achievements : SG.Achievements
    {
        #region EVENTS
        public override void SubmitAllAchievements()
        {
            OnGetSuperCat();
            OnGameEnd();
            OnWinFriends();
            OnInviteFriends();
            OnUseSausages();
            OnGetGoldfishes();
            OnLevelUp();
            OnLeagueUp();
            OnChainDone();
            OnCatLevelUp();
            OnUpdateHonored();
            OnGetRevenue();
            OnLeagueLegendary();
        }
        public void OnGetSuperCat()
        {
            foreach (GetSuperCats achievement in getSuperCats)
                if (achievement.isDone) AchievementComplete(achievement);
        }

        public void OnGameEnd()
        {
            foreach (GetGameSessions achievement in getGameSessions)
                if (achievement.isDone) AchievementComplete(achievement);

            if (chosenOne.isDone) AchievementComplete(chosenOne);
        }

        public void OnWinFriends()
        {
            foreach (TournamentWonFriends achievement in tournamentWonFriends)
                if (achievement.isDone) AchievementComplete(achievement);
        }

        public void OnInviteFriends()
        {
            foreach (InviteFriends achievement in inviteFriends)
                if (achievement.isDone) AchievementComplete(achievement);
        }

        public void OnUseSausages()
        {
            foreach (UseSausages achievement in useSausages)
                if (achievement.isDone) AchievementComplete(achievement);
        }

        public void OnGetGoldfishes()
        {
            if (goldfishes.isDone) AchievementComplete(goldfishes);

            if (millionaire.isDone) AchievementComplete(millionaire);
        }

        public void OnLevelUp()
        {
            if (level.isDone) AchievementComplete(level);
        }

        public void OnLeagueUp()
        {
            if (league.isDone) AchievementComplete(league);
        }

        public void OnChainDone()
        {
            if (combo.isDone) AchievementComplete(combo);
        }

        public void OnCatLevelUp()
        {
            if (catLevel.isDone) AchievementComplete(catLevel);
        }

        public void OnUpdateHonored()
        {
            if (honored.isDone) AchievementComplete(honored);
        }

        public void OnGetRevenue()
        {
            if (patron.isDone) AchievementComplete(patron);
        }

        public void OnLeagueLegendary()
        {
            if (great8.isDone) AchievementComplete(great8);
        }
        #endregion

        public void AchievementComplete(Achievement achievement)
        {
            if (Utils.IsPlatformMobile() && LocalUser.authenticated)
            {
                var id = Utils.IsPlatform(Platform.iOS) ? achievement.appleGameCenterId : achievement.googleGamesId;
                ReportProgress(id);

                AchievementSave(achievement);
            }
            else if (!PlayerPrefs.HasKey(achievement.appleGameCenterId))
            {
                if (!achievement.hide && Core.ui.medalShow.Show(achievement))
                    AchievementSave(achievement);
            }
        }

        public GetSuperCats[] getSuperCats;
        public GetGameSessions[] getGameSessions;
        public TournamentWonFriends[] tournamentWonFriends;
        public InviteFriends[] inviteFriends;
        public UseSausages[] useSausages;

        public Goldfishes goldfishes;
        public Level level;
        public League league;
        public Combo combo;
        public CatLevel catLevel;

        public Goldfishes millionaire;
        public GetGameSessions chosenOne;
        public LegendaryPlace great8;
        public Honored honored;
        public Revenue patron;

        public float catSale
        {
            get
            {
                if (getSuperCats[2].isDone) return 1f - (float)getSuperCats[2].bonus / 100f;
                if (getSuperCats[1].isDone) return 1f - (float)getSuperCats[1].bonus / 100f;
                if (getSuperCats[0].isDone) return 1f - (float)getSuperCats[0].bonus / 100f;
                return 1f;
            }
        }
        public float easyGetDisco
        {
            get
            {
                if (getGameSessions[2].isDone) return 1f + (float)getGameSessions[2].bonus / 100f;
                if (getGameSessions[1].isDone) return 1f + (float)getGameSessions[1].bonus / 100f;
                if (getGameSessions[0].isDone) return 1f + (float)getGameSessions[0].bonus / 100f;
                return 1f;
            }
        }
        public float moreExpCat
        {
            get
            {
                if (tournamentWonFriends[2].isDone) return 1f + (float)tournamentWonFriends[2].bonus / 100f;
                if (tournamentWonFriends[1].isDone) return 1f + (float)tournamentWonFriends[1].bonus / 100f;
                if (tournamentWonFriends[0].isDone) return 1f + (float)tournamentWonFriends[0].bonus / 100f;
                return 1f;
            }
        }
        public float moreGoldfishes
        {
            get
            {
                if (inviteFriends[2].isDone) return 1f + (float)inviteFriends[2].bonus / 100f;
                if (inviteFriends[1].isDone) return 1f + (float)inviteFriends[1].bonus / 100f;
                if (inviteFriends[0].isDone) return 1f + (float)inviteFriends[0].bonus / 100f;
                return 1f;
            }
        }
        public TimeSpan lessTimeSausage
        {
            get
            {
                if (useSausages[2].isDone) return new TimeSpan(useSausages[2].bonus, 0, 0);
                if (useSausages[1].isDone) return new TimeSpan(useSausages[1].bonus, 0, 0);
                if (useSausages[0].isDone) return new TimeSpan(useSausages[0].bonus, 0, 0);
                return new TimeSpan(0, 0, 0);
            }
        }
        public int addLeagueTime { get { return goldfishes.isDone ? goldfishes.bonus : 0; } }
        public float easyGetMultiplier { get { return level.isDone ? 1f + level.bonus / 100f : 1f; } }
        public int moreDiscoTime { get { return league.isDone ? league.bonus : 0; } }
        public float increaseComboDistance { get { return combo.isDone ? 1f + combo.bonus / 100f : 1f; } }
        public float easyGetCat { get { return catLevel.isDone ? 1f - catLevel.bonus / 100f : 1f; } }

        public void Init()
        {
            goldfishes = new Goldfishes(3, 50000, 1, googleAchievementIds[0], "Goldfishes");
            list.Add(goldfishes);

            getSuperCats = new GetSuperCats[]
            {
            new GetSuperCats(0, 4, 10, googleAchievementIds[1], "GetSuperCats1"),
            new GetSuperCats(1, 8, 20, googleAchievementIds[2], "GetSuperCats2"),
            new GetSuperCats(2, 12, 30, googleAchievementIds[3], "GetSuperCats3")
            };
            list.AddRange(getSuperCats);

            level = new Level(4, 50, 20, googleAchievementIds[4], "Level");
            list.Add(level);

            getGameSessions = new GetGameSessions[]
            {
            new GetGameSessions(0, 500, 10, googleAchievementIds[5], "GetGameSessions1"),
            new GetGameSessions(1, 1000, 20, googleAchievementIds[6], "GetGameSessions2"),
            new GetGameSessions(2, 2500, 30, googleAchievementIds[7], "GetGameSessions3")
            };
            list.AddRange(getGameSessions);

            league = new League(5, Core.gameplay.leagues.Length, 2, googleAchievementIds[8], "League");
            list.Add(league);

            tournamentWonFriends = new TournamentWonFriends[]
            {
            new TournamentWonFriends(0, 4, 25, googleAchievementIds[9], "TournamentWonFriends1"),
            new TournamentWonFriends(1, 8, 50, googleAchievementIds[10], "TournamentWonFriends2"),
            new TournamentWonFriends(2, 12, 75, googleAchievementIds[11], "TournamentWonFriends3")
            };
            list.AddRange(tournamentWonFriends);

            combo = new Combo(6, 30, 10, googleAchievementIds[12], "Combo");
            list.Add(combo);

            inviteFriends = new InviteFriends[]
            {
            new InviteFriends(0, 10, 10, googleAchievementIds[13], "InviteFriends1"),
            new InviteFriends(1, 25, 20, googleAchievementIds[14], "InviteFriends2"),
            new InviteFriends(2, 50, 30, googleAchievementIds[15], "InviteFriends3")
            };
            list.AddRange(inviteFriends);

            catLevel = new CatLevel(7, 10, 20, googleAchievementIds[16], "CatLevel");
            list.Add(catLevel);

            useSausages = new UseSausages[]
            {
            new UseSausages(0, 25, 1, googleAchievementIds[17], "UseSausages1"),
            new UseSausages(1, 50, 2, googleAchievementIds[18], "UseSausages2"),
            new UseSausages(2, 100, 3, googleAchievementIds[19], "UseSausages3")
            };
            list.AddRange(useSausages);

            millionaire = new Goldfishes(3, 1000000, 1, googleAchievementIds[20], "Millionaire", hide: true);
            chosenOne = new GetGameSessions(3, 7777, 1, googleAchievementIds[21], "ChosenOne", hide: true);
            great8 = new LegendaryPlace(3, 8, googleAchievementIds[22], "Great8", hide: true);
            honored = new Honored(3, googleAchievementIds[23], "Honoured", hide: true);
            patron = new Revenue(3, 10, googleAchievementIds[24], "Patron", hide: true);
        }

        public class GetSuperCats : Achievement
        {
            public GetSuperCats(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.collection.Count;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.cat;
                this.hide = hide;
            }
        }
        public class GetGameSessions : Achievement
        {
            public GetGameSessions(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.gameSessions;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.timer;
                this.hide = hide;
            }
        }
        public class TournamentWonFriends : Achievement
        {
            public TournamentWonFriends(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.maxWonFriends;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.tournament;
                this.hide = hide;
            }
        }
        public class InviteFriends : Achievement
        {
            public InviteFriends(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.invitedFriends.Count;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.friends;
                this.hide = hide;
            }
        }
        public class UseSausages : Achievement
        {
            public UseSausages(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId)
            {
                this.current = () => Core.user.useSausagesHistory;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.sausage;
            }
        }
        public class Goldfishes : Achievement
        {
            public Goldfishes(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.coinsMAX;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.fish;
                this.hide = hide;
            }
        }
        public class Level : Achievement
        {
            public Level(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.level;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.star;
                this.hide = hide;
            }
        }
        public class League : Achievement
        {
            public League(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.gameplay.GetLeagueCount(Core.user.permanentRecord);
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.championship;
                this.hide = hide;
            }
        }
        public class Combo : Achievement
        {
            public Combo(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.chainLengthHistory;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.combo;
                this.hide = hide;
            }
        }
        public class CatLevel : Achievement
        {
            public CatLevel(byte rank, int targetValue, int bonus, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.maxCatLevel;
                this.rank = rank;
                this.target = targetValue;
                this.bonus = bonus;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                icon = Game.factory.medalIcons.catLevelUp;
                this.hide = hide;
            }
        }
        public class LegendaryPlace : Achievement
        {
            public LegendaryPlace(byte rank, int targetValue, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.legendaryPlace;
                this.rank = rank;
                this.target = targetValue;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                this.hide = hide;

                this.check = () => current() <= target;
            }
        }
        public class Honored : Achievement
        {
            public Honored(byte rank, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => Core.user.honored;
                this.rank = rank;
                this.target = 1;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                this.hide = hide;
            }
        }
        public class Revenue : Achievement
        {
            public Revenue(byte rank, int targetValue, string googleGamesId, string appleGameCenterId, bool hide = false)
            {
                this.current = () => (int)Core.user.revenue;
                this.rank = rank;
                this.target = targetValue;
                this.googleGamesId = googleGamesId;
                this.appleGameCenterId = appleGameCenterId;
                this.hide = hide;
            }
        }
    }
}