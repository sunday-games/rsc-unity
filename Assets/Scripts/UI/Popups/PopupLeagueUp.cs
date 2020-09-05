using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupLeagueUp : Popup
{
    public GameObject window;

    public Text rewardText;
    public Text leagueNameText;
    public Text leagueText;
    public GameObject[] leagueFX;

    public GameObject shareButton;
    public Text shareBonusText;
    public GameObject bonus;

    public override void Init()
    {
        sound.Play(sound.winPrize);

        foreach (GameObject go in user.league.leagueUpFX) go.SetActive(true);
        leagueNameText.text = Localization.Get("league" + user.league.name);
        leagueNameText.color = user.league.color;
        leagueText.color = leagueNameText.color;

        rewardText.text = Localization.Get("timerReward", "+" + (gameplay.GetLeagueTimeBonus(user.permanentRecord) - gameplay.GetLeagueTimeBonus(gameplay.oldPermanentRecord)));

        shareButton.SetActive(fb.isLogin);
        bonus.SetActive(Missions.isGoldfishes);
        shareBonusText.text = balance.reward.coinsForShareLeagueUp.ToString();

        achievements.OnLeagueUp();
    }

    public override void AfterInit()
    {
        if (!user.IsTutorialShown(Tutorial.Part.LeagueUp))
        {
            if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.LeagueUp1, Tutorial.Part.LeagueUp }, new Transform[] { window.transform });
            else ui.tutorial.Show(Tutorial.Part.LeagueUp, new Transform[] { window.transform });
        }
    }

    public override void OnEscapeKey() { Next(); }

    public void Next() { ui.PopupShow(ui.result); }

    public override void PreReset()
    {
        foreach (GameObject go in leagueFX) go.SetActive(false);
    }

    public void ShareLeagueUp()
    {
        ui.Block();
        server.CheckConnection(succeess =>
        {
            if (succeess)
            {
#if FACEBOOK
                fb.Share(
                    Localization.Get("shareLeagueUp"),
                    Localization.Get("shareLeagueUpDescription", Localization.Get("league" + user.league.name) + " " + Localization.Get("league")),
                    fb.sharePicLinks.leagueUp, 
                    ShareResult,
                    "ShareLeagueUp");
#endif
            }
            else ui.Unblock();
        });
    }
    public void ShareResult(bool success)
    {
        Game.ui.Unblock();

        if (success)
        {
            shareButton.SetActive(false);
            user.UpdateCoins(balance.reward.coinsForShareLeagueUp, true);
            ui.header.ShowCoinsIn(shareButton.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.7f);
        }
    }
}