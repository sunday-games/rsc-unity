using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class LocalizationExtensions
{
    public static T ToEnum<T>(this string data)
    {
        return (T)Enum.Parse(typeof(T), data, true);
    }

    public static string Localize(this string key)
    {
        return Localization.Get(key);
    }

    public static string Localize(this string key, params object[] parameters)
    {
        return Localization.Get(key, parameters);
    }
}

public class Localization : MonoBehaviour
{
    public string spreadsheetId = "1c-E-PoRHR_qwkUkhbEDejLcNtzdXonbXy7fBjHeZ7aA";
    public string filePath = @"Assets\Scripts\SG\Localization\Resources\";
    public string urlSpreadsheet { get { return "https://docs.google.com/spreadsheets/d/" + spreadsheetId; } }
    public string urlCSV { get { return "https://docs.google.com/spreadsheets/d/" + spreadsheetId + "/export?format=csv"; } }
    public string fileName = "Localization";
    public SystemLanguage defaultLanguage = SystemLanguage.English;

    static Localization instance { get { return FindObjectOfType(typeof(Localization)) as Localization; } }

    [HideInInspector]
    public static List<Localize> localizeComponents = new List<Localize>();
    [HideInInspector]
    public static SystemLanguage language = SystemLanguage.Unknown;
    static int languageIndex = -1;
    static SystemLanguage[] languages;
    static Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();

    public static void Init()
    {
        var asset = Resources.Load<TextAsset>(instance.fileName);
        if (asset == null)
        {
#if UNITY_EDITOR
            EditorCoroutine.StartCoroutine(instance.LoadCoroutine(), instance);
#endif
        }
        else
        {
            LoadDictionary(asset);
            LoadLanguage();
        }
    }

    public IEnumerator LoadCoroutine()
    {
        var www = new WWW(urlCSV);

        yield return www;

        if (!string.IsNullOrEmpty(www.error) || string.IsNullOrEmpty(www.text))
        {
            Debug.LogError("Localization - Download Error");
        }
        else
        {
            Save(www.text, filePath + fileName + ".csv");

            Debug.Log("Localization - Download Success");

            LoadDictionary(Resources.Load<TextAsset>(fileName));
            LoadLanguage();
        }

        www.Dispose();
    }

    static void Save(string text, string path)
    {
        if (File.Exists(path)) File.Delete(path);

        var file = File.CreateText(path);
        file.WriteLine(text);
        file.Close();

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    static void LoadDictionary(TextAsset asset)
    {
        var reader = new ByteReader(asset.bytes);
        var row = reader.ReadCSV();

        if (row.size < 2)
        {
            Debug.LogError("Localization - There must be at least two columns in a valid CSV file. Columns: " + row.size);
            return;
        }

        if (row[0] != "KEY")
        {
            Debug.LogError("Localization - The first row must be KEY. First row: " + row[0]);
            return;
        }

        languages = new SystemLanguage[row.size - (row[row.size - 1] == "TAG" ? 2 : 1)];
        for (int i = 0; i < languages.Length; ++i)
            languages[i] = row[i + 1].ToEnum<SystemLanguage>();

        dictionary.Clear();

        while (row != null)
        {
            if (row.size > 1)
            {
                // Add a single line from a CSV file to the Localization list
                var temp = new string[row.size - 1];
                for (int i = 1; i < row.size; ++i)
                    temp[i - 1] = row[i];

                try
                {
                    dictionary.Add(row[0], temp);
                }
                catch (Exception e)
                {
                    Debug.LogError("Localization - Unable to add '" + row[0] + "' to the Localization dictionary: " + e.Message);
                }
            }
            row = reader.ReadCSV();
        }

        Debug.LogFormat("Localization - Loaded {0} languages", languages.Length);
    }

    static void LoadLanguage()
    {
        if (PlayerPrefs.HasKey("language"))
            SetLanguage(PlayerPrefs.GetString("language").ToEnum<SystemLanguage>());
        else if (Array.IndexOf(languages, SystemLanguage.Russian) >= 0 &&
                 (Application.systemLanguage == SystemLanguage.Russian ||
                 Application.systemLanguage == SystemLanguage.Belarusian ||
                 Application.systemLanguage == SystemLanguage.Ukrainian))
            SetLanguage(SystemLanguage.Russian);
        else if (Array.IndexOf(languages, SystemLanguage.English) >= 0)
            SetLanguage(SystemLanguage.English);
        else
            SetLanguage(instance.defaultLanguage);
    }

    static void SaveLanguage()
    {
        PlayerPrefs.SetString("language", language.ToString());
    }

    static void SetLanguage(SystemLanguage newLanguage)
    {
        language = newLanguage;
        SaveLanguage();

        string[] keys;
        if (!dictionary.TryGetValue("KEY", out keys))
            return;
        for (int i = 0; i < keys.Length; ++i)
            if (keys[i].ToEnum<SystemLanguage>() == language)
            {
                languageIndex = i;
                break;
            }

        foreach (var component in localizeComponents)
            component.UpdateText();
    }

    public static SystemLanguage SetNextLanguage()
    {
        SetLanguage(languages[languageIndex == languages.Length - 1 ? 0 : languageIndex + 1]);
        return language;
    }

    public static string Get(string key)
    {
        string[] values;
        if (languageIndex != -1 && dictionary.TryGetValue(key, out values) && languageIndex < values.Length)
            return values[languageIndex];

        Debug.LogError("Localization - Key not found: " + key);
        return key;
    }

    public static string Get(string key, params object[] parameters)
    {
        string text = Get(key);

        if (text == key) return key;

        if (language == SystemLanguage.Russian)
        {
            string[] first = text.Split(new char[] { '[' });
            if (first.Length == 2)
            {
                // "{X} [0|1|2]"
                // 1 если ’ заканчиваетс€ на 1, но не на 11
                // 2 если ’ заканчиваетс€ 2/3/4, но не на 12/13/14
                // 0 во всех других случа€х

                string[] last = first[1].Split(new char[] { ']' });
                string[] center = last[0].Split(new char[] { '|' });

                string par = parameters[0].ToString();
                string parLast = par.Substring(par.Length - 1, 1);

                int index = 0;
                if (parLast == "1" && !par.EndsWith("11"))
                    index = 1;
                else if ((parLast == "2" && !par.EndsWith("12")) || (parLast == "3" && !par.EndsWith("13")) || (parLast == "4" && !par.EndsWith("14")))
                    index = 2;

                return string.Format(first[0] + center[index] + last[1], parameters);
            }
        }

        return string.Format(text, parameters);
    }

    public static bool Exists(string key)
    {
        return dictionary.ContainsKey(key);
    }
}