using System;
using System.Collections.Generic;

namespace SG
{
    public class Analytic : Core
    {
        public void Init()
        {
            foreach (var analyticsManager in GetComponents<AnalyticsManager>())
                analyticsManager.Init();
        }

        public static List<Action<string, string>> onEventImportant = new List<Action<string, string>>();
        public static void EventImportant(string category, string name)
        {
            foreach (var action in onEventImportant) action(category, name);
            foreach (var action in onEvent) action(category, name);
        }
        public static List<Action<string, string>> onEvent = new List<Action<string, string>>();
        public static void Event(string category, string name)
        {
            foreach (var action in onEvent) action(category, name);
        }

        public static List<Action<string, Dictionary<string, object>>> onEventPropertiesImportant = new List<Action<string, Dictionary<string, object>>>();
        public static void EventPropertiesImportant(string name, Dictionary<string, object> properties)
        {
            foreach (var action in onEventPropertiesImportant) action(name, properties);
            foreach (var action in onEventProperties) action(name, properties);
        }
        public static void EventPropertiesImportant(string name, string key, object value)
        {
            EventPropertiesImportant(name, new Dictionary<string, object>() { { key, value } });
        }

        public static List<Action<string, Dictionary<string, object>>> onEventProperties = new List<Action<string, Dictionary<string, object>>>();
        public static void EventProperties(string name, Dictionary<string, object> properties)
        {
            foreach (var action in onEventProperties) action(name, properties);
        }
        public static void EventProperties(string name, string key, object value)
        {
            EventProperties(name, new Dictionary<string, object>() { { key, value } });
        }

        public static List<Action<PurchaseData>> onEventRevenue = new List<Action<PurchaseData>>();
        public static void EventRevenue(PurchaseData data)
        {
            if (data == null) return;

            foreach (var action in onEventRevenue) action(data);
        }

        public static List<Action> onEventUserLogin = new List<Action>();
        public static void EventUserLogin()
        {
            foreach (var action in onEventUserLogin) action();
        }

        public static List<Action<Dictionary<string, object>>> onSetUserProperties = new List<Action<Dictionary<string, object>>>();
        public static void SetUserProperties(Dictionary<string, object> properties)
        {
            foreach (var e in onSetUserProperties) e(properties);
        }
        public static void SetUserProperties(string key, object value)
        {
            SetUserProperties(new Dictionary<string, object>() { { key, value } });
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
        public static int Round(int value)
        {
            for (int i = 0; i < 9; i++)
                if (value < 2 * Math.Pow(10, i + 1)) return Round(value, i);

            return 2000000000;
        }
    }
}