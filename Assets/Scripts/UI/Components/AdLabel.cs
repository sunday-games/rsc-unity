using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace SG.RSC
{
    public class AdLabel : Core
    {
        public GameObject label;
        public Text rewardText;

        int reward;

        bool needShow = false; // Эта фигня написана в те вемена когда isReadyVideoRewarded не давал ответ сразу, а с колбеком. Потом на всякий случай не стал убирать

        public void TryShow()
        {
            if (showedTimes > 2 || Random.value > 0.3f) return;

            needShow = true;

            if (ads.isReadyRewarded) Show();
        }

        void Show()
        {
            if (ui.current != ui.prepare || !needShow) return;
            needShow = false;

            Analytic.EventProperties("Ads", "VideoRewarded Label", "Show");

            reward = balance.reward.coinsForAdView + Random.Range(0, 4) * 50;
            rewardText.text = reward.ToString();

            transform.localScale = new Vector3(1f, 0f, 1f);
            gameObject.SetActive(true);
            transform.DOScaleY(1f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (!user.IsTutorialShown(Tutorial.Part.VideoRewarded))
                    ui.tutorial.Show(Tutorial.Part.VideoRewarded, new Transform[] { label.transform }, reward.SpaceFormat());
            });
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;

            transform.DOScaleY(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => { gameObject.SetActive(false); });
        }

        int showedTimes = 0;
        public void ShowAds()
        {
            Analytic.EventProperties("Ads", "VideoRewarded Label", "Click");

            ads.ShowRewarded(
                () =>
                {
                    ++showedTimes;
                    user.UpdateCoins(reward, true);
                    ui.header.ShowCoinsIn(rewardText.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.2f);
                    Hide();
                },
                () =>
                {
                    ++showedTimes;
                    Hide();
                });
        }
    }
}