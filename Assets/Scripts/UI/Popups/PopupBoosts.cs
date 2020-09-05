using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PopupBoosts : Popup
{
    [Space(10)]

    public RectTransform window;
    public Vector2 windowFullSize = new Vector2(545f, 650f);
    public Vector2 windowHalfSize = new Vector2(545f, 340f);

    [Space(10)]

    public GameObject title;
    public GameObject cat;
    public RectTransform bubble;
    public Text bubbleText;

    [Space(10)]

    public BoostView[] boostViews;

    public int cost
    {
        get
        {
            int _cost = 0;
            foreach (var boostView in boostViews)
                if (boostView.selected && boostView.boost.count < 1) _cost += boostView.boost.price;
            return _cost;
        }
    }

    public override void Init()
    {
        byte i = 0;
        foreach (var boostView in boostViews)
            if (boostView.boost.avalible())
            {
                ++i;
                boostView.root.SetActive(true);
            }
            else boostView.root.SetActive(false);

        if (i > 2)
        {
            title.SetActive(true);
            cat.SetActive(false);
            window.sizeDelta = windowFullSize;
        }
        else
        {
            title.SetActive(false);
            cat.SetActive(true);
            window.sizeDelta = windowHalfSize;

            bubbleText.text = Localization.Get("boostsDescription");
        }
    }

    public override void OnEscapeKey() { Back(); }
    public void Back()
    {
        ResetBoosts(withGoldfishes: true);

        ui.PopupClose();
    }

    public void ResetBoosts(bool withGoldfishes = false)
    {
        foreach (var boostView in boostViews)
            if (boostView.selected)
            {
                if (withGoldfishes) ui.header.ShowCoinsIn(boostView.boostParent.position, 8, ui.canvas[3].transform, scale: 0.8f);

                boostView.selected = false;
            }
    }

    public void Next()
    {
        if (user.coins < cost + ui.prepare.cost && !build.premium)
        {
            ui.notEnoughGoldfishes.mode = PopupNotEnoughGoldfishes.Mode.Boosts;
            ui.PopupShow(ui.notEnoughGoldfishes);
        }
        else if (user.coins < cost + ui.prepare.cost && build.premium)
        {
            iTween.Stop(ui.header.coinsText.gameObject);
            ui.header.coinsText.transform.localScale = Vector3.one;
            iTween.PunchScale(ui.header.coinsText.gameObject, Vector3.one, 1f);
        }
        else
        {
            foreach (var boostView in boostViews)
                if (boostView.selected) user.UseBoost(boostView.boost);

            if (ui.prepare.cost > 0) user.UpdateCoins(-ui.prepare.cost, false);

            ResetBoosts();

            gameplay.Play();
        }
    }

    public void ChangeBubbleText(string text)
    {
        bubbleText.text = text;
        bubble.localScale = Vector3.zero;
        bubble.DOScale(Vector3.one, 0.4f).SetEase(Ease.InOutBack);
    }
}
