using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class IAPItem : Core
    {
        public IAP iap;

        [Space(10)]
        public Image iconImage;
        public Text titleText;
        public Text discountText;
        public Text priceText;

        public void Start()
        {
            titleText.text = Localization.Get("iapCoins", iap.amount);
            if (discountText != null) discountText.text = Localization.Get("iapDiscount", iap.discount);
            priceText.text = iap.priceLocalized;
        }

        public void Buy()
        {
            Analytic.EventImportant("Shop - Press Buy", iap.name);

            iapManager.Purchase(iap, isBuy => { });
        }
    }
}