using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SG.Push
{
    public class PushManager : MonoBehaviour
    {
        static PushManager _instance;
        public static PushManager instance => _instance ? _instance : _instance = FindObjectOfType<PushManager>();

#if UNITY_EDITOR
        public DebugData debug;
        [Serializable]
        public class DebugData
        {
            public float confirmTime = 2f;
            public Dictionary<string, object> subscription = new Dictionary<string, object> {
                { "endpoint", "https://updates.push.services.mozilla.com/wpush/v2/xxx" },
                { "keys", new Dictionary<string, object> {
                    { "auth", "xxx" },
                    { "p256dh", "xxx" } }
                }
            };
        }
#endif

        Result subscribeResult;
        public IEnumerator Subscribe(Action<Dictionary<string, object>> callback)
        {
            subscribeResult = null;
#if UNITY_EDITOR
            yield return new WaitForSeconds(debug.confirmTime);
            subscribeResult = new Result(new Dictionary<string, object> { { "subscription", debug.subscription } });
#elif UNITY_WEBGL
            PushSubscribe();
#else
            subscribeResult = new Result().SetError("not implemented");
#endif
            while (subscribeResult == null) yield return null;

            if (!subscribeResult.success)
            {
                Log.Error("PushManager - NotifySubscribe Error: " + subscribeResult.error);
                callback?.Invoke(null);
                yield break;
            }

            callback?.Invoke(subscribeResult.data["subscription"] as Dictionary<string, object>);
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        static extern void PushSubscribe();

        public void NotifySubscribe(string json)
        {
            Log.Info("PushManager - NotifySubscribe: " + json);
            subscribeResult = new Result(json);
        }
#endif
    }
}