using UnityEngine;

public class BuildSettingsFeatures : MonoBehaviour
{
    [Space(10)]
    public bool premium = false;
    public bool externalNotifications = true;
    public bool facebook = true;
    public bool parentGate = false;
    public bool promocodes = true;
    public bool localPurchaseVerification = false;
    public bool serverPurchaseVerification = true;
    public bool googlePlayGames = false;
    //public bool readPhoneState = false;
    public bool replayKit = false;
    public bool ingameAdvisor = false;

    [Space(10)]
    public bool chartboost = false;
    public bool appodeal = false;
    public bool googleAnalytics = false;
    public bool gameAnalytics = false;
    public bool amplitude = false;
    public bool appMetrica = false;
    public bool unityAnalytics = false;
    public bool unityAds = false;

    [Space(10)]
    public string productName;
    public string ID;
    public string APPLE_ID;
    public string AMAZON_ID;
    public string appleDeveloperTeamID;
}
