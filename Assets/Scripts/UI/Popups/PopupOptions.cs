﻿using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class PopupOptions : Popup
    {
        [Space(10)]
        public Text soundText;
        public Text voiceText;
        public Text musicText;

        [Space(10)]
        public Text languageText;

        [Space(10)]
        public Text notificationsText;

        [Space(10)]
        public GameObject promocodeButton;
        public GameObject youtubeButton;
        public GameObject emailButton;

        [Space(10)]
        public GameObject likeButton;
        public GameObject vkButton;
        public GameObject twitterButton;
        public GameObject facebookButton;
        public GameObject rateButton;

        [Space(10)]
        public Text versionText;

        public override void Init()
        {
            soundText.text = Localization.Get(sound.ON ? "on" : "off");
            voiceText.text = Localization.Get(sound.voiceON ? "on" : "off");
            musicText.text = Localization.Get(music.ON ? "on" : "off");
            notificationsText.text = Localization.Get(Notifications.ON ? "on" : "off");
            if (languageText != null) languageText.text = Localization.Get(Localization.language.ToString());

            versionText.text = Localization.Get("version", Configurator.Instance.appInfo.version + "." + Configurator.Instance.appInfo.versionCode);

            rateButton.SetActive(Utils.IsPlatformMobile());

            likeButton.SetActive(server.links.fbGroup.IsNotEmpty() && Utils.IsStore(Store.Facebook));

            facebookButton.SetActive(server.links.fbGroupShort.IsNotEmpty() && !Utils.IsStore(Store.Facebook));

            vkButton.SetActive(server.links.vkGroup.IsNotEmpty() && Localization.language == SystemLanguage.Russian && !Utils.IsStore(Store.Facebook));

            twitterButton.SetActive(server.links.twitter.IsNotEmpty());

            youtubeButton.SetActive(server.links.youtube.IsNotEmpty());

            promocodeButton.SetActive(build.promocodes);

            emailButton.SetActive(!advisor || string.IsNullOrEmpty(advisor.GetProjectId(Localization.language)));
        }
        public override void Reset() { }

        public void SoundToggle()
        {
            sound.ON = !sound.ON;
            soundText.text = Localization.Get(sound.ON ? "on" : "off");
        }

        public void VoiceToggle()
        {
            sound.voiceON = !sound.voiceON;
            voiceText.text = Localization.Get(sound.voiceON ? "on" : "off");
        }

        public void MusicToggle()
        {
            music.ON = !music.ON;
            if (music.ON) music.Switch(music.menu);
            else music.TurnOff();
            musicText.text = Localization.Get(music.ON ? "on" : "off");
        }

        public void NotificationsToggle()
        {
            Notifications.ON = !Notifications.ON;
            if (!Notifications.ON) Notifications.CancelAll();
            notificationsText.text = Localization.Get(Notifications.ON ? "on" : "off");
        }

        public void LanguageToggle()
        {
            Localization.SetNextLanguage();
            languageText.text = Localization.Get(Localization.language.ToString());
            Init();
        }

        public void OpenFacebookPage()
        {
            SG_Utils.OpenLink(server.links.fbGroupShort, "FacebookGroup");
        }

        public void OpenHelp()
        {
            if (advisor && !string.IsNullOrEmpty(advisor.GetProjectId(Localization.language)))
                ui.PopupShow(ui.advisor);
            else
                SG_Utils.OpenLink(Localization.language == SystemLanguage.Russian ? server.links.helpPageRU : server.links.helpPage, "SiteHelp");
        }

        public void OpenTwitter()
        {
            SG_Utils.OpenLink(server.links.twitter, "Twitter");
        }

        public void OpenVKPage()
        {
            SG_Utils.OpenLink(server.links.vkGroup, "VKontacteGroup");
        }

        public void OpenYoutube()
        {
            SG_Utils.OpenLink(server.links.youtube, "Youtube");
        }

        public void OpenLike()
        {
            SG_Utils.OpenLink(server.links.fbGroup, "FacebookGroup");
        }

        public void OpenRateApp()
        {
            string link = null;
            if (platform == Platform.iOS)
                link = (SystemInfo.operatingSystem.Contains(" 6.") ? "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=" : "itms-apps://itunes.apple.com/app/id") + build.APPLE_ID;
            else if (platform == Platform.Android)
                link = "market://details?id=" + build.ID;
            else if (Utils.IsStore(Store.Amazon))
                link = "amzn://apps/android?p=" + build.ID;
            else
                link = server.links.landingPage;

            SG_Utils.OpenLink(link, "RateApp");

            PlayerPrefs.SetInt("RateApp", int.MaxValue); // Флаг, чтобы окно Rate App больше не вылезало
        }

        public void SendEmail()
        {
            if (Utils.IsStore(Store.Facebook))
            {
                SG_Utils.OpenLink(server.links.contactsPage, "SiteContacts");
                return;
            }

            string subject = Localization.Get("mailSubject", Random.Range(100000, 999999));

            string body = Localization.Get("mailBody", SystemInfo.deviceModel, SystemInfo.operatingSystem,
                SG_Utils.resolution,
                SystemInfo.systemMemorySize,
                SystemInfo.processorType,
                SystemInfo.graphicsDeviceName,
                SystemInfo.graphicsMemorySize,
                SystemInfo.graphicsDeviceVersion,
                user.isId ? user.id : "-",
                user.deviceId.IsNotEmpty() ? "-" : user.deviceId,
                user.facebookId.IsNotEmpty() ? "-" : user.facebookId,
                user.gameCenterId.IsNotEmpty() ? "-" : user.gameCenterId,
                user.googleGamesId.IsNotEmpty() ? "-" : user.googleGamesId,
                Configurator.Instance.appInfo.version);

            SG_Utils.Email(server.links.supportEmail, subject, body);
        }
    }
}