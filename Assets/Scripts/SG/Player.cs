using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG.RSC
{
    public class Player : Core
    {
        protected Dictionary<string, object> data;
        public bool blockSave = false;

        public virtual void Init()
        {
            Log.Debug($"Player - Load: {ObscuredPrefs.GetString("user", "{}")}");

            data = Json.Deserialize(ObscuredPrefs.GetString("user", "{}")) as Dictionary<string, object>;

            if (ObscuredPrefs.HasKey("firstDate") && ObscuredPrefs.GetString("user", "{}") == "{}") LoadV1();

            StartCoroutine(SaveCoroutine());
        }

        void LoadV1()
        {
            // data.Add("id", ObscuredPrefs.GetString("id", null));

            data.Add("facebookId", ObscuredPrefs.GetString("facebookID", null));
            data.Add("name", ObscuredPrefs.GetString("name", null));
            data.Add("firstName", ObscuredPrefs.GetString("firstName", null));
            data.Add("lastName", ObscuredPrefs.GetString("lastName", null));

            data.Add("socialName", ObscuredPrefs.GetString("socialName", null));
            data.Add("gameCenterId", ObscuredPrefs.GetString("gameCenterID", null));
            data.Add("googleGamesId", ObscuredPrefs.GetString("googleID", null));

            data.Add("status", ObscuredPrefs.GetInt("status", 0));

            data.Add("revenue", (float)ObscuredPrefs.GetInt("revenue", 0));

            data.Add("firstDate", ObscuredPrefs.GetString("firstDate", SG_Utils.dateNowFormated));
            data.Add("firstVersion", ObscuredPrefs.GetString("firstVersion", Configurator.Instance.appInfo.version));

            data.Add("tutorial", ObscuredPrefs.GetString("tutorial", "{}"));

            data.Add("level", ObscuredPrefs.GetInt("level", 0));

            data.Add("record", ObscuredPrefs.GetInt("record", 0));
            data.Add("permanentRecord", ObscuredPrefs.GetInt("permanentRecord", 0));

            data.Add("coins", ObscuredPrefs.GetInt("coins", balance.reward.startCoins));

            data.Add("spins", ObscuredPrefs.GetInt("spins", 1));

            data.Add("nextFreeSpinDate", new DateTime(2017, 2, 10).ToTimestamp());

            data.Add("boosts", ObscuredPrefs.GetString("boosts", "{}"));
            data.Add("collection", ObscuredPrefs.GetString("collection", null));
            data.Add("invitedFriends", ObscuredPrefs.GetString("invitedFriends", "[]"));
            data.Add("tournamentsWon", ObscuredPrefs.GetString("tournamentsWon", "[]"));
            data.Add("stats", ObscuredPrefs.GetString("stats", "{}"));

        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(10);
        DateTime lastSaveTime;
        bool needSave = false;
        protected void Save(bool force = false)
        {
            needSave = true;

            if (force) SaveNow();
        }
        IEnumerator SaveCoroutine()
        {
            lastSaveTime = DateTime.Now;

            while (true)
            {
                while (DateTime.Now - lastSaveTime < timeSpan) yield return null;

                SaveNow();

                yield return null;
            }
        }
        void SaveNow()
        {
            if (blockSave || !needSave) return;

            // Log.Debug("Player - Save: " + Json.Serialize(data));

            ObscuredPrefs.SetString("user", Json.Serialize(data));
            // ObscuredPrefs.Save();

            lastSaveTime = DateTime.Now;
            needSave = false;
        }
        void OnApplicationQuit() { SaveNow(); }
        void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveNow(); }
    }
}