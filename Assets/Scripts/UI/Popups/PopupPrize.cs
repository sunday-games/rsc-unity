using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupPrize : Popup
{
    public Text countText;
    public Image prizeImage;
    public Text collectButtonText;

    public override void Init()
    {
        sound.Play(sound.winPrize);

        if (countText.text == Localization.Get("catbox")) collectButtonText.text = Localization.Get("open");
        else collectButtonText.text = Localization.Get("collect");

        if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Goldfishes)
        {
            prizeImage.sprite = pics.goldfish;
            countText.text = ui.luckyWheel.lastPrizeCount.SpaceFormat();
        }
        else if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Catbox)
        {
            prizeImage.sprite = pics.catbox;
            countText.text = "";
        }
        else if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Sausage)
        {
            prizeImage.sprite = pics.sausage;
            countText.text = "";
        }
        else if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Hats)
        {
            prizeImage.sprite = pics.hat;
            countText.text = ui.luckyWheel.lastPrizeCount.SpaceFormat();
        }
        else if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Valentines)
        {
            prizeImage.sprite = pics.heart;
            countText.text = ui.luckyWheel.lastPrizeCount.SpaceFormat();
        }
        else if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Bats)
        {
            prizeImage.sprite = pics.bat;
            countText.text = ui.luckyWheel.lastPrizeCount.SpaceFormat();
        }
    }

    public void Get()
    {
        if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Catbox)
        {
            ui.getCatbox.Setup(ui.getCatbox.catboxSimple);
            ui.PopupShow(ui.getCatbox);
            return;
        }

        if (ui.luckyWheel.lastPrize == PopupLuckyWheel.Prizes.Goldfishes)
            ui.header.ShowCoinsIn(countText.transform.position, 15, ui.canvas[3].transform, shift: 0.4f, delay: 0.8f);

        ui.PopupClose();
    }

    public override void OnEscapeKey() { Get(); }
}
