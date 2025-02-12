using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupHighscore : Popup
    {
        public GameObject window;
        public Text scoreText;

        public GameObject shareButton;
        public Text shareBonusText;
        public GameObject bonus;

        public override void Init()
        {
            scoreText.text = gameplay.score.SpaceFormat();
            iTween.PunchScale(scoreText.gameObject, new Vector3(1, 1, 0), 1);

            sound.Play(sound.winPrize);

            shareButton.SetActive(fb.isLogin);
            bonus.SetActive(Missions.isGoldfishes);
            shareBonusText.text = balance.reward.coinsForShareHighscore.ToString();
        }

        public override void Reset() { }

        public override void OnEscapeKey() { Next(); }

        public void Next()
        {
            if (gameplay.GetLeague(gameplay.score) != gameplay.GetLeague(gameplay.oldPermanentRecord) && Missions.isChampionship)
                ui.PopupShow(ui.leagueUp);
            else
                ui.PopupShow(ui.result);
        }

        public void ShareHighscore()
        {
            ui.Block();
            server.CheckConnection(succeess =>
            {
                if (succeess)
                {
#if FACEBOOK
                fb.Share(
                    Localization.Get("newHighScore"), 
                    Localization.Get("shareBeatMe" + user.gender, gameplay.score.SpaceFormat()), 
                    fb.sharePicLinks.highscore,
                    ShareResult,
                    "ShareHighscore");
#endif
            }
                else ui.Unblock();
            });
        }
        public void ShareResult(bool success)
        {
            ui.Unblock();

            if (success)
            {
                shareButton.SetActive(false);
                user.UpdateCoins(balance.reward.coinsForShareHighscore, true);
                ui.header.ShowCoinsIn(shareButton.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.7f);
            }
        }
    }
}