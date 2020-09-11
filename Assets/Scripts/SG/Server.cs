using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

namespace SG
{
    public class Server : Core
    {
        public float timeout = 10f;

        [Space(10)]
        public Links links;
        [Serializable]
        public class Links
        {
            public string checkConnection;          // https://www.google.com
            [Space(10)]
            public string serverRelease;            // https://rsc-main-jetty.azurewebsites.net/rsc-server/
                                                    // http://rsc-server.sunday-games.com/rsc-server/
            public string serverDebug;              // http://10.0.1.6:8080/rsc-server/
            public string server { get { return build.serverDebug ? serverDebug : serverRelease; } }
            public string syncUser;                 // users/sync
            public string deleteUser;               // users/delete
            public string verifyPromocode;          // codes/find
            public string verifyPurchaseApple;      // iap/apple
            public string verifyPurchaseGoogle;     // iap/google
            public string tournament;               // tournament/records
            public string championship;             // championship/records
            [Space(10)]
            public string hosting;                  // http://sunday-games.com/games/ready-set-cat/
            [Space(10)]
            public string vkGroup;                  // https://vk.com/readysetcat
            public string fbGroup;                  // https://www.facebook.com/readysetcat
            public string fbGroupShort;             // https://bit.ly/rsc-facebook
            public string twitter;                  // https://twitter.com/readysetcat
            public string youtube;                  // https://www.youtube.com/channel/UC-BEKVsRHlgkKTupFa05iAA
            [Space(10)]
            public string landingPage;
            [Space(10)]
            public string supportEmail;
            [Space(10)]
            public string contactsPage;
            [Space(10)]
            public string helpPage;
            public string helpPageRU;
            [Space(10)]
            public string[] timeServers = new[] { "pool.ntp.org", "time1.google.com" };
        }

        public void SyncUser(Dictionary<string, object> userData, Action<Download> callback)
        {
            RequestDict("Sync User", links.server + links.syncUser, userData, callback);
        }

        public void DeleteUser(string facebookId, Action<Download> callback)
        {
            RequestDict("Delete User", links.server + links.deleteUser,
                new Dictionary<string, object> { { "facebookId", facebookId } }, callback);
        }

        public void VerifyPromocode(string code, Action<Download> callback)
        {
            RequestDict("Verify Promocode", links.server + links.verifyPromocode,
                new Dictionary<string, object> { { "code", code }, { "deviceId", user.deviceId } }, callback);
        }

        public void GetTournament(Action<List<Rival>, long, List<Rival>, int> callback)
        {
            var players = new List<object>();
            players.Add(user.facebookId);
            foreach (var friend in user.friends) players.Add(friend.facebookID);

            var request = new Dictionary<string, object>()
            {
                { "userId", string.IsNullOrEmpty(user.id) ? "me" : user.id},
                { "record", user.record },
                { "players", players },
                { "currentTournamentEndDate", user.recordTimestamp },
            };

            RequestDict("Get Tournament", links.server + links.tournament, request, download =>
            {
                if (!download.isSuccess)
                {
                    callback(null, 0, null, 0);
                    return;
                }

                LogDebug("Server - Get Tournament - Server response: {0}", download.www.text);

                var response = Json.Deserialize(download.www.text) as Dictionary<string, object>;

                if (response == null || !response.ContainsKey("currentTournamentEndDate"))
                {
                    LogError("Server - Get Tournament - Cant parse response: {0}", download.www.text);
                    callback(null, 0, null, 0);
                    return;
                }

                object temp;

                List<Rival> tournamentPlayers = null;
                if (response.TryGetValue("tournamentPlayers", out temp) && temp != null)
                {
                    tournamentPlayers = new List<Rival>();
                    var tournamentPlayersList = temp as List<object>;
                    foreach (Dictionary<string, object> rival in tournamentPlayersList)
                    {
                        tournamentPlayers.Add(new Rival(
                            (string)rival["userId"],
                            (string)rival["name"],
                            Convert.ToInt32(rival["level"]),
                            (string)rival["fbId"],
                            Convert.ToInt32(rival["record"])));

                        if (tournamentPlayers.Count > 200) break;
                    }
                }

                StartCoroutine(DownloadAtlas(tournamentPlayers, () =>
                {
                    List<Rival> currentTournamentPlayers = null;
                    if (response.TryGetValue("currentTournamentPlayers", out temp) && temp != null)
                    {
                        currentTournamentPlayers = new List<Rival>();
                        var playersList = temp as List<object>;
                        foreach (Dictionary<string, object> rival in playersList)
                        {
                            currentTournamentPlayers.Add(new Rival(
                                (string)rival["userId"],
                                (string)rival["name"],
                                Convert.ToInt32(rival["level"]),
                                (string)rival["fbId"],
                                Convert.ToInt32(rival["record"])));

                            if (currentTournamentPlayers.Count > 200) break;
                        }
                    }

                    foreach (var friend in user.invitableFriends) currentTournamentPlayers.Add(friend);

                    StartCoroutine(DownloadAtlas(currentTournamentPlayers, () =>
                    {
                        callback(
                            currentTournamentPlayers,
                            Convert.ToInt64(response["currentTournamentEndDate"]),
                            tournamentPlayers,
                            Convert.ToInt32(response["tournamentCoins"]));
                    }));
                }));
            }, hashValidation: false);
        }

        public void GetChampionship(Action<List<Rival>> callback)
        {
            var request = new Dictionary<string, object>()
            {
                { "permanentRecord", user.permanentRecord },
                { "userId", string.IsNullOrEmpty(user.id) ? "me" : user.id},
            };

            RequestDict("Get Championship", links.server + links.championship, request, download =>
             {
                 if (!download.isSuccess)
                 {
                     callback(null);
                     return;
                 }

                 LogDebug("Server - Get Championship - Server response: {0}", download.www.text);

                 var rivalsDict = Json.Deserialize(download.www.text) as List<object>;

                 if (rivalsDict == null)
                 {
                     LogError("Server - Get Championship - Cant parse response: {0}", download.www.text);
                     callback(null);
                     return;
                 }

                 var rivals = new List<Rival>();
                 foreach (Dictionary<string, object> rivalDict in rivalsDict)
                 {
                     Rival rival = null;

                     if ((string)rivalDict["userId"] == Rival.ID_ME)
                     {
                         rival = new Rival(
                             Rival.ID_ME,
                             !string.IsNullOrEmpty(user.nameToView) ? user.nameToView : Localization.Get("player"),
                             user.level,
                             string.Empty,
                             user.permanentRecord);

                         if (user.socialPic != null) rival.userPicTexture = user.socialPic;
                     }
                     else if ((string)rivalDict["userId"] == user.id)
                     {
                         rival = new Rival(
                             (string)rivalDict["userId"],
                             user.firstName,
                             user.level,
                             user.facebookId,
                             user.permanentRecord);
                     }
                     else
                     {
                         rival = new Rival(
                             (string)rivalDict["userId"],
                             (string)rivalDict["name"],
                             Convert.ToInt32(rivalDict["level"]),
                             (string)rivalDict["fbId"],
                             Convert.ToInt32(rivalDict["record"]));
                     }

                     rivals.Add(rival);

                     if (rivals.Count > 200) break;
                 }

                 //if (fb.isInit)
                 //{
                 var rivalsToLoad = new List<Rival>();
                 foreach (var rival in rivals)
                     if (!string.IsNullOrEmpty(rival.facebookID)) rivalsToLoad.Add(rival);

                 StartCoroutine(DownloadAtlas(rivalsToLoad, () => { callback(rivals); }));
                 //}
                 //else
                 //{
                 //    server.CheckConnection(succeess => { if (succeess) fb.TryInit(); });
                 //    callback(rivals);
                 //}
             }, hashValidation: false);
        }

        public void VerifyPurchase(PurchaseData data, Action<bool?> callback)
        {
            var url = links.server;

            var request = new Dictionary<string, object>()
            {
                { "productId", data.iap.sku },
                { "orderId", data.transaction },
                { "deviceId", user.deviceId },
                { "userId", user.id },
            };

            if (platform == Platform.AppStore || platform == Platform.tvOS)
            {
                url += links.verifyPurchaseApple;

                request.Add("base64EncodedReceipt", data.receipt);
                request.Add("debug", isDebug);
            }
            else if (platform == Platform.GooglePlay)
            {
                url += links.verifyPurchaseGoogle;

                request.Add("packageName", build.ID);
                request.Add("purchaseToken", (Json.Deserialize(data.receipt) as Dictionary<string, object>)["purchaseToken"]);
                request.Add("developerPayload", data.receipt);
            }

            RequestDict("Verify Purchase", url, request, download =>
            {
                if (download.isSuccess && download.responseDict != null)
                {
                    if (download.responseDict.ContainsKey("valid") && (bool)download.responseDict["valid"])
                    {
                        callback?.Invoke(true);
                        if (!string.IsNullOrEmpty(data.transaction)) PlayerPrefs.SetString("Purchase " + data.transaction, "Success");
                    }
                    else
                    {
                        if (download.responseDict.ContainsKey("message"))
                            LogError("Server - Verify Purchase - Store Error: " + download.responseDict["message"]);

                        callback?.Invoke(false);
                        if (!string.IsNullOrEmpty(data.transaction)) PlayerPrefs.SetString("Purchase " + data.transaction, "Failed");
                    }
                }
                else if (download.isCorrupted)
                {
                    LogError("Server - Verify Purchase - Corrupted");
                    callback?.Invoke(false);
                }
                else
                {
                    LogError("Server - Verify Purchase - Unknown Error");
                    callback?.Invoke(null);
                }
            });
        }

        //DateTime utcDataTime = DateTime.MinValue;
        //DateTime updateDataTime = DateTime.MinValue;
        //public void GetTime(Action<DateTime> callback)
        //{
        //    if (utcDataTime != DateTime.MinValue && (DateTime.UtcNow - updateDataTime).TotalMinutes < 10)
        //    {
        //        callback(utcDataTime + (DateTime.UtcNow - updateDataTime));
        //        return;
        //    }

        //    Download("Get Time", new WWW(links.gameServer + "time"), download =>
        //    {
        //        if (download.isSuccess)
        //        {
        //            LogDebug("Server - Get Time ({0}) - Server response: {1}", download.time, download.www.text);

        //            var response = Json.Deserialize(download.www.text) as Dictionary<string, object>;

        //            if (response != null && response.ContainsKey("millis"))
        //            {
        //                updateDataTime = DateTime.UtcNow;
        //                utcDataTime = Utils.TimestampToTime(Convert.ToInt64(response["millis"]));
        //                callback(utcDataTime);
        //            }
        //            else
        //            {
        //                LogError("Server - Get Time ({0}) - Cant parse response: {1}", download.time, download.www.text);
        //                callback(DateTime.MinValue);
        //            }
        //        }
        //        else callback(DateTime.MinValue);
        //    });
        //}

        public DateTime timeUTC;
        public void UpdateTime(Action<bool> callback)
        {
            foreach (var url in links.timeServers)
            {
                try
                {
                    var ntpData = new byte[48];
                    ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

                    var ipEndPoint = new IPEndPoint(Dns.GetHostEntry(url).AddressList[0], 123);
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    socket.SendTimeout = socket.ReceiveTimeout = 8000;
                    socket.Connect(ipEndPoint);
                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();

                    var intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
                    var fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];
                    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                    timeUTC = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);
                    //lastUpdateTime = DateTime.Now;

                    Log("Server - UpdateTime - Success. Time Shift: {0}", (DateTime.UtcNow - timeUTC).TotalSeconds);
                    callback(true);
                    return;
                }
                catch (Exception e)
                {
                    LogError("Server - UpdateTime - {0} - Error: {1}", url, e);
                }
            }

            callback(false);
        }

        IEnumerator DownloadAtlas(List<Rival> rivals, Action callback)
        {
            if (rivals != null && rivals.Count > 0)
            {
                Texture2D userPicsAtlas = null;
                if (rivals.Count < 10) userPicsAtlas = new Texture2D(512, 512);
                else if (rivals.Count < 50) userPicsAtlas = new Texture2D(1024, 1024);
                else userPicsAtlas = new Texture2D(2048, 2048);

                int loadedPics = 0;
                var textures = new Texture2D[rivals.Count];
                for (int i = 0; i < rivals.Count; i++)
                    if (!string.IsNullOrEmpty(rivals[i].userPicUrl))
                        DownloadPic(rivals[i].userPicUrl, i, (texture, j) => { textures[j] = texture; ++loadedPics; });
#if FACEBOOK
                    else fb.LoadFriendImgFromId(rivals[i].facebookID, i, (texture, j) => { textures[j] = texture; ++loadedPics; });
#else
                // else DownloadPic(FacebookManager.GetPicURL(rivals[i].facebookID), i, (texture, j) => { textures[j] = texture; ++loadedPics; });
#endif

                var startTime = Time.time;
                while (loadedPics < rivals.Count && Time.time - startTime < timeout) yield return null;

                var rects = userPicsAtlas.PackTextures(textures, 0, userPicsAtlas.height);

                for (int i = 0; i < rivals.Count; i++)
                    if (rects[i] != zeroRect)
                    {
                        rivals[i].userPicTexture = userPicsAtlas;
                        rivals[i].userPicTextureRect = rects[i];
                    }
            }

            callback();
        }
        void DownloadPic(string url, int i, Action<Texture2D, int> callback)
        {
            Download.Create(gameObject).Run("Downloading Pic", url,
                download => { if (download.isSuccess) callback(download.www.texture, i); });
        }

        public void DownloadPic(UnityEngine.UI.RawImage image, string url)
        {
            Download.Create(gameObject).Run("Downloading Pic", url, download =>
            {
                if (download.isSuccess)
                {
                    var texture = download.www.texture;
                    if (texture != null) image.texture = texture;
                }
            });
        }

        public void DownloadPic(UnityEngine.UI.Image image, string url)
        {
            Download.Create(gameObject).Run("Downloading Pic", url, download =>
            {
                if (download.isSuccess)
                {
                    var texture = download.www.texture;
                    if (texture != null)
                        image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            });
        }

        public void CheckConnection(Action<bool> callback)
        {
            if (platform == Platform.Facebook) { callback(true); return; }

            Download.Create(gameObject).Run("Check Connection", links.checkConnection,
                download => { callback(download.isSuccess); });
        }

        void RequestDict(string name, string url, Dictionary<string, object> requestDict, Action<Download> callback, bool hashValidation = true)
        {
            var request = Json.Serialize(requestDict);

            var headers = new Dictionary<string, string>() {
                { "Content-Type", "application/json" },
                { "Version", build.version },
                { "Key", (request + build.s).MD5() },
            };

            LogDebug("Server - {0} - Url: '{1}', Key: '{2}', Request: {3}", name, url, headers["Key"], request);

            Download.Create(gameObject).Run(name, new WWW(url, Encoding.UTF8.GetBytes(request), headers), download =>
            {
                if (!download.isSuccess || string.IsNullOrEmpty(download.www.text))
                {
                    callback(download);
                    return;
                }

                if (hashValidation)
                {
                    if (!download.www.responseHeaders.ContainsKey("KEY") || download.www.responseHeaders["KEY"] != (download.www.text + build.s).MD5())
                    {
                        LogError("Server - {0} - Hash validation failed. Response: {1}", name, download.www.text);
                        download.status = Download.Status.Corrupted;
                        callback(download);
                        return;
                    }
                }

                download.responseDict = Json.Deserialize(download.www.text) as Dictionary<string, object>;

                callback(download);
            });
        }
    }
}