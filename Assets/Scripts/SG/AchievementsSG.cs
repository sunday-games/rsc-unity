using UnityEngine;
using System;
using System.Collections.Generic;

namespace SG.RSC
{
    public class AchievementsSG : Core
    {
        private UnityEngine.SocialPlatforms.ILocalUser socialUser => Social.localUser;

        public bool loginAtStart = true;

        [TextArea(2, 10)]
        public string googleGameData;
        public string[] googleAchievementIds;

        public virtual void SubmitAllAchievements()
        {
        }

        public virtual void Login(Action<bool> callback)
        {
            if (Utils.IsPlatform(Platform.Editor))
            {
                callback(false);
            }
            else if (Utils.IsPlatform(Platform.iOS, Platform.tvOS))
            {
                if (loginAtStart || user.gameCenterId.IsNotEmpty()) Authenticate(callback);
                else callback(false);
            }
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
            else if (platform == Platform.Android)
            {
                if (isDebug) GooglePlayGames.PlayGamesPlatform.DebugLogEnabled = true;
                GooglePlayGames.PlayGamesPlatform.Activate();

                if (loginAtStart || !string.IsNullOrEmpty(user.googleGamesId)) Authenticate(callback);
                else callback(false);
            }
#endif
        }

        public void AchievementComplete(Achievement achievement)
        {
            if (Utils.IsPlatform(Platform.Mobile) && socialUser.authenticated)
            {
                string id = Utils.IsPlatform(Platform.iOS) ? achievement.appleGameCenterId : achievement.googleGamesId;

                Social.ReportProgress(id, 100.0f,
                    success => { if (!success) Log.Error("Social - Achievement ReportProgress Failed (" + id + ")"); });

                AchievementSave(achievement);
            }
            else if (!PlayerPrefs.HasKey(achievement.appleGameCenterId))
            {
                if (!achievement.hide && ui.medalShow.Show(achievement))
                    AchievementSave(achievement);
            }
        }

        void AchievementSave(Achievement achievement)
        {
            if (!PlayerPrefs.HasKey(achievement.appleGameCenterId))
            {
                //Analytic.EventAchievement(achievement.name);
                PlayerPrefs.SetInt(achievement.appleGameCenterId, 1);

                // fb.Achievement(server.links.hosting + @"achievements/test.html");
            }
        }

        public void Authenticate(Action<bool> callback = null)
        {
            Log.Info("Social - Authenticate...");

            if (Utils.IsPlatform(Platform.iOS))
            {
#if UNITY_IOS
                UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif
                socialUser.Authenticate(success =>
                {
                    if (success)
                    {
                        Log.Info("Social - Authenticate Success");

                        if (user.gameCenterId.IsEmpty())
                            SubmitAllAchievements();

                        user.gameCenterId = socialUser.id;
                        user.socialName = socialUser.userName;
                        if (socialUser.image != null) Log.Debug("Social - Image Loaded");
                    }
                    else
                        Log.Error("Social - Authenticate Failed");

                    callback?.Invoke(success);
                });
            }
            else if (Utils.IsPlatform(Platform.Android))
            {
                socialUser.Authenticate(success =>
                {
                    if (success)
                    {
                        Log.Info("Social - Authenticate Success");

                        if (string.IsNullOrEmpty(user.googleGamesId)) SubmitAllAchievements();

                        user.googleGamesId = socialUser.id;
                        user.socialName = socialUser.userName;
                        if (socialUser.image != null) Log.Debug("Social - Image Loaded");
                    }
                    else Log.Error("Social - Authenticate Failed");

                    callback?.Invoke(success);
                });
            }
        }

        public void ShowAchievements()
        {
            if (socialUser.authenticated)
                Social.ShowAchievementsUI();
            else
                Authenticate(success => { if (success) Social.ShowAchievementsUI(); });
        }

        public void ShowLeaderboard()
        {
            if (socialUser.authenticated)
                Social.ShowLeaderboardUI();
            else
                Authenticate(success => { if (success) Social.ShowLeaderboardUI(); });
        }

        public List<Achievement> list = new List<Achievement>();

        public class Achievement
        {
            public string name => GetType().Name;
            public string nameText => Localization.Get("achievement" + name);
            public string getText => Localization.Get("achievement" + name + "Get", target.SpaceFormat());
            public string bonusText => Localization.Get("achievement" + name + "Bonus", bonus.SpaceFormat());
            public bool isDone => check != null ? check() : current() >= target;
            public float progress => current() / (float)target;

            public Func<bool> check = null;

            public string googleGamesId = string.Empty;
            public string appleGameCenterId = string.Empty;
            public Sprite icon = null;
            public byte rank = 0;
            public int bonus = 0;
            public Func<int> current = null;
            public int target = 0;
            public bool hide = false;
        }
    }
}