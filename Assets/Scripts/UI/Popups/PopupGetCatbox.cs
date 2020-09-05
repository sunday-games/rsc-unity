using UnityEngine;
using UnityEngine.UI;

public class PopupGetCatbox : Popup
{
    [Space(10)]

    public Catbox catboxSimple;
    public Catbox catboxPremium;
    public Catbox catboxNY;
    public Catbox catboxSV;
    public Catbox catboxHW;

    [Space(10)]
    public GameObject window;
    public GameObject shareButton;
    public Text shareBonusText;
    public Text catText;

    Catbox catbox;

    public void Setup(Catbox catbox)
    {
        catboxSimple.gameObject.SetActive(false);
        catboxPremium.gameObject.SetActive(false);
        catboxNY?.gameObject.SetActive(false);
        catboxSV?.gameObject.SetActive(false);
        catboxHW?.gameObject.SetActive(false);

        this.catbox = catbox;
        catbox.gameObject.SetActive(true);
    }

    public override void Init()
    {
        if (user.lastGetCat == null)
        {
            Debug.LogError("Catbox - LastGetCat don't set");
            return;
        }

        catbox.catSlot.Init(user.lastGetCat);

        if (user.isLastGetCatLevelUp)
        {
            previous = ui.levelUpCat;
            ui.levelUpCat.previous = ui.collection;
            ui.collection.previous = ui.prepare;
        }
        else
        {
            previous = ui.collection;
            ui.collection.previous = ui.prepare;
        }

        window.SetActive(false);

        Invoke("ShowWindow", 1.5f);
    }
    void ShowWindow()
    {
        sound.Play(sound.winPrize);

        catText.text = Localization.Get("buyCatboxDone", user.lastGetCat.type.localizedName);
        shareBonusText.text = balance.reward.coinsForShareCat.ToString();
        shareButton.SetActive(fb.isLogin);
        window.SetActive(true);
    }

    public override void Reset()
    {
        catbox.catSlot.Clear();
    }

    public void ShareCat()
    {
        window.SetActive(false);
        server.CheckConnection(succeess =>
            {
                if (succeess)
                {
#if FACEBOOK
                    string description = user.collection.Count >= gameplay.superCats.Length ? Localization.Get("newCatDescriptionFull") : Localization.Get("newCatDescription", user.collection.Count, gameplay.superCats.Length);
                    fb.Share(
                        title: Localization.Get("newCatTitle", user.lastGetCat.type.localizedName),
                        description: description, 
                        picture: string.Format(fb.sharePicLinks.cat, user.lastGetCat.type.name),
                        callback: ShareResult,
                        sourcePopup: "ShareCat");
#endif
                }
                else
                {
                    window.SetActive(true);
                }
            });
    }
    void ShareResult(bool success)
    {
        window.SetActive(true);

        if (success)
        {
            shareButton.SetActive(false);
            user.UpdateCoins(balance.reward.coinsForShareCat, true);
            ui.header.ShowCoinsIn(shareButton.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.7f);
        }
    }
}
