using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class UserSG : Player
    {
        public override void Init()
        {
            base.Init();

            if (!data.ContainsKey("id")) data.Add("id", null);

#if PREVENT_READ_PHONE_STATE
            if (!data.ContainsKey("deviceId")) data.Add("deviceId", UnityEngine.Random.Range(100000000, 999999999).ToString());
#else
            if (!data.ContainsKey("deviceId")) data.Add("deviceId", SystemInfo.deviceUniqueIdentifier);
#endif

            if (!data.ContainsKey("facebookId")) data.Add("facebookId", null);
            if (!data.ContainsKey("email")) data.Add("email", null);
            if (!data.ContainsKey("name")) data.Add("name", null);
            if (!data.ContainsKey("firstName")) data.Add("firstName", null);
            if (!data.ContainsKey("lastName")) data.Add("lastName", null);
            if (!data.ContainsKey("gender")) data.Add("gender", 0);

            if (!data.ContainsKey("socialName")) data.Add("socialName", null);
            if (!data.ContainsKey("gameCenterId")) data.Add("gameCenterId", null);
            if (!data.ContainsKey("googleGamesId")) data.Add("googleGamesId", null);

            if (!data.ContainsKey("status")) data.Add("status", 0);

            if (!data.ContainsKey("revenue")) data.Add("revenue", 0.0f);

            if (!data.ContainsKey("version")) data.Add("version", Configurator.Instance.appInfo.version);
            if (version != Configurator.Instance.appInfo.version) data["version"] = Configurator.Instance.appInfo.version;

            if (!data.ContainsKey("deviceInfo")) data.Add("deviceInfo", Json.Serialize(SG_Utils.deviceInfo));

            if (!data.ContainsKey("firstDate")) data.Add("firstDate", SG_Utils.dateNowFormated);

            if (!data.ContainsKey("firstVersion")) data.Add("firstVersion", Configurator.Instance.appInfo.version);

            TutorialLoad();

            //if (!data.ContainsKey("invitedBy")) data.Add("invitedBy", null);

            Analytic.SetUserProperties(new Dictionary<string, object>() {
                    { "firstDate", firstDate },
                    { "firstVersion", firstVersion },
                    { "version", version },
                });

            StartCoroutine(SyncCoroutine());
        }

        public string firstDate
        {
            get { return (string)data["firstDate"]; }
            protected set
            {
                data["firstDate"] = value;
                Save();
            }
        }

        public string firstVersion
        {
            get { return (string)data["firstVersion"]; }
            protected set
            {
                data["firstVersion"] = value;
                Save();
            }
        }

        public string version
        {
            get { return (string)data["version"]; }
            protected set
            {
                data["version"] = value;
                Save();
            }
        }

        public bool isId { get { return !string.IsNullOrEmpty(id); } }
        public string id
        {
            get { return (string)data["id"]; }
            protected set
            {
                data["id"] = value;
                Save();
            }
        }

        public string deviceId
        {
            get { return (string)data["deviceId"]; }
            protected set
            {
                data["deviceId"] = value;
                Save();
            }
        }

        public bool isForeignId
        {
            get
            {
                return !string.IsNullOrEmpty(facebookId);
                // || !string.IsNullOrEmpty(gameCenterId)
                // || !string.IsNullOrEmpty(googleGamesId);
            }
        }

        public string socialName
        {
            get { return (string)data["socialName"]; }
            set
            {
                data["socialName"] = value;
                Save();
            }
        }

        public string gameCenterId
        {
            get { return (string)data["gameCenterId"]; }
            set
            {
                data["gameCenterId"] = value;
                Save();
            }
        }

        public string googleGamesId
        {
            get { return (string)data["googleGamesId"]; }
            set
            {
                data["googleGamesId"] = value;
                Save();
            }
        }

        public string email
        {
            get { return (string)data["email"]; }
            set
            {
                data["email"] = value;
                Save();
            }
        }

        public string fullName
        {
            get { return (string)data["name"]; }
            set
            {
                data["name"] = value;
                Save();
            }
        }

        public string firstName
        {
            get { return (string)data["firstName"]; }
            set
            {
                data["firstName"] = value;
                Save();
            }
        }

        public string lastName
        {
            get { return (string)data["lastName"]; }
            set
            {
                data["lastName"] = value;
                Save();
            }
        }

        public enum Gender : byte { Unknown = 0, Male = 1, Female = 2 }
        public Gender gender
        {
            get { return (Gender)Convert.ToByte(data["gender"]); }
            set
            {
                data["gender"] = Convert.ToByte(value);
                Save();
            }
        }

        public string nameToView
        {
            get
            {
                if (!string.IsNullOrEmpty(firstName)) return firstName;
                else if (!string.IsNullOrEmpty(fullName)) return fullName;
                else if (!string.IsNullOrEmpty(socialName)) return socialName;
                else return null;
            }
        }

        public enum Status : byte { Normal = 0, ForceSync = 1, Banned = 6 }
        public Status status
        {
            get { return (Status)Convert.ToByte(data["status"]); }
            set
            {
                data["status"] = Convert.ToByte(value);
                Save();
            }
        }

        public float revenue
        {
            get { return Convert.ToSingle(data["revenue"]); }
            protected set
            {
                data["revenue"] = value;
                Analytic.SetUserProperties("revenue", revenue);
                Save();
            }
        }


        public virtual void Buy(IAP_SG iap)
        {
            revenue += Convert.ToSingle(iap.priceUSD);
        }

        public string facebookId
        {
            get { return (string)data["facebookId"]; }
            set
            {
                data["facebookId"] = value;
                Save();
            }
        }
        public virtual void SetFacebookData(IDictionary<string, object> facebookData)
        {
            if (facebookData == null) return;

            object temp;

            bool isFirstTimeFacebookLogin = string.IsNullOrEmpty(facebookId);

            if (facebookData.TryGetValue("id", out temp)) facebookId = (string)temp;
            if (facebookData.TryGetValue("name", out temp)) fullName = (string)temp;
            if (facebookData.TryGetValue("first_name", out temp)) firstName = (string)temp;
            if (facebookData.TryGetValue("last_name", out temp)) lastName = (string)temp;
            if (facebookData.TryGetValue("email", out temp)) email = (string)temp;
            if (facebookData.TryGetValue("gender", out temp)) gender = (string)temp == "male" ? Gender.Male : Gender.Female;

            if (isFirstTimeFacebookLogin)
                Analytic.SetUserProperties(new Dictionary<string, object>() {
                { "facebookId", facebookId },
                { "fullName", fullName },
                { "email", email },
                { "gender", gender.ToString() },
            });

            //        {
            //  "id": "10204916518582922",
            //  "first_name": "Sergey",
            //  "last_name": "Kopov",
            //  "gender": "male",
            //  "age_range": {
            //    "min": 21
            //  },
            //  "email": "sergey.kopov@gmail.com",
            //  "friends": {
            //    "data": [
            //      {
            //        "first_name": "Nadia",
            //        "id": "10152536787152921",
            //        "picture": {
            //          "data": {
            //            "height": 200,
            //            "is_silhouette": false,
            //            "url": "https://scontent.xx.fbcdn.net/hprofile-xtp1/v/t1.0-1/c3.0.200.200/p200x200/12644751_10153453883592921_1769918035433820971_n.jpg?oh=ae9422673558c11eaad585a82de045cf&oe=573DE034",
            //            "width": 200
            //          }
            //        }
            //      },
            //      {
            //        "first_name": "Cassy",
            //        "id": "10154104640055550",
            //        "picture": {
            //          "data": {
            //            "height": 200,
            //            "is_silhouette": false,
            //            "url": "https://scontent.xx.fbcdn.net/hprofile-xlp1/v/t1.0-1/c0.0.200.200/p200x200/10427686_10154497538070550_3539351136513485076_n.jpg?oh=1a4559e9d234287f48671a833b2f0083&oe=573BC92F",
            //            "width": 200
            //          }
            //        }
            //      }
            //    ],
            //    "paging": {
            //      "next": "https://graph.facebook.com/v2.5/10204916518582922/friends?limit=2&fields=first_name,id,picture.width%28200%29.height%28200%29&access_token=CAALCpbbMpiMBAEusGokZAW2MglA6eo7hOOr0Bte0iGUeP2HmA4aUuApcBGt0MuK9dzmfLtezVgvRm6Lawe1lzFkME2FdTppzObiKX8cZASVeIohXHrg8IskTJ2Vba5hHnEbbgDYi5YxkYf1X1KHPzqdauU7bI2Jzsd4nyawtPZCwyyRSCRvndYP0ZAY7dTAT0WPgYO1YQThjOtWHiyRt&offset=2&__after_id=enc_AdCcdnvoxFY4TsuUZBK9NZBDuNXorDAxRY7WslZCiOZAhZBFZCwwJ0ohRCuTXWZAtmSFkCXdXMZD"
            //    },
            //    "summary": {
            //      "total_count": 905
            //    }
            //  },
            //  "invitable_friends": {
            //    "data": [
            //      {
            //        "first_name": "Александр",
            //        "picture": {
            //          "data": {
            //            "height": 200,
            //            "is_silhouette": false,
            //            "url": "https://scontent.xx.fbcdn.net/hprofile-xpl1/v/t1.0-1/p200x200/12688194_948009925247283_7846382134337642944_n.jpg?oh=ef70ffec6fc5dfbec6ed3e7253dc5b6b&oe=576C6440",
            //            "width": 200
            //          }
            //        },
            //        "id": "AVmIeKb_xKyqoqtwSoN34bz8KTNtizuCKdCEZSdXxPrunRHVdE5oYKYNWjvhblCTfGwj7BiW-EuqRXspVpzelG37fiy3fpQRt3FNz-mKJeTr-A"
            //      },
            //      {
            //        "first_name": "Aleksey",
            //        "picture": {
            //          "data": {
            //            "height": 200,
            //            "is_silhouette": false,
            //            "url": "https://scontent.xx.fbcdn.net/hprofile-xta1/v/t1.0-1/p200x200/10406801_10205409122241644_5094310415932755059_n.jpg?oh=b21d13d5389a85f96c0b62b2522f1323&oe=572DAF99",
            //            "width": 200
            //          }
            //        },
            //        "id": "AVnm11pdjHBaxE0x0yyVAVZMTy6dRekqtVWTT5YkGJ0m9k9tMwu_2Ow8Dskm2BE-7JmPvKUKpl84HyB5Oo1VMYHa2gPBttc9yYS742OF9e0jvw"
            //      }
            //    ],
            //    "paging": {
            //      "cursors": {
            //        "before": "QVZAraThVa2ozSDNQdnV1N1F1SWM5VFhHLTZAGaU1XdVBSQUpoMGRyaEYyZAm15bC0yWF9CcVNvY1VONGp6Q2h6c0xWQUFKM0FCckljcmYwVzZArQ0ZABdUdlM2EyaDlyWXkwbUQzMTZAaYkloeWRaRVEZD",
            //        "after": "QVZAtRzR2andlT19xMjZAIX1plMTYxTk5DZAHRuNE8wV1RuYXFKUmotYTF5d19UQkxrSFAwZAmtTRmF6ZAzh6aGhwM0UxelhJUmcyVFhrVEZAhYXMtQl9jWkp5anZAwRWNiSWh4a2RvZAVpVRV9sWkx4dmcZD"
            //      },
            //      "next": "https://graph.facebook.com/v2.5/10204916518582922/invitable_friends?access_token=CAALCpbbMpiMBAEusGokZAW2MglA6eo7hOOr0Bte0iGUeP2HmA4aUuApcBGt0MuK9dzmfLtezVgvRm6Lawe1lzFkME2FdTppzObiKX8cZASVeIohXHrg8IskTJ2Vba5hHnEbbgDYi5YxkYf1X1KHPzqdauU7bI2Jzsd4nyawtPZCwyyRSCRvndYP0ZAY7dTAT0WPgYO1YQThjOtWHiyRt&pretty=0&fields=first_name%2Cpicture.width%28200%29.height%28200%29&limit=2&after=QVZAtRzR2andlT19xMjZAIX1plMTYxTk5DZAHRuNE8wV1RuYXFKUmotYTF5d19UQkxrSFAwZAmtTRmF6ZAzh6aGhwM0UxelhJUmcyVFhrVEZAhYXMtQl9jWkp5anZAwRWNiSWh4a2RvZAVpVRV9sWkx4dmcZD"
            //    }
            //  }
            //}
        }


        public string invitedBy
        {
            get { return (string)data["invitedBy"]; }
            set
            {
                data["invitedBy"] = value;
                Save();
            }
        }
        public virtual void Invited(string id)
        {
            if (!string.IsNullOrEmpty(invitedBy))
            {
                Log.Info($"User - Already Invited by {invitedBy}");
                return;
            }

            invitedBy = id;
        }


        Dictionary<string, object> tutorial = new Dictionary<string, object>();
        void TutorialLoad()
        {
            if (data.ContainsKey("tutorial"))
                tutorial = Json.Deserialize((string)data["tutorial"]) as Dictionary<string, object>;
            else
                data.Add("tutorial", "{}");
        }
        void TutorialSave(bool syncServer)
        {
            data["tutorial"] = Json.Serialize(tutorial);
            Save();

            if (syncServer && isId) SyncServer();
        }
        public bool IsTutorialShown(Tutorial.Part part)
        {
            if (tutorial.ContainsKey(part.name))
                return Convert.ToInt32(tutorial[part.name]) > 0 ? true : false;
            else
            {
                tutorial[part.name] = 0;
                return false;
            }
        }
        public void TutorialShown(Tutorial.Part part)
        {
            if (tutorial.ContainsKey(part.name))
                tutorial[part.name] = Convert.ToInt32(tutorial[part.name]) + 1;
            else
                tutorial[part.name] = 1;

            TutorialSave(true);
        }


        public void SyncServer(bool force = false, Action callback = null)
        {
            needSync = true;

            if (force) SyncNow(callback);
        }
        TimeSpan timeSpan = TimeSpan.FromSeconds(120);
        DateTime lastSyncTime;
        bool needSync = false;
        IEnumerator SyncCoroutine()
        {
            lastSyncTime = DateTime.Now;

            while (true)
            {
                while (DateTime.Now - lastSyncTime < timeSpan) yield return null;

                SyncNow();

                yield return null;
            }
        }
        void SyncNow(Action callback = null)
        {
            if (blockSave || !needSync || !isForeignId)
            {
                callback?.Invoke();
                return;
            }

            server.SyncUser(data, download =>
            {
                if (download.success && download.responseDict != null)
                    ServerToLocal(download.responseDict);

                callback?.Invoke();
            });

            lastSyncTime = DateTime.Now;
            needSync = false;
        }

        protected virtual void ServerToLocal(Dictionary<string, object> serverData)
        {
            data = serverData;

            // Analytic.SetUserProperties(new Dictionary<string, object>() { { "revenue", revenue }, });

            Save();
        }
    }
}