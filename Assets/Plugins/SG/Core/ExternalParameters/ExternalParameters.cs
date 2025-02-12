using System;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SG.ExternalParameters
{
    public class ExternalParameters : MonoBehaviour
    {
        public static DictSO parameters;
        public static Action<DictSO> OnGetParameters;

        public static string referrer;
        public static Action<string> onGetReferrer;

        public DebugData debug;
        [Serializable]
        public class DebugData
        {
            public bool documentReferrerTest = false;
            public string documentReferrer = "https://sunday.games/page?utm_source=test_source&utm_medium=test_medium";
            [Space(10)]
            public bool absoluteURLTest = false;
            public string absoluteURL = "https://sunday.games/tools?number=123&double=1.23&text=abc";
            [UI.Button("DebugGetParameters")] public bool getParameters;
        }

        void Awake()
        {
#if UNITY_2019_2_OR_NEWER
            Application.deepLinkActivated += url => TryGetParameters(url);
#endif

            OnGetParameters += parameters =>
            {
                foreach (var text in new string[] { "ref", "referrer", "referral" })
                    if (parameters.IsValue(text))
                    {
                        GetReferrer(parameters[text].ToString());
                        break;
                    }
            };
        }

        void Start()
        {
            TryGetParameters(Application.absoluteURL);

#if UNITY_EDITOR
            if (debug.absoluteURLTest)
                TryGetParameters(debug.absoluteURL);
#endif

#if UNITY_EDITOR || UNITY_WEBGL
            if (referrer.IsEmpty())
                TryGetDocumentReferrer(Configurator.Instance.appInfo.domains);
#endif
        }

        void TryGetParameters(string url)
        {
            if (url.IsEmpty())
                return;

            Log.Debug($"ExternalParameters - Get url: '{url}'");

            GetParameters(ParseQuery(new Uri(url).Query));
        }

        void GetParameters(DictSO parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return;

            ExternalParameters.parameters = parameters;

            Log.Info($"ExternalParameters - Get parameters: {Json.Serialize(parameters)}");

            OnGetParameters?.Invoke(parameters);
        }

        void GetReferrer(string referrer)
        {
            if (referrer.IsEmpty())
                return;

            ExternalParameters.referrer = referrer;

            Log.Info($"ExternalParameters - Get referrer: '{referrer}'");

            onGetReferrer?.Invoke(referrer);
        }

#if UNITY_EDITOR || UNITY_WEBGL
        void TryGetDocumentReferrer(IList<string> ignoreHosts)
        {
            string referrer = null;
#if UNITY_EDITOR
            if (debug.documentReferrerTest)
                referrer = debug.documentReferrer;
#elif UNITY_WEBGL
            referrer = GetDocumentReferrer();
#endif

            if (referrer.IsEmpty())
                return;

            Log.Info($"ExternalParameters - document.referrer: '{referrer}'");

            if (!Uri.IsWellFormedUriString(referrer, UriKind.Absolute))
            {
                GetReferrer(referrer);
                return;
            }

            var uri = new Uri(referrer);

            var host = uri.DnsSafeHost;
            if (ignoreHosts != null && ignoreHosts.Contains(host))
                host = string.Empty;

            var parameters = ParseQuery(uri.Query);

            if (parameters.Count == 0)
            {
                GetReferrer(host);
                return;
            }

            var paramsList = new List<string>();

            if (parameters.TryGetString("utm_source", out string utm_source) && utm_source != "referral")
                paramsList.Add(utm_source);
            if (parameters.TryGetString("utm_medium", out string utm_medium) && utm_medium != "referral")
                paramsList.Add(utm_medium);

            if (paramsList.Count == 0)
            {
                GetReferrer(host);
                return;
            }

            GetReferrer(host + (host.IsEmpty() ? "" : ":") + string.Join(",", paramsList));
        }

        [DllImport("__Internal")] static extern string GetDocumentReferrer();
#endif

#if UNITY_EDITOR
        public void Setup()
        {
            SetupBuildPostProcess();
            SetupAndroidManifest();
        }

        void SetupBuildPostProcess()
        {
            var path = "Assets/Plugins/iOS/Editor/IosBuildPostProcess.cs";

            if (!Utils.LoadFromFile(path, out string text))
                return;

            if (Configurator.Instance.appInfo.deepLinkUrl.IsNotEmpty())
            {
                var uri = new Uri(Configurator.Instance.appInfo.deepLinkUrl);
                var replacement = $@"
        var gameDomain = ""{uri.Host}"";
        ";

                var startText = @"// ExternalParameters";
                var endText = @"// /ExternalParameters";

                var startIndex = text.IndexOf(startText) + startText.Length;
                var endIndex = text.IndexOf(endText);

                Utils.SaveToFile(path,
                    text.Remove(startIndex, endIndex - startIndex).Insert(startIndex, replacement));
            }
        }

        void SetupAndroidManifest()
        {
            var path = "Assets/Plugins/Android/AndroidManifest.xml";

            if (!Utils.LoadFromFile(path, out string text))
                return;

     
            var replacement = $@"
      <intent-filter>
        <action android:name='${{packageId}}.deeplink.launch' />
        <category android:name='android.intent.category.DEFAULT' />
      </intent-filter>
      <intent-filter>";

            if (Configurator.Instance.appInfo.deepLinkUrl.IsNotEmpty())
            {
                var uri = new Uri(Configurator.Instance.appInfo.deepLinkUrl);
                replacement += $@"
        <data android:scheme='{uri.Scheme}' android:host='{uri.Host}' {(uri.AbsolutePath.Length > 1 ? $"android:path='{uri.AbsolutePath}' " : "")}/>";
            }

            replacement += $@"
        <action android:name='android.intent.action.VIEW' />
        <category android:name='android.intent.category.DEFAULT' />
        <category android:name='android.intent.category.BROWSABLE' />
      </intent-filter>
      ";

            var startText = @"<!-- ExternalParameters start -->";
            var endText = @"<!-- ExternalParameters end -->";

            var startIndex = text.IndexOf(startText) + startText.Length;
            var endIndex = text.IndexOf(endText);

            Utils.SaveToFile(path,
                text.Remove(startIndex, endIndex - startIndex).Insert(startIndex, replacement));
        }
#endif

        static DictSO ParseQuery(string query)
        {
            var parameters = new DictSO();

            var parametersColl = HttpUtility.ParseQueryString(query);

            foreach (var key in parametersColl.AllKeys)
            {
                if (key == null)
                    continue;

                if (parameters.ContainsKey(key))
                    continue;

                var value = parametersColl.Get(key);

                if (value == null)
                    continue;

                object result = value;

                if (int.TryParse(value, out int intValue))
                    result = intValue;
                else if (double.TryParse(value, out double doubleValue))
                    result = doubleValue;

                parameters.Add(key, result);
            }

            return parameters;
        }
    }
}