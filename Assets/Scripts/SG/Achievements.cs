using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public class Achievements : Core
    {
        public bool loginAtStart = true;

        [TextArea(2,10)]
        public string googleGameData;
        public string[] googleAchievementIds;

        public virtual void SubmitAllAchievements()
        {
        }

        public virtual void Login(Action<bool> callback)
        {
            if (platform == Platform.Editor)
            {
                callback(false);
            }
            else if (platform == Platform.AppStore || platform == Platform.tvOS)
            {
                if (loginAtStart || !string.IsNullOrEmpty(user.gameCenterId)) Authenticate(callback);
                else callback(false);
            }
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
            else if (platform == Platform.GooglePlay)
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
            if ((platform == Platform.AppStore || platform == Platform.GooglePlay) && Social.localUser.authenticated)
            {
                string id = platform == Platform.AppStore ? achievement.appleGameCenterId : achievement.googleGamesId;

                Social.ReportProgress(id, 100.0f, (success) => { if (!success) LogError("Social - Achievement ReportProgress Failed (" + id + ")"); });

                AchievementSave(achievement);
            }
            else if (!PlayerPrefs.HasKey(achievement.appleGameCenterId))
            {
                if (!achievement.hide && ui.medalShow.Show(achievement)) AchievementSave(achievement);
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
            Log("Social - Authenticate...");

            if (platform == Platform.AppStore)
            {
#if UNITY_IOS
                UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif
                Social.localUser.Authenticate((success) =>
                {
                    if (success)
                    {
                        Log("Social - Authenticate Success");

                        if (string.IsNullOrEmpty(user.gameCenterId)) SubmitAllAchievements();

                        user.gameCenterId = Social.localUser.id;
                        user.socialName = Social.localUser.userName;
                        if (Social.localUser.image != null) LogDebug("Social - Image Loaded");
                    }
                    else LogError("Social - Authenticate Failed");

                    if (callback != null) callback(success);
                });
            }
            else if (platform == Platform.GooglePlay)
            {
                Social.localUser.Authenticate((success) =>
                {
                    if (success)
                    {
                        Log("Social - Authenticate Success");

                        if (string.IsNullOrEmpty(user.googleGamesId)) SubmitAllAchievements();

                        user.googleGamesId = Social.localUser.id;
                        user.socialName = Social.localUser.userName;
                        if (Social.localUser.image != null) LogDebug("Social - Image Loaded");
                    }
                    else LogError("Social - Authenticate Failed");

                    if (callback != null) callback(success);
                });
            }
        }

        public void ShowAchievements()
        {
            if (Social.localUser.authenticated) Social.ShowAchievementsUI();
            else Authenticate((success) => { if (success) Social.ShowAchievementsUI(); });
        }

        public void ShowLeaderboard()
        {
            if (Social.localUser.authenticated) Social.ShowLeaderboardUI();
            else Authenticate((success) => { if (success) Social.ShowLeaderboardUI(); });
        }

        public List<Achievement> list = new List<Achievement>();

        public class Achievement
        {
            public string name { get { return this.GetType().Name; } }
            public string nameText { get { return Localization.Get("achievement" + name); } }
            public string getText { get { return Localization.Get("achievement" + name + "Get", target.SpaceFormat()); } }
            public string bonusText { get { return Localization.Get("achievement" + name + "Bonus", bonus.SpaceFormat()); } }
            public bool isDone { get { return check != null ? check() : current() >= target; } }
            public float progress { get { return current() / (float)target; } }

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