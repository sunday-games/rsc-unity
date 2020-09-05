using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PopupShop : Popup
{
    [Space(10)]
    public Text saleTimer;

    [Space(10)]
    public RectTransform window;
    public float windowBaseHeight;
    public float itemHeight;
    public GameObject error;
    public Text errorText;
    public GameObject adFree;

    [Space(10)]
    public ShopSlot sausages;
    public ShopSlot goldfishes;
    public ShopSlot catboxPremium;
    public ShopSlot catboxSimple;
    public ShopSlot hats;
    public ShopSlot hearts;
    public ShopSlot bats;

    [HideInInspector]
    public int xGoldfishes = 1;
    public int aquarium
    {
        get
        {
            int coins = 0;

            foreach (CatItem catItem in user.collection) coins += catItem.cost;
            coins = Mathf.CeilToInt((float)coins * balance.shop.goldfish / 1000f) * 1000;
            return coins < 2000 ? 2000 : coins;
        }
    }

    [HideInInspector]
    public int xSausages = 1;

    public override void Init()
    {
        if (!build.ads || user.revenue > 0f || ads.interstitialFrequency < 0) adFree.SetActive(false);
        else
        {
            adFree.SetActive(true);
            iTween.RotateAdd(adFree, iTween.Hash("z", 10f, "easeType", "easeInOutQuad", "loopType", "pingPong", "time", 2f));
        }

        catboxPremium.gameObject.SetActive(user.isCanGetPremiumBox && user.collection.Count > 0);
        catboxSimple.gameObject.SetActive(!user.isCanGetPremiumBox && user.isCanGetSimpleBox && user.collection.Count > 0);
        goldfishes.gameObject.SetActive(Missions.isGoldfishes);
        sausages.gameObject.SetActive(Missions.isLuckyWheel);
        hats?.gameObject.SetActive(Events.newYear.isActive && !Events.newYear.isHaveGift);
        hearts?.gameObject.SetActive(Events.stValentin.isActive && !Events.stValentin.isHaveGift);
        bats?.gameObject.SetActive(Events.halloween.isActive && !Events.halloween.isHaveGift);

        byte n = 0;
        if (catboxPremium.gameObject.activeSelf) ++n;
        if (catboxSimple.gameObject.activeSelf) ++n;
        if (goldfishes.gameObject.activeSelf) ++n;
        if (sausages.gameObject.activeSelf) ++n;
        if (hats && hats.gameObject.activeSelf) ++n;
        if (hearts && hearts.gameObject.activeSelf) ++n;
        if (bats && bats.gameObject.activeSelf) ++n;
        window.sizeDelta = new Vector2(window.sizeDelta.x, windowBaseHeight + itemHeight * n);

        catboxPremium.descriptionText.text = Localization.Get("buyCatboxPremiumDescription");
        catboxSimple.descriptionText.text = Localization.Get("buyCatboxSimpleDescription");
        goldfishes.descriptionText.text = Localization.Get("buyGoldfishesDescription", aquarium.SpaceFormat());
        sausages.descriptionText.text = Localization.Get("buySausagesDescription", balance.shop.sausages);
        if (hats) hats.descriptionText.text = Localization.Get("buyHatsDescription", balance.events.hatsPurchase);
        if (hearts) hearts.descriptionText.text = Localization.Get("buyValentinesDescription", balance.events.heartsPurchase);
        if (bats) bats.descriptionText.text = Localization.Get("buyBatsDescription", balance.events.batsPurchase);

        if (!iapManager.isInitialized)
        {
            sausages.price.SetActive(false);
            goldfishes.price.SetActive(false);
            catboxPremium.price.SetActive(false);
            catboxSimple.price.SetActive(false);
            hats?.price.SetActive(false);
            hearts?.price.SetActive(false);
            bats?.price.SetActive(false);

            error.SetActive(true);
            errorText.text = Localization.Get("shopUnavalible");
            return;
        }

        sausages.priceText.text = iapManager.sausages.priceLocalized;
        goldfishes.priceText.text = iapManager.goldfishes.priceLocalized;
        catboxPremium.priceText.text = iapManager.catboxPremium.priceLocalized;
        catboxSimple.priceText.text = iapManager.catboxSimple.priceLocalized;
        if (hats) hats.priceText.text = iapManager.hats.priceLocalized;
        if (hearts) hearts.priceText.text = iapManager.valentines.priceLocalized;
        if (bats) bats.priceText.text = iapManager.bats.priceLocalized;

        sausages.price.SetActive(true);
        goldfishes.price.SetActive(true);
        catboxPremium.price.SetActive(true);
        catboxSimple.price.SetActive(true);
        hats?.price.SetActive(true);
        hearts?.price.SetActive(true);
        bats?.price.SetActive(true);

        error.SetActive(false);

        catboxPremium.sale.SetActive(false);
        sausages.sale.SetActive(false);
        goldfishes.sale.SetActive(false);

        if (Events.sale.isActive)
        {
            StartCoroutine("SaleTimer");

            foreach (Dictionary<string, object> saleItem in Events.sale.data)
            {
                if (IAP.FromName((string)saleItem["iap"]) == iapManager.catboxPremium)
                {
                    catboxPremium.sale.SetActive(true);
                    catboxPremium.saleText.text = iapManager.catboxPremiumSale.priceLocalized;
                }
                else if (IAP.FromName((string)saleItem["iap"]) == iapManager.sausages)
                {
                    sausages.sale.SetActive(true);
                    sausages.saleText.text = (balance.shop.sausages * xSausages).SpaceFormat();
                }
                else if (IAP.FromName((string)saleItem["iap"]) == iapManager.goldfishes)
                {
                    goldfishes.sale.SetActive(true);
                    goldfishes.saleText.text = (aquarium * xGoldfishes).SpaceFormat();
                }
            }
        }
    }

    public override void PreReset()
    {
        StopAllCoroutines();
    }

    IEnumerator SaleTimer()
    {
        while (Events.sale.isActive)
        {
            saleTimer.text = Events.sale.timeLeft.Localize();
            yield return new WaitForSeconds(1f);
        }
        Init();
    }

    public void SelectCatboxPremium()
    {
        Buy(catboxPremium.sale.activeSelf ? iapManager.catboxPremiumSale : iapManager.catboxPremium);
    }

    public void SelectCatboxSimple()
    {
        Buy(iapManager.catboxSimple);
    }

    public void SelectGoldfishes()
    {
        Buy(iapManager.goldfishes);
    }

    public void SelectSpins()
    {
        Buy(iapManager.sausages);
    }

    public void SelectHats()
    {
        Buy(iapManager.hats);
    }

    public void SelectHearts()
    {
        Buy(iapManager.valentines);
    }

    public void SelectBats()
    {
        Buy(iapManager.bats);
    }

    public void Buy(IAP iap)
    {
        iapManager.Purchase(iap, purchaseSuccess =>
            {
                if (purchaseSuccess)
                {
                    if (iap == iapManager.sausages) ui.PopupShow(ui.getSpins);
                    else if (iap == iapManager.goldfishes) ui.PopupShow(ui.getGoldfishes);
                    else if (iapManager.hats && iap == iapManager.hats) ui.PopupClose();
                    else if (iapManager.valentines && iap == iapManager.valentines) ui.PopupClose();
                    else if (iapManager.bats && iap == iapManager.bats)
                    {
                        ui.PopupShow(ui.halloween);
                        ui.halloween.previous = ui.prepare;
                    }
                    else if (iap == iapManager.catboxPremium || iap == iapManager.catboxPremiumSale)
                    {
                        ui.getCatbox.Setup(ui.getCatbox.catboxPremium);
                        ui.PopupShow(ui.getCatbox);
                    }
                    else if (iap == iapManager.catboxSimple)
                    {
                        ui.getCatbox.Setup(ui.getCatbox.catboxSimple);
                        ui.PopupShow(ui.getCatbox);
                    }
                }
            });
    }
}
