using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PopupEventNewYearHats : Popup
{
    public Text HNYText;
    public Image hatsImage;
    public Text hatsText;
    public Text collectHatsText;
    public GameObject catbox;
    public Text timerText;
    public GameObject buyButton;

    public override void Init()
    {
        HNYText.text = Localization.Get(DateTime.Now > Events.newYear.mainDate ? "happyNewYear" : "happyNewYearSoon");

        hatsText.text = user.newYearHats + " / " + balance.events.hatsForGift;
        if (user.newYearHats < balance.events.hatsForGift && !build.premium)
        {
            buyButton.SetActive(true);
            collectHatsText.text = Localization.Get("collectHatsToGetCat");
        }
        else
        {
            buyButton.SetActive(false);
            collectHatsText.text = Localization.Get("collectHatsToGetCatDone");
        }

        StartCoroutine(ShakeCatbox());

        if (Events.newYear.isActive)
        {
            timerText.gameObject.SetActive(true);
            StartCoroutine(Timer());
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }

    IEnumerator Timer()
    {
        while (Events.newYear.isActive)
        {
            timerText.text = Events.newYear.timeLeft.Localize();
            yield return new WaitForSeconds(1f);
        }
        Init();
    }

    IEnumerator ShakeCatbox()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            iTween.ShakeRotation(catbox, new Vector3(0, 0, 6), 1);
        }
    }

    public override void Reset()
    {
        StopAllCoroutines();
    }

    public void GetCat()
    {
        if (user.newYearHats < balance.events.hatsForGift)
        {
            iTween.PunchScale(hatsImage.gameObject, new Vector3(0.5f, 0.5f, 0), 1);
        }
        else
        {
            user.newYearHats -= balance.events.hatsForGift;
            user.GetCat(CatType.GetCatType(Cats.Santa));
            ui.getCatbox.Setup(ui.getCatbox.catboxNY);
            ui.PopupShow(ui.getCatbox);
        }
    }
}