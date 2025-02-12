using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SG.RSC
{
    public class PopupEventStValentin : Popup
    {
        public Image heartImage;
        public Text heartText;
        public Text collectHeartsText;
        public GameObject catbox;
        public Text timerText;
        public GameObject buyButton;

        public override void Init()
        {
            heartText.text = user.stValentinHearts + " / " + balance.events.heartsForGift;
            buyButton.SetActive(user.stValentinHearts < balance.events.heartsForGift && !build.premium);
            collectHeartsText.text = Localization.Get(user.stValentinHearts < balance.events.heartsForGift ? "collectHeartsToGetCat" : "collectHeartsToGetCatDone");

            StartCoroutine(ShakeCatbox());

            if (Events.stValentin.isActive)
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
            while (Events.stValentin.isActive)
            {
                timerText.text = Events.stValentin.timeLeft.Localize();
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
            if (user.stValentinHearts < balance.events.heartsForGift)
            {
                iTween.PunchScale(heartImage.gameObject, new Vector3(0.5f, 0.5f, 0), 1);
            }
            else
            {
                user.stValentinHearts -= balance.events.heartsForGift;
                user.GetCat(CatType.GetCatType(Cats.Lady));
                ui.getCatbox.Setup(ui.getCatbox.catboxSV);
                ui.PopupShow(ui.getCatbox);
            }
        }
    }
}