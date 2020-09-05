using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if FACEBOOK
using Facebook.Unity;
using Facebook.Unity.Settings;
#endif

public class FacebookManager : Core
{
    [Space(10)]
    public float timeout = 10f;

    [Space(10)]
    public string appId;
    // Main 776966945678883
    // TNT 917276901670067

    public string userAccessToken
    {
        get { return PlayerPrefs.GetString("facebookAccessToken", string.Empty); }
        set { PlayerPrefs.SetString("facebookAccessToken", value); }
    }

    [Space(10)]
    public SharePicLinks sharePicLinks;
    [Serializable]
    public class SharePicLinks
    {
        public string cat;          // share/cat{0}.jpg
        public string highscore;    // share/highscore.jpg
        public string leagueUp;     // share/leagueUp.jpg
        public string tournament;   // share/tournament.jpg
    }

    public void Setup()
    {
#if FACEBOOK
        if (Resources.FindObjectsOfTypeAll<FacebookSettings>().Length == 0) return;

        var settings = Resources.FindObjectsOfTypeAll<FacebookSettings>()[0];

        FacebookSettings.AppLabels[0] = build.productName;
        FacebookSettings.AppIds[0] = appId;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(settings);
#endif

#endif
    }

    [HideInInspector]
    public bool isInit = false;

#if FACEBOOK
    #region LOGIN
    bool error = false;
    public AccessToken token { get { return AccessToken.CurrentAccessToken; } }

    bool isData = false;
    public bool isLogin => FB.IsLoggedIn && isData;

    public void TryInit(Action<bool> callback = null)
    {
        Log("Facebook - Try Init...");

        if (isInit) callback(isInit);
        else StartCoroutine(InitCoroutine(callback));
    }
    IEnumerator InitCoroutine(Action<bool> callback)
    {
        FB.Init(OnInit, OnHideUnity);

        float startTime = Time.time;
        while (Time.time - startTime < timeout && !isInit) yield return null;

        callback?.Invoke(isInit);
    }
    void OnInit()
    {
        Log("Facebook - Init Success");

        isInit = true;

        if (platform == Platform.AppStore || platform == Platform.GooglePlay)
        {
            FB.ActivateApp();
            FB.Mobile.FetchDeferredAppLinkData(OnDeeplinkCaught);
        }
    }
    void OnHideUnity(bool isGameShown)
    {
        Time.timeScale = isGameShown ? 1 : 0;
    }

    public void TryLogin(Action<IDictionary<string, object>> callback = null)
    {
        Log("Facebook - Trying Login...");

        if (FB.IsLoggedIn)
            StartCoroutine(GetDataCoroutine(callback));
        else if (isInit)
            StartCoroutine(LoginCoroutine(callback));
        else
        {
            TryInit();
            callback?.Invoke(null);
        }
    }
    IEnumerator LoginCoroutine(Action<IDictionary<string, object>> callback = null)
    {
        var permissions = new List<string> { "public_profile", "email", "user_friends" };

        FB.LogInWithReadPermissions(permissions, LoginCallback);

        error = false;
        float startTime = Time.time;
        while (Time.time - startTime < timeout && !FB.IsLoggedIn && !error) yield return null;

        if (FB.IsLoggedIn)
            StartCoroutine(GetDataCoroutine(callback));
        else
            callback?.Invoke(null);
    }
    void LoginCallback(ILoginResult result)
    {
        if (result != null) Log("Facebook - Login Response: " + result.RawResult);

        if (FB.IsLoggedIn)
        {
            Log("Facebook - Login Success as PlayerId: " + token.UserId);
            foreach (string permission in token.Permissions) Log("Facebook - Permission: " + permission);
        }
        else
        {
            Log("Facebook - Login Failure");
            error = true;
        }
    }

    IDictionary<string, object> data = null;
    IEnumerator GetDataCoroutine(Action<IDictionary<string, object>> callback = null)
    {
        // https://developers.facebook.com/tools/explorer/776966945678883

        // https://developers.facebook.com/docs/graph-api/reference/v8.0/user/invitable_friends

        var query = "me?fields=id,name,first_name,last_name,email,friends.limit(100).fields(first_name,id,picture.width(130).height(130))";
        // query += ",invitable_friends.limit(50).fields(first_name,picture.width(130).height(130))";

        Log("Facebook - Get Data...");
        FB.API(query, HttpMethod.GET, GetDataCallback);

        error = false;
        float startTime = Time.time;
        while (Time.time - startTime < timeout && data == null && !error) yield return null;

        if (callback != null)
        {
            callback(data);
            data = null;
        }
    }
    void GetDataCallback(IResult result)
    {
        if (result == null)
        {
            LogError("Facebook - Get Data - Failed");
            error = true;
            return;
        }

        LogDebug("Facebook - Get Data - Response: " + result.RawResult);

        if (!string.IsNullOrEmpty(result.Error))
        {
            LogError("Facebook - Get Data Error: " + result.Error);
            error = true;
            return;
        }

        data = result.ResultDictionary;
        isData = true;
    }
    #endregion

    #region INVITE
    Action<List<string>> callbackInviteFriends;
    public void InviteFriends(string message, string title, Action<List<string>> callback)
    {
        callbackInviteFriends = callback;

        FB.AppRequest(
            message,
            null,
            new List<object>() { "app_non_users" },
            null,
            0,
            string.Empty,
            title,
            InviteFriendCallback);
    }
    void InviteFriendsCallback(IAppRequestResult result)
    {
        if (result == null)
        {
            LogError("Facebook - InviteFriends Failed");
            callbackInviteFriends(null);
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            LogError("Facebook - InviteFriends Error: " + result.Error);
            callbackInviteFriends(null);
        }
        else if (result.Cancelled)
        {
            Log("Facebook - InviteFriends Cancelled: " + result.RawResult);
            callbackInviteFriends(null);
        }
        else
        {
            Log("Facebook - InviteFriends Success");
            Analytic.EventImportant("Facebook", "InviteFriends");

            var facebookIds = new List<string>();
            foreach (var id in result.To) facebookIds.Add(id);

            callbackInviteFriends(facebookIds);
        }
    }

    public void InviteFriend(string message, string title, Action<List<string>> callback, string facebookId)
    {
        callbackInviteFriends = callback;

        FB.AppRequest(
            message,
            new string[] { facebookId },
            null,
            null,
            0,
            string.Empty,
            title,
            InviteFriendCallback);
    }
    void InviteFriendCallback(IAppRequestResult result)
    {
        if (result == null)
        {
            LogError("Facebook - InviteFriend Failed");
            callbackInviteFriends(null);
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            LogError("Facebook - InviteFriend Error: " + result.Error);
            callbackInviteFriends(null);
        }
        else if (result.Cancelled)
        {
            Log("Facebook - InviteFriend Cancelled: " + result.RawResult);
            callbackInviteFriends(null);
        }
        else
        {
            Log("Facebook - InviteFriend Success");

            Analytic.EventImportant("Facebook", "InviteFriend");

            var facebookIds = new List<string>();
            if (result.To != null)
                foreach (var id in result.To)
                    facebookIds.Add(id);

            callbackInviteFriends(facebookIds);
        }
    }
    #endregion

    #region SHARE
    Action<bool> callbackShare = null;
    string sourcePopupShare = null;
    public void Share(string title, string description, string picture, Action<bool> callback, string sourcePopup = null)
    {
        callbackShare = callback;
        sourcePopupShare = sourcePopup;

        FB.ShareLink(
            contentURL: new Uri(server.links.landingPage),
            contentTitle: title,
            contentDescription: description,
            photoURL: new Uri(server.links.hosting + picture),
            callback: ShareCallback);
    }

    void ShareCallback(IShareResult result)
    {
        if (result == null)
        {
            LogError("Facebook - Share Failed");
            callbackShare(false);
            return;
        }

        Log("Facebook - Share Response: " + result.RawResult);

        if (!string.IsNullOrEmpty(result.Error))
        {
            LogError("Facebook - Share Error: " + result.Error);
            callbackShare(false);
        }
        else if (result.Cancelled)
        {
            Log("Facebook - Share Cancelled");
            callbackShare(false);
        }
        else if (result.ResultDictionary.ContainsKey("postId") || result.ResultDictionary.ContainsKey("posted") ||
            result.ResultDictionary.ContainsKey("post_id"))
        {
            Log("Facebook - Share Success");
            if (!string.IsNullOrEmpty(sourcePopupShare)) Analytic.EventImportant("Facebook", "Share " + sourcePopupShare);
            callbackShare(true);
        }
        else
        {
            Log("Facebook - Share Failed - Unknown");
            callbackShare(false);
        }
    }
    #endregion

    public void LoadFriendImgFromId(string id, int i, Action<Texture2D, int> callback)
    {
        if (!isInit)
        {
            TryInit();
            callback(null, i);
            return;
        }

        FB.API($"/{id}/picture?g&width={130}&height={130}", HttpMethod.GET, delegate (IGraphResult result)
        {
            if (result.Error != null)
            {
                LogError("Facebook - Image Load {0} - Error: {1}", id, result.Error);
                callback(null, i);
            }
            else if (result.Texture == null)
                callback(null, i);
            else
                callback(result.Texture, i);
        });
    }
#else
    public bool isLogin = false;
#endif

    public enum PicType { square, small, normal, large }
    public string GetPicURL(string facebookId, PicType type = PicType.square, int width = 130, int height = 130)
    {
        // https://graph.facebook.com/10204916518582922/picture?type=square&height=130&width=130
        return "https://graph.facebook.com/" + facebookId + "/picture?type=" + type + "&width=" + width + "&height=" + height;
    }

    Action<IDictionary<string, object>> callbackLogin = null;
    bool forceLogin;

    public void ProceedLogin(Action<IDictionary<string, object>> callback, bool force = true)
    {
        callbackLogin = callback;
        forceLogin = force;

#if FACEBOOK
        TryInit(OnFacebookInit);
#else
        callbackLogin(null);
#endif
    }

    void OnFacebookInit(bool isFacebookInit)
    {
        if (!isFacebookInit)
        {
            callbackLogin(null);
            return;
        }

        if (!forceLogin && string.IsNullOrEmpty(user.facebookId))
        {
            Log("Facebook - There no Facebook login previously and forceLogin off - Don't try login");
            callbackLogin(null);
            return;
        }

#if FACEBOOK
        TryLogin(callbackLogin);
#else
        callbackLogin(null);
#endif
    }

    void OnDeeplinkCaught(IAppLinkResult result)
    {
        if (!string.IsNullOrEmpty(result.Url))
            Log("FacebookManager - OnDeeplinkCaught: " + result.Url);
    }
}