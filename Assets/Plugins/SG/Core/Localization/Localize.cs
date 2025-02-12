using UnityEngine;
using System.Collections.Generic;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;

namespace SG
{
    public class Localize : MonoBehaviour
    {
        public string key;

        [Space]
        public string prefix;
        public int value;

        [Space]
        public SystemLanguage[] showOnlyForThisLanguages;

        public bool useInSubelements = true;

        private Text _text;
        private Dropdown _dropdown;
        private List<string> _dropdownKeys;

        private void Start()
        {
            if (key.IsEmpty())
            {
                enabled = false;
                return;
            }

            Localization.onLanguageChanged += UpdateText;

            _text = GetComponent<Text>();
            _dropdown = GetComponent<Dropdown>();

            if (_dropdown)
            {
                _dropdownKeys = new List<string>();
                foreach (var option in _dropdown.options)
                    _dropdownKeys.Add(option.text);
            }

            UpdateText(Localization.language);
        }

        private void UpdateText(SystemLanguage language)
        {
            if (key.IsEmpty())
                key = name;

            var text = value != 0 ? key.Localize(value) : key.Localize();

            if (!prefix.IsEmpty())
                text = prefix + text;

            if (_text)
            {
                _text.text = text;
            }
            else if (_dropdown)
            {
                _dropdown.captionText.text = _dropdownKeys[_dropdown.value].ToLower().Localize();
                if (useInSubelements)
                    for (int i = 0; i < _dropdown.options.Count; i++)
                    {
                        var option = _dropdown.options[i];
                        option.text = _dropdownKeys[i].ToLower().Localize();
                    }
            }
        }

        private void OnEnable()
        {
            if (showOnlyForThisLanguages == null || showOnlyForThisLanguages.Length == 0)
                return;

            foreach (var language in showOnlyForThisLanguages)
                if (Localization.language == language)
                {
                    if (_text)
                        _text.enabled = true;

                    foreach (var children in GetComponentsInChildren<Transform>(true))
                        if (children != transform)
                            children.gameObject.SetActive(true);
                    return;
                }

            if (_text)
                _text.enabled = false;

            foreach (var children in GetComponentsInChildren<Transform>(true))
                if (children != transform)
                    children.gameObject.SetActive(false);
        }
    }
}