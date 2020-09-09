using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BuildSettings : Core
{
    [Header("Debug")]
    public DebugLevel debugLevel = DebugLevel.Debug;
    public enum DebugLevel { None = 0, Error = 1, Release = 2, Debug = 3 }

    public Platform debugPlatform = Platform.Unknown;
    public bool balanceDebug = false;
    public bool parseDebug = false;
    public bool serverDebug = false;
    public bool unlockAll = false;
    public bool cheats = false;
    public bool exceptionWatcher = false;
    public bool FPSWatcher = false;

    [Space(10)]
    public BuildSettingsFeatures iOS;
    public BuildSettingsFeatures android;
    public BuildSettingsFeatures tizen;

    [Header("Features")]
    public bool premium = false;
    public bool externalNotifications = true;
    public bool facebook = true;
    public bool parentGate = false;
    public bool promocodes = true;
    public bool localPurchaseVerification = false;
    public bool serverPurchaseVerification = true;
    public bool googlePlayGames = false;
    public bool readPhoneState = false;
    public bool replayKit = true;
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
    public bool isUpdateNeeded { get { return versionCode < currentVersionCode; } }
    [HideInInspector]
    public int criticalVersionCode;
    public bool isCriticalUpdateNeeded { get { return versionCode < criticalVersionCode; } }
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
#if UNITY_TIZEN
        BuildSettingsFeatures copyFrom = tizen;
#elif UNITY_IOS
        BuildSettingsFeatures copyFrom = iOS;
#elif UNITY_ANDROID
        BuildSettingsFeatures copyFrom = android;
#else
        BuildSettingsFeatures copyFrom = null;
#endif
        if (copyFrom == null) return;

        premium = copyFrom.premium;
        externalNotifications = copyFrom.externalNotifications;
        facebook = copyFrom.facebook;
        parentGate = copyFrom.parentGate;
        promocodes = copyFrom.promocodes;
        googlePlayGames = copyFrom.googlePlayGames;
        //readPhoneState = copyFrom.readPhoneState;
        localPurchaseVerification = copyFrom.localPurchaseVerification;
        serverPurchaseVerification = copyFrom.serverPurchaseVerification;
        replayKit = copyFrom.replayKit;
        ingameAdvisor = copyFrom.ingameAdvisor;

        chartboost = copyFrom.chartboost;
        appodeal = copyFrom.appodeal;
        googleAnalytics = copyFrom.googleAnalytics;
        gameAnalytics = copyFrom.gameAnalytics;
        amplitude = copyFrom.amplitude;
        appMetrica = copyFrom.appMetrica;
        unityAnalytics = copyFrom.unityAnalytics;
        unityAds = copyFrom.unityAds;      

        productName = copyFrom.productName;
        ID = copyFrom.ID;
        APPLE_ID = copyFrom.APPLE_ID;
        AMAZON_ID = copyFrom.AMAZON_ID;
        appleDeveloperTeamID = copyFrom.appleDeveloperTeamID;
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

    public void Setup()
    {
        // foreach (var atlas in tntSpriteAtlas) atlas.includeInBuild = isTNT;

        Copy();

        var defineSymbols = new System.Text.StringBuilder();
        if (!premium) defineSymbols.Append("IAP_PURCHASES;");
        if (localPurchaseVerification) defineSymbols.Append("LOCAL_PURCHASE_VERIFICATION;");
        if (chartboost) defineSymbols.Append("CHARTBOOST;");
        if (googleAnalytics) defineSymbols.Append("GOOGLE_ANALYTICS;");
        if (gameAnalytics) defineSymbols.Append("GAME_ANALYTICS;");
        if (amplitude) defineSymbols.Append("AMPLITUDE;");
        if (appMetrica) defineSymbols.Append("APP_METRICA;");
        if (appodeal) defineSymbols.Append("APPODEAL;");
        if (unityAds) defineSymbols.Append("UNITY_ADS;");
        if (facebook) defineSymbols.Append("FACEBOOK;");
        if (replayKit) defineSymbols.Append("REPLAY_KIT;");
        if (googlePlayGames) defineSymbols.Append("GOOGLE_PLAY_GAMES;");
        // if (!readPhoneState) defineSymbols.Append("PREVENT_READ_PHONE_STATE;");
        // if (!readPhoneState) defineSymbols.Append("ACTK_PREVENT_READ_PHONE_STATE;");
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
#endif

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(currentScene.path, true) };

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
