using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Version 1.0.1

namespace IngameAdvisor
{
    public class Advisor : MonoBehaviour
    {
        public List<Project> projects;
        [Serializable]
        public class Project
        {
            public SystemLanguage language;
            public string id;
            public Project(SystemLanguage language, string id)
            {
                this.language = language;
                this.id = id;
            }
        }

        [Space(10)]
        public float timeout = 10f;

        [Space(10)]
        public bool logging = false;

        public string GetProjectId(SystemLanguage language)
        {
            foreach (var project in projects)
                if (project.language == language) return project.id;
            return null;
        }

        public void Ask(Request request, SystemLanguage language, Action<Status, Response, Request> callback)
        {
            if (request == null || callback == null)
            {
                Debug.LogError("Ingame Advisor - Parameters can't be empty");
                return;
            }

            if (string.IsNullOrEmpty(GetProjectId(language)))
            {
                Debug.LogErrorFormat("Ingame Advisor - Language {0} is unavailable", language);
                return;
            }

            if (logging) Debug.Log("Ask: " + request.text);

            Download("https://zenbot.org/api/" + GetProjectId(language), request, callback);
        }

        [Serializable]
        public class Var
        {
            public string name;
            public string value;
            public string scope;

            public Var(string name, object value, string scope = "user")
            {
                this.name = name;
                this.value = value.ToString();
                this.scope = scope;
            }
        }
        [Serializable]
        public class Reply
        {
            public string name;
            public string answer;

            public string url;

            public string email;
            public string subject;
            public string body;

            public Reply(string name)
            {
                this.name = name;
            }
        }

        public class Response
        {
            public string context;
            /// <summary>
            /// Response Id
            /// </summary>
            public string input;
            /// <summary>
            /// Response Text
            /// </summary>
            public string output;
            public string lang;
            public float score;
            public bool modal;
            /// <summary>
            /// Quick replies
            /// </summary>
            public List<string> samples;
            public List<Reply> replies
            {
                get
                {
                    var _replies = new List<Reply>();
                    foreach (var sample in samples)
                        if (sample.Contains("{")) _replies.Add(JsonUtility.FromJson<Reply>(sample));
                        else _replies.Add(new Reply(sample));
                    return _replies;
                }
            }
            /// <summary>
            /// Meta Data
            /// </summary>
            public List<Var> vars;

            public string imageUrl
            {
                get
                {
                    foreach (var v in vars)
                        if (v.name == "image") return v.value;
                    return null;
                }
            }

            public string url
            {
                get
                {
                    foreach (var v in vars)
                        if (v.name == "url") return v.value;
                    return null;
                }
            }
        }
        public class Request
        {
            /// <summary>
            /// Request Text
            /// </summary>
            public string text;
            /// <summary>
            /// User Id (optional)
            /// </summary>
            public string user;
            /// <summary>
            /// Meta Data (optional)
            /// </summary>
            public List<Var> vars;

            public Request(string text)
            {
                this.text = text;
            }
        }

        public enum Status { Success, Error, Timeout, NoConnection };
        static Dictionary<string, string> headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
        void Download(string url, Request request, Action<Status, Response, Request> callback)
        {
            var json = JsonUtility.ToJson(request);

            var www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(json), headers);

            if (logging) Debug.LogFormat("Ingame Advisor - Download from url: {0}\nRequest: {1}", url, json);

            StopAllCoroutines();
            StartCoroutine(DownloadCoroutine(request, www, callback));
            StartCoroutine(TimeoutCoroutine(request, www, callback));
        }
        IEnumerator DownloadCoroutine(Request request, WWW www, Action<Status, Response, Request> callback)
        {
            float startTime = Time.time;
            yield return www;
            var time = string.Format("{0:0.0}", Time.time - startTime);

            StopAllCoroutines();

            if (!string.IsNullOrEmpty(www.error))
            {
                if (www.error.Contains("Could not resolve host"))
                {
                    Debug.LogFormat("Ingame Advisor - NoConnection");
                    callback(Status.NoConnection, null, request);
                }
                else
                {
                    Debug.LogErrorFormat("Ingame Advisor - Error ({0}):\n{1}", time, www.error);
                    callback(Status.Error, null, request);
                }
            }
            else
            {
                if (logging) Debug.LogFormat("Ingame Advisor - Success ({0}). Response: {1}", time, www.text);
                callback(Status.Success, JsonUtility.FromJson<Response>(www.text), request);
            }

            www.Dispose();
        }
        IEnumerator TimeoutCoroutine(Request request, WWW www, Action<Status, Response, Request> callback)
        {
            float startTime = Time.time;
            while (Time.time - startTime < timeout) yield return null;

            StopAllCoroutines();

            Debug.LogError("Ingame Advisor - Timeout");

            callback(Status.Timeout, null, request);

            www.Dispose();
        }
    }
}