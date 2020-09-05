using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if MAT
using MATSDK;
#endif

namespace SG
{
    public class MATManager : AnalyticsManager
    {
        public IOS iOS;
        [System.Serializable]
        public class IOS
        {
            public string id;
            public string key;
        }
        public Android android;
        [System.Serializable]
        public class Android
        {
            public string id;
            public string key;
        }

#if MAT
    public override void Init()
    {
        if (!build.MAT) return;

        try
        {
            if (platform == Platform.AppStore) MATBinding.Init(iOS.id, iOS / key);
            else if (platform == Platform.GooglePlay) MATBinding.Init(android.id, android.key);

            MATBinding.SetDebugMode(isDebug);
            MATBinding.MeasureSession();
        }
        catch (Exception e)
        {
            LogError("Analytic - MAT Init Exception - " + e.Message);
            build.MAT = false;
            return;
        }

        Analytic.onEventRevenue.Add(EventRevenue);
        Analytic.onEventUserLogin.Add(UserLogin);
    }

    void EventRevenue(PurchaseData data)
    {
        var matEvent = new MATEvent("purchase");
        matEvent.revenue = data.iap.revenue;
        matEvent.currencyCode = data.iap.currencyCode;
        matEvent.eventItems = new MATItem[] { new MATItem(data.iap.sku) };
        MATBinding.MeasureEvent(matEvent);
    }

    void UserLogin()
    {
        // MATBinding.SetAge(34);
        if (!string.IsNullOrEmpty(user.facebookId)) MATBinding.SetFacebookUserId(user.facebookId);
        if (user.gender != "Unknown") MATBinding.SetGender(user.gender == "Male" ? 1 : 0);
        if (!string.IsNullOrEmpty(user.googleId)) MATBinding.SetGoogleUserId(user.googleId);
        // MATBinding.SetLocation(111, 222, 333);
        // MATBinding.SetPayingUser(true);
        // MATBinding.SetPhoneNumber("111-222-3456");
        // MATBinding.SetTwitterUserId("twitter_user_id");
        if (!string.IsNullOrEmpty(user.id)) MATBinding.SetUserId(user.id);
        // if (!string.IsNullOrEmpty(user.name)) MATBinding.SetUserName(user.name);
        if (!string.IsNullOrEmpty(user.email)) MATBinding.SetUserEmail(user.email);
    }
#endif
    }
}