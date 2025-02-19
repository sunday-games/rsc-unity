using UnityEngine;
using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SG.RSC
{
    public static class SG_Utils
    {
        public static byte[] ToByteArray(this UnityEngine.Object obj)
        {
            // http://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
            MemoryStream ms = new MemoryStream();
            (new BinaryFormatter()).Serialize(ms, obj);
            return ms.ToArray();
        }
        public static UnityEngine.Object ToObject(this byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (UnityEngine.Object)(new BinaryFormatter()).Deserialize(memStream);
        }

        public static string UrlEncode(this string instring)
        {
            // http://forum.antichat.ru/showthread.php?t=290347
            StringReader strRdr = new StringReader(instring);
            StringWriter strWtr = new StringWriter();
            int charValue = strRdr.Read();
            while (charValue != -1)
            {
                if (((charValue >= 48) && (charValue <= 57)) || ((charValue >= 65) && (charValue <= 90)) || ((charValue >= 97) && (charValue <= 122)))
                    strWtr.Write((char)charValue);
                else if (charValue == 32)
                    strWtr.Write("+");
                else
                    strWtr.Write("%{0:x2}", charValue);

                charValue = strRdr.Read();
            }
            return strWtr.ToString();
        }

        public static string MD5(this string text)
        {
            var hashBytes = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(text));

            var hashString = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++) hashString.Append(Convert.ToString(hashBytes[i], 16).PadLeft(2, '0'));

            return hashString.ToString().PadLeft(32, '0');
        }

        public static int ELO(int A, int B, bool isWin)
        {
            // http://ru.wikipedia.org/wiki/%D0%A0%D0%B5%D0%B9%D1%82%D0%B8%D0%BD%D0%B3_%D0%AD%D0%BB%D0%BE
            float Ea = (1 / (1 + Mathf.Pow(10f, (B - A) / 400f)));
            int Sa = isWin ? 1 : 0;
            int k = A < 1400 ? 30 : (A < 2400 ? 15 : 10);

            return (int)Mathf.Ceil(k * (Sa - Ea));
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static DateTime epoch = new DateTime(1970, 1, 1);
        public static DateTime ToDateTime(this long timestamp) { return epoch.AddMilliseconds(timestamp); }
        public static long ToTimestamp(this DateTime dateTime) { return (long)(dateTime - epoch).TotalMilliseconds; }

        public static string dateNowFormated
        {
            get
            {
                var dateString = new StringBuilder();
                dateString.Append(DateTime.Now.Year);
                dateString.Append("-");
                if (DateTime.Now.Month < 10) dateString.Append("0");
                dateString.Append(DateTime.Now.Month);
                dateString.Append("-");
                if (DateTime.Now.Day < 10) dateString.Append("0");
                dateString.Append(DateTime.Now.Day);

                return dateString.ToString();
            }
        }

        public static float aspectRatio_4x3 = 1.33f;
        public static float aspectRatio_5x3 = 1.67f;
        public static float aspectRatio_16x9 = 1.78f;
        public static float aspectRatio_3x4 = 0.75f;
        public static float aspectRatio_3x5 = 0.6f;
        public static float aspectRatio_9x16 = 9f / 16f; // ~.5625
        public static float aspectRatio_8x16 = 8f / 16f; // .5
        public static float aspectRatio_iPhoneX = 1125f / 2436f; // ~.4618
        public static float aspectRatio => Screen.width / (float)Screen.height;

        // от 2048x1152 (0,5625) и до 2048x1536 (0,75)
        // от 1024x576 и до 1024x768
        public static string resolution => Screen.width + "x" + Screen.height;
        public static float screenSize => Mathf.Sqrt(Screen.width / Screen.dpi * Screen.width / Screen.dpi + Screen.height / Screen.dpi * Screen.height / Screen.dpi);
        public static Dictionary<string, object> deviceInfo
        {
            get
            {
                return new Dictionary<string, object> {
                { "operatingSystem", SystemInfo.operatingSystem },
                { "resolution", resolution },
                { "dpi", Screen.dpi },
                { "systemMemorySize", SystemInfo.systemMemorySize },
                { "deviceModel", SystemInfo.deviceModel },
                { "deviceName", SystemInfo.deviceName },
                { "graphicsDeviceName", SystemInfo.graphicsDeviceName },
                { "graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor },
                { "graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion },
                { "graphicsMemorySize", SystemInfo.graphicsMemorySize },
                { "processorCount", SystemInfo.processorCount },
                { "processorType", SystemInfo.processorType } };
            }
        }

        public static void OpenLink(string link, string eventName = "")
        {
            if (Core.build.parentGate && Core.ui.parentGate != null)
            {
                Core.ui.parentGate.Show(() =>
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                Application.ExternalEval("window.open('" + link + "')");
#else
                Application.OpenURL(link);
#endif
            });

                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval("window.open('" + link + "')");
#else
            Application.OpenURL(link);
#endif

            if (!string.IsNullOrEmpty(eventName)) Analytic.Event("OpenLink", eventName);
        }

        public static void Email(string email, string subject, string body)
        {
            Log.Info($"Email Send \n\n TO: {email} \n\n SUBJECT: {subject} \n\n BODY: {body}");

            OpenLink("mailto:" + email + "?subject=" + WWW.EscapeURL(subject).Replace("+", "%20") + "&body=" + WWW.EscapeURL(body).Replace("+", "%20"), "MailApp");
        }

        public static void TakeScreenshot()
        {
            string path = string.Format("{0}_{1}-{2}-{3}_{4}-{5}-{6}.png", Core.build.pathForScreenshots, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            Log.Info("Screenshot saved to " + path);
            ScreenCapture.CaptureScreenshot(path);
        }

        /// <summary>
        /// Округляет целое число. Например, Round(123, 1) = 120, Round(123, 2) = 100
        /// </summary>
        /// <param name="value">Число</param>
        /// <param name="numberTens">Количество знаков</param>
        public static int Round(int value, int numberTens)
        {
            if (numberTens < 1) return value;

            double tens = Math.Pow(10, numberTens);

            return (int)(Math.Round((double)value / tens) * tens);
        }

        /// <summary>
        /// Автоматическое округление. Если число меньше 20, то не округляет. Если больше, то округляет на один знак. И т.д.
        /// </summary>
        public static int Round(this int value)
        {
            for (int i = 0; i < 9; i++)
                if (value < 2 * Math.Pow(10, i + 1)) return Round(value, i);

            return 2000000000;
        }

        public enum DataTimeFormats { One, Two, Three }
        public static string Localize(this TimeSpan timeSpan, DataTimeFormats format = DataTimeFormats.Three)
        {
            string result = "";

            if (format == DataTimeFormats.One)
            {
                if (timeSpan.Days > 0) result = timeSpan.Days + Localization.Get("daysShort");
                else if (timeSpan.Hours > 0) result = timeSpan.Hours + Localization.Get("hoursShort");
                else if (timeSpan.Minutes > 0) result = timeSpan.Minutes + Localization.Get("minutesShort");
                else result = timeSpan.Seconds + Localization.Get("secondsShort");
            }
            else if (format == DataTimeFormats.Two)
            {
                if (timeSpan.Days > 0)
                    result = timeSpan.Days + Localization.Get("daysShort") + " " + timeSpan.Hours + Localization.Get("hoursShort");
                else if (timeSpan.Hours > 0)
                    result = timeSpan.Hours + Localization.Get("hoursShort") + " " + timeSpan.Minutes + Localization.Get("minutesShort");
                else if (timeSpan.Minutes > 0)
                    result = timeSpan.Minutes + Localization.Get("minutesShort") + " " + timeSpan.Seconds + Localization.Get("secondsShort");
                else
                    result = timeSpan.Seconds + Localization.Get("secondsShort");
            }
            else if (format == DataTimeFormats.Three)
            {
                if (timeSpan.Days > 0)
                    result = timeSpan.Days + Localization.Get("daysShort")
                        + " " + timeSpan.Hours + Localization.Get("hoursShort")
                        + " " + timeSpan.Minutes + Localization.Get("minutesShort");
                else if (timeSpan.Hours > 0)
                    result = timeSpan.Hours + Localization.Get("hoursShort")
                        + " " + timeSpan.Minutes + Localization.Get("minutesShort")
                        + " " + timeSpan.Seconds + Localization.Get("secondsShort");
                else if (timeSpan.Minutes > 0)
                    result = timeSpan.Minutes + Localization.Get("minutesShort")
                        + " " + timeSpan.Seconds + Localization.Get("secondsShort");
                else
                    result = timeSpan.Seconds + Localization.Get("secondsShort");
            }

            return result;
        }
    }
}