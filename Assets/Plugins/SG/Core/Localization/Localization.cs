using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public static class LocalizationExtensions
    {
        public static string Localize(this string key) => Localization.Get(key);
        public static string Localize(this string key, params object[] parameters) => Localization.Get(key, parameters);
    }

    public class Localization : MonoBehaviour
    {
        public static Localization Instance;

        private static LocalizationSource[] Sources => Instance.GetComponentsInChildren<LocalizationSource>();

#if UNITY_EDITOR
        [UI.Button("Download")] public bool download;
        [UnityEditor.MenuItem("Sunday/Localization/Update")]
        public static void Download()
        {
            if (!Configurator.Instance)
                Configurator.Instance = Configurator.FindInstance();

            if (!Instance)
                Instance = Configurator.Instance.GetComponentInChildren<Localization>();

            foreach (var source in Sources)
                source.Download();
        }
#endif
        public static SystemLanguage language { get; private set; } = SystemLanguage.Unknown;
        public static Action<SystemLanguage> onLanguageChanged;
        public static void SetLanguage(SystemLanguage language)
        {
            //Log.Info($"Localization - {language} language selected");

            Localization.language = language;

            PlayerPrefs.SetString("language", language.ToString());

            languageIndex = languages.IndexOf(language);

            onLanguageChanged?.Invoke(language);
        }

        public static int languageIndex { get; private set; } = -1;
        public static SystemLanguage[] languages;

        [Header("Supported Languages")]
        public SystemLanguage defaultLanguage = SystemLanguage.English;
        public LanguageMapping[] otherLanguages;
        [Serializable]
        public class LanguageMapping
        {
            public SystemLanguage language;
            public SystemLanguage[] mapping;
        }

        static Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
        static void Add(ByteReader reader, int languageColumn)
        {
            var row = reader.GetRow();
            while (row != null)
            {
                if (row[languageColumn - 1].IsNotEmpty())
                {
                    var lines = new string[languages.Length];
                    for (int i = 0; i < lines.Length; ++i)
                        lines[i] = row[i + languageColumn];

                    if (!dictionary.ContainsKey(row[languageColumn - 1]))
                        dictionary[row[languageColumn - 1]] = lines;
                    else
                        Log.Warning($"Localization - Unable to add '{row[languageColumn - 1]}' to the dictionary / duplicate keyword");
                }
                row = reader.GetRow();
            }
        }


        public static void Init()
        {
            if (!Instance)
                Instance = Configurator.Instance.GetComponentInChildren<Localization>();

            languages = new SystemLanguage[Instance.otherLanguages.Length + 1];

            dictionary.Clear();

            foreach (var source in Sources)
            {
                var reader = new ByteReader(source.File.bytes);

                var columns = reader.GetRow();
                int columnCount;
                for (columnCount = 0; columnCount < columns.size; ++columnCount)
                    if (columns[columnCount].IsEnum<SystemLanguage>())
                        break;

                for (int i = 0; i < languages.Length; ++i)
                    languages[i] = columns[i + columnCount].ToEnum<SystemLanguage>();

                Add(reader, columnCount);
            }

            var systemLanguage = Application.systemLanguage;
            //Log.Debug("Localization - System language: " + systemLanguage);

            if (PlayerPrefs.HasKey("language"))
            {
                SetLanguage(PlayerPrefs.GetString("language").ToEnum<SystemLanguage>());
                return;
            }

            foreach (var supportedLanguage in Instance.otherLanguages)
                if (supportedLanguage.language == systemLanguage || supportedLanguage.mapping.Contains(systemLanguage))
                {
                    SetLanguage(supportedLanguage.language);
                    return;
                }

            SetLanguage(Instance.defaultLanguage);
        }

        public static bool IsKey(string key) { return dictionary.ContainsKey(key); }

        public static string Get(string key)
        {
            if (key.IsEmpty())
                return key;

            if (dictionary.TryGetValue(key, out string[] values))
            {
                if (languageIndex == -1 || languageIndex >= values.Length)
                {
                    Log.Error($"Localization - Language index {languageIndex} is wrong");
                    return key;
                }

                if (values[languageIndex].IsNotEmpty())
                    return values[languageIndex];
                else if (values[0].IsNotEmpty())
                    return values[0];
            }

            Log.Warning($"Localization - Key '{key}' not found");
            return key;
        }

        public static string Get(string key, params object[] parameters)
        {
            string text = Get(key);

            if (text == key)
                return key;

            string[] first = text.Split('[');
            if (first.Length == 2)
            {
                // "{X} [0|1|2]"
                // 0: most cases
                // 1: 1, 21, 31, ...
                // 2: 2, 3, 4, 22, 23, 24, 32, 33, 34, ... 

                string[] last = first[1].Split(']');
                string[] center = last[0].Split('|');

                string par = parameters[0].ToString();
                string parLast = par.Substring(par.Length - 1, 1);

                int index = 0;
                if (parLast == "1" && !par.EndsWith("11"))
                    index = 1;
                else if ((parLast == "2" && !par.EndsWith("12")) || (parLast == "3" && !par.EndsWith("13")) || (parLast == "4" && !par.EndsWith("14")))
                    index = 2;

                return string.Format(first[0] + center[index] + last[1], parameters);
            }

            return string.Format(text, parameters);
        }

        public static string Get(TimeSpan time, byte count = 3)
        {
            if (count == 1)
            {
                if (time.Days > 0)
                    return time.Days + "daysShort".Localize();
                else if (time.Hours > 0)
                    return time.Hours + "hoursShort".Localize();
                else if (time.Minutes > 0)
                    return time.Minutes + "minutesShort".Localize();
                else
                    return time.Seconds + "secondsShort".Localize();
            }
            else if (count == 2)
            {
                if (time.Days > 0)
                    return time.Days + "daysShort".Localize() + " " + time.Hours + "hoursShort".Localize();
                else if (time.Hours > 0)
                    return time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize();
                else if (time.Minutes > 0)
                    return time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else
                    return time.Seconds + "secondsShort".Localize();
            }
            else if (count == 3)
            {
                if (time.Days > 0)
                    return time.Days + "daysShort".Localize() + " " + time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize();
                else if (time.Hours > 0)
                    return time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else if (time.Minutes > 0)
                    return time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else
                    return time.Seconds + "secondsShort".Localize();
            }
            else // if (count == 4)
            {
                if (time.Days > 0)
                    return time.Days + "daysShort".Localize() + " " + time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else if (time.Hours > 0)
                    return time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else if (time.Minutes > 0)
                    return time.Minutes + "minutesShort".Localize() + " " + time.Seconds + "secondsShort".Localize();
                else
                    return time.Seconds + "secondsShort".Localize();
            }
        }

        public static string GetWithNoSeconds(TimeSpan time)
        {
            if (time.Days > 0)
                return time.Days + "daysShort".Localize() + " " + time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize();
            else if (time.Hours > 0)
                return time.Hours + "hoursShort".Localize() + " " + time.Minutes + "minutesShort".Localize();
            else if (time.Minutes > 0)
                return time.Minutes + "minutesShort".Localize();
            else
                return "lessThanOneMinute".Localize();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Sunday/Localization/Change Language")]
#endif
        public static void SetNextLanguage() =>
            SetLanguage(languages[languageIndex == languages.Length - 1 ? 0 : languageIndex + 1]);

        public static void SetPreviousLanguage() =>
            SetLanguage(languages[languageIndex == 0 ? languages.Length - 1 : languageIndex - 1]);

        public static void SetLanguageIndex(int index) => SetLanguage(languages[index]);

        public int GetLanguageIndex() => languageIndex;


        [Header("Unique Chars")]
        public string additionalChars = "0123456789 ?!~*%/@$€£₽ØΞ";
        [TextArea(1, 5)]
        public string uniqueChars;
#if UNITY_EDITOR
        [UI.Button("SaveUniqueChars")] public bool saveUniqueChars;
        public void SaveUniqueChars()
        {
            var chars = new List<char>();
            foreach (var s in additionalChars)
                if (!chars.Contains(s))
                    chars.Add(s);

            foreach (var source in Sources)
                foreach (var s in source.File.text)
                {
                    if (!chars.Contains(char.ToLower(s)))
                        chars.Add(char.ToLower(s));
                    if (!chars.Contains(char.ToUpper(s)))
                        chars.Add(char.ToUpper(s));
                }

            chars.Sort();

            chars.Remove(' ');
            chars.Remove('\n');
            chars.Remove('\r');

            var temp = new System.Text.StringBuilder();
            foreach (var s in chars)
                temp.Append(s);

            uniqueChars = temp.ToString();

            Utils.SaveToFile(Configurator.resourcesPath + "/Localization/unique-chars.txt", uniqueChars);

            Log.Info("Unique Chars: " + chars.Count);
        }
#endif
    }
}