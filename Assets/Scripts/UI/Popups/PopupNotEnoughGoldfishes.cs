using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PopupNotEnoughGoldfishes : Popup
{
    public enum Mode { Cats, Boosts }
    [HideInInspector]
    public Mode mode = Mode.Cats;

    [Space(10)]

    public Text bubbleText;

    [Space(10)]

    public Text buyCoinsCountText;
    public Text adCoinsCountText;
    public Text inviteCoinsCountText;

    [Space(10)]

    public GameObject priceGoldfishes;
    public Text priceGoldfishesText;
    public Image buyButton;
    public Text buyButtonText;

    [Space(10)]

    public Button watchButton;
    public Image watchButtonImage;
    public Text watchButtonText;

    [Space(10)]

    public Text withoutButtonText;

    Color unactiveColor = new Color(0.3f, 0.3f, 0.3f);

    int coins;

    public override void Init()
    {
        buyCoinsCountText.text = (ui.shop.aquarium * ui.shop.xGoldfishes).SpaceFormat();
        adCoinsCountText.text = balance.reward.coinsForAdView.ToString();
        inviteCoinsCountText.text = balance.reward.coinsForInvite.ToString();

        coins = user.coins;

        Refresh(Vector3.zero);
    }

    void Refresh(Vector3 coinsPosition)
    {
        if (coins != user.coins)
        {
            coins = user.coins;
            ui.header.ShowCoinsIn(coinsPosition, 8, ui.canvas[3].transform, shift: 0.4f, delay: 0.6f);
        }

        if (ui.prepare.cost + ui.boosts.cost <= user.coins)
        {
            ui.PopupClose();
            return;
        }

        if (mode == Mode.Cats)
        {
            bubbleText.text = Localization.Get("notEnoughGoldfishesForCats", ui.prepare.cost + ui.boosts.cost - user.coins);
            withoutButtonText.text = Localization.Get("playWithoutSuperCats");
        }
        else
        {
            bubbleText.text = Localization.Get("notEnoughGoldfishesForBoosts", ui.prepare.cost + ui.boosts.cost - user.coins);
            withoutButtonText.text = Localization.Get("playWithoutBoosts");
        }

        if (iapManager.isInitialized)
        {
            priceGoldfishes.SetActive(true);
            priceGoldfishesText.text = iapManager.goldfishes.priceLocalized;
            buyButton.color = Color.white;
            buyButtonText.color = Color.white;
        }
        else
        {
            priceGoldfishes.SetActive(false);
            buyButton.color = unactiveColor;
            buyButtonText.color = unactiveColor;
        }

        //watchButton.interactable = false;
        //watchButtonImage.color = unactiveColor;
        //watchButtonText.color = unactiveColor;
        if (ads.isReadyVideoRewarded())
        {
            watchButton.interactable = true;
            watchButtonImage.color = Color.white;
            watchButtonText.color = Color.white;
        }
        else
        {
            watchButton.interactable = false;
            watchButtonImage.color = unactiveColor;
            watchButtonText.color = unactiveColor;
        }
    }

    public void WatchAds()
    {
        ads.ShowVideoRewarded(() =>
        {
            user.UpdateCoins(balance.reward.coinsForAdView, true);
            Refresh(adCoinsCountText.transform.position);
        }, null);
    }

    public void BuyGoldfishes()
    {
        if (iapManager.isInitialized)
            iapManager.Purchase(iapManager.goldfishes, purchaseSuccess => { Refresh(buyCoinsCountText.transform.position); });
    }

    public void InviteFriends()
    {
        if (fb.isLogin)
            ui.FacebookInviteFriends(Vector3.zero, () => { Refresh(inviteCoinsCountText.transform.position); });
        else
            ui.FacebookLogin();
    }

    public void StartGameWithoutCats()
    {
        if (gameplay.isPlaying) return;

        if (mode == Mode.Cats)
        {
            foreach (CatItem catItem in user.collection) catItem.isInstalled = -1;
            foreach (CatSlot slot in ui.prepare.catSlots) slot.Clear();
        }
        else if (mode == Mode.Boosts)
        {
            if (ui.prepare.cost > 0) user.UpdateCoins(-ui.prepare.cost, false);

            ui.boosts.ResetBoosts();
        }

        ui.header.UpdateCoins(force: true);

        gameplay.Play();
    }
}
