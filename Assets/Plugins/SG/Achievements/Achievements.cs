using UnityEngine;
using System;
using System.Collections.Generic;

namespace SG
{
    public class Achievements : MonoBehaviour
    {
        protected static UnityEngine.SocialPlatforms.ILocalUser LocalUser => Social.localUser;

        public static Texture2D UserPic =>
            (LocalUser != null && LocalUser.authenticated && Utils.IsPlatform(Platform.Android)) ? LocalUser.image : null;

        public bool loginAtStart = true;

        [TextArea(2, 10)]
        public string googleGameData;
        public string[] googleAchievementIds;

        public virtual void SubmitAllAchievements() { }

        public virtual void Login(string id, Action<bool> callback)
        {
            if (Utils.IsPlatformEditor())
            {
                callback(false);
            }
            else if (Utils.IsPlatform(Platform.iOS, Platform.tvOS))
            {
                if (loginAtStart || id.IsNotEmpty()) Authenticate(callback);
                else callback(false);
            }
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
            else if (Utils.IsPlatform(Platform.Android))
            {
                if (isDebug) GooglePlayGames.PlayGamesPlatform.DebugLogEnabled = true;
                GooglePlayGames.PlayGamesPlatform.Activate();

                if (loginAtStart || id.IsNotEmpty()) Authenticate(callback);
                else callback(false);
            }
#endif
        }

        protected void AchievementSave(Achievement achievement)
        {
            if (!PlayerPrefs.HasKey(achievement.appleGameCenterId))
            {
                //Analytic.EventAchievement(achievement.name);
                PlayerPrefs.SetInt(achievement.appleGameCenterId, 1);

                // fb.Achievement(server.links.hosting + @"achievements/test.html");
            }
        }

        public static Action<UnityEngine.SocialPlatforms.ILocalUser> OnAuthenticate;
        public void Authenticate(Action<bool> callback = null)
        {
            Log.Info("Social - Authenticate...");

#if UNITY_IOS
            UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif

            LocalUser.Authenticate(success =>
            {
                if (success)
                {
                    Log.Info("Social - Authenticate Success");

                    OnAuthenticate?.Invoke(LocalUser);

                    if (LocalUser.image != null) Log.Debug("Social - Image Loaded");
                }
                else
                {
                    Log.Error("Social - Authenticate Failed");
                }

                callback?.Invoke(success);
            });
        }

        public void ShowAchievements()
        {
            if (LocalUser.authenticated)
                Social.ShowAchievementsUI();
            else
                Authenticate(success => { if (success) Social.ShowAchievementsUI(); });
        }

        public void ShowLeaderboard()
        {
            if (LocalUser.authenticated)
                Social.ShowLeaderboardUI();
            else
                Authenticate(success => { if (success) Social.ShowLeaderboardUI(); });
        }

        protected void ReportProgress(string id, double progress = 100.0)
        {
            Social.ReportProgress(id, progress,
                success =>
                {
                    if (!success) Log.Error($"Social - Achievement ReportProgress Failed ({id})");
                });
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