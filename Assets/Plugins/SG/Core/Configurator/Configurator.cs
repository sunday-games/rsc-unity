using System;
using System.Collections;
using UnityEngine;
using SpriteAtlas = UnityEngine.U2D.SpriteAtlas;

namespace SG
{
    public static class ConfiguratorExtensions
    {
        public static Coroutine Start(this IEnumerator routine) => Configurator.Instance.StartCoroutine(routine);
        public static void Stop(this IEnumerator routine) => Configurator.Instance.StopCoroutine(routine);
        public static void Stop(this Coroutine routine) => Configurator.Instance.StopCoroutine(routine);
    }

    public class Configurator : MonoBehaviour
    {
        public static Action onChanged;
        public static Action<Result> onSetuped;

        public static Configurator Instance;
        public static Configurator FindInstance(string fileName = "Configurator") => Resources.Load<Configurator>(fileName);
        public static Configurator FindAndSetInstance(string fileName = "Configurator") => Instance != null ? Instance : Instance = FindInstance(fileName);

        public static bool production => CurrentEnvironment.Instance.Environment == Environment.Name.Prod || CurrentEnvironment.Instance.TestProd;

        public static string ApiUrl => Environment.Get(CurrentEnvironment.Instance.Environment).ApiUrl;
        public static string WsUrl => Environment.Get(CurrentEnvironment.Instance.Environment).WsUrl;

        public static int CurrentMinorVersion => int.Parse(Instance.appInfo.version.Split('.')[^1]);

        public Settings Settings;

        [Space] public Store Store;
        public RenderPipeline renderPipeline = RenderPipeline.Buildin;

        [Space] public UI.Style style;
        public static UI.Style Style => Instance.style;
        [UI.Button("UpdateStyle")] public bool updateStyle;
        public void UpdateStyle() => style?.Update();

        [Space] public UnityEngine.Object frameworkFolder;
        public UnityEngine.Object resourcesFolder;
        public UnityEngine.Object webglTemplateFolder;
#if UNITY_EDITOR
        public static string resourcesPath => UnityEditor.AssetDatabase.GetAssetPath(Instance.resourcesFolder);
        public static string frameworkPath => UnityEditor.AssetDatabase.GetAssetPath(Instance.frameworkFolder);
        public static string webglTemplatePath => UnityEditor.AssetDatabase.GetAssetPath(Instance.webglTemplateFolder);
#endif
        [Space] public SpriteAtlas iconsWhite;
        public Sprite GetIconWhite(string name) => iconsWhite ? iconsWhite.GetSprite(name) : null;
        public SpriteAtlas iconsColor;
        public Sprite GetIconColor(string name) => iconsColor ? iconsColor.GetSprite(name) : null;
        public SpriteAtlas iconsColorCards;
        public Sprite GetIconColorCards(string name) => iconsColorCards ? iconsColorCards.GetSprite(name) : null;

        [Space] public AppInfo appInfo;
        [Space] public bool exchangeRates = false;
        [Space] public bool payments = false;
        [Space] public bool blockchain = false;
        [Space] public bool analytics = false;

        [Header("Ads")] public bool unityAds = false;
        bool ads => unityAds;

        [Header("Loading Screen")] public bool favicon = false;
        public bool mobileRedirect = false;
        public string progressText;
        [TextArea(1, 10)] public string descriptionText;

        [Header("Other")] public bool clipboard = false;
        public bool externalParameters = false;
        public bool mobileAppChecker = false;
        public bool push = false;
        public bool localization = false;

        [Header("Logs")] public bool Debug;
        public string[] LogTags;
#if UNITY_EDITOR
        [UI.Button("ResetLogTags_OnClick")] public bool ResetLogTags;

        public void ResetLogTags_OnClick()
        {
            LogTags = Log.Tags.ALL;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Space] [UI.Button("Apply")] public bool apply;
        public void Apply()
        {
            Instance = this;

            SetupPlayerSettings();

            void SetupPlayerSettings()
            {
                if (appInfo.title.IsNotEmpty())
                    UnityEditor.PlayerSettings.productName = appInfo.title;

                if (appInfo.version.IsNotEmpty())
                    UnityEditor.PlayerSettings.bundleVersion = appInfo.version;
                if (appInfo.versionCode > 0)
                    UnityEditor.PlayerSettings.iOS.buildNumber = appInfo.versionCode.ToString();
                if (appInfo.versionCode > 0)
                    UnityEditor.PlayerSettings.Android.bundleVersionCode = appInfo.versionCode;
            }

            SetupDefines();

            SetupInstances();

            void SetupInstances()
            {
                // TODO: PushManager
#if SG_BLOCKCHAIN
                transform.CreateComponent<BlockchainPlugin.BlockchainManager>(blockchain, "Blockchain");
#endif
#if SG_EXCHANGERATES
                transform.CreateComponent<ExchangeRates>(exchangeRates, "Exchange Rates");
#endif
#if SG_CLIPBOARD
                transform.CreateComponent<ClipboardPlugin.Clipboard>(clipboard, "Clipboard");
#endif
#if SG_LOCALIZATION
                transform.CreateComponent<Localization>(localization, "Localization");
#endif
#if SG_APPCHECKER
                transform.CreateComponent<MobileAppChecker>(mobileAppChecker, "Mobile App Checker");
#endif
#if FACEBOOK
                transform.CreateComponent<FacebookManager>(facebook, "Facebook")?.Setup();
#endif
#if SG_EXTERNALPARAMS
                transform.CreateComponent<ExternalParameters.ExternalParameters>(externalParameters, "External Parameters")?.Setup();
#endif
#if SG_ANALYTICS
                transform.CreateComponent<Analytics.AnalyticsManager>(analytics, "Analytics")?.SetupInstances();
#endif
#if SG_ADS
                var adsInstance = CreateComponent<Ads.Manager>(ads, "Ads");
                if (adsInstance)
                    CreateComponent<Ads.UnityAds>(unityAds, "Unity Ads", parent: adsInstance.transform);
#endif
#if SG_PAYMENTS
                transform.CreateComponent<Payments.OrderManager>(payments, name: "Payments")?.Setup();
#endif
            }

            SetupIndexHTML();

            void SetupIndexHTML()
            {
                if (webglTemplateFolder == null)
                    return;

                // https://docs.unity3d.com/Manual/webgl-templates.html
                var html =
                    $@"<!DOCTYPE html>
<html lang='en-us'>

{Head()}

{Body()}

</html>";

                Utils.SaveToFile(webglTemplatePath + "/index.html", html);

                string Head()
                {
                    var text = @"
<head>
	<meta charset='utf-8'>
	<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no'>
	<title>{{{ PRODUCT_NAME }}}</title>
	<link rel='stylesheet' href='TemplateData/style.css'>

	<script type='text/javascript'>
		this.name = '{{{ PRODUCT_NAME }}}';
	</script>
";

                    if (favicon)
                        text += @"
	<meta name='msapplication-TileColor' content='#ffffff'>
	<meta name='theme-color' content='#ffffff'>
	<link rel='apple-touch-icon' sizes='180x180'		href='/apple-touch-icon.png'>
	<link rel='icon' type='image/png' sizes='32x32'		href='/favicon-32x32.png'>
	<link rel='icon' type='image/png' sizes='16x16'		href='/favicon-16x16.png'>
	<link rel='manifest'								href='/site.webmanifest'>
	<link rel='mask-icon' color='#5bbad5'				href='/safari-pinned-tab.svg'>
";
                    if (mobileRedirect)
                        text += $@"
	<script>
		window.globalSettings = {{}};
		window.globalSettings.mobileUrls = {{}};
		window.globalSettings.mobileUrls.iOS = '{appInfo.appleUrl}';
		window.globalSettings.mobileUrls.Android = '{appInfo.androidUrl}';
		window.globalSettings.mobileUrls.any = '{appInfo.defaultMobileRedirectUrl}';
	</script>
";

#if SG_ANALYTICS
                    text += Analytics.AnalyticsManager.SetupIndexHTML_Head();
#endif
#if SG_BLOCKCHAIN
                    text += BlockchainPlugin.BlockchainManager.SetupIndexHTML_Head();
#endif
#if SG_PAYMENTS && (PAYPAL || CARDPAY)
                    text += @"
	<script src='js/popup.js'></script>
	<script src='js/wallet-utility.js'></script>
";

                    text += @"
	<script src='js/checkout.js'></script>
";
#endif
                    if (push)
                        text += @"
	<script src='js/push/main.js'></script>
";
                    if (clipboard)
                        text += @"
	<script src='js/clipboard.js'></script>
";
                    if (mobileRedirect)
                        text += @"
	<script src='js/mobileDetect.js'></script>
";

                    // if (discordWidget) head += @"
                    //    <script src='https://cdn.jsdelivr.net/npm/@widgetbot/crate@3' async defer>
                    //        new Crate({server:'704972524446023717', channel:'704972929611595879', shard:'https://e.widgetbot.io'})
                    //    </script>
                    //";

                    text += @"
</head>
";
                    return text;
                }

                string Body()
                {
                    var text = @"
<body class='{{{ SPLASH_SCREEN_STYLE.toLowerCase() }}}'>
    
    <div id='unity-container' class='unity-desktop'>
        <canvas id='unity-canvas'></canvas>
    </div>
    
    <div id='loading-cover' style='display:none;'>
        <div id='unity-loading-bar'>
            <div id='unity-progress-bar-empty' style='display: none;'>
                <div id='unity-progress-bar-full'></div>
            </div>
            <div class='spinner'></div>
        </div>
        <div class='version-text'>Version {{{ PRODUCT_VERSION }}}</div>
        <div class='tooltip' id='tooltip'>If your browser displays an error, please hard refresh the page</div>
    </div>
    
    <div id='unity-fullscreen-button' style='display: none;'></div>

<script>
    function detectOS() {
        const userAgent = window.navigator.userAgent;

        if (userAgent.indexOf('Windows') !== -1) return 'windows';
        else if (userAgent.indexOf('Mac OS') !== -1 || userAgent.indexOf('Macintosh') !== -1) return 'mac';
        else return 'other';
    }

    function updateTooltipText() {
        const os = detectOS();
        const versionTextElement = document.getElementById('tooltip');
        
        if (os === 'windows')
            versionTextElement.textContent = 'If your browser displays an error, please hard refresh the page by pressing Ctrl + F5';
        else if (os === 'mac')
            versionTextElement.textContent = 'If your browser displays an error, please hard refresh the page by pressing Command + Shift + R';
    }

    window.onload = updateTooltipText;
</script>

<script>
    const hideFullScreenButton = '{{{ HIDE_FULL_SCREEN_BUTTON }}}';

    const buildUrl = 'Build';
    
    const loaderUrl = buildUrl + '/{{{ LOADER_FILENAME }}}';
    
    const config = {
        dataUrl: buildUrl + '/{{{ DATA_FILENAME }}}',
        frameworkUrl: buildUrl + '/{{{ FRAMEWORK_FILENAME }}}',
        codeUrl: buildUrl + '/{{{ CODE_FILENAME }}}',
#if MEMORY_FILENAME
        memoryUrl: buildUrl + '/{{{ MEMORY_FILENAME }}}',
#endif
#if SYMBOLS_FILENAME
        symbolsUrl: buildUrl + '/{{{ SYMBOLS_FILENAME }}}',
#endif
        streamingAssetsUrl: 'StreamingAssets',
        companyName: '{{{ COMPANY_NAME }}}',
        productName: '{{{ PRODUCT_NAME }}}',
        productVersion: '{{{ PRODUCT_VERSION }}}',
    };

    const container = document.querySelector('#unity-container');
    const canvas = document.querySelector('#unity-canvas');
    const loadingCover = document.querySelector('#loading-cover');
    const progressBarEmpty = document.querySelector('#unity-progress-bar-empty');
    const progressBarFull = document.querySelector('#unity-progress-bar-full');
    const fullscreenButton = document.querySelector('#unity-fullscreen-button');
    const spinner = document.querySelector('.spinner');

    const canFullscreen = (function() {
        for (const key of [
            'exitFullscreen',
            'webkitExitFullscreen',
            'webkitCancelFullScreen',
            'mozCancelFullScreen',
            'msExitFullscreen', ])
        {
            if (key in document) { return true; }
        }
        return false;
    }());

    canvas.style.background = ""url('TemplateData/img/background.jpg') center / cover"";
    loadingCover.style.display = '';

    const script = document.createElement('script');
    script.src = loaderUrl;
    script.onload = () => {

	    function onProgress(progress) {
            spinner.style.display = 'none';
            progressBarEmpty.style.display = '';
            progressBarFull.style.width = `${ 100 * progress}%`;
        };

	    function onSuccess(unityInstance) {
			window.unityInstance = unityInstance;
			
			const bindedSendMessage = unityInstance.SendMessage.bind(unityInstance);

			const unityNotifier = (sendMessage, unityObject) => (message, value) => {
				const unityMethod = 'Notify' + message;
				if (!!value)
					sendMessage(unityObject, unityMethod, JSON.stringify(value));
				else
					sendMessage(unityObject, unityMethod);
			};

";

#if SG_PAYMENTS && (PAYPAL || CARDPAY)
                    text += $@"
                window.Popup = window.createPopup(window, unityNotifier(bindedSendMessage, '{Payments.CheckoutManager.goName}'));
                window.Checkout = window.createCheckout(window, unityNotifier(bindedSendMessage, '{Payments.CheckoutManager.goName}'));
";
#endif
#if SG_BLOCKCHAIN
                    text += BlockchainPlugin.BlockchainManager.SetupIndexHTML_OnSuccess();
#endif
                    if (push)
                        text += @"
		        window.Push = window.createPush(window, unityNotifier(bindedSendMessage, 'Push'));
		        window.Push.init();
";
                    if (clipboard)
                        text += @"
		        window.Clipboard = window.createClipboard(window);
";

                    text += @"

            loadingCover.style.display = 'none';

            if (canFullscreen)
            {
                if (!hideFullScreenButton) { fullscreenButton.style.display = ''; }
                fullscreenButton.onclick = () => { unityInstance.SetFullscreen(1); };
            }
        };

	    function onError(message) {
	        alert(message);
	    };

        createUnityInstance(canvas, config, onProgress).then(onSuccess).catch(onError);
    };

    document.body.appendChild(script);

</script>

</body>";
                    return text;
                }
            }

            // TODO Move to Google Resolver
            SetupDependenciesIOS("Assets/Plugins/iOS/Editor/podfile");

            void SetupDependenciesIOS(string path)
            {
                if (!Utils.LoadFromFile(path, out string text))
                    return;

                var dependencies = "";

                //            if (IsSupported(Wallet.Names.LumiCollect)) dependencies += $@"
                //    pod 'LumiSDK'
                //";

                // TODO
                //                if (facebook)
                //                    dependencies += $@"
                //    pod 'Bolts', '~> 1.7'
                //    pod 'FBSDKCoreKit', '~> 5.2'
                //    pod 'FBSDKLoginKit', '~> 5.2'
                //    pod 'FBSDKShareKit', '~> 5.2'
                //";

                var startText = "# Dependencies start";
                var endText = "# Dependencies end";

                var startIndex = text.IndexOf(startText) + startText.Length;
                var endIndex = text.IndexOf(endText);

                Utils.SaveToFile(path,
                    text.Remove(startIndex, endIndex - startIndex).Insert(startIndex, dependencies));
            }

            SetupDependenciesAndroid("Assets/Plugins/Android/mainTemplate.gradle");

            void SetupDependenciesAndroid(string path)
            {
                if (!Utils.LoadFromFile(path, out string text))
                    return;

                var dependencies = "";

                //            if (IsSupported(Wallet.Names.Arkane)) dependencies += @"
                //	implementation group: 'net.openid', name: 'appauth', version: '0.7.1'
                //	implementation 'com.squareup.okio:okio:1.15.0'
                //	implementation 'com.squareup.okhttp3:okhttp:3.12.0'
                //";
                //            if (IsSupported(Wallet.Names.MeetOne)) dependencies += @"
                //	implementation 'com.alibaba:fastjson:1.1.70.android'
                //";
                //            if (IsSupported(Wallet.Names.LumiCollect)) dependencies += @"
                //	implementation 'com.github.lumitechnologies:Lumi-SDK-Android:1.0.0'
                //	implementation 'com.github.lumitechnologies:commons-android:1.0.1'
                //";

                // TODO
                //                if (facebook)
                //                    dependencies += @"
                //	implementation 'com.parse.bolts:bolts-android:1.4.0'
                //	implementation 'com.facebook.android:facebook-core:5.5.1'
                //	implementation 'com.facebook.android:facebook-applinks:5.5.1'
                //	implementation 'com.facebook.android:facebook-login:5.5.1'
                //	implementation 'com.facebook.android:facebook-share:5.5.1'
                //";

                var startText = "// Dependencies start";
                var endText = "// Dependencies end";

                var startIndex = text.IndexOf(startText) + startText.Length;
                var endIndex = text.IndexOf(endText);

                Utils.SaveToFile(path,
                    text.Remove(startIndex, endIndex - startIndex).Insert(startIndex, dependencies));
            }

            //#if SG_LOCALIZATION
            //            if (localization)
            //                Localization.Download(force: true);
            //#endif
            onChanged?.Invoke();

            Log.Info("Configurator - Setup complete");
        }
#endif // UNITY_EDITOR

        public void SetupDefines(string[] includeDefines = null, string[] excludeDefines = null)
        {
            if (frameworkFolder == null)
                return;

            var defines = "";

            if (Debug)
                defines += "-define:" + Log.Tags.DEBUG + Const.lineBreak;

            var logTags = includeDefines?.Length > 0 ? includeDefines : LogTags;
            foreach (var logTag in logTags)
            {
                if (!logTag.IsEmpty() && (!(excludeDefines?.Length > 0) || !excludeDefines.Contains(logTag)))
                {
                    defines += "-define:" + logTag + Const.lineBreak;
                    if (Debug)
                        defines += "-define:" + logTag + "_DEBUG" + Const.lineBreak;
                }
            }

            if (payments)
            {
                defines += "-define:SG_PAYMENTS" + Const.lineBreak;

                defines += Payments.OrderManager.Defines();
            }
            if (localization)
                defines += "-define:SG_LOCALIZATION" + Const.lineBreak;
            if (exchangeRates)
                defines += "-define:SG_EXCHANGERATES" + Const.lineBreak;
            if (clipboard)
                defines += "-define:SG_CLIPBOARD" + Const.lineBreak;
            if (mobileAppChecker)
                defines += "-define:SG_APPCHECKER" + Const.lineBreak;
            if (analytics)
                defines += "-define:SG_ANALYTICS" + Const.lineBreak;
            if (unityAds)
                defines += "-define:UNITY_ADS" + Const.lineBreak;
            if (externalParameters)
                defines += "-define:SG_EXTERNALPARAMS" + Const.lineBreak;
#if SG_ANALYTICS
            defines += Analytics.AnalyticsManager.Defines();
#endif
            if (blockchain)
            {
                defines += "-define:SG_BLOCKCHAIN" + Const.lineBreak;
#if SG_BLOCKCHAIN
                defines += BlockchainPlugin.BlockchainManager.Defines();
#endif
            }

            if (renderPipeline == RenderPipeline.Universal)
                defines += "-define:URP" + Const.lineBreak;
            else if (renderPipeline == RenderPipeline.HighDefinition)
                defines += "-define:HDRP" + Const.lineBreak;

            defines += "-define:ECS_EXCEPTIONS" + Const.lineBreak;

            Utils.SaveToFile("Assets/csc.rsp", defines);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public static void Init(Action<Result> callback = null)
        {
            Instance = Instantiate(FindInstance());
            Instance.InitCoroutine(callback).Start();
        }

        private IEnumerator InitCoroutine(Action<Result> callback = null)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var InitResult = new Result().SetSuccess();

#if SG_ANALYTICS
            Analytics.AnalyticsManager.Init();
#endif

#if SG_LOCALIZATION
            Localization.Init();
#endif

#if SG_PAYMENTS
            if (exchangeRates)
                ExchangeRates.Init(production);

            GetComponentInChildren<Payments.OrderManager>().Init();
#endif
#if SG_BLOCKCHAIN
            if (blockchain)
                yield return FindFirstObjectByType<BlockchainPlugin.BlockchainManager>().Init(InitResult);
#else
            yield return null;
#endif
            if (InitResult.success)
                Log.Init.Info("Configurator - Initialization Complete");
            else
                Log.Init.Error("Configurator - Initialization Error: " + InitResult.error);

            onSetuped?.Invoke(InitResult);

            callback?.Invoke(InitResult);
        }

        public static void Clear()
        {
#if SG_BLOCKCHAIN
            BlockchainPlugin.TransactionManager.RemoveAll();
#endif

#if SG_PAYMENTS
            Payments.OrderManager.RemoveAll();

            foreach (var provider in Payments.PaymentProvider.All.Values)
                provider.Clean();
#endif // SG_PAYMENTS
        }

        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Log.Info("PlayerPrefs.DeleteAll");
        }
    }
}