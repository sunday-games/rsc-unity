using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace SG.RSC
{
    public class MedalShow : MonoBehaviour
    {
        public Transform medalParent;
        public Text descriptionText;
        public float time = 1f;
        public Vector2 hidePosition = new Vector2(0f, 112f);

        Medal medal;
        bool isShow = false;

        RectTransform _rectTransform;
        RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = transform as RectTransform;
                return _rectTransform;
            }
        }

        [ContextMenu("ShowTest")]
        public void ShowTest() { Show(Core.achievements.goldfishes); }

        public bool Show(Achievements.Achievement achievement)
        {
            if (isShow) return false;

            isShow = true;

            descriptionText.text = achievement.getText;
            medal = Medal.Create(medalParent, achievement);

            rectTransform.anchoredPosition = hidePosition;
            gameObject.SetActive(true);

            StartCoroutine(Showing());

            return true;
        }

        IEnumerator Showing()
        {
            rectTransform.DOAnchorPos(Vector2.zero, time).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(4f);

            rectTransform.DOAnchorPos(hidePosition, time).SetEase(Ease.InBack);
            yield return new WaitForSeconds(1f);

            Destroy(medal.gameObject);
            gameObject.SetActive(false);
            isShow = false;
        }
    }
}