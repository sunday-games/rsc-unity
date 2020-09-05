using UnityEngine;

public class Localize : Core
{
    public string key;

    [Space(10)]
    public string prefix;
    public int value;
    public bool upper = false;

    [Space(10)]
    public SystemLanguage[] showOnlyForThisLanguages;

    UnityEngine.UI.Text _textComponent;
    UnityEngine.UI.Text textComponent { get { return _textComponent ? _textComponent : _textComponent = GetComponent<UnityEngine.UI.Text>(); } }
    // TMPro.TextMeshProUGUI textMeshComponent;

    void Start()
    {
        Localization.localizeComponents.Add(this);

        // textMeshComponent = GetComponent<TMPro.TextMeshProUGUI>();
        UpdateText();
    }

    public void UpdateText()
    {
        if (string.IsNullOrEmpty(key)) key = name;

        var text = value != 0 ? key.Localize(value) : key.Localize();
        if (!string.IsNullOrEmpty(prefix)) text = prefix + text;
        if (upper) text = text.ToUpper();

        if (textComponent != null) textComponent.text = text;
        // else if (textMeshComponent != null) textMeshComponent.text = text;
    }

    void OnEnable()
    {
        if (showOnlyForThisLanguages == null || showOnlyForThisLanguages.Length == 0)
            return;

        foreach (var lang in showOnlyForThisLanguages)
            if (Localization.language == lang)
            {
                if (textComponent) textComponent.enabled = true;
                //if (textMeshComponent != null) textMeshComponent.enabled = true;
                foreach (var children in GetComponentsInChildren<Transform>(true))
                    if (children != transform) children.gameObject.SetActive(true);
                return;
            }

        if (textComponent) textComponent.enabled = false;
        //if (textMeshComponent != null) textMeshComponent.enabled = false;
        foreach (var children in GetComponentsInChildren<Transform>(true))
            if (children != transform) children.gameObject.SetActive(false);
    }
}