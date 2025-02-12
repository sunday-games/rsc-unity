using UnityEngine;
using System;
using System.Collections.Generic;

namespace SG.Analytics
{
    public abstract class Analytics : MonoBehaviour
    {
        public bool logRevenueOnlyOnProduction = true;

        protected bool? production = null;

        public virtual void Init(bool production) { }

        public virtual void SetDataCollection(bool dataCollection)  { }

        public virtual bool Event(string name) => false;

        public virtual bool Event(string category, string name) => false;

        public virtual bool Event(string category, string name, string subname) => false;

        public virtual bool Event(string name, Dictionary<string, object> parameters) => false;

        public virtual bool View(string name) => false;

        public virtual bool Login(string id) => false;

        public virtual bool Revenue(PurchaseData data) => false;
    }

    public class PurchaseData
    {
        public string id;
        public string buyer;
        public string beneficiary;
        public string seller;
        public string referrer;
        public decimal revenue;
        public double income;
        public Currency currency;
        public DateTime date;
        public ItemData item;

        public class ItemData
        {
            public string id;
            public string name;
            public string category;
        }

        public static implicit operator bool(PurchaseData data) => data != null;
    }
}