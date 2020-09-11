using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BuildSettings : Core
{
    public BuildSettingsFeatures target;

    [Header("Debug")]
    public DebugLevel debugLevel = DebugLevel.Debug;
    public enum DebugLevel { None = 0, Error = 1, Release = 2, Debug = 3 }

    public Platform debugPlatform = Platform.Unknown;
    public bool balanceDebug = false;
    public bool serverDebug = false;
    public bool unlockAll = false;
    public bool cheats = false;
    public bool exceptionWatcher = false;
    public bool FPSWatcher = false;

    [Header("Features")]
    public bool premium = false;
    public bool externalNotifications = true;
    public bool facebook = true;
    public bool parentGate = false;
    public bool promocodes = true;
    public bool localPurchaseVerification = false;
    public bool serverPurchaseVerification = true;
    public bool googlePlayGames = false;
    public bool ingameAdvisor = false;

    [Space(10)]
    public bool chartboost = false;
    public bool googleAnalytics = false;
    public bool gameAnalytics = false;
    public bool amplitude = false;
    public bool appMetrica = false;
    public bool unityAnalytics = false;

    [Space(10)]
    public bool appodeal = false;
    public bool unityAds = false;
    public new bool ads => appodeal || unityAds;

    [Space(10)]
    public string version;
    public int versionCode;
    [HideInInspector]
    public int currentVersionCode;
    public bool isUpdateNeeded => versionCode < currentVersionCode;
    [HideInInspector]
    public int criticalVersionCode;
    public bool isCriticalUpdateNeeded => versionCode < criticalVersionCode;
    [HideInInspector]
    public string updateUrl = null;

    [Space(10)]
    public string keystorePass; // Kf,bhbyN
    public string keyaliasName; // stream
    public string keyaliasPass; // 5b9vZYIS

    [Space(10)]
    public string s; // starflame = c561cd985a0033a02b683081e20c561c

    [Space(10)]
    public string productName;
    public string ID;
    public string APPLE_ID;
    public string AMAZON_ID;
    public string appleDeveloperTeamID;

    [Space(10)]
    public string googlePublicKey;

    [Space(10)]
    public string pathForScreenshots;

    [Space(10)]
    public AndroidStore androidStore = AndroidStore.GooglePlay;
    public enum AndroidStore { GooglePlay, Amazon }

    [Space(10)]
    public int maxFPS = 60;

    [Space(10)]
    public UnityEngine.U2D.SpriteAtlas[] tntSpriteAtlas;

    [Space(10)]
    [TextArea(20, 100)]
    public string notes;

    void Copy()
    {
        premium = target.premium;
        externalNotifications = target.externalNotifications;
        facebook = target.facebook;
        parentGate = target.parentGate;
        promocodes = target.promocodes;
        googlePlayGames = target.googlePlayGames;
        localPurchaseVerification = target.localPurchaseVerification;
        serverPurchaseVerification = target.serverPurchaseVerification;
        ingameAdvisor = target.ingameAdvisor;

        chartboost = target.chartboost;
        appodeal = target.appodeal;
        googleAnalytics = target.googleAnalytics;
        gameAnalytics = target.gameAnalytics;
        amplitude = target.amplitude;
        appMetrica = target.appMetrica;
        unityAnalytics = target.unityAnalytics;
        unityAds = target.unityAds;      

        productName = target.productName;
        ID = target.ID;
        APPLE_ID = target.APPLE_ID;
        AMAZON_ID = target.AMAZON_ID;
        appleDeveloperTeamID = target.appleDeveloperTeamID;
        googlePublicKey = target.googlePublicKey;
    }

    public void SetupDebug()
    {
        debugLevel = DebugLevel.Debug;
        cheats = true;
        exceptionWatcher = true;

        Setup();
    }

    public void SetupRelease()
    {
        debugLevel = DebugLevel.Release;
        cheats = false;
        exceptionWatcher = false;

        Setup();
    }

    void Setup()
    {
        // foreach (var atlas in tntSpriteAtlas) atlas.includeInBuild = isTNT;

        Copy();

        var defineSymbols = new System.Text.StringBuilder();
        if (!premium) defineSymbols.Append("IAP_PURCHASES;");
        if (chartboost) defineSymbols.Append("CHARTBOOST;");
        if (googleAnalytics) defineSymbols.Append("GOOGLE_ANALYTICS;");
        if (gameAnalytics) defineSymbols.Append("GAME_ANALYTICS;");
        if (amplitude) defineSymbols.Append("AMPLITUDE;");
        if (appMetrica) defineSymbols.Append("APP_METRICA;");
        if (appodeal) defineSymbols.Append("APPODEAL;");
        if (unityAds) defineSymbols.Append("UNITY_ADS;");
        if (facebook) defineSymbols.Append("FACEBOOK;");
        if (googlePlayGames) defineSymbols.Append("GOOGLE_PLAY_GAMES;");
#if UNITY_EDITOR
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbols.ToString());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbols.ToString());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, defineSymbols.ToString());

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, ID);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ID);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.WebGL, ID);
        PlayerSettings.productName = productName;
        PlayerSettings.bundleVersion = version;
#if UNITY_TIZEN
        PlayerSettings.bundleVersion += "." + versionCode;
#endif
        PlayerSettings.iOS.buildNumber = versionCode.ToString();
        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasName = keyaliasName;
        PlayerSettings.Android.keyaliasPass = keyaliasPass;
        PlayerSettings.iOS.appleDeveloperTeamID = appleDeveloperTeamID;

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(currentScene.path, true) };
#endif // UNITY_EDITOR

        var googleAnalyticsManager = FindObjectOfType<SG.GoogleAnalyticsManager>();
        googleAnalyticsManager?.Setup();

        var facebookManager = FindObjectOfType<FacebookManager>();
        facebookManager?.Setup();

#if GAME_ANALYTICS
        var gameAnalytics = FindObjectOfType(typeof(GameAnalyticsManager)) as GameAnalyticsManager;
        gameAnalytics.settings.Build[0] = version;
        gameAnalytics.settings.UpdateGameKey(0, gameAnalytics.iOS.gameKey);
        gameAnalytics.settings.UpdateSecretKey(0, gameAnalytics.iOS.secretKey);
        gameAnalytics.settings.Build[1] = version;
        gameAnalytics.settings.UpdateGameKey(1, androidStore == AndroidStore.Amazon ? gameAnalytics.amazon.gameKey : gameAnalytics.android.gameKey);
        gameAnalytics.settings.UpdateSecretKey(1, androidStore == AndroidStore.Amazon ? gameAnalytics.amazon.secretKey : gameAnalytics.android.secretKey);
        EditorUtility.SetDirty(gameAnalytics.settings);
#endif

#if CHARTBOOST
        var chartboostProvider = GameObject.Find("Chartboost").GetComponent<ChartboostProvider>();
        chartboostProvider.settings.iOSAppId = chartboostProvider.iOS.ID;
        chartboostProvider.settings.iOSAppSecret = chartboostProvider.iOS.signature;
        chartboostProvider.settings.androidAppId = chartboostProvider.android.ID;
        chartboostProvider.settings.androidAppSecret = chartboostProvider.android.signature;
        chartboostProvider.settings.amazonAppId = chartboostProvider.amazon.ID;
        chartboostProvider.settings.amazonAppSecret = chartboostProvider.amazon.signature;
        chartboostProvider.settings.SetAndroidPlatformIndex(androidStore == AndroidStore.GooglePlay ? 0 : 1);
        EditorUtility.SetDirty(chartboostProvider.settings);
#endif

        if (SG.AppMetricaManager.instance != null) SG.AppMetricaManager.instance.Setup();

        (FindObjectOfType(typeof(ExceptionWatcher)) as ExceptionWatcher).enabled = exceptionWatcher;

        (FindObjectOfType(typeof(FPSWatcher)) as FPSWatcher).enabled = FPSWatcher;

        // Почему то это не работает
        //var achievements = game.GetComponent<Achievements>();
        //GPGSProjectSettings.Instance.Set("proj.AppId", achievements.googleGameId);
        //GPGSProjectSettings.Instance.Save();

        // AssetDatabase.Refresh();
#if UNITY_EDITOR
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
#endif
    }
}
