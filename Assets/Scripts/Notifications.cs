using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Notifications : Core
{
    // Google Project Number: 696475165707
    // Google API key: AIzaSyAhD2IFgpMcjU7sryPQzjv4kl3NYvc4f4g

    // TNT
    // Google Project Number: 375103891482
    // Google API key: AIzaSyCgyznFPe_UoN2XBEHXQL-hobrmFoyeGJE

    public class Notification
    {
        public string name;
        public string icon;
        public Color color;

        public Notification(string name, string icon, Color color)
        {
            this.name = name;
            this.icon = icon;
            this.color = color;
        }

        public string localizedTitle { get { return Localization.Get("notification" + name + "Title"); } }
        public string localizedText { get { return Localization.Get("notification" + name + "Text"); } }
        public string localizedTicker { get { return Localization.Get("notification" + name + "Ticker"); } }
    }
    public static Notification newSausage;
    public static Notification tournamentEnd;

    static bool _ON;
    public static bool ON
    {
        get { return _ON; }
        set
        {
            _ON = value;
            PlayerPrefs.SetString("notifications", ON ? "on" : "off");
        }
    }

    void Awake()
    {
        newSausage = new Notification("NewSausage", "icon_sausage", Color.red);
        tournamentEnd = new Notification("TournamentEnd", "icon_tournament", Color.red);

        if (build.externalNotifications)
        {
            // ParseManager.SetupPush();
        }
    }

    void Start()
    {
        _ON = PlayerPrefs.GetString("notifications", "on") == "on";

        // Analytic.Event("Options", "Notifications", ON ? "ON" : "OFF");

        // Create(Types.Test, DateTime.Now + new TimeSpan(0, 0, 10));
    }


    public static void Create(Notification notification, DateTime utcDateTime, bool sound = true, bool vibro = true)
    {
        if (!ON) return;

        if (PlayerPrefs.HasKey("Notification " + utcDateTime.Ticks)) return;
        else PlayerPrefs.SetString("Notification " + utcDateTime.Ticks, "Scheduled");

        Debug.Log("Push - " + notification.name + " (ID: " + utcDateTime.Ticks + ") notification scheduled");

        var timeShift = new TimeSpan(0, UnityEngine.Random.Range(0, 30), 0);

#if UNITY_IPHONE
        var localNotification = new UnityEngine.iOS.LocalNotification();
        localNotification.alertAction = notification.localizedTitle;
        localNotification.alertBody = notification.localizedText;
        localNotification.fireDate = utcDateTime + timeShift;
        localNotification.userInfo = new Dictionary<string, string>() {
            { "id", utcDateTime.Ticks.ToString() },
            { "type", notification.name } };
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
#endif

#if UNITY_ANDROID
        Assets.SimpleAndroidNotifications.NotificationManager.SendWithAppIcon(
             delay: utcDateTime - DateTime.UtcNow + timeShift,
             title: notification.localizedTitle,
             message: notification.localizedText,
             smallIconColor: notification.color
             // smallIcon: Assets.SimpleAndroidNotifications.NotificationIcon.Message
             );
#endif
    }

    public static void Cancel(long id)
    {
#if UNITY_IPHONE
        int count = UnityEngine.iOS.NotificationServices.localNotificationCount;
        for (int i = 0; i < count; ++i)
        {
            UnityEngine.iOS.LocalNotification notification = UnityEngine.iOS.NotificationServices.GetLocalNotification(i);
            if (notification.userInfo["id"].ToString() == id.ToString())
            {
                UnityEngine.iOS.NotificationServices.CancelLocalNotification(notification);
                Debug.Log("Push - Notification ID " + id + " canceled");
                return;
            }
        }
        Debug.Log("Push - Notification ID " + id + " not canceled - Didn't find this ID");
#endif

#if UNITY_ANDROID
        Assets.SimpleAndroidNotifications.NotificationManager.Cancel((int)id);

        Debug.Log("Push - Notification ID " + id + " canceled");
#endif
    }

    public static void CancelAll()
    {
#if UNITY_IPHONE
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
#endif

#if UNITY_ANDROID
        Assets.SimpleAndroidNotifications.NotificationManager.CancelAll();
#endif
    }


    public static void SendPush(string alert, string facebookId, string channel = null, int expirationHours = 0)
    {
        if (build.externalNotifications)
        {
            // ParseManager.SendPush(alert, facebookId, channel, expirationHours);
        }
    }

    public static void OnExternalNotificationReceived(string data)
    {
        // Example {"onStart":true,"header":"Тест","pw_msg":"1","p":"1","vib":"1","from":"696475165707","title":"Я просто тестирую так что не обращай внимания ✌️","android.support.content.wakelockid":2,"collapse_key":"do_not_collapse","foreground":false}
        Debug.Log("Push - External Notification Received: " + data);
    }
}