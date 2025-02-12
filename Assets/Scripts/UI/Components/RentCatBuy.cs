using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG.RSC
{
    public class RentCatBuy : Core
    {
        public Text priceText;
        public Text descriptionText;

        public Transform catParent;

        CatItem catItem { get { return ui.rentCat.catItem; } }

        CatItemView catView;

        Action callback = null;

        public void Show(Action callback = null)
        {
            this.callback = callback;

            descriptionText.text = Localization.Get("rentCatBuy", catItem.type.localizedName, catItem.level);
            priceText.text = iapManager.cat.priceLocalized;

            catView = Instantiate(catItem.type.itemViewPrefab) as CatItemView;
            catView.transform.SetParent(catParent, false);
            catView.transform.localScale = 2 * Vector3.one;
            catView.footer.SetActive(false);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            ui.rentCat.RemoveCat();

            Destroy(catView.gameObject);

            gameObject.SetActive(false);

            if (callback != null) callback();
        }

        public void Buy()
        {
            ObscuredPrefs.SetString("rentCatBuyName", catItem.type.name);
            ObscuredPrefs.SetInt("rentCatBuyLavel", catItem.level);

            iapManager.Purchase(iapManager.cat, purchaseSuccess =>
            {
                if (purchaseSuccess)
                {
                    Analytic.EventProperties("RentCat", catItem.type.name, "BUY");

                    Hide();
                }
            });
        }
    }
}