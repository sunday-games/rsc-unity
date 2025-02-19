using System;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG.RSC
{
    public abstract class Core : MonoBehaviour
    {
        public static void Init(Action callback = null)
        {
            if (Configurator.Instance != null)
            {
                Log.Error("Configurator is already inited");
                return;
            }

#if UNITY_EDITOR
            platform = Platform.Editor;
#elif UNITY_WEBGL
            platform = Platform.Facebook;
#elif UNITY_TIZEN
	        platform = Platform.Tizen;
#elif UNITY_IOS
	        platform = Platform.iOS;
#elif UNITY_ANDROID
            if (build.androidStore == BuildSettings.AndroidStore.GooglePlay) platform = Platform.Android;
            else platform = Platform.Amazon;
#endif

            // if (build.debugPlatform != Platform.Unknown) platform = build.debugPlatform;

            ObscuredPrefs.lockToDevice = ObscuredPrefs.DeviceLockLevel.None;
            //if (platform != Platform.iOS) ObscuredPrefs.lockToDevice = ObscuredPrefs.DeviceLockLevel.Soft;
            ObscuredPrefs.CryptoKey = build.s;

            Configurator.Init(result =>
            {
                // Settings = Resources.Load<Settings>("Settings");

                //if (!Settings.Load())
                //{
                //    Settings.SetDefaults();
                //    Settings.ApplyAndSave();
                //}

                if (build.maxFPS > 0) Application.targetFrameRate = build.maxFPS;

                achievements.Init();
                SG.Achievements.OnAuthenticate += localUser =>
                {
                    if (Utils.IsPlatform(Platform.iOS))
                    {
                        if (user.gameCenterId.IsEmpty())
                            achievements.SubmitAllAchievements();

                        user.gameCenterId = localUser.id;
                    }
                    else if (Utils.IsPlatform(Platform.Android))
                    {
                        if (user.googleGamesId.IsEmpty())
                            achievements.SubmitAllAchievements();

                        user.googleGamesId = localUser.id;
                    }

                    user.socialName = localUser.userName;
                };

                ads?.Init();

                Events.Init();

                config.Setup();

                Missions.Init();

                ui.Init();
                // Core.UI.Init();

                callback?.Invoke();
            });
        }

        static BuildSettings _build;
        public static BuildSettings build => _build == null ? _build = FindFirstObjectByType<BuildSettings>() : _build;

        public static Config config;
        public static User user;
        public static Game gameplay;
        public static Factory factory;
        public static FacebookManager fb;
        public static UI ui;
        public static IAPManager iapManager;
        public static Notifications notifications;
        public static Server server;
        public static Ads.Manager ads;
        public static Achievements achievements;
        public static Sound sound;
        public static Music music;
        public static Balance balance;
        public static IngameAdvisor.Advisor advisor;
        public static Platform platform = Platform.Android;
        public static Factory.Sprites pics => factory.sprites;

        public static bool isDebug => (int)build.debugLevel > 2;

        public static UnityEngine.SceneManagement.Scene currentScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        public static bool isTNT => currentScene.name == "TNT";
        public static bool isRiki => currentScene.name == "Riki";

        public static Vector2 halfVector2 = new Vector2(0.5f, 0.5f);

        public static Vector3 halfVector3 = new Vector3(0.5f, 0.5f, 0f);
        public static Vector3 thirdVector3 = new Vector3(0.3f, 0.3f, 0);
        public static Vector3 tenthVector3 = new Vector3(0.1f, 0.1f, 0f);

        public static Vector3 halfScale = new Vector3(0.5f, 0.5f, 1f);
        public static Vector3 tenthScale = new Vector3(0.1f, 0.1f, 1f);

        public static Rect zeroRect = new Rect(0f, 0f, 0f, 0f);

        public static bool smallScreen
        {
            get
            {
                if (!ui.smallLevelForSmallScreens)
                    return false;
                else
                    return (Screen.width == 640 && Screen.height == 960) || (Screen.width == 640 && Screen.height == 1136);
            }
        }
    }
}