using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG.RSC
{
    public class User : UserSG
    {
        public override void Init()
        {
            base.Init();

            if (!data.ContainsKey("level")) data.Add("level", 0);

            if (!data.ContainsKey("record")) data.Add("record", 0);
            if (!data.ContainsKey("recordTime")) data.Add("recordTime", 0);
            if (!data.ContainsKey("permanentRecord")) data.Add("permanentRecord", 0);

            if (!data.ContainsKey("coins")) data.Add("coins", balance.reward.startCoins);

            if (!data.ContainsKey("spins")) data.Add("spins", 1);
            if (!data.ContainsKey("nextFreeSpinDate")) data.Add("nextFreeSpinDate", 0);

            Missions.UNLOCK(level);
            BoostsLoad();
            CollectionLoad();
            InvitedFriendsLoad();
            TournamentsWonLoad();
            StatsLoad();
        }

        public override void SetFacebookData(IDictionary<string, object> facebookData)
        {
            if (facebookData == null) return;

            object temp;

            base.SetFacebookData(facebookData);

            if (facebookData.TryGetValue("friends", out temp))
            {
                var friendList = ((Dictionary<string, object>)temp)["data"] as List<object>;
                friends.Clear();
                foreach (Dictionary<string, object> friend in friendList)
                {
                    var picture = ((Dictionary<string, object>)(friend["picture"]))["data"] as Dictionary<string, object>;

                    friends.Add(new Rival(
                        string.Empty,
                        (string)friend["first_name"],
                        0,
                        (string)friend["id"],
                        0,
                        (string)picture["url"],
                        Convert.ToInt32(picture["width"]),
                        Convert.ToInt32(picture["height"])));
                }
            }

            if (facebookData.TryGetValue("invitable_friends", out temp))
            {
                var invitableFriendListAll = (temp as Dictionary<string, object>)["data"] as List<object>;
                invitableFriends.Clear();

                var invitableFriendList = new List<object>();
                if (invitableFriendListAll.Count > 3)
                    for (int i = 0; i < 3; i++)
                    {
                        int n = UnityEngine.Random.Range(0, invitableFriendListAll.Count);
                        invitableFriendList.Add(invitableFriendListAll[n]);
                        invitableFriendListAll.RemoveAt(n);
                    }
                else invitableFriendList = invitableFriendListAll;

                foreach (Dictionary<string, object> friend in invitableFriendList)
                {
                    var picture = (friend["picture"] as Dictionary<string, object>)["data"] as Dictionary<string, object>;

                    invitableFriends.Add(new Rival(
                        string.Empty,
                        (string)friend["first_name"],
                        0,
                        (string)friend["id"],
                        0,
                        (string)picture["url"],
                        Convert.ToInt32(picture["width"]),
                        Convert.ToInt32(picture["height"])));
                }
            }
        }

        protected override void ServerToLocal(Dictionary<string, object> serverData)
        {
            data = serverData;

            // Analytic.SetUserProperties(new Dictionary<string, object>() { { "revenue", revenue }, });

            Missions.UNLOCK(level);
            BoostsLoad();
            CollectionLoad();
            InvitedFriendsLoad();
            TournamentsWonLoad();
            StatsLoad();

            Save();

            ui.header.UpdateAll();
        }

        public override void Buy(IAP_SG iap)
        {
            base.Buy(iap);

            achievements.OnGetRevenue();

            if (iap == iapManager.catboxPremium)
                GetPremiumCatbox();
            else if (iap == iapManager.catboxPremiumSale)
                GetPremiumCatbox();
            else if (iap == iapManager.catboxSimple)
                GetSimpleCatbox();
            else if (iap == iapManager.goldfishes)
                UpdateCoins(ui.shop.aquarium * ui.shop.xGoldfishes, false);
            else if (iap == iapManager.sausages)
                UpdateSpins(balance.shop.sausages * ui.shop.xSausages, false);
            else if (iap == iapManager.skipMissions)
                LevelUp();
            else if (iap == iapManager.cat)
                GetRentCat();
            else if (iapManager.hats && iap == iapManager.hats)
                newYearHats += balance.events.hatsPurchase;
            else if (iapManager.valentines && iap == iapManager.valentines)
                stValentinHearts += balance.events.heartsPurchase;
            else if (iapManager.bats && iap == iapManager.bats)
                halloweenBats += balance.events.batsPurchase;

            SyncServer(force: true);
        }


        public int level
        {
            get { return Convert.ToInt32(data["level"]); }
            protected set
            {
                data["level"] = value;
                Analytic.SetUserProperties("level", level);
                Save();
            }
        }
        public void UpdateLevel(int value, bool syncServer)
        {
            level = value;

            Missions.UNLOCK(level);

            if (syncServer && isId) SyncServer();
        }
        public void LevelUp()
        {
            if (level >= Missions.MAX_LEVEL) return;

            Level l = Missions.LEVELS[level];

            foreach (Mission mission in l.missions)
                Analytic.EventProperties("Missions", mission.name + " " + mission.target, Analytic.Round(mission.current()).ToString());

            foreach (Mission mission in l.missions)
                if (mission.clear != null) mission.clear();

            if (l.gift == Gifts.catbox && !isCanGetSimpleBox)
                Missions.AddGift(level, Gifts.aquariumSmall, 10);

            if (l.gift == Gifts.aquariumSmall) UpdateCoins(l.giftCount, true);
            else if (l.gift == Gifts.sausage) UpdateSpins(l.giftCount, true);
            else if (l.gift == Gifts.catbox) GetSimpleCatbox();
            else if (l.gift == Gifts.boosterbox) GetBonusBox();

            if (l.giftSprite == gameplay.boosts.time.sprite) GetBoost(gameplay.boosts.time, 3);
            else if (l.giftSprite == gameplay.boosts.experience.sprite) GetBoost(gameplay.boosts.experience, 3);
            else if (l.giftSprite == gameplay.boosts.multiplier.sprite) GetBoost(gameplay.boosts.multiplier, 3);
            else if (l.giftSprite == gameplay.boosts.firework.sprite) GetBoost(gameplay.boosts.firework, 3);

            UpdateLevel(level + 1, true);

            ui.header.UpdateLevel();

            achievements.OnLevelUp();

            Analytic.EventLevelUp(level);
        }
        public bool isLevelOK { get { return 0 < level && level < Missions.MAX_LEVEL; } }


        public int coins
        {
            get { return Convert.ToInt32(data["coins"]); }
            protected set
            {
                data["coins"] = value;
                Analytic.SetUserProperties("coins", coins);
                Save();
            }
        }
        public void UpdateCoins(int change, bool syncServer)
        {
            if (change == 0) return;

            coins += change;
            Log.Info("Player" + (change > 0 ? " earn " + change : " spend " + -change) + " coins");

            if (syncServer && isId) SyncServer();

            if (coins > coinsMAX) coinsMAX = coins;

            if (change > 0) achievements.OnGetGoldfishes();
        }


        Dictionary<string, object> boosts = new Dictionary<string, object>();
        void BoostsLoad()
        {
            if (data.ContainsKey("boosts"))
            {
                boosts = Json.Deserialize((string)data["boosts"]) as Dictionary<string, object>;

                if (boosts != null)
                {
                    foreach (var boost in gameplay.boostList)
                        if (boosts.ContainsKey(boost.name)) boost.count = Convert.ToInt32(boosts[boost.name]);
                    return;
                }

                boosts = new Dictionary<string, object>();
            }

            foreach (var boost in gameplay.boostList)
                boosts[boost.name] = 0;

            data.Add("boosts", Json.Serialize(boosts));
        }
        void BoostsSave()
        {
            foreach (var boost in gameplay.boostList)
                boosts[boost.name] = boost.count;

            data["boosts"] = Json.Serialize(boosts);
            Save();
        }
        public void UseBoost(Boost boost)
        {
            if (boost.count > 0)
            {
                --boost.count;
                BoostsSave();

                Analytic.Event("Boosts", "Use");
            }
            else
            {
                UpdateCoins(-boost.price, true);

                Analytic.Event("Boosts", "Buy");
            }

            Missions.OnUseBoost(boost);

            boost.ON = true;
        }
        public void GetBoost(Boost boost, int count)
        {
            boost.count += count;
            BoostsSave();
        }
        public Boost[] lastGetBoosts = new Boost[3];
        public void GetBonusBox()
        {
            var avalibleBoosts = new List<Boost>();
            foreach (var boost in gameplay.boostList)
                if (boost.avalible()) avalibleBoosts.Add(boost);

            for (int i = 0; i < lastGetBoosts.Length; ++i)
            {
                lastGetBoosts[i] = avalibleBoosts[UnityEngine.Random.Range(0, avalibleBoosts.Count)];
                lastGetBoosts[i].count++;
                Log.Info("Player get " + lastGetBoosts[i].name);
            }

            BoostsSave();
        }
        void GetBonusBoxAll()
        {
            for (int i = 0; i < lastGetBoosts.Length; ++i)
            {
                lastGetBoosts[i] = gameplay.boostList[UnityEngine.Random.Range(0, gameplay.boostList.Length)];
                lastGetBoosts[i].count++;
                Log.Info("Player get " + lastGetBoosts[i].name);
            }

            BoostsSave();
        }


        public long permanentRecord
        {
            get { return Convert.ToInt64(data["permanentRecord"]); }
            protected set
            {
                data["permanentRecord"] = value;
                Analytic.SetUserProperties("permanentRecord", record);
                ui.main.isChampionshipUpdateNeeded = true;
                Save();
            }
        }
        public long record
        {
            get { return Convert.ToInt64(data["record"]); }
            protected set
            {
                data["record"] = value;
                Analytic.SetUserProperties("record", record);
                ui.main.isTournamentUpdateNeeded = true;
                Save();
            }
        }
        public long recordTimestamp
        {
            get { return Convert.ToInt64(data["recordTime"]); }
            set
            {
                data["recordTime"] = value;
                Save();
            }
        }
        public void ResetRecord()
        {
            record = 0;
        }
        public void UpdateRecord(long newRecord)
        {
            if (status == Status.Banned) return;

            bool syncServer = false;

            if (Missions.isTournament && record < newRecord)
            {
                record = newRecord;
                syncServer = true;
            }

            if (permanentRecord < newRecord)
            {
                permanentRecord = newRecord;
                Log.Info("New Permanent Record: " + permanentRecord);
                syncServer = true;
            }

            if (syncServer && isId) SyncServer();
        }
        public League league { get { return gameplay.GetLeague(permanentRecord); } }


        public List<CatItem> collection = new List<CatItem>();
        void CollectionLoad()
        {
            if (data.ContainsKey("collection"))
                collection = Deserialize((string)data["collection"]);
            else
                data.Add("collection", null);
        }
        public void CollectionSave(bool syncServer)
        {
            data["collection"] = Serialize(collection);
            Save();

            if (syncServer && isId) SyncServer();
        }
        string Serialize(List<CatItem> list)
        {
            string data = string.Empty;
            foreach (CatItem item in list)
            {
                data += item.ToString();
                if (item != list[list.Count - 1]) data += ";";
            }
            return data;
        }
        List<CatItem> Deserialize(string data)
        {
            var list = new List<CatItem>();

            if (!string.IsNullOrEmpty(data))
            {
                var splited = data.Split(new char[] { ';' });
                foreach (string itemData in splited) list.Add(new CatItem(itemData));
            }
            return list;
        }

        public CatItem lastGetCat = null;
        public bool isLastGetCatLevelUp = false;
        public CatItem GetCat(CatType type, int level = 1)
        {
            if (type == null) return null;

            CatItem catItem = null;

            if (isOwned(type))
            {
                isLastGetCatLevelUp = true;

                catItem = GetItem(type);

                catItem.LevelUp(resetExp: false);
            }
            else
            {
                isLastGetCatLevelUp = false;

                catItem = new CatItem(type, level, 0);
                collection.Add(catItem);
                Log.Info("Player get " + type.name + " Cat");

                Analytic.EventProperties("Progress", "OpenCatBox", catItem.type.name);
                Analytic.EventProperties("Progress", "Collection", collection.Count.ToString());
                achievements.OnGetSuperCat();
            }

            lastGetCat = catItem;

            CollectionSave(true);

            return catItem;
        }

        public void GetSimpleCatbox()
        {
            string cat;

            ++openCatbox;

            Log.Info("Open " + openCatbox + "th Simple Catbox");

            if (openCatbox == 1) cat = new string[] { Cats.Boom.ToString(), Cats.Cap.ToString() }[UnityEngine.Random.Range(0, 2)];
            else if (openCatbox == 2) cat = new string[] { Cats.Snow.ToString(), Cats.Zen.ToString() }[UnityEngine.Random.Range(0, 2)];
            else if (openCatbox == 3) cat = "Exist";
            else if (openCatbox == 4) cat = new string[] { Cats.Mage.ToString(), Cats.Joker.ToString() }[UnityEngine.Random.Range(0, 2)];
            else if (openCatbox == 5) cat = "Exist";
            else if (openCatbox == 6) cat = Cats.King.ToString();
            else if (openCatbox == 7) cat = "Random";
            else if (openCatbox == 8) cat = "Exist";
            else if (openCatbox == 9) cat = new string[] { Cats.Loki.ToString(), Cats.Disco.ToString(), Cats.Flint.ToString() }[UnityEngine.Random.Range(0, 3)];
            else if (openCatbox == 10) cat = "Exist";
            else cat = "Random";

            CatType catType = null;

            if (cat == "Random")
            {
                List<CatType> avalibleCats = new List<CatType>();
                foreach (CatType c in gameplay.superCats)
                    if ((!isOwned(c) && !c.isEventCat) || (isOwned(c) && !GetItem(c).isMaxLevel)) avalibleCats.Add(c);

                catType = avalibleCats[UnityEngine.Random.Range(0, avalibleCats.Count)];
            }
            else if (cat == "Exist")
                catType = collection[UnityEngine.Random.Range(0, collection.Count)].type;
            else
                catType = CatType.GetCatType(cat);

            GetCat(catType);
        }
        public void GetPremiumCatbox()
        {
            CatType catType = null;
            foreach (Cats cat in balance.reward.superCatsInPremiumBox)
                if (!isOwned(CatType.GetCatType(cat)))
                {
                    catType = CatType.GetCatType(cat);
                    break;
                }

            GetCat(catType);
        }
        public void GetRentCat()
        {
            if (ui.rentCat.catItem != null)
                GetCat(ui.rentCat.catItem.type, ui.rentCat.catItem.level);
            else if (ObscuredPrefs.HasKey("rentCatBuyName") && ObscuredPrefs.HasKey("rentCatBuyLavel"))
                GetCat(CatType.GetCatType(ObscuredPrefs.GetString("rentCatBuyName")), ObscuredPrefs.GetInt("rentCatBuyLavel"));
        }

        public int maxCatLevel
        {
            get
            {
                int result = 0;
                foreach (CatItem cat in collection)
                    if (cat.level > result) result = cat.level;
                return result;
            }
        }
        public int averageCatLevel
        {
            get
            {
                if (collection.Count == 0) return 0;

                int result = 0;
                foreach (CatItem cat in collection)
                    result += cat.level;
                return result / collection.Count;
            }
        }
        public bool isOwned(CatType type)
        {
            foreach (CatItem item in collection) if (item.type == type) return true;
            return false;
        }
        public bool isOwned(Cats cat)
        {
            foreach (CatItem item in collection) if (item.type.name == cat.ToString()) return true;
            return false;
        }
        public CatItem GetItem(CatType type)
        {
            foreach (CatItem item in collection) if (item.type == type) return item;
            return null;
        }
        public CatItem GetItem(Cats cat)
        {
            foreach (CatItem item in collection) if (item.type.name == cat.ToString()) return item;
            return null;
        }
        public bool isCanGetSimpleBox
        {
            get
            {
                foreach (Cats cat in balance.reward.superCatsInSimpleBox)
                    if (!isOwned(cat)) return true;

                foreach (CatItem catItem in collection)
                    if (!catItem.isMaxLevel) return true;

                return false;
            }
        }
        public bool isCanGetPremiumBox { get { return collection.Count < gameplay.superCats.Length; } }


        public int spins
        {
            get { return Convert.ToInt32(data["spins"]); }
            protected set
            {
                data["spins"] = value;
                Analytic.SetUserProperties("spins", spins);
                Save();
            }
        }
        public void UpdateSpins(int change, bool syncServer)
        {
            if (change == 0) return;

            spins += change;
            Log.Info("Player" + (change > 0 ? " earn " + change : " spend " + -change) + " sausage");

            if (syncServer && isId) SyncServer();
        }
        public DateTime nextFreeSpinDate
        {
            get
            {
                var timestamp = Convert.ToInt64(data["nextFreeSpinDate"]);

                if (timestamp > 14867262485170) return new DateTime(2017, 2, 10);  // Это костыль из-за бага

                return timestamp.ToDateTime();
            }
            protected set
            {
                data["nextFreeSpinDate"] = value.ToTimestamp();
                Save();
            }
        }
        public void nextFreeSpinDateSave(DateTime newValue, bool syncServer)
        {
            nextFreeSpinDate = newValue;

            if (syncServer && isId) SyncServer();
        }
        public int TotalSpins(DateTime utcDateTimeNow) { return spins + (IsFreeSpin(utcDateTimeNow) ? 1 : 0); }
        public TimeSpan TimeToNextFreeSpin(DateTime utcDateTimeNow) { return nextFreeSpinDate - utcDateTimeNow; }
        public bool IsFreeSpin(DateTime utcDateTimeNow) { return utcDateTimeNow >= nextFreeSpinDate; }
        public void UseSpin()
        {
            if (spins > 0) UpdateSpins(-1, true);

            Missions.OnUseSausage();
            achievements.OnUseSausages();
        }
        public void UseFreeSpin(DateTime utcDateTimeNow)
        {
            DateTime nextFreeSpinTime = utcDateTimeNow + (balance.freeSpinTime - achievements.lessTimeSausage);

            nextFreeSpinDateSave(nextFreeSpinTime, true);

            Notifications.Create(Notifications.newSausage, (nextFreeSpinTime + TimeSpan.FromHours(1)).ToLocalTime());

            Missions.OnUseSausage();
            achievements.OnUseSausages();
        }


        public List<Rival> friends = new List<Rival>();

        public List<Rival> invitableFriends = new List<Rival>();

        public List<object> invitedFriends = new List<object>();
        void InvitedFriendsLoad()
        {
            if (data.ContainsKey("invitedFriends"))
                invitedFriends = Json.Deserialize((string)data["invitedFriends"]) as List<object>;
            else
                data.Add("invitedFriends", "[]");
        }
        void InvitedFriendsSave(bool syncServer)
        {
            data["invitedFriends"] = Json.Serialize(invitedFriends);
            Save();

            if (syncServer && isId) SyncServer();
        }
        public int AddInvitedFriends(List<string> facebookIDs)
        {
            int newInvited = 0;
            foreach (string facebookID in facebookIDs)
                if (!invitedFriends.Contains(facebookID))
                {
                    invitedFriends.Add(facebookID);
                    newInvited++;
                }

            if (newInvited > 0)
            {
                UpdateCoins(newInvited * balance.reward.coinsForInvite, false);

                InvitedFriendsSave(true);

                achievements.OnInviteFriends();
            }

            return newInvited;
        }

        public List<object> tournamentsWon = new List<object>();
        void TournamentsWonLoad()
        {
            if (data.ContainsKey("tournamentsWon"))
                tournamentsWon = Json.Deserialize((string)data["tournamentsWon"]) as List<object>;
            else
                data.Add("tournamentsWon", "[]");
        }
        void TournamentsWonSave(bool syncServer)
        {
            data["tournamentsWon"] = Json.Serialize(tournamentsWon);
            Save();

            if (syncServer && isId) SyncServer();
        }
        public void AddWonTournament(int place, int members)
        {
            if (members - place > 0)
            {
                tournamentsWon.Add(members - place);
                TournamentsWonSave(true);

                achievements.OnWinFriends();
            }
        }

        public int allWonFriends
        {
            get
            {
                int result = 0;
                foreach (object o in tournamentsWon) result += Convert.ToInt32(o);
                return result;
            }
        }
        public int maxWonFriends
        {
            get
            {
                int result = 0;
                foreach (object o in tournamentsWon) if (result < Convert.ToInt32(o)) result = Convert.ToInt32(o);
                return result;
            }
        }


        Dictionary<string, object> stats = new Dictionary<string, object>();
        void StatsLoad()
        {
            if (data.ContainsKey("stats"))
                stats = Json.Deserialize((string)data["stats"]) as Dictionary<string, object>;
            else
                data.Add("stats", "{}");
        }
        void StatsSave(bool syncServer)
        {
            data["stats"] = Json.Serialize(stats);
            Save();

            if (syncServer && isId) SyncServer();
        }

        public int gameSessions
        {
            get
            {
                if (!stats.ContainsKey("gameSessions")) stats["gameSessions"] = 0;
                return Convert.ToInt32(stats["gameSessions"]);
            }
            set { stats["gameSessions"] = value; StatsSave(true); }
        }
        public int getCats
        {
            get
            {
                if (!stats.ContainsKey("getCats")) stats["getCats"] = 0;
                return Convert.ToInt32(stats["getCats"]);
            }
            set { stats["getCats"] = value; StatsSave(true); }
        }
        public int useCats
        {
            get
            {
                if (!stats.ContainsKey("useCats")) stats["useCats"] = 0;
                return Convert.ToInt32(stats["useCats"]);
            }
            set { stats["useCats"] = value; StatsSave(true); }
        }
        public int getMultiplier
        {
            get
            {
                if (!stats.ContainsKey("getMultiplier")) stats["getMultiplier"] = 0;
                return Convert.ToInt32(stats["getMultiplier"]);
            }
            set { stats["getMultiplier"] = value; StatsSave(true); }
        }
        public int getGoldfishes
        {
            get
            {
                if (!stats.ContainsKey("getGoldfishes")) stats["getGoldfishes"] = 0;
                return Convert.ToInt32(stats["getGoldfishes"]);
            }
            set { stats["getGoldfishes"] = value; StatsSave(true); }
        }
        public int getFever
        {
            get
            {
                if (!stats.ContainsKey("getFever")) stats["getFever"] = 0;
                return Convert.ToInt32(stats["getFever"]);
            }
            set { stats["getFever"] = value; StatsSave(true); }
        }
        public int chainLength
        {
            get
            {
                if (!stats.ContainsKey("chainLength")) stats["chainLength"] = 0;
                return Convert.ToInt32(stats["chainLength"]);
            }
            set { stats["chainLength"] = value; StatsSave(true); }
        }
        public int loopLength
        {
            get
            {
                if (!stats.ContainsKey("loopLength")) stats["loopLength"] = 0;
                return Convert.ToInt32(stats["loopLength"]);
            }
            set { stats["loopLength"] = value; StatsSave(true); }
        }
        public int chainSequence
        {
            get
            {
                if (!stats.ContainsKey("chainSequence")) stats["chainSequence"] = 0;
                return Convert.ToInt32(stats["chainSequence"]);
            }
            set { stats["chainSequence"] = value; StatsSave(true); }
        }
        public int recordStat
        {
            get
            {
                if (!stats.ContainsKey("recordStat")) stats["recordStat"] = 0;
                return Convert.ToInt32(stats["recordStat"]);
            }
            set { stats["recordStat"] = value; StatsSave(true); }
        }
        public int recordWithoutMultipliers
        {
            get
            {
                if (!stats.ContainsKey("recordWithoutMultipliers")) stats["recordWithoutMultipliers"] = 0;
                return Convert.ToInt32(stats["recordWithoutMultipliers"]);
            }
            set { stats["recordWithoutMultipliers"] = value; StatsSave(true); }
        }
        public int recordWithoutLime
        {
            get
            {
                if (!stats.ContainsKey("recordWithoutLime")) stats["recordWithoutLime"] = 0;
                return Convert.ToInt32(stats["recordWithoutLime"]);
            }
            set { stats["recordWithoutLime"] = value; StatsSave(true); }
        }
        public int recordWithoutGinger
        {
            get
            {
                if (!stats.ContainsKey("recordWithoutGinger")) stats["recordWithoutGinger"] = 0;
                return Convert.ToInt32(stats["recordWithoutGinger"]);
            }
            set { stats["recordWithoutGinger"] = value; StatsSave(true); }
        }
        public int getCatsAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("getCatsAtOneGame")) stats["getCatsAtOneGame"] = 0;
                return Convert.ToInt32(stats["getCatsAtOneGame"]);
            }
            set { stats["getCatsAtOneGame"] = value; StatsSave(true); }
        }
        public int getFeverAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("getFeverAtOneGame")) stats["getFeverAtOneGame"] = 0;
                return Convert.ToInt32(stats["getFeverAtOneGame"]);
            }
            set { stats["getFeverAtOneGame"] = value; StatsSave(true); }
        }
        public int getGoldfishesAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("getGoldfishesAtOneGame")) stats["getGoldfishesAtOneGame"] = 0;
                return Convert.ToInt32(stats["getGoldfishesAtOneGame"]);
            }
            set { stats["getGoldfishesAtOneGame"] = value; StatsSave(true); }
        }
        public int useCatsAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("useCatsAtOneGame")) stats["useCatsAtOneGame"] = 0;
                return Convert.ToInt32(stats["useCatsAtOneGame"]);
            }
            set { stats["useCatsAtOneGame"] = value; StatsSave(true); }
        }
        public int newYearHats
        {
            get
            {
                if (!stats.ContainsKey("newYearHats")) stats["newYearHats"] = 0;
                return Convert.ToInt32(stats["newYearHats"]);
            }
            set { stats["newYearHats"] = value; StatsSave(true); }
        }
        public int stValentinHearts
        {
            get
            {
                if (!stats.ContainsKey("stValentinHearts")) stats["stValentinHearts"] = 0;
                return Convert.ToInt32(stats["stValentinHearts"]);
            }
            set { stats["stValentinHearts"] = value; StatsSave(true); }
        }
        public int halloweenBats
        {
            get
            {
                if (!stats.ContainsKey("halloweenBats")) stats["halloweenBats"] = 0;
                return Convert.ToInt32(stats["halloweenBats"]);
            }
            set { stats["halloweenBats"] = value; StatsSave(true); }
        }
        public int loops
        {
            get
            {
                if (!stats.ContainsKey("loops")) stats["loops"] = 0;
                return Convert.ToInt32(stats["loops"]);
            }
            set { stats["loops"] = value; StatsSave(true); }
        }
        public int loopsAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("loopsAtOneGame")) stats["loopsAtOneGame"] = 0;
                return Convert.ToInt32(stats["loopsAtOneGame"]);
            }
            set { stats["loopsAtOneGame"] = value; StatsSave(true); }
        }
        public int useFireworkAtOneGame
        {
            get
            {
                if (!stats.ContainsKey("useFireworkAtOneGame")) stats["useFireworkAtOneGame"] = 0;
                return Convert.ToInt32(stats["useFireworkAtOneGame"]);
            }
            set { stats["useFireworkAtOneGame"] = value; StatsSave(true); }
        }
        public int useFireworkBoomSmall
        {
            get
            {
                if (!stats.ContainsKey("useFireworkBoomSmall")) stats["useFireworkBoomSmall"] = 0;
                return Convert.ToInt32(stats["useFireworkBoomSmall"]);
            }
            set { stats["useFireworkBoomSmall"] = value; StatsSave(true); }
        }
        public int useFireworkBoomBig
        {
            get
            {
                if (!stats.ContainsKey("useFireworkBoomBig")) stats["useFireworkBoomBig"] = 0;
                return Convert.ToInt32(stats["useFireworkBoomBig"]);
            }
            set { stats["useFireworkBoomBig"] = value; StatsSave(true); }
        }
        public int useFireworkRocket
        {
            get
            {
                if (!stats.ContainsKey("useFireworkRocket")) stats["useFireworkRocket"] = 0;
                return Convert.ToInt32(stats["useFireworkRocket"]);
            }
            set { stats["useFireworkRocket"] = value; StatsSave(true); }
        }
        public int useFireworkColor
        {
            get
            {
                if (!stats.ContainsKey("useFireworkColor")) stats["useFireworkColor"] = 0;
                return Convert.ToInt32(stats["useFireworkColor"]);
            }
            set { stats["useFireworkColor"] = value; StatsSave(true); }
        }
        public int getCatsHistory
        {
            get
            {
                if (!stats.ContainsKey("getCatsHistory")) stats["getCatsHistory"] = 0;
                return Convert.ToInt32(stats["getCatsHistory"]);
            }
            set { stats["getCatsHistory"] = value; StatsSave(true); }
        }
        public int useSausages
        {
            get
            {
                if (!stats.ContainsKey("useSausages")) stats["useSausages"] = 0;
                return Convert.ToInt32(stats["useSausages"]);
            }
            set { stats["useSausages"] = value; StatsSave(true); }
        }
        public int useSausagesHistory
        {
            get
            {
                if (!stats.ContainsKey("useSausagesHistory")) stats["useSausagesHistory"] = 0;
                return Convert.ToInt32(stats["useSausagesHistory"]);
            }
            set { stats["useSausagesHistory"] = value; StatsSave(true); }
        }
        public int useBoosts
        {
            get
            {
                if (!stats.ContainsKey("useBoosts")) stats["useBoosts"] = 0;
                return Convert.ToInt32(stats["useBoosts"]);
            }
            set { stats["useBoosts"] = value; StatsSave(true); }
        }
        public int legendaryPlace
        {
            get
            {
                if (!stats.ContainsKey("legendaryPlace")) stats["legendaryPlace"] = 999;
                return Convert.ToInt32(stats["legendaryPlace"]);
            }
            set { stats["legendaryPlace"] = value; StatsSave(true); }
        }
        public int honored
        {
            get
            {
                if (!stats.ContainsKey("honored")) stats["honored"] = 0;
                return Convert.ToInt32(stats["honored"]);
            }
            set { stats["honored"] = value; StatsSave(true); }
        }
        public int chainLengthHistory
        {
            get
            {
                if (!stats.ContainsKey("chainLengthHistory")) stats["chainLengthHistory"] = 0;
                return Convert.ToInt32(stats["chainLengthHistory"]);
            }
            set { stats["chainLengthHistory"] = value; StatsSave(true); }
        }
        public int openCatbox
        {
            get
            {
                if (!stats.ContainsKey("openCatbox")) stats["openCatbox"] = 0;
                return Convert.ToInt32(stats["openCatbox"]);
            }
            set { stats["openCatbox"] = value; StatsSave(true); }
        }
        public int coinsMAX
        {
            get
            {
                if (!stats.ContainsKey("coinsMAX")) stats["coinsMAX"] = 0;
                return Convert.ToInt32(stats["coinsMAX"]);
            }
            set { stats["coinsMAX"] = value; StatsSave(true); }
        }
    }
}