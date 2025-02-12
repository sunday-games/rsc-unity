using UnityEngine;
using System;

namespace SG.Ads
{
    public abstract class Provider : MonoBehaviour
    {
        [HideInInspector]
        public bool isInit = false;
        public abstract void Init();

        public virtual bool isReadyInterstitial() { return false; }
        public virtual void ShowInterstitial(Action success, Action failed) { }

        public virtual bool isReadyVideoRewarded() { return false; }
        public virtual void ShowRewarded(Action success, Action failed) { }
    }
}