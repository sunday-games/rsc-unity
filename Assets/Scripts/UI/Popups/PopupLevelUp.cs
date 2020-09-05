using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupLevelUp : Popup
{
    public Text levelText;
    public Image giftImage;
    public Image multiplierImage;
    public Text multiplierText;
    public Text descriptionText;
    public Text buttonText;
    public Text nextCatboxText;

    Level level;
    public override void Init()
    {
        sound.Play(sound.winPrize);

        level = Missions.LEVELS[user.level - 1];

        levelText.text = Localization.Get("level", user.level);

        buttonText.text = Localization.Get(level.gift == Gifts.catbox || level.gift == Gifts.boosterbox ? "open" : "collect");

        descriptionText.text = level.giftDescription();

        nextCatboxText.text = string.Empty;
        if (level.gift == Gifts.catbox) // if (user.collection.Count > 0)
            for (int i = user.level; i < Missions.MAX_LEVEL; i++)
                if (Missions.LEVELS[i].gift == Gifts.catbox)
                {
                    nextCatboxText.text = Localization.Get("nextCatbox", i + 1);
                    break;
                }

        if (level.giftSprite != null)
        {
            giftImage.gameObject.SetActive(true);
            giftImage.sprite = level.giftSprite;

            if (level.giftSprite == pics.multiplier)
            {
                if (isRiki)
                {
                    multiplierText.gameObject.SetActive(true);
                    multiplierText.text = "x" + Missions.maxMultiplier;
                }
                else
                {
                    multiplierImage.gameObject.SetActive(true);
                    multiplierImage.sprite = pics.multipliers[Missions.maxMultiplier - 2];
                }
            }
            else
            {
                if (isRiki) multiplierText.gameObject.SetActive(false);
                else multiplierImage.gameObject.SetActive(false);
            }
        }
        else giftImage.gameObject.SetActive(false);
    }

    public override void Reset()
    {
    }

    public void GetGift()
    {
        if (level.giftSprite == pics.tournament || level.giftSprite == pics.championship)
        {
            ui.PopupShow(ui.main);
        }
        else if (level.giftSprite == pics.luckyWheel)
        {
            ui.PopupShow(ui.luckyWheel);
            ui.luckyWheel.previous = ui.main;
        }
        else if (level.gift == Gifts.catbox)
        {
            ui.getCatbox.Setup(ui.getCatbox.catboxSimple);
            ui.PopupShow(ui.getCatbox);
        }
        else if (level.giftSprite == pics.boosterbox)
        {
            ui.PopupShow(ui.getBonusbox);
        }
        else
        {
            if (level.gift == Gifts.aquariumSmall)
                ui.header.ShowCoinsIn(descriptionText.transform.position, 15, ui.canvas[3].transform, shift: 0.4f, delay: 0.6f);

            if (level.giftSprite == pics.goldfish) ui.header.UpdateAll();

            ui.PopupClose();
        }
    }

    public override void OnEscapeKey() { GetGift(); }
}
