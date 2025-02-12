using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class Splash : MonoBehaviour
    {
        public Text loadingText;

        void Start()
        {
            SetText(Localization.language == SystemLanguage.Russian ? "Загрузка..." : "Loading...");
        }

        public void SetText(string text)
        {
            if (loadingText) loadingText.text = text;
        }
    }
}