using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using ListO = System.Collections.Generic.List<object>;
using UnityEngine;
using UnityEngine.Networking;

namespace SG
{
    public class Download
    {
        private const int PRINT_MAX_LENGTH = 1024;
        private const int DEFAULT_TIMEOUT = 20;

        public static int versionCode;
        public static Action<int> onUpdateNeeded;

        public static Action<string> onLoadingStart;
        public static Action<string> onLoadingStop;

        public static Action<string> onError;

        public static class Errors
        {
            public static string timeout = "timeout";
            public static string noConnection = "no connection";
            public static string corrupted = "corrupted";
            public static string serverUnavailable = "server unavailable";
            public static string clientUpdateRequired = "client update required";

            public static string orderAlreadyExists = "order already exists";
            public static string orderNotFound = "not found";
        }

        private object _request;
        public bool success = false;
        public string errorMessage;
        public long responseCode;
        private Action<UnityWebRequest> logRequest;
        private bool logResponse = true;
        public string responseText;
        public object responseData;
        public DictSO responseDict;
        public ListO responseList;
        public Texture2D responseTexture;

        public Sprite GetSprite(float pixelsPerUnit = 100f)
        {
            return responseTexture == null ? null : Sprite.Create(responseTexture, new Rect(0f, 0f, responseTexture.width, responseTexture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public Sprite GetCryptoAssetSprite(Vector2 customPivot)
        {
            return responseTexture == null
                ? null
                : Sprite.Create(responseTexture, new Rect(0f, 0f, responseTexture.width, responseTexture.height),
                    customPivot, //most cryptoasset pictures have empty space on bottom
                    1000f / (512f / responseTexture.height)); //it's for fix size of sprite
        }
#if UNITY_VECTORGRAPHICS
        public Sprite GetSpriteSVG(float pixelsPerUnit = 100f)
        {
            // https://github.com/Unity-Technologies/vector-graphics-samples/blob/master/Assets/RuntimeDemo/SVGRuntimeLoad.cs

            // https://docs.unity3d.com/Packages/com.unity.vectorgraphics@1.0/api/Unity.VectorGraphics.VectorUtils.TessellationOptions.html

            var tessellationOptions = new Unity.VectorGraphics.VectorUtils.TessellationOptions()
            {
                StepDistance = 100.0f,
                MaxCordDeviation = 0.5f,
                MaxTanAngleDeviation = 0.1f,
                SamplingStepSize = 0.01f,
            };

            var geoms = Unity.VectorGraphics.VectorUtils.TessellateScene(
                    Unity.VectorGraphics.SVGParser.ImportSVG(new StringReader(responseText)).Scene,
                    tessellationOptions);

            return Unity.VectorGraphics.VectorUtils.BuildSprite(geoms, pixelsPerUnit, Unity.VectorGraphics.VectorUtils.Alignment.Center, Vector2.zero, 128);
        }
#endif
        protected UnityWebRequest webRequest;
        protected float time;
        protected float size;

        private Type _type;
        public enum Type { AUTO, GET, GET_IMAGE, POST, PUT, DELETE }

        public Download(string url, object request = null, Type type = Type.AUTO)
        {
            _type = type == Type.AUTO ? (request != null ? Type.POST : Type.GET) : type;

            if (_type == Type.GET_IMAGE)
            {
                logRequest = _ => Log.Network.Debug("Download " + url);

                webRequest = UnityWebRequestTexture.GetTexture(url);
            }
            else if (_type == Type.GET)
            {
                logRequest = _ => Log.Network.Debug("Download " + url);

                if (request != null)
                {
                    url += "?";
                    foreach (KeyValuePair<string, object> param in (DictSO) request)
                        url += UnityWebRequest.EscapeURL(param.Key) + "=" + UnityWebRequest.EscapeURL(param.Value.ToString()) + "&";
                    url = url.TrimEnd('&');
                }

                webRequest = UnityWebRequest.Get(url);
            }
            else if (_type == Type.DELETE)
            {
                logRequest = _ => Log.Network.Debug("Download " + url);

                webRequest = UnityWebRequest.Delete(url);
            }
            else // POST, PUT
            {
                _request = request ?? new DictSO();

                var requestString = Json.Serialize(_request);

                webRequest = new UnityWebRequest(
                    url,
                    _type.ToString(),
                    new DownloadHandlerBuffer(),
                    new UploadHandlerRaw(requestString.ToBytes()));

                SetRequestHeader("Content-Type", "application/json");
                SetDefaultHeaders();

                logRequest = _ => Log.Network.Debug($"Download {webRequest.url}{Const.lineBreak}Request: {Truncate(requestString)}");
            }

            webRequest.timeout = DEFAULT_TIMEOUT;
        }

        public Download(string url, string request, Type type)
        {
            webRequest = new UnityWebRequest(url, type.ToString(), new DownloadHandlerBuffer(), new UploadHandlerRaw(request.ToBytes()));
            webRequest.timeout = DEFAULT_TIMEOUT;

            SetRequestHeader("Content-Type", "application/json");
            SetDefaultHeaders();

            logRequest = _ => Log.Network.Debug($"Download {webRequest.url}{Const.lineBreak}Request: {Truncate(request)}");
        }

        public Download(string url, string fileParamName, string fileName, byte[] fileData, DictSO formParams)
        {
            var formData = new List<IMultipartFormSection> { new MultipartFormFileSection(fileParamName, fileData, fileName, null) };
            if (formParams != null)
            {
                foreach (var (key, value) in formParams)
                {
                    formData.Add(new MultipartFormDataSection(key, value.ToString()));
                }
            }

            webRequest = UnityWebRequest.Post(url, formData);
            webRequest.timeout = DEFAULT_TIMEOUT;

            SetDefaultHeaders();

            logRequest = _ => Log.Network.Debug($"Upload {webRequest.url}{Const.lineBreak}Request: {formParams}");
        }

        private void SetDefaultHeaders()
        {
            SetRequestHeader("Version", Configurator.Instance.appInfo.version);
            SetRequestHeader("Language", Application.systemLanguage.ToString());
            SetRequestHeader("Platform", Application.platform.ToString());
#if UNITY_SERVER
            SetRequestHeader("Client-Hostname", SystemInfo.deviceName.Replace("`", "").Replace("’", ""));
            var serviceName = System.Environment.GetEnvironmentVariable("SERVICE_NAME");
            if (serviceName != null)
                SetRequestHeader("Service-Name", serviceName);
#endif
        }

        protected Action<Download> _callback;
        public Download SetCallback(Action<Download> callback)
        {
            _callback = callback;
            return this;
        }

        public Download SetRequestHeader(string name, string value)
        {
            if (value.IsNotEmpty())
                webRequest.SetRequestHeader(name, value);
            return this;
        }

        public Download SetRequestHeaderVersionCode()
        {
            SetRequestHeader("Version-Code", Configurator.Instance.appInfo.versionCode.ToString());
            return this;
        }

        public static ITokenPlayer DEFAULT_PLAYER;
        private ITokenPlayer _player;
        public interface ITokenPlayer
        {
            public void SetToken(Download download);
            public void RefreshToken(Action<string> callback);
        }
        public Download SetPlayer(ITokenPlayer player)
        {
            _player = player ?? DEFAULT_PLAYER;
            _player?.SetToken(this);
            return this;
        }

        private string _validationKey;
        public Download SetValidation(string validationKey)
        {
            _validationKey = validationKey;
            SetRequestHeader("Key", Utils.MD5(Json.Serialize(_request) + validationKey));
            return this;
        }

        private string _loadingName;
        public Download SetLoadingName(string loadingName)
        {
            _loadingName = loadingName;
            return this;
        }

        private bool _triggerErrorEvent = true;
        public Download IgnoreError()
        {
            _triggerErrorEvent = false;
            return this;
        }

        public Download SetTimeout(int timeout)
        {
            if (webRequest != null)
                webRequest.timeout = timeout;
            else
                Log.Network.Error("Fail to set timeout");
            return this;
        }

        public void Run()
        {
            logRequest.Invoke(webRequest);

            if (Application.isEditor)
            {
                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                { }

                AfterDownloaded();
            }
            else
            {
                Configurator.FindAndSetInstance().StartCoroutine(RequestCoroutine());
            }
        }

        public Download NoLogRequest()
        {
            logRequest = _ => { };
            return this;
        }

        public Download NoLogResponse()
        {
            logResponse = false;
            return this;
        }

        public IEnumerator RequestCoroutineDebug(Func<object, DictSO> debugFunc)
        {
            yield return new WaitForSeconds(1f);

            responseCode = 200;
            responseData = debugFunc(_request);
            if (responseData != null)
                if (responseData is DictSO data)
                    responseDict = data;
                else if (responseData is ListO list)
                    responseList = list;

            OnComplete();
            webRequest.Dispose();
            _callback?.Invoke(this);
        }

        public IEnumerator RequestCoroutine()
        {
            if (_loadingName.IsNotEmpty())
                onLoadingStart?.Invoke(_loadingName);

            var startTime = Time.time;
            yield return webRequest.SendWebRequest();
            time = Time.time - startTime;

            AfterDownloaded();
        }

        private void AfterDownloaded()
        {
            size = webRequest.downloadedBytes / 1048576f;

            responseCode = webRequest.responseCode;

            if (responseCode > 0 && webRequest.downloadHandler is DownloadHandlerTexture downloadHandlerTexture)
            {
                responseTexture = downloadHandlerTexture.texture;
            }
            else
            {
                responseText = webRequest.downloadHandler.text;

                if (responseText.IsNotEmpty())
                {
                    responseData = Json.Deserialize(responseText);
                    if (responseData != null)
                    {
                        if (responseData is DictSO data)
                            responseDict = data;
                        else if (responseData is ListO list)
                            responseList = list;
                    }
                }
            }

            //if (_player != null && responseCode == 401 && _failToRefreshToken == false)
            //{
            //    Log.Network.Info($"Download {webRequest.url} - Token update required. Updating...");

            //    _player.RefreshToken(
            //        error =>
            //        {
            //            if (error != null)
            //            {
            //                Log.Network.Info($"Download {webRequest.url} - Token updating failed. Error: {error}");

            //                _failToRefreshToken = true;
            //                AfterDownloaded();

            //                return;
            //            }

            //            Log.Network.Info($"Download {webRequest.url} - Token updated. Trying again...");

            //            new Download(webRequest.url, _request, _type) // WTF
            //                .SetPlayer(_player)
            //                .SetTriggerErrorEvent(_triggerErrorEvent)
            //                .SetCallback(_callback)
            //                .Run();
            //        });

            //    return;
            //}

            var versionCode = webRequest.GetResponseHeader("Version-Code");
            if (versionCode.IsNotEmpty())
            {
                Log.Network.Warning($"Download {webRequest.url} - Client update required. Current version code: " + versionCode);
                Download.versionCode = versionCode.ToInt();
            }

            if (200 <= responseCode && responseCode < 400)
                OnComplete();
            else
                OnError();

            webRequest.Dispose();

            if (_loadingName.IsNotEmpty())
                onLoadingStop?.Invoke(_loadingName);

            _callback?.Invoke(this);
        }

        public bool completed => time > 0f;

        protected virtual void OnComplete()
        {
            success = true;
            if (logResponse)
                Log.Network.Debug($"Download {webRequest.url} - Success ({time:0.00}). Code: {responseCode}" + Const.lineBreak +
                                  "Request: " + Truncate(Json.Serialize(_request)) + Const.lineBreak +
                                  "Response: " + Truncate(responseText));
            if (size > 0.5f)
                Log.Network.Warning($"Download {webRequest.url} - Downloaded {size} mb");

            if (_validationKey.IsNotEmpty())
            {
                var key = webRequest.GetResponseHeader("Key");

                if (key.IsNotEmpty())
                {
                    var expectedKey = Utils.MD5(responseText + _validationKey);

                    if (key != expectedKey)
                    {
                        success = false;
                        errorMessage = Errors.corrupted;
                        Log.Network.Error($"Download {webRequest.url} - Hash validation failed: {key} (expected: {expectedKey})");
                    }
                }
            }

            if (webRequest.GetResponseHeader("Version-Code").IsNotEmpty())
                onUpdateNeeded?.Invoke(versionCode);
        }

        protected virtual void OnError()
        {
            errorMessage = webRequest.error;

            Log.Network.Error($"Download {webRequest.url} - Error ({time.ToString("0.00")}). Code: {responseCode} - {errorMessage}" + Const.lineBreak +
                              "Request:" + Json.Serialize(_request) + Const.lineBreak +
                              "Response: " + Truncate(responseText));

            if (responseCode == 0)
                errorMessage = Errors.noConnection;
            else if (responseCode == 406)
                errorMessage = Errors.corrupted;
            else if (responseCode == 502)
                errorMessage = Errors.serverUnavailable;
            else if (errorMessage.IsNotEmpty() && errorMessage.Contains(Errors.timeout))
                errorMessage = Errors.serverUnavailable;
            else if (responseDict != null && responseDict.IsValue("message"))
                errorMessage = responseDict["message"].ToString();

            if (_triggerErrorEvent)
                onError?.Invoke(errorMessage);
        }

#if UNITY_EDITOR
        private static string Truncate(string value) => value;
#else
        private static string Truncate(string value) =>
            value is { Length: > PRINT_MAX_LENGTH } ? value[..PRINT_MAX_LENGTH] + "<truncated>" : value;
#endif
    }
}