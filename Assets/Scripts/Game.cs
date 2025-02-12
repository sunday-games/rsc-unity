using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG.RSC
{
    public class Game : Core
    {
        [Space(10)]
        public Balance balanceMain;
        public Balance balanceDebug;

        [Space(10)]
        [HideInInspector]
        public Boost[] boostList;
        public Boosts boosts;
        [Serializable]
        public class Boosts
        {
            public BoostMultiplier multiplier;
            public BoostExperience experience;
            public BoostTime time;
            public BoostFirework firework;
        }

        [Space(10)]
        public League[] leagues;
        public League leagueLegendary;
        public League GetLeague(long score)
        {
            League myLeague = leagueLegendary;
            foreach (League l in leagues)
                if (l.score > score)
                {
                    myLeague = l;
                    break;
                }

            return myLeague;
        }
        public int GetLeagueCount(long score)
        {
            int count = 0;
            foreach (League l in leagues)
                if (l.score < score) count++;
                else break;
            return count;
        }
        public int GetLeagueTimeBonus(long score) { return GetLeagueCount(score) * (balance.leagueTimeBonus + achievements.addLeagueTime); }

        [Space(10)]
        public CatType[] basicCats;
        public CatType[] superCats;

        void Awake()
        {
            Log.Info($"Game Init - Version {build.version}.{build.versionCode}");

            gameplay = this;
            config = GetComponent<Config>();
            ui = GetComponent<UI>();
            factory = GetComponent<Factory>();
            fb = GetComponent<FacebookManager>();
            iapManager = GetComponentInChildren<IAPManager>();
            notifications = GetComponent<Notifications>();
            server = GetComponentInChildren<Server>();
            analytic = GetComponentInChildren<Analytic>();
            ads = GetComponentInChildren<Ads.Manager>();
            achievements = GetComponent<Achievements>();
            sound = GetComponentInChildren<Sound>();
            music = GetComponentInChildren<Music>();
            boostList = GetComponentsInChildren<Boost>();
            advisor = GetComponentInChildren<IngameAdvisor.Advisor>();
            balance = build.balanceDebug ? balanceDebug : balanceMain;

#if UNITY_EDITOR
            platform = Platform.Editor;
#elif UNITY_WEBGL
        platform = Platform.Facebook;
#elif UNITY_TIZEN
	    platform = Platform.Tizen;
#elif UNITY_IOS
	    platform = Platform.iOS;
#elif UNITY_ANDROID
        if (build.androidStore == BuildSettings.AndroidStore.GooglePlay) platform = Platform.Android;
        else platform = Platform.Amazon;
#endif
            // if (build.debugPlatform != Platform.Unknown) platform = build.debugPlatform;

            UnityEngine.Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ObscuredPrefs.lockToDevice = ObscuredPrefs.DeviceLockLevel.None;
            //if (platform != Platform.iOS) ObscuredPrefs.lockToDevice = ObscuredPrefs.DeviceLockLevel.Soft;
            ObscuredPrefs.CryptoKey = build.s;

            if (build.maxFPS > 0) Application.targetFrameRate = build.maxFPS;

            Localization.Init();

            achievements.Init();

            ads.Init();

            analytic.Init();

            Events.Init();

            config.Setup();

            Missions.Init();

            ui.Init();

            user = gameObject.AddComponent<User>();
            user.Init();
        }

        void Start()
        {
            factory.Init();

            if (build.unlockAll) ui.ProgressMAX();

            ui.splash.SetText(Localization.Get("checkConnection"));

            LoginAtStartup(ShowUI);
        }

        void LoginAtStartup(Action callback)
        {
            server.CheckConnection(isConnection =>
            {
                if (!isConnection) { callback(); return; }

                config.Load();

                StartCoroutine(LoginCoroutine(callback));
            });
        }

        enum Status { Trying, Success, Fail }
        IEnumerator LoginCoroutine(Action callback)
        {
            Status socialStatus = Status.Trying;
            Status facebookStatus = Status.Trying;

            achievements.Login(success => { socialStatus = success ? Status.Success : Status.Fail; });
            fb.ProceedLogin(facebookData =>
            {
                user.SetFacebookData(facebookData);
                facebookStatus = facebookData != null ? Status.Success : Status.Fail;
            }, force: false);

            var startTime = Time.time;
            while (Time.time - startTime < 5f && (socialStatus == Status.Trying || facebookStatus == Status.Trying)) yield return null;

            user.SyncServer(true, callback);
        }

        public void ShowUI()
        {
            iapManager.OnLogin();

            ui.splash.gameObject.SetActive(false);

            ui.canvas[0].gameObject.SetActive(true);
            ui.header.gameObject.SetActive(true);
            ui.header.UpdateAll();

            music.Switch(music.menu);

            if (Missions.isTournament || Missions.isChampionship)
                ui.PopupShow(ui.main);
            else if (user.level > 0)
                ui.PopupShow(ui.prepare);
            else
                ui.PopupShow(ui.intro);
        }

        //    #region LOGIN
        //    Action callbackLogin = null;
        //    bool forceLogin;
        //    public void Login(Action callback, bool force = true)
        //    {
        //        callbackLogin = callback;
        //        forceLogin = force;

        //       Log.Info(Loading - Facebook Init...");
        //        fb.TryInit(OnFacebookInit);
        //    }


        //    void OnFacebookInit(bool isFacebookInit)
        //    {
        //        if (!isFacebookInit)
        //        {
        //           Log.Info(Loading - Facebook Init failed - Loading Stop");
        //            callbackLogin();
        //            return;
        //        }

        //        if (!forceLogin && string.IsNullOrEmpty(user.facebookId))
        //        {
        //           Log.Info(Loading - There no Facebook login previously - Loading Stop");
        //            callbackLogin();
        //            return;
        //        }

        //#if FACEBOOK
        //       Log.Info(Loading - There Facebook login previously - Facebook Login...");
        //        fb.TryLogin(OnFacebookLogin);
        //#endif
        //    }


        //    void OnFacebookLogin(IDictionary<string, object> facebookData)
        //    {
        //        if (facebookData == null)
        //        {
        //           Log.Info(Loading - Facebook Login failed - Loading Stop");
        //            callbackLogin();
        //            return;
        //        }

        //        user.SetFacebookUserData(facebookData);

        //       Log.Info(Loading - Facebook Login Success - Parse Login...");
        //        parse.LoginViaFacebook(OnParseLogin);
        //    }

        //    void OnParseLogin(bool isParseLogin)
        //    {
        //        if (!isParseLogin)
        //        {
        //           Log.Info(Loading - Parse Login failed - Loading Stop");
        //            callbackLogin();
        //            return;
        //        }

        //       Log.Info(Loading - Parse Login Success - Merge local and parse data...");
        //#if PARSE
        //        user.Load(callbackLogin);
        //#endif
        //    }
        //    #endregion

        #region GAME
        [HideInInspector]
        public bool isPlaying = false;
        [HideInInspector]
        public bool isPause = false;
        [HideInInspector]
        public bool isFever = false;

        ObscuredFloat _seconds;
        public float seconds
        {
            get { return (float)_seconds; }
            set
            {
                _seconds = value;
                ui.game.timeText.text = _seconds.ToString();
                ui.game.timeImage.fillAmount = _seconds / (balance.baseTime + GetLeagueTimeBonus(user.permanentRecord));
            }
        }
        float freezedSeconds = 0;
        public void FreezeSeconds(float seconds)
        {
            freezedSeconds += seconds;
        }
        public bool isTimeFreezed { get { return freezedSeconds > 0; } }

        ObscuredInt _score;
        public int score
        {
            get { return (int)_score; }
            set
            {
                _score = value;
                Missions.OnGetScore(_score);

                iTween.PunchScale(ui.game.scoreText.gameObject, UI.halfVector3, 1f);
            }
        }
        public void GetScores(Vector2 position, int countCats = 0, int countPumpkins = 0)
        {
            if (countCats <= 0 && countPumpkins <= 0) return;

            GetFever(countCats + countPumpkins);

            int add = 0;

            if (countCats > 0)
                for (int i = 0; i < countCats; i++) add += balance.baseScore * (i + 1) * (i + 1);

            if (countPumpkins > 0)
                add += countPumpkins * (int)user.GetItem(Cats.Jack).power;

            add *= (isFever ? 3 : 1) * multiplier;

            score += add;

            factory.CreateShowText(position, add);

            if (countCats > 0) Missions.OnGetCats(countCats);
        }

        [HideInInspector]
        public float multiplierProgress = 0f;
        [HideInInspector]
        public int multiplierDroped = 1;
        ObscuredInt _multiplier;
        public int multiplier
        {
            get { return (int)_multiplier; }
            set
            {
                _multiplier = value;
                Missions.OnGetMultiplier(_multiplier);

                ui.game.UpdateMultiplier();
            }
        }

        ObscuredInt _coins;
        public int coins
        {
            get { return (int)_coins; }
            set
            {
                _coins = value;
                level.coinsText.text = ((int)_coins).SpaceFormat();
            }
        }
        public void GetCoin()
        {
            int count = balance.reward.coinValue * (isFever ? 3 : 1);
            coins += count;
            Missions.OnGetGoldfishes(count);
            iTween.PunchScale(level.coinsImage.gameObject, UI.halfVector3, 1f);
            sound.Play(sound.getCoin);
        }

        [HideInInspector]
        public float feverSpeedUp;
        float _fever;
        public float fever
        {
            get { return _fever; }
            set
            {
                _fever = Mathf.Clamp(value, level.minFever, 1f);

                level.feverImage.fillAmount = _fever;
                if (!isFever)
                    level.feverImage.color =
                    new Color(level.feverImage.color.r, level.feverImage.color.g, level.feverImage.color.b, _fever + 0.2f);
            }
        }
        public void GetFever(int count)
        {
            if (!Missions.isFever || isFever) return;

            for (int i = 0; i < count; i++)
                fever += (feverSpeedUp + feverSpeedUp * 0.5f * i) * achievements.easyGetDisco;
        }
        float bonusFeverSeconds = 0;
        public void StartFever(float seconds)
        {
            bonusFeverSeconds += seconds;
        }
        public bool isTimeToFever { get { return fever >= level.maxFever || bonusFeverSeconds > 0; } }

        [HideInInspector]
        public Map level;

        public void Play()
        {
            user.blockSave = true;
            isPlaying = true;

            level = smallScreen ? ui.levelsSmall[0] : ui.levels[0];
            level.gameObject.SetActive(true);
            level.feverImage.color = Color.white;

            seconds = balance.baseTime + GetLeagueTimeBonus(user.permanentRecord);
            if (boosts.time.ON) seconds += boosts.time.power;

            score = 0;
            viewScore = 0;
            fever = level.minFever;
            feverSpeedUp = balance.fever.speedUp;
            coins = 0;
            multiplier = 1;
            freezedSeconds = 0f;
            bonusFeverSeconds = 0f;
            multiplierProgress = 0;
            multiplierDroped = 1;

            StartCoroutine("Timer");
        }

        IEnumerator Timer()
        {
            if (isTNT && !user.IsTutorialShown(Tutorial.Part.GameBasics))
            {
                ui.PopupShow(ui.text);
                yield return new WaitForSeconds(0.45f);

                ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.GameBasics1, Tutorial.Part.GameBasics2, Tutorial.Part.GameBasics3, Tutorial.Part.GameBasics4 });
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();

                factory.CreateCatsBasic(balance.catsInGame);

                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                factory.CreateCatsBasic(balance.catsInGame);

                ui.PopupShow(ui.text);
                yield return new WaitForSeconds(0.6f);
            }

            if (isTNT && !user.IsTutorialShown(Tutorial.Part.GameBasics))
            {
                ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.GameBasics5, Tutorial.Part.GameBasics6, Tutorial.Part.GameBasics });
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (!user.IsTutorialShown(Tutorial.Part.GameBasics))
            {
                ui.tutorial.Show(Tutorial.Part.GameBasics);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (!user.IsTutorialShown(Tutorial.Part.GameCombo))
            {
                ui.tutorial.Show(Tutorial.Part.GameCombo);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.BOX_UNLOCK_LEVELS[0] && user.useCats == 0 && ui.prepare.catSlots[0].catItem != null)
            {
                ui.tutorial.Show(Tutorial.Part.CatUse);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (!user.IsTutorialShown(Tutorial.Part.Fever) && Missions.isFever)
            {
                if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Fever1, Tutorial.Part.Fever });
                else ui.tutorial.Show(Tutorial.Part.Fever);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.COMBO_LOOP_LEVEL && user.loopLength == 0)
            {
                ui.tutorial.Show(Tutorial.Part.GameComboLoop);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.FIREWORK_BOOM_SMALL_LEVEL && user.useFireworkBoomSmall == 0)
            {
                ui.tutorial.Show(Tutorial.Part.FireworkBoomSmall);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.FIREWORK_ROCKET_LEVEL && user.useFireworkRocket == 0)
            {
                ui.tutorial.Show(Tutorial.Part.FireworkRocket);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.FIREWORK_BOOM_BIG_LEVEL && user.useFireworkBoomBig == 0)
            {
                ui.tutorial.Show(Tutorial.Part.FireworkBoomBig);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.FIREWORK_COLOR_LEVEL && user.useFireworkColor == 0)
            {
                ui.tutorial.Show(Tutorial.Part.FireworkColor);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (user.level == Missions.MULTIPLIER_X5_LEVEL && user.getMultiplier < 5)
            {
                ui.tutorial.Show(Tutorial.Part.MultiplierX5);
                while (ui.tutorial.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }
            else if (ui.rentCat.isTimeToRentCat && !build.premium)
            {
                ui.rentCat.Show(ui.prepare.freeCatSlot);
                while (ui.rentCat.gameObject.activeSelf) yield return new WaitForEndOfFrame();
            }

            music.Switch(music.game, 3);

            sound.PlayVoice(sound.voiceClips.ready);
            ui.text.mainText.text = Localization.Get("beforeGameReady");
            ui.text.mainText.transform.localScale = UI.halfVector3;
            iTween.ScaleTo(ui.text.mainText.gameObject, Vector3.one, 0.4f);
            yield return new WaitForSeconds(sound.voiceClips.readyTime);

            sound.PlayVoice(sound.voiceClips.set);
            ui.text.mainText.text = Localization.Get("beforeGameSet");
            ui.text.mainText.transform.localScale = UI.halfVector3;
            iTween.ScaleTo(ui.text.mainText.gameObject, Vector3.one, 0.4f);
            yield return new WaitForSeconds(sound.voiceClips.setTime);

            sound.PlayVoice(sound.voiceClips.cat);
            ui.text.mainText.text = Localization.Get("beforeGameCat");
            ui.text.mainText.transform.localScale = UI.halfVector3;
            iTween.ScaleTo(ui.text.mainText.gameObject, Vector3.one, 0.4f);
            yield return new WaitForSeconds(sound.voiceClips.catTime);

            Missions.OnGameStart();

            ui.game.InitSlots();
            ui.PopupShow(ui.game);

            StartCoroutine("FeverManager");

            StartCoroutine("ScoreManager");

            StartCoroutine("ChainHighlightManager");

            if (isRiki) StartCoroutine("RikiAnimationManager");

            if (boosts.firework.ON) factory.CreateFirework(factory.fireworkPrefabs.color, Vector3.zero);

            while (seconds > 0)
            {
                yield return new WaitForSeconds(1f);
                seconds--;

                // GC.Collect();

                // Log.Debug("LIVE = {0}, POOL = {1}, ALL = {2}", Factory.LIVE_STUFF.Count, Factory.POOL_CATS.Count, Factory.STUFF.Count);

                while (isPause) yield return new WaitForEndOfFrame();

                if (isTimeFreezed) ui.game.FreezeShow();
                while (isTimeFreezed)
                {
                    while (isPause) yield return new WaitForEndOfFrame();

                    yield return new WaitForSeconds(1f);
                    freezedSeconds--;
                    if (!isTimeFreezed) ui.game.FreezeHide();
                }

                if (seconds < 9 && !ui.game.isAlertON) ui.game.AlertON();
                else if (seconds > 10 && ui.game.isAlertON) ui.game.AlertOFF();
            }
            isPlaying = false;

            if (CatBasic.CHAIN.Count > 0) CatBasic.CHAIN[CatBasic.CHAIN.Count - 1].ChainEnd(); // Распускаем цепочку

            ui.game.AlertOFF();

            music.TurnOff(2f);

            sound.PlayVoice(sound.voiceClips.timeIsOut);

            ui.PopupShow(ui.text);
            ui.text.mainText.text = Localization.Get("timeIsUp");
            ui.text.mainText.transform.localScale = UI.halfVector3;
            iTween.ScaleTo(ui.text.mainText.gameObject, Vector3.one, 0.4f);
            yield return new WaitForSeconds(2f);

            Stuff stuffToActivate = null;
            foreach (Stuff stuff in Factory.LIVE_STUFF)
                if (stuff is Firework || stuff.GetType().IsSubclassOf(typeof(CatSuper)) || stuff is CatJoker || stuff is CatJokerRiki)
                {
                    stuffToActivate = stuff;
                    break;
                }
            if (stuffToActivate != null)
            {
                ui.text.mainText.gameObject.SetActive(false);
                ui.game.multiplierText.transform.SetParent(ui.text.transform, true);
                ui.game.scoreText.transform.SetParent(ui.text.transform, true);

                while (true)
                {
                    Factory.LIVE_STUFF.Remove(stuffToActivate);
                    stuffToActivate.Activate(Vector2.zero);

                    stuffToActivate = null;
                    foreach (Stuff stuff in Factory.LIVE_STUFF)
                        if (stuff is Firework || stuff.GetType().IsSubclassOf(typeof(CatSuper)) || stuff is CatJoker || stuff is CatJokerRiki)
                        {
                            stuffToActivate = stuff;
                            break;
                        }

                    if (stuffToActivate != null) yield return new WaitForSeconds(0.4f);
                    else break;
                }

                yield return new WaitForSeconds(2f);

                ui.text.mainText.gameObject.SetActive(true);
                ui.game.multiplierText.transform.SetParent(ui.game.add.transform, true);
                ui.game.scoreText.transform.SetParent(ui.game.add.transform, true);
            }

            ui.text.mainText.text = string.Empty;

            ClearGame();

            oldPermanentRecord = user.permanentRecord;
            oldCoins = user.coins;

            if (user.level > 0) user.UpdateRecord(score);
            user.UpdateCoins(coins, true);

            achievements.OnGameEnd();
            Missions.OnGameEnd();

            Analytic.EventPropertiesImportant(AnalyticsManager.Names.Game, new Dictionary<string, object> {
            { "Score", Analytic.Round(score).SpaceFormat() },
            { "Goldfishes", Analytic.Round(coins).SpaceFormat() },
            { "Multiplier " + Missions.maxMultiplier, multiplier.ToString() },
        });

            music.Switch(music.result);

            if (ui.rentCat.catItem != null)
                ui.rentCatBuy.Show(OpenNext);
            else
                OpenNext();
        }

        void OpenNext()
        {
            if ((ui.game.catSlots[0].catItem != null && ui.game.catSlots[0].catItem.expGame > 0) ||
                (ui.game.catSlots[1].catItem != null && ui.game.catSlots[1].catItem.expGame > 0) ||
                (ui.game.catSlots[2].catItem != null && ui.game.catSlots[2].catItem.expGame > 0) ||
                (ui.game.catSlots[3].catItem != null && ui.game.catSlots[3].catItem.expGame > 0))
                ui.PopupShow(ui.promoteCats);
            else if (score > oldPermanentRecord && user.level > 0)
                ui.PopupShow(ui.highscore);
            else
                ui.PopupShow(ui.result);
        }

        [HideInInspector]
        public long oldPermanentRecord;
        [HideInInspector]
        public int oldCoins;

        IEnumerator FeverManager()
        {
            while (isPlaying)
            {
                if (isTimeToFever)
                {
                    isFever = true;
                    ui.game.FeverON();
                    sound.PlayVoice(sound.voiceClips.disco);
                    Missions.OnGetFever();
                    feverSpeedUp *= balance.fever.speedUpReducer;

                    float feverTime = 0;

                    if (bonusFeverSeconds > 0)
                    {
                        feverTime += bonusFeverSeconds;
                        bonusFeverSeconds = 0;
                    }

                    if (fever >= level.maxFever)
                        feverTime += balance.fever.time + achievements.moreDiscoTime;
                    fever = level.maxFever;

                    float startTime = Time.time;

                    while (isPlaying && startTime + feverTime > Time.time)
                    {
                        if (isPause)
                        {
                            float startWaitTime = Time.time;
                            while (isPause)
                                yield return new WaitForEndOfFrame();
                            startTime += Time.time - startWaitTime;
                        }

                        if (isTimeFreezed)
                        {
                            float startWaitTime = Time.time;
                            while (isTimeFreezed)
                                yield return new WaitForEndOfFrame();
                            startTime += Time.time - startWaitTime;
                        }

                        if (bonusFeverSeconds > 0)
                        {
                            feverTime += bonusFeverSeconds;
                            bonusFeverSeconds = 0;
                        }

                        fever = (level.maxFever - level.minFever) * (feverTime - Time.time + startTime) / feverTime + level.minFever;

                        yield return new WaitForEndOfFrame();
                    }
                    isFever = false;
                    ui.game.FeverOFF();
                }
                else
                {
                    while (isPause)
                        yield return new WaitForEndOfFrame();

                    while (isTimeFreezed)
                    {
                        if (isTimeToFever) break;

                        yield return new WaitForEndOfFrame();
                    }
                    yield return new WaitForEndOfFrame();

                    fever -= balance.fever.speedDown * Time.deltaTime;
                }
            }
        }

        int viewScore;
        IEnumerator ScoreManager()
        {
            while (ui.current == ui.game || ui.current == ui.pause || ui.current == ui.text)
            {
                if (score > viewScore)
                {
                    int c = score - viewScore;

                    if (c > 100000000) viewScore += 50000000;
                    else if (c > 10000000) viewScore += 5000000;
                    else if (c > 1000000) viewScore += 500000;
                    else if (c > 100000) viewScore += 50000;
                    else if (c > 10000) viewScore += 5000;
                    else if (c > 1000) viewScore += 500;
                    else if (c > 100) viewScore += 50;
                    else if (c > 10) viewScore += 10;
                    else ++viewScore;
                }

                ui.game.scoreText.text = viewScore.SpaceFormat();

                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator ChainHighlightManager()
        {
            while (isPlaying)
            {
                if (CatBasic.CHAIN.Count > 0) CatBasic.HighlightChain();

                yield return new WaitForSeconds(0.4f);
            }
        }

        IEnumerator RikiAnimationManager()
        {
            while (isPlaying)
            {
                yield return new WaitForSeconds(0.8f);

                Stuff stuff;

                do stuff = Factory.LIVE_STUFF[UnityEngine.Random.Range(0, Factory.LIVE_STUFF.Count)];
                while (!(stuff is CatBasicRiki) || !(stuff as CatBasicRiki).isCanDrawAttention);

                (stuff as CatBasicRiki).DrawAttention();
            }
        }

        public void RestartGame()
        {
            isPlaying = false;

            ui.rentCat.RemoveCat();

            if (isFever)
            {
                isFever = false;
                ui.game.FeverOFF();
            }

            music.Switch(music.menu);

            ClearGame();

            ResetGame();

            ui.header.UpdateCoins(force: true);

            ui.PopupShow(ui.prepare);
        }

        public void ClearGame()
        {
            StopCoroutine("Timer");
            StopCoroutine("FeverManager");
            StopCoroutine("RikiAnimationManager");

            Missions.AtOneGameMissionsClear();

            Factory.ClearStuff();

            Pumpkin.pumpkins.Clear();

            level.gameObject.SetActive(false);
        }

        public void ResetGame()
        {
            Events.newYear.isItemTryDrop = false;
            Events.stValentin.isItemTryDrop = false;
            Events.halloween.isItemTryDrop = false;

            CatBasic.CHAIN.Clear();

            foreach (var boost in boostList) boost.ON = false;
        }
        #endregion
    }
}