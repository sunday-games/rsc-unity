using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public abstract class IAPStore : Core
    {
        public abstract void Init();

        public abstract void Purchase(IAP iap);

        // public abstract void Consume(IAP iap);

        public virtual void OnLogin() { }

        public virtual void Verify(PurchaseData data, Action<bool> callback) { }
    }

    public class PurchaseData
    {
        public IAP iap;
        public string transaction = null;
        public string receipt = null;
        public string signature = null;

        public PurchaseData(string product, string transaction = null, string receipt = null)
        {
            this.iap = IAP.FromSKU(product);
            this.transaction = transaction;
            this.receipt = receipt;
        }

        public PurchaseData(IAP iap, string transaction = null, string receipt = null)
        {
            this.iap = iap;
            this.transaction = transaction;
            this.receipt = receipt;
        }
    }
}
