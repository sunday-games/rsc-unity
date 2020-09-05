using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// Manual https://docs.google.com/document/d/1s9Rj8qpaVihMYYPUzgfTG2DK7ZnapthX8sTVnGAga5A/edit?usp=sharing

public class RemoteLogger : Core
{
    public static RemoteLogger instance;

    static bool isEnable = false;

    public static void Enable()
    {
        if (isEnable) return;

        isEnable = true;

        build.debugLevel = BuildSettings.DebugLevel.Debug;

        PlayerPrefs.SetInt("remoteLogger", 1);

        Debug.Log("Remote Logger - Enabled");
        Application.logMessageReceived += instance.HandleLog;
        instance.StartCoroutine(instance.WaitForRequest());
    }

    public static void Disable()
    {
        isEnable = false;

        build.debugLevel = BuildSettings.DebugLevel.Release;

        PlayerPrefs.SetInt("remoteLogger", 0);

        Application.logMessageReceived -= instance.HandleLog;
        instance.StopAllCoroutines();
        Debug.Log("Remote Logger - Disabled");
    }

    public string googleScriptUrl = "https://script.google.com/macros/s/AKfycbxOGL3DPW1tPtGBBVHPeNq3zziGAgpsbBSSlY-gJeLTOF0bj4E/exec";

    Queue<string> logs = new Queue<string>();
    const int maxParams = 10;

    void Awake()
    {
        instance = this;

        if (DateTime.Now.Year == 2010) RemoteLogger.Enable();
        else if (DateTime.Now.Year == 2011) RemoteLogger.Disable();

        if (PlayerPrefs.GetInt("remoteLogger", 0) == 1) RemoteLogger.Enable();
    }

    const string warning = "Warning: ";
    const string error = "Error: ";
    const string exception = "Exception: ";
    const string n = "\n";
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log) logs.Enqueue(logString);
        else if (type == LogType.Warning) logs.Enqueue(warning + logString);
        else if (type == LogType.Error) logs.Enqueue(error + logString + n + stackTrace);
        else if (type == LogType.Exception) logs.Enqueue(exception + logString + n + stackTrace);
    }

    const string deviceId = "?deviceId=";
    const string p0 = "&p0=";
    const string p = "&p";
    const string equal = "=";
    IEnumerator WaitForRequest()
    {
        while (true)
        {
            if (logs.Count > 0)
            {
                int count = logs.Count > maxParams ? maxParams : logs.Count;

                var url = new StringBuilder(googleScriptUrl).Append(deviceId).Append(SystemInfo.deviceUniqueIdentifier);
                for (int i = 0; i < count; ++i)
                    url.Append(p).Append(i).Append(equal).Append(SG_Utils.UrlEncode(logs.Dequeue()));

                var www = new WWW(url.ToString());
                yield return www;

                // if (www.error != null) { };

                www.Dispose();
            }

            yield return null;
        }
    }
}