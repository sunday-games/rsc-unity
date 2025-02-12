using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class PopupResult : Popup
    {
        public Vector2 scorePosition = new Vector2(0, 120);
        public Vector2 scoreEventPosition = new Vector2(0, 140);
        public RectTransform score;
        public Text scoreText;

        [Space(10)]
        public Vector2 earnedPosition = new Vector2(0, -10);
        public Vector2 earnedEventPosition = new Vector2(0, 10);
        public RectTransform earned;
        public Text coinsText;
        public GameObject hat;
        public GameObject heart;
        public GameObject bat;

        [Space(10)]
        public GameObject button;
        public Text buttonText;

        public override void Init()
        {
            if (user.level < 1) user.LevelUp();

            scoreText.text = gameplay.score.SpaceFormat();

            earned.gameObject.SetActive(Missions.isGoldfishes);

            if (Missions.isGoldfishes && gameplay.coins > 0)
            {
                ui.header.coins = gameplay.oldCoins - ui.prepare.cost;
                ui.header.coinsTarget = gameplay.oldCoins - ui.prepare.cost;

                coinsText.text = gameplay.coins.SpaceFormat();
                buttonText.text = Localization.Get("collect");
            }
            else
            {
                coinsText.text = "0";
                buttonText.text = Localization.Get("next");
            }

            if (Events.newYear.isItemGet || Events.stValentin.isItemGet || Events.halloween.isItemGet)
            {
                score.anchoredPosition = scoreEventPosition;
                earned.anchoredPosition = earnedEventPosition;
            }
            else if (Missions.isGoldfishes)
            {
                score.anchoredPosition = scorePosition;
                earned.anchoredPosition = earnedPosition;
            }
            else
            {
                score.anchoredPosition = Vector2.zero;
            }

            if (hat != null) hat.gameObject.SetActive(Events.newYear.isItemGet);
            Events.newYear.isItemGet = false;

            if (heart != null) heart.gameObject.SetActive(Events.stValentin.isItemGet);
            Events.stValentin.isItemGet = false;

            if (bat != null) bat.gameObject.SetActive(Events.halloween.isItemGet);
            Events.halloween.isItemGet = false;

            button.SetActive(true);

            user.blockSave = false;
        }

        public override void OnEscapeKey() { Next(); }


        public void Next()
        {
            gameplay.ResetGame();

            if (Missions.isGoldfishes)
            {
                if (gameplay.coins > 0)
                    ui.header.ShowCoinsIn(coinsText.transform.position, 12, ui.canvas[3].transform, shift: 0.3f, delay: 0.8f);
                else
                    ui.header.UpdateCoins(force: true);
            }

            ads.ShowInterstitial(true, true, true, OpenPrepareOrRateApp, OpenPrepareOrRateApp, OpenPrepareOrRateApp);
        }

        public void OpenPrepareOrRateApp()
        {
            previous = ui.prepare;

            if (user.gameSessions - PlayerPrefs.GetInt("RateApp", 0) > balance.showRateAppEveryGame && // Показывать Rate App раз в 50 сессий
                (platform == Platform.iOS || platform == Platform.Android))
                previous = ui.rateApp;

            ui.PopupClose();
        }

        public override void Reset()
        {
            music.Switch(music.menu, 0.6f);
        }
    }
}