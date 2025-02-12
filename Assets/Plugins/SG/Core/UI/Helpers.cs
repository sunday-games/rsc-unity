using UnityEngine;

namespace SG.UI
{
    public static class Helpers
    {
        public static void OpenLink(string url)
        {
            if (url.IsEmpty())
            {
                Log.Error("Url is empty");
                return;
            }

            Log.Debug("Opening " + url);
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval($"window.open('{url}')");
#else
            Application.OpenURL(url);
#endif
        }
        public static void SendEmail(string email, string subject, string body)
        {
            Log.Info($"Sending email...\nTO: {email}\nSUBJECT: {subject}\nBODY: {body}");

            OpenLink($"mailto:{email}?subject={subject.EscapeURL().Replace("+", "%20")}&body={body.EscapeURL().Replace("+", "%20")}");
        }
        public static bool IsValidEmail(string email)
        {
            if (email.IsEmpty())
                return false;

            var splited = email.Split('@');
            if (splited.Length != 2)
                return false;

            splited = splited[1].Split('.');
            if (splited.Length < 2)
                return false;

            return true;
        }

        public static void TakeScreenshot()
        {
            var path = Configurator.Instance.appInfo.title + "_screenshot_" + UnityEngine.Screen.width + "x" + UnityEngine.Screen.height + "_" + System.DateTime.Now.Ticks + ".png";
            ScreenCapture.CaptureScreenshot(path);
            Log.Info("Screenshot saved to " + path);
        }

        // от 2048x1152 (0,5625) и до 2048x1536 (0,75)
        // от 1024x576 и до 1024x768
        public static float aspectRatio => UnityEngine.Screen.width / (float) UnityEngine.Screen.height;
        public static float screenSize => Mathf.Sqrt(UnityEngine.Screen.width / UnityEngine.Screen.dpi * UnityEngine.Screen.width / UnityEngine.Screen.dpi + UnityEngine.Screen.height / UnityEngine.Screen.dpi * UnityEngine.Screen.height / UnityEngine.Screen.dpi);
    }
}