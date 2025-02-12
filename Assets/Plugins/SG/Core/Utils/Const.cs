using UnityEngine;
using System;

namespace SG
{
    public static class Const
    {
        static Const()
        {
            tan60 = Mathf.Tan(60f * Mathf.Deg2Rad);
        }
        public static float tan60;

        public const int hoursInDay = 24;
        public const int minutesInDay = 60 * hoursInDay;
        public const int secondsInDay = 60 * minutesInDay;

        public static DateTime epoch = new DateTime(1970, 1, 1);

        public static TimeSpan day = TimeSpan.FromDays(1);

        public static Vector3 DEFAULT = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public static Vector2 vector2_05_05 = new Vector2(0.5f, 0.5f);

        public static Vector3 vector3_05_05_0 = new Vector3(0.5f, 0.5f, 0f);
        public static Vector3 vector3_03_03_0 = new Vector3(0.3f, 0.3f, 0);
        public static Vector3 vector3_01_01_0 = new Vector3(0.1f, 0.1f, 0f);

        public static Vector3 vector3_05_05_1 = new Vector3(0.5f, 0.5f, 1f);
        public static Vector3 vector3_01_01_1 = new Vector3(0.1f, 0.1f, 1f);

        public static Quaternion quaternion_05 = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);

        public static Rect rect_0 = new Rect(0f, 0f, 0f, 0f);

        public static Color white_05 = new Color(1f, 1f, 1f, 0.5f);
        public static Color white_025 = new Color(1f, 1f, 1f, 0.25f);

        public static Color black_05 = new Color(0f, 0f, 0f, 0.5f);
        public static Color black_025 = new Color(0f, 0f, 0f, 0.25f);

        public const string dateTimeFormat_MDY = "MMMM d, yyyy";
        public const string dateTimeFormat_DMY = "dd MMMM yyyy";
        public const string dateTimeFormat_DMYHMS = "dd MMM yyyy, HH:mm:ss";
        public const string dateTimeFormat_DMYHM = "dd MMM yyyy, HH:mm";

        public const string lineBreak = "\n";
        public const string doubleLineBreak = "\n\n";
    }
}