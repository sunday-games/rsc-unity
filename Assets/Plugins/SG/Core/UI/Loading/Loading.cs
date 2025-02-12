using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public class Loading : MonoBehaviour
    {
        public Text MainText;
        public Text DotsText;
        public Button CloseButton;

        [System.NonSerialized] private List<string> _list = new List<string>();

        public bool IsShown => gameObject.activeSelf;

        public void Show(string text = null)
        {
            if (text.IsNotEmpty() && !_list.Contains(text))
                _list.Add(text);

            MainText.SetText(text ?? "Loading");
            DotsText?.SetText("");

            gameObject.SetActive(true);
        }

        public void Hide() => Hide(text: null);
        public void Hide(string text)
        {
            if (text.IsNotEmpty())
                _list.Remove(text);
            else
                _list.Clear();

            if (_list.Count > 0)
                MainText.text = _list[0];
            else
                gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            StopAllCoroutines();

            if (DotsText)
            {
                StartCoroutine(DotUpdater());
                IEnumerator DotUpdater()
                {
                    while (true)
                    {
                        yield return new WaitForSeconds(1f);

                        if (DotsText.text == "")
                            DotsText.text = ".";
                        else if (DotsText.text == ".")
                            DotsText.text = "..";
                        else if (DotsText.text == "..")
                            DotsText.text = "...";
                        else // if (DotsText.text == "...")
                            DotsText.text = "";
                    }
                }
            }

            if (CloseButton)
            {
                CloseButton.Deactivate();

                StartCoroutine(CloseButtonUpdater());
                IEnumerator CloseButtonUpdater()
                {
                    yield return new WaitForSeconds(5f);
                    CloseButton.Activate();
                }
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]

        [Button("TestShow_OnClick")] public bool TestShow;
        public void TestShow_OnClick() => Show(Random.Range(1000, 9999).ToString());

        [Button("TestHide_OnClick")] public bool TestHide;
        public void TestHide_OnClick() => Hide(_list.Count > 0 ? _list[0] : null);
#endif
    }
}