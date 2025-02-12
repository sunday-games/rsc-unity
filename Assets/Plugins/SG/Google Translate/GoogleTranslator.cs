using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SG
{
    public class GoogleTranslator : MonoBehaviour
    {
        static GoogleTranslator instance;
        static string apiKey;
        static Dictionary<SystemLanguage, string> langFullToShort = new Dictionary<SystemLanguage, string>
        {
            [SystemLanguage.English] = "en",
            [SystemLanguage.Russian] = "ru",
            [SystemLanguage.German] = "de",
            [SystemLanguage.French] = "fr",
            [SystemLanguage.Italian] = "it",
            [SystemLanguage.Portuguese] = "pt",
            [SystemLanguage.Chinese] = "zh",
            [SystemLanguage.Japanese] = "ja",
        };

        public static void Setup(string apiKey)
        {
            GoogleTranslator.apiKey = apiKey;

            instance = FindObjectOfType<GoogleTranslator>();

            if (instance == null)
            {
                instance = new GameObject("Google Translator").AddComponent<GoogleTranslator>();
                instance.transform.SetParent(Configurator.Instance.transform);
            }
        }

        public static void Translate(SystemLanguage source, SystemLanguage target, string text, Action<string> callback)
        {
            instance.queue.Enqueue((source, target, text, callback));

            if (instance.queue.Count < 2)
                instance.StartCoroutine(instance.TranslateTextRoutine());
        }

        Queue<(SystemLanguage source, SystemLanguage target, string text, Action<string> callback)> queue = new Queue<(SystemLanguage source, SystemLanguage target, string text, Action<string> callback)>();
        UnityWebRequest webRequest;
        IEnumerator TranslateTextRoutine()
        {
            while (queue.Count > 0)
            {
                (SystemLanguage source, SystemLanguage target, string text, Action<string> callback) = queue.Dequeue();

                var webRequest = UnityWebRequest.Post(
                     "https://translation.googleapis.com/language/translate/v2?key=" + apiKey,
                     new List<IMultipartFormSection>
                     {
                        new MultipartFormDataSection("Content-Type", "application/json; charset=utf-8"),
                        new MultipartFormDataSection("source", langFullToShort[source]),
                        new MultipartFormDataSection("target", langFullToShort[target]),
                        new MultipartFormDataSection("format", "text"),
                        new MultipartFormDataSection("q", text)
                     });

                yield return webRequest.SendWebRequest();

                var responseDict = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                if (responseDict != null &&
                    responseDict.TryGetDict("data", out Dictionary<string, object> dataDict) &&
                    dataDict.TryGetList("translations", out List<object> translationsList) &&
                    translationsList.Count > 0 &&
                    (translationsList[0] as Dictionary<string, object>).TryGetString("translatedText", out string translatedText))
                {
                    Log.Debug($"GoogleTranslator {source}: '{text}' >> {target}: '{translatedText}'");

                    callback?.Invoke(translatedText);
                }
                else
                {
                    Log.Error($"GoogleTranslator {source}: '{text}' >> {target} - Failed. Error: {webRequest.downloadHandler.error}\nContent: {webRequest.downloadHandler.text}");

                    callback?.Invoke(null);
                }

                webRequest.Dispose();
            }
        }
    }
}