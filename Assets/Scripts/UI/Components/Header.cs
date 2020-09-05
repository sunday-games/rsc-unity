using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;

public class Header : Core
{
    public float showTime = 0.3f;
    public Vector2 hidePosition = new Vector2(0f, 112f);
    public Color fontColor;

    [Space(10)]
    public Text levelText;
    public Image levelImage;

    [Space(10)]
    public GameObject coinsSlot;
    public Vector2 coinsPosition = new Vector2(298f, 0f);
    public Vector2 coinsWithBatsPosition = new Vector2(385f, 0f);
    public Text coinsText;

    [Space(10)]
    public Text hatsText;

    [Space(10)]
    public Text heartsText;

    [Space(10)]
    public Text batsText;

    [Space(10)]
    public Text saleTimerText;

    [Space(10)]
    public GameObject shopButton;
    public AdLabel adLabel;

    RectTransform _rectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null) _rectTransform = transform as RectTransform;
            return _rectTransform;
        }
    }

    void Awake()
    {
        shopButton.SetActive(!build.premium);
    }

    public void Show()
    {
        rectTransform.DOAnchorPos(Vector2.zero, showTime).SetEase(Ease.OutQuad);

        UpdateLevel();

        UpdateHats();

        UpdateHearts();

        UpdateBats();

        shopButton.SetActive(!build.premium && Missions.isGoldfishes);

        if (Events.sale.isActive) StartCoroutine("SaleTimer");
    }

    public void Hide()
    {
        rectTransform.DOAnchorPos(hidePosition, showTime).SetEase(Ease.InQuad);

        StopAllCoroutines();
    }

    public void UpdateAll()
    {
        UpdateCoins(force: true);

        UpdateLevel();

        UpdateHats();

        UpdateHearts();

        UpdateBats();

        shopButton.SetActive(Missions.isGoldfishes);
    }

    IEnumerator SaleTimer()
    {
        while (Events.sale.isActive)
        {
            saleTimerText.text = Events.sale.timeLeft.Localize(SG_Utils.DataTimeFormats.Two);
            yield return new WaitForSeconds(1f);
        }
    }

    public void UpdateCoins(bool force = false)
    {
        coinsSlot.SetActive(Missions.isGoldfishes);

        coinsTarget = user.coins - ui.prepare.cost - ui.boosts.cost;

        if (force) coins = coinsTarget;
    }

    public void UpdateHats()
    {
        if (hatsText != null) hatsText.text = user.newYearHats.ToString();
    }

    public void UpdateHearts()
    {
        if (heartsText != null) heartsText.text = user.stValentinHearts.ToString();
    }

    public void UpdateBats()
    {
        if (batsText != null) batsText.text = user.halloweenBats.ToString();
        (coinsSlot.transform as RectTransform).anchoredPosition = Events.halloween.isActive && !Events.halloween.isHaveGift ? coinsWithBatsPosition : coinsPosition;
    }

    public void UpdateLevel()
    {
        levelText.text = user.level.ToString();

        levelImage.fillAmount = 0;
        if (user.isLevelOK)
        {
            foreach (Mission mission in Missions.LEVELS[user.level].missions)
                if (mission.isDone) levelImage.fillAmount += 1f / 3f;
        }
        else levelImage.fillAmount = 1;
    }

    [HideInInspector]
    public int coinsTarget;
    int _coins;
    public int coins
    {
        get { return _coins; }
        set
        {
            _coins = value;

            coinsText.text = _coins.SpaceFormat();
            coinsText.color = _coins < 0 ? Color.red : fontColor;
        }
    }
    void Update()
    {
        if (coins != coinsTarget)
        {
            int c = coins - coinsTarget;

            if (c < 0)
            {
                if (c < -1000) coins += 100;
                else if (c < -100) coins += 50;
                else if (c < -10) coins += 10;
                else ++coins;
            }
            else
            {
                if (c > 1000) coins -= 100;
                else if (c > 100) coins -= 50;
                else if (c > 10) coins -= 10;
                else --coins;
            }
        }
    }

    public void ShowCoinsIn(Vector3 from, int count, Transform parent, float scale = 1f, float shift = 0.5f, float delay = 0.3f)
    {
        StartCoroutine(FlyingCoinsIn(from, coinsText.transform, 8, parent, scale, shift, delay));
    }
    IEnumerator FlyingCoinsIn(Vector3 from, Transform to, int count, Transform parent, float scale, float shift, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            Mover.Create(ui.game.coinPrefab, parent, from, to, UnityEngine.Random.Range(0, shift), scale);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(delay);

        sound.Play(sound.getCoins);
        UpdateCoins();
    }

    public void ShowCoinsOut(Transform to, int count, Transform parent, float scale = 1f, float shift = 0.5f)
    {
        StartCoroutine(FlyingCoinsOut(ui.header.coinsText.transform.position, to, 8, parent, scale, shift));
    }
    IEnumerator FlyingCoinsOut(Vector3 from, Transform to, int count, Transform parent, float scale, float shift)
    {
        sound.Play(sound.getCoins);
        UpdateCoins();

        for (int i = 0; i < count; i++)
        {
            Mover.Create(ui.game.coinPrefab, parent, from, to, UnityEngine.Random.Range(0, shift), scale);
            yield return new WaitForEndOfFrame();
        }
    }
}
