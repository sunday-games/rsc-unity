using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

namespace SG.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextWithUrls : MonoBehaviour, IPointerClickHandler
    {
        private const string SEARCH_PATTERN = "((http://|https://|www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})";

        public string HyperlinkFormat;

        private TextMeshProUGUI _source;
        private TextMeshProUGUI Source
        {
            get
            {
                if (_source == null)
                    _source = GetComponent<TextMeshProUGUI>();
                return _source;
            }
        }

        public void SetTextAndActivate(string text)
        {
            if (text.IsNotEmpty())
            {
                var regx = new Regex(SEARCH_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var matches = regx.Matches(text);
                foreach (Match match in matches)
                    text = text.Replace(match.Value, string.Format(HyperlinkFormat, match.Value, match.Value));
            }

            Source.SetTextAndActivate(text);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(Source, eventData.position, eventData.pressEventCamera);
            if (linkIndex == -1)
                return;

            var linkInfo = Source.textInfo.linkInfo[linkIndex];
            var selectedLink = linkInfo.GetLinkID();
            if (selectedLink.IsNotEmpty())
                Helpers.OpenLink(selectedLink);
        }
    }
}