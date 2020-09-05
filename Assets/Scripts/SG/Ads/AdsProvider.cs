using UnityEngine;
using System;

namespace SG
{
    public abstract class AdsProvider : Core
    {
        [HideInInspector]
        public bool isInit = false;
        public abstract void Init();

        public virtual bool isReadyInterstitial() { return false; }
        public virtual void ShowInterstitial(Action success, Action failed) { }

        public virtual bool isReadyVideoRewarded() { return false; }
        public virtual void ShowVideoRewarded(Action success, Action failed) { }
    }
}