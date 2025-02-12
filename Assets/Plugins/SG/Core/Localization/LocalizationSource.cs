using UnityEngine;
using System.Collections;

namespace SG
{
    public class LocalizationSource : MonoBehaviour
    {
        public string SpreadsheetId = "1z5eMWzCndlX2FHkHnh_CAcEWcxrouVyHVl5dWeRwIa0";
        public string SheetId = "0";

        public TextAsset File => Resources.Load<TextAsset>("Localization/" + name);

        [UI.Button("Open")] public bool open;
        public void Open() =>
            UI.Helpers.OpenLink($"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/edit#gid={SheetId}");

#if UNITY_EDITOR
        public void Download()
        {
            new Download($"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/export?gid={SheetId}&format=csv")
                .SetCallback(download =>
                {
                    if (download.success)
                        Utils.SaveToFile(Configurator.resourcesPath + $"/Localization/{name}.csv", download.responseText);
                })
                .Run();
        }
#endif
    }
}