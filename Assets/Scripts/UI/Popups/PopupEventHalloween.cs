using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PopupEventHalloween : Popup
{
    public Image batImage;
    public Text batText;
    public Text collectBatsText;
    public GameObject catbox;
    public Text timerText;
    public GameObject buyButton;

    public override void Init()
    {
        batText.text = user.halloweenBats + " / " + balance.events.batsForGift;
        buyButton.SetActive(user.halloweenBats < balance.events.batsForGift && !build.premium);
        collectBatsText.text = Localization.Get(user.halloweenBats < balance.events.batsForGift ? "collectBatsToGetCat" : "collectBatsToGetCatDone");

        StartCoroutine(ShakeCatbox());

        if (Events.halloween.isActive)
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
        while (Events.halloween.isActive)
        {
            timerText.text = Events.halloween.timeLeft.Localize();
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
        if (user.halloweenBats < balance.events.batsForGift)
        {
            iTween.PunchScale(batImage.gameObject, new Vector3(0.5f, 0.5f, 0), 1);
        }
        else
        {
            user.halloweenBats -= balance.events.batsForGift;
            user.GetCat(CatType.GetCatType(Cats.Jack));
            ui.getCatbox.Setup(ui.getCatbox.catboxHW);
            ui.PopupShow(ui.getCatbox);
        }
    }
}