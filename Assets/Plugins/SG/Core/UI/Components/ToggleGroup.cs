using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.UI
{
    public class ToggleGroup : MonoBehaviour
    {
        public List<Toggle> Toggles { get; private set; }
        [HideInInspector]
        public Toggle Current;
        public int CurrentIndex => Toggles.IndexOf(Current);

        public Action<int> OnValueChanged;

        private void Awake()
        {
            Toggles = transform.GetComponentsInChildren<Toggle>(includeInactive: true).ToList();
            if (Toggles.Count > 0)
            {
                for (int i = 0; i < Toggles.Count; i++)
                    SetupToggle(Toggles[i], i);

                Toggles[0].SetValue(true, invokeEvent: false);
            }
        }

        public Toggle AddOption(string text)
        {
            foreach (var toggle in Toggles)
                if (!toggle.gameObject.activeSelf)
                {
                    toggle.SetText(text);
                    toggle.gameObject.SetActive(true);
                    return toggle;
                }

            var newToggle = Toggles[0].Copy();
            Toggles.Add(SetupToggle(newToggle, Toggles.Count, text));
            return newToggle;
        }

        public void ClearOptions()
        {
            Toggles.ForEach(toggle => toggle.gameObject.SetActive(false));
        }

        private Toggle SetupToggle(Toggle toggle, int index, string text = null)
        {
            toggle.group = this;

            toggle.SetCallback(isOn => { if (isOn) OnValueChanged?.Invoke(index); });

            toggle.SetText(text);

            return toggle;
        }
    }
}