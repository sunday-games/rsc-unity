using UnityEngine;

namespace SG.RSC
{
    public abstract class Core : MonoBehaviour
    {
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
        public static Analytic analytic;
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

        //public static void Log.Error(object text)
        //{
        //    if ((int)build.debugLevel > 0) Debug.LogError(text);
        //}
        //public static void Log.Error(object text, params object[] parameters)
        //{
        //    if ((int)build.debugLevel > 0) Debug.LogError(string.Format(text.ToString(), parameters));
        //}
        //public static voidLog.Infoobject text)
        //{
        //    if ((int)build.debugLevel > 1) Debug.Log(text);
        //}
        //public static voidLog.Infoobject text, params object[] parameters)
        //{
        //    if ((int)build.debugLevel > 1) Debug.Log(string.Format(text.ToString(), parameters));
        //}
        //public static void Log.Debug(object text)
        //{
        //    if ((int)build.debugLevel > 2) Debug.Log(text);
        //}
        //public static void Log.Debug(object text, params object[] parameters)
        //{
        //    if ((int)build.debugLevel > 2) Debug.Log(string.Format(text.ToString(), parameters));
        //}
    }
}