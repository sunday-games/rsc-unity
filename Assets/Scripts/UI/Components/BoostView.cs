using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoostView : Core
{
    public Boost boost;

    [Space(10)]

    public GameObject root;

    [Space(10)]

    public Transform boostParent;
    public Image boostImage;
    public GameObject lightImage;

    [Space(10)]

    public Button button;
    public Image buttonImage;
    public Text buttonText;

    [Space(10)]

    public Image priceImage;
    public Text priceText;

    [Space(10)]

    public Text countText;

    [HideInInspector]
    public bool selected = false;

    Color unactiveBoostImageColor = new Color(1f, 1f, 1f, 0.7f);

    Color unactiveButtonColor = new Color(0.6f, 0.6f, 0.6f);

    void OnEnable()
    {
        if (boost == gameplay.boosts.experience && ui.prepare.cost == 0)
        {
            button.interactable = false;
            buttonImage.color = unactiveButtonColor;
            buttonText.color = unactiveButtonColor;
        }
        else
        {
            button.interactable = true;
            buttonImage.color = Color.white;
            buttonText.color = Color.white;
        }

        priceText.text = boost.price.SpaceFormat();

        UpdateView();
    }

    public void Select()
    {
        selected = !selected;

        iTween.PunchScale(boostImage.gameObject, selected ? new Vector3(0.3f, 0.3f, 0f) : new Vector3(-0.3f, -0.3f, 0f), 1f);

        if (boost.count < 1)
        {
            if (selected) ui.header.ShowCoinsOut(boostParent, 8, boostParent, scale: 0.8f);
            else ui.header.ShowCoinsIn(boostParent.position, 8, boostParent, scale: 0.8f);
        }

        UpdateView();
    }

    public void ShowDescription()
    {
        if (ui.boosts.cat.activeSelf)
            ui.boosts.ChangeBubbleText(Localization.Get("tutorial" + boost.tutorialPart.name));
        else
            ui.tutorial.Show(boost.tutorialPart, new Transform[] { transform });
    }

    void UpdateView()
    {
        if (selected)
        {
            boostImage.color = Color.white;
            iTween.PunchScale(boostImage.gameObject, new Vector3(0.3f, 0.3f, 0f), 1f);
            lightImage.gameObject.SetActive(true);
            buttonImage.sprite = pics.buttons.orange;
            buttonText.text = Localization.Get("remove");
        }
        else
        {
            boostImage.color = unactiveBoostImageColor;
            iTween.PunchScale(boostImage.gameObject, new Vector3(-0.3f, -0.3f, 0f), 1f);
            lightImage.gameObject.SetActive(false);
            buttonImage.sprite = pics.buttons.green;
            buttonText.text = Localization.Get("take");
        }

        if (boost.count > 0)
        {
            priceImage.gameObject.SetActive(false);
            countText.gameObject.SetActive(true);

            if (selected) countText.text = Localization.Get("left", boost.count - 1);
            else countText.text = Localization.Get("left", boost.count);
        }
        else
        {
            priceImage.gameObject.SetActive(true);
            countText.gameObject.SetActive(false);
        }
    }
}
