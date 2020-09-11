using UnityEngine;
using System;

namespace SG
{
    public class IAPManager : Core
    {
        public string skuPrefix = "com.sundaygames.";

        [HideInInspector]
        public IAPStore store = null;

        bool _isInitialized = false;
        public bool isInitialized
        {
            get
            {
                if (!_isInitialized) Init();
                return _isInitialized;
            }
            set { _isInitialized = value; }
        }

        void Start()
        {
            if (build.premium) return;

#if IAP_PURCHASES && UNITY_PURCHASING
            if (platform == Platform.AppStore || platform == Platform.GooglePlay || platform == Platform.WindowsPhone || platform == Platform.Tizen)
                store = GetComponent<IAPMobile>();
#endif
#if IAP_PURCHASES && FACEBOOK
            if (platform == Platform.Facebook)
                store = GetComponent<IAPFacebookCanvas>();
#endif
            IAP.IAPs = GetComponentsInChildren<IAP>();

            Init();
        }

        public void Init()
        {
            if (store != null) store.Init();
            else isInitialized = true;
        }

        Action<bool> purchaseCallback = null;
        public void Purchase(IAP iap, Action<bool> callback)
        {
            if (build.parentGate && ui.parentGate != null)
            {
                ui.parentGate.Show(() =>
                {
                    purchaseCallback = callback;
                    ui.LoadingShow();
                    if (store != null) store.Purchase(iap);
                    else PurchaseSucceed(new PurchaseData(iap.sku));

                }, () => { callback(false); });

                return;
            }

            purchaseCallback = callback;
            ui.LoadingShow();
            if (store != null) store.Purchase(iap);
            else PurchaseSucceed(new PurchaseData(iap.sku));
        }

        public void PurchaseSucceed(PurchaseData data)
        {
            if (data.iap != null)
            {
                user.Buy(data.iap);

                Analytic.EventRevenue(data);
            }

            if (purchaseCallback != null)
            {
                ui.LoadingHide();
                purchaseCallback(true);
                purchaseCallback = null;
            }
        }

        public virtual void PurchaseFailed()
        {
            if (purchaseCallback != null)
            {
                ui.LoadingHide();
                purchaseCallback(false);
                purchaseCallback = null;
            }
        }

        public void OnLogin() { if (store != null) store.OnLogin(); }
    }
}