using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PopupLuckyWheel : Popup
{
    [Space(10)]
    public GameObject window;
    [Space(10)]
    public Image arrowImage;
    public Vector2 arrowImagePosition = new Vector2(0, 260);
    public Text timerText;
    public GameObject loading;
    public GameObject noConnection;
    public Image spinsImage;
    public Text spinsText;
    [Space(10)]
    public Animator catAnimator;
    public Animator wheelAnimator;
    public Image wheelImage;
    [Space(10)]
    public GameObject buySpinsButton;
    [Space(10)]
    public GameObject hatsPrize;
    public GameObject heartsPrize;
    public GameObject batsPrize;
    public GameObject coins5000Prize;
    [Space(10)]
    public GameObject catboxPrize;
    public GameObject coins20000Prize;

    Vector3 windowUnactiveScale = new Vector3(0.8f, 0.8f, 1f);

    DateTime utcDateTimeNow;

    bool readyToSpin = false;

    public override void Init()
    {
        readyToSpin = false;

        spinsText.text = user.TotalSpins(DateTime.UtcNow).ToString();
        window.transform.localScale = windowUnactiveScale;
        loading.SetActive(false);
        timerText.gameObject.SetActive(false);
        noConnection.SetActive(false);
        buySpinsButton.gameObject.SetActive(!build.premium);
        catAnimator.SetBool("sausage", false);
        arrowImage.gameObject.SetActive(false);

        if (user.spins > 0)
        {
            ReadyToSpin();
        }
        else
        {
            loading.SetActive(true);

            server.UpdateTime(success =>
            {
                loading.SetActive(false);

                utcDateTimeNow = server.timeUTC;

                if (!success)
                {
                    loading.SetActive(false);
                    noConnection.SetActive(true);
                }
                else if (user.IsFreeSpin(utcDateTimeNow))
                {
                    ReadyToSpin();
                }
                else
                {
                    timerText.gameObject.SetActive(true);

                    StartCoroutine(LuckyWheelTimer());
                }
            });
        }

        coins5000Prize.SetActive(false);
        if (hatsPrize != null) hatsPrize.SetActive(false);
        if (heartsPrize != null) heartsPrize.SetActive(false);
        if (batsPrize != null) batsPrize.SetActive(false);

        if (Events.newYear.isActive && !Events.newYear.isHaveGift) hatsPrize.SetActive(true);
        else if (Events.stValentin.isActive && !Events.stValentin.isHaveGift) heartsPrize.SetActive(true);
        else if (Events.halloween.isActive && !Events.halloween.isHaveGift) batsPrize.SetActive(true);
        else coins5000Prize.SetActive(true);

        catboxPrize.SetActive(false);
        coins20000Prize.SetActive(false);
        if (user.isCanGetSimpleBox) catboxPrize.SetActive(true);
        else coins20000Prize.SetActive(true);
    }

    void ReadyToSpin()
    {
        readyToSpin = true;
        loading.SetActive(false);
        buySpinsButton.gameObject.SetActive(false);
        window.transform.localScale = Vector3.one;

        catAnimator.SetBool("sausage", true);
        arrowImage.gameObject.SetActive(true);
        iTween.MoveAdd(arrowImage.gameObject, iTween.Hash(
            "y", 1, "easeType", "easeInOutQuad", "loopType", "pingPong", "time", 1f));
    }


    public override void AfterInit()
    {
        if (!user.IsTutorialShown(Tutorial.Part.LuckyWheel))
        {
            if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.LuckyWheel1, Tutorial.Part.LuckyWheel }, new Transform[] { window.transform });
            else ui.tutorial.Show(Tutorial.Part.LuckyWheel, new Transform[] { window.transform });
        }
        else if (user.TotalSpins(DateTime.UtcNow) <= 0)
            TutorialBuySausages();
    }

    public void TutorialBuySausages()
    {
        if (!user.IsTutorialShown(Tutorial.Part.BuySausages) && !build.premium)
            ui.tutorial.Show(Tutorial.Part.BuySausages, new Transform[] { buySpinsButton.transform });
    }


    IEnumerator LuckyWheelTimer()
    {
        float startTime;

        while (!user.IsFreeSpin(utcDateTimeNow))
        {
            timerText.text = user.TimeToNextFreeSpin(utcDateTimeNow).Localize();

            startTime = Time.unscaledTime;
            while (Time.unscaledTime - startTime < 1f) yield return new WaitForEndOfFrame();

            utcDateTimeNow += new TimeSpan(0, 0, (int)(Time.unscaledTime - startTime));
        }
        Init();
    }

    public override void Reset()
    {
        spin = false;
        iTween.Stop(arrowImage.gameObject);
        arrowImage.rectTransform.anchoredPosition = arrowImagePosition;
        StopAllCoroutines();
    }

    bool spin = false;
    public void StartSpin()
    {
        if (readyToSpin)
        {
            if (!spin) StartCoroutine(Spin());
        }
        else
        {
            if (timerText.gameObject.activeSelf) iTween.PunchScale(timerText.gameObject, halfVector3, 1);
            else if (noConnection.activeSelf) iTween.PunchScale(noConnection, halfVector3, 1);
        }
    }


    // оборот 0.92, деление	0.155
    public float[] intervals = new float[] { 48f, 108f, 168f, 228f, 288f, 348f };
    // public float[] t1 = new float[] { 0.2f, 0.3f, 0.4f, 1.2f, 1.3f, 1.4f };
    // public float[] t2 = new float[] { 0.1f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };

    IEnumerator Spin()
    {
        spin = true;

        sound.Play(sound.luckyWheel);

        spinsText.text = (user.TotalSpins(DateTime.UtcNow) - 1).ToString();
        iTween.PunchScale(spinsImage.gameObject, new Vector3(0.5f, 0.5f, 0), 1);
        arrowImage.gameObject.SetActive(false);

        catAnimator.speed = 1f;
        catAnimator.SetBool("spin", true);
        catAnimator.SetBool("sausage", false);

        wheelAnimator.SetBool("spin", true);
        wheelAnimator.speed = 0.1f;
        while (wheelAnimator.speed < 1f)
        {
            wheelAnimator.speed = Mathf.Clamp(wheelAnimator.speed + 0.5f * Time.deltaTime, 0f, 1f);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));

        sound.Play(sound.punch);
        catAnimator.SetBool("spin", false);
        while (wheelAnimator.speed > 0f)
        {
            wheelAnimator.speed = Mathf.Clamp(wheelAnimator.speed - 0.4f * Time.deltaTime, 0f, 1f);
            catAnimator.speed = Mathf.Clamp(catAnimator.speed - 0.4f * Time.deltaTime, 0f, 1f);
            yield return new WaitForEndOfFrame();
        }
        wheelAnimator.SetBool("spin", false);

        float angle = wheelImage.rectTransform.rotation.eulerAngles.z;

        if (intervals[0] <= angle && angle < intervals[1])
        {
            if (hatsPrize && hatsPrize.activeSelf)
            {
                user.newYearHats += 3;
                lastPrize = Prizes.Hats;
                lastPrizeCount = 3;
                Analytic.EventProperties("Other", "LuckyWheel", "3 Hats");
            }
            else if (heartsPrize && heartsPrize.activeSelf)
            {
                user.stValentinHearts += 3;
                lastPrize = Prizes.Valentines;
                lastPrizeCount = 3;
                Analytic.EventProperties("Other", "LuckyWheel", "3 Valentines");
            }
            else if (batsPrize && batsPrize.activeSelf)
            {
                user.halloweenBats += 3;
                lastPrize = Prizes.Bats;
                lastPrizeCount = 3;
                Analytic.EventProperties("Other", "LuckyWheel", "3 Bats");
            }
            else
            {
                lastPrize = Prizes.Goldfishes;
                lastPrizeCount = 5000;
                user.UpdateCoins(lastPrizeCount, false);
                Analytic.EventProperties("Other", "LuckyWheel", "5 000 Goldfishes");
            }
        }
        else if (intervals[1] <= angle && angle < intervals[2])
        {
            lastPrize = Prizes.Goldfishes;
            lastPrizeCount = 50;
            user.UpdateCoins(lastPrizeCount, false);
            Analytic.EventProperties("Other", "LuckyWheel", "50 Goldfishes");
        }
        else if (intervals[2] <= angle && angle < intervals[3])
        {
            if (user.isCanGetSimpleBox)
            {
                lastPrize = Prizes.Catbox;
                lastPrizeCount = 1;
                user.GetSimpleCatbox();
                Analytic.EventProperties("Other", "LuckyWheel", "Catbox");
            }
            else
            {
                lastPrize = Prizes.Goldfishes;
                lastPrizeCount = 20000;
                user.UpdateCoins(lastPrizeCount, false);
                Analytic.EventProperties("Other", "LuckyWheel", "20 000 Goldfishes");
            }
        }
        else if (intervals[3] <= angle && angle < intervals[4])
        {
            lastPrize = Prizes.Goldfishes;
            lastPrizeCount = 100;
            user.UpdateCoins(lastPrizeCount, false);
            Analytic.EventProperties("Other", "LuckyWheel", "100 Goldfishes");
        }
        else if (intervals[4] <= angle && angle < intervals[5])
        {
            lastPrize = Prizes.Sausage;
            lastPrizeCount = 1;
            user.UpdateSpins(lastPrizeCount, false);
            Analytic.EventProperties("Other", "LuckyWheel", "Sausage");
        }
        else // (intervals[5] <= angle || angle < intervals[1])
        {
            lastPrize = Prizes.Goldfishes;
            lastPrizeCount = 500;
            user.UpdateCoins(lastPrizeCount, false);
            Analytic.EventProperties("Other", "LuckyWheel", "500 Goldfishes");
        }

        if (user.spins > 0) user.UseSpin();
        else user.UseFreeSpin(utcDateTimeNow);

        sound.Stop(sound.luckyWheel);
        spin = false;

        ui.PopupShow(ui.prize);
    }

    public enum Prizes { Goldfishes, Catbox, Sausage, Hats, Valentines, Bats }
    [HideInInspector]
    public Prizes lastPrize;
    [HideInInspector]
    public int lastPrizeCount;
}