using UnityEngine;
using System.Collections.Generic;

namespace SG.Analytics
{
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance;

        private static Analytics[] _analytics = new Analytics[0];

        public static string Defines()
        {
            Instance = Configurator.Instance.GetComponentInChildren<AnalyticsManager>();

            string defines = "";
            if (Instance.FacebookAnalytics)
                defines += "-define:FACEBOOK" + Const.lineBreak;

            return defines;
        }

        public static void Init()
        {
            Instance = Configurator.Instance.GetComponentInChildren<AnalyticsManager>();

#if !UNITY_SERVER
            _analytics = Instance.GetComponentsInChildren<Analytics>();
            foreach (var a in _analytics)
                a.Init(Configurator.production);
#endif
        }

        public static string SetupIndexHTML_Head()
        {
            string head = "";

            if (Instance.GoogleAnalytics)
            {
                var googleAnalyticsInstance = FindFirstObjectByType<GoogleAnalytics>();
                var id = Configurator.production ? googleAnalyticsInstance.releaseId : googleAnalyticsInstance.debugId;

                if (id.IsEmpty())
                    Log.Error("Configurator - GoogleAnalytics - releaseId and debugId must be set");

                head += $@"
	<script>
		(function(i, s, o, g, r, a, m) {{
			i['GoogleAnalyticsObject'] = r;
			i[r] = i[r] || function() {{ (i[r].q = i[r].q || []).push(arguments) }}, i[r].l = 1 * new Date();
			a = s.createElement(o), m = s.getElementsByTagName(o)[0];
			a.async = 1;
			a.src = g;
			m.parentNode.insertBefore(a, m)
		}})(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');
		ga('create', '{id}', 'auto');
		ga('require', 'ecommerce');";
                if (googleAnalyticsInstance.sendPageViewOnLoadingScreen)
                    head += @"
		ga('send', 'pageview', 'Loading');";
                head += @"
	</script>
";
            }

            if (Instance.FacebookAnalytics)
            {
                var facebookAnalyticsInstance = FindFirstObjectByType<FacebookAnalytics>();
                var pixelId = Configurator.production ? facebookAnalyticsInstance.pixelProdId : facebookAnalyticsInstance.pixelTestId;

                if (pixelId.IsEmpty())
                    Log.Error("Configurator - FacebookAnalytics - pixelId must be set");

                head += $@"
	<script>
		!function(f, b, e, v, n, t, s) {{
			if (f.fbq) return;
			n = f.fbq = function() {{ n.callMethod ? n.callMethod.apply(n, arguments) : n.queue.push(arguments) }};
			if (!f._fbq) f._fbq = n;
			n.push = n;
			n.loaded = !0;
			n.version = '2.0';
			n.queue = [];
			t = b.createElement(e);
			t.async = !0;
			t.src = v;
			s = b.getElementsByTagName(e)[0];
			s.parentNode.insertBefore(t, s)
		}}(window, document, 'script', 'https://connect.facebook.net/en_US/fbevents.js');
		fbq('init', '{pixelId}');
		fbq('track', 'PageView');
	</script>
	<noscript>
		<img height='1' width='1' style='display:none' src='https://www.facebook.com/tr?id=645849955787498&ev=PageView&noscript=1' />
	</noscript>
";
                // TODO id=645849955787498 WTF
            }

            return head;
        }

        public static void SetDataCollection(bool dataCollection) { foreach (var a in _analytics) a.SetDataCollection(dataCollection); }

        public static void Event(string name) { foreach (var a in _analytics) a.Event(name); }

        public static void Event(string category, string name) { foreach (var a in _analytics) a.Event(category, name); }

        public static void Event(string category, string name, string subname) { foreach (var a in _analytics) a.Event(category, name, subname); }

        public static void Event(string name, Dictionary<string, object> parameters) { foreach (var a in _analytics) a.Event(name, parameters); }

        public static void View(string name) { foreach (var a in _analytics) a.View(name); }

        public static void Login(string id) { foreach (var a in _analytics) a.Login(id); }

        public static void Revenue(PurchaseData data) { foreach (var a in _analytics) a.Revenue(data); }

        public bool GoogleAnalytics = false;
        public bool UnityAnalytics = false;
        public bool FacebookAnalytics = false;

        public void SetupInstances()
        {
            transform.CreateComponent<GoogleAnalytics>(GoogleAnalytics, "Google Analytics");
            transform.CreateComponent<UnityAnalytics>(UnityAnalytics, "Unity Analytics");
            transform.CreateComponent<FacebookAnalytics>(FacebookAnalytics, "Facebook Analytics");
        }
    }
}