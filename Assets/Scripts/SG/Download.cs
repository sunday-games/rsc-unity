using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Download : Core
{
    public static Download Create(GameObject parent) { return parent.AddComponent<Download>(); }

    public string id;
    public Action<Download> callback = null;
    public WWW www = null;
    public Dictionary<string, object> responseDict = null;
    public Sprite SpriteCreate()
    {
        return Sprite.Create(www.texture,
            new Rect(0f, 0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
    }
    public float timeout = 10f;
    string time = null;

    public enum Status { Unknown, Success, Error, Timeout, NoConnection, Corrupted };
    public Status status = Status.Unknown;
    public bool isSuccess { get { return status == Status.Success; } }
    public bool isCorrupted { get { return status == Status.Corrupted; } }

    public void Run(string id, string url, Action<Download> callback)
    {
        this.id = id;
        this.www = new WWW(url);
        this.callback = callback;

        LogDebug("Download - {0} - Run. Url: {1}", id, url);

        StartCoroutine("DownloadCoroutine");
        StartCoroutine("TimeoutCoroutine");
    }

    public void Run(string id, WWW www, Action<Download> callback)
    {
        this.id = id;
        this.www = www;
        this.callback = callback;

        LogDebug("Download - {0} - Run", id);

        StartCoroutine("DownloadCoroutine");
        StartCoroutine("TimeoutCoroutine");
    }

    IEnumerator DownloadCoroutine()
    {
        float startTime = Time.time;
        yield return www;
        time = string.Format("{0:0.0}", Time.time - startTime);

        StopCoroutine("TimeoutCoroutine");

        if (!string.IsNullOrEmpty(www.error))
        {
            if (www.error.Contains("Could not resolve host"))
            {
                Debug.LogFormat("Download - {0} - NoConnection ({1}): {2}", id, time, www.error);

                status = Status.NoConnection;
                callback(this);
            }
            else if (www.error.Contains("403"))
            {
                LogError("Download - {0} - Corrupted ({1})\nError: {2}\nMessage: {3}", id, time, www.error, www.text);

                status = Status.Corrupted;
                callback(this);
            }
            else
            {
                LogError("Download - {0} - Error ({1}): {2}", id, time, www.error);

                status = Status.Error;
                callback(this);
            }
        }
        else
        {
            LogDebug("Download - {0} - Success ({1}). Response: {2}", id, time, www.text);

            status = Status.Success;
            callback(this);
        }

        www.Dispose();

        Destroy(this);
    }

    IEnumerator TimeoutCoroutine()
    {
        float startTime = Time.time;
        while (Time.time - startTime < timeout) yield return null;
        time = string.Format("{0:0.0}", Time.time - startTime);

        StopCoroutine("DownloadCoroutine");

        LogDebug("Download - {0} - Timeout ({1})", id, time);

        status = Status.Timeout;
        callback(this);

        www.Dispose();

        Destroy(this);
    }
}