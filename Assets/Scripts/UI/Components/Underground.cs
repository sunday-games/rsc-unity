using UnityEngine;
using System.Collections;

public class Underground : Core
{
    public GameObject undergroundAdd;

    [Space(10)]

    public GameObject appleButton;
    public GameObject googlePlayButton;
    public GameObject amazonButton;

    [Space(10)]

    public string appStoreEN = "https://itunes.apple.com/en/app/id{0}";
    public string appStoreRU = "https://itunes.apple.com/ru/app/id{0}";
    public string googlePlay = "https://play.google.com/store/apps/details?id={0}&referrer=utm_source%3DFacebook%26utm_medium%3DGame%26utm_term%3DMainMenu";
    public string amazon = "http://www.amazon.com/dp/{0}";

    void Start()
    {
        if (platform != Platform.Facebook)
        {
            if (undergroundAdd != null) undergroundAdd.SetActive(false);
            gameObject.SetActive(false);
        }
        else
        {
            appleButton.SetActive(!string.IsNullOrEmpty(build.APPLE_ID));
            googlePlayButton.SetActive(!string.IsNullOrEmpty(build.ID));
            amazonButton.SetActive(!string.IsNullOrEmpty(build.AMAZON_ID));
        }
    }

    public void OpenApple()
    {
        SG_Utils.OpenLink(string.Format(Localization.language == SystemLanguage.Russian ? appStoreRU : appStoreEN, build.APPLE_ID), "AppStore");
    }

    public void OpenGooglePlay()
    {
        SG_Utils.OpenLink(string.Format(googlePlay, build.ID), "GooglePlay");
    }

    public void OpenAmazon()
    {
        SG_Utils.OpenLink(string.Format(amazon, build.AMAZON_ID), "Amazon");
    }
}
