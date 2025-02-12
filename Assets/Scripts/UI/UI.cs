using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SG.RSC
{
    // 1 поинт = 51,2 пикселя
    public class UI : Core
    {
        public bool smallLevelForSmallScreens = true;
        public Map[] levels;
        public Map[] levelsSmall;

        [Space(10)]
        public Canvas[] canvas;

        [Space(10)]
        public Transform showTextParent;

        [Space(10)]
        public Splash splash;
        public Header header;
        public Blinds blinds;
        public GameObject blocker;
        public GameObject loading;
        public Tutorial tutorial;
        public MedalShow medalShow;
        public RentCat rentCat;
        public RentCatBuy rentCatBuy;
        public ParentGate parentGate;

        #region POPUPS
        [Space(10)]
        public PopupMain main;
        public PopupGame game;
        public PopupPromoteCats promoteCats;
        public PopupResult result;
        public PopupPrepare prepare;
        public PopupPause pause;
        public PopupOptions options;
        public PopupProfile profile;
        public PopupCollection collection;
        public PopupGetBonusbox getBonusbox;
        public PopupGetCatbox getCatbox;
        public PopupGetGoldfishes getGoldfishes;
        public PopupGetSpins getSpins;
        public PopupLuckyWheel luckyWheel;
        public PopupPrize prize;
        public PopupShop shop;
        public PopupLevelUp levelUp;
        public PopupText text;
        public PopupHighscore highscore;
        public PopupTournamentResults tournamentResults;
        public PopupLevelUpCat levelUpCat;
        public PopupProgress progress;
        public PopupLeagueUp leagueUp;
        public PopupNotEnoughGoldfishes notEnoughGoldfishes;
        public PopupRateApp rateApp;
        public PopupIntro intro;
        public PopupBoosts boosts;
        public PopupPromocode promocode;
        public new PopupAdvisor advisor;
        public PopupEventNewYearHats newYearHats;
        public PopupEventStValentin stValentin;
        public PopupEventHalloween halloween;

        [HideInInspector]
        public Popup current;

        public void PopupShowModal(Popup next)
        {
            PopupShow(next);

            if (current != null)
                next.previous = null;
        }
        public void PopupShow(Popup next)
        {
            if (current == next)
            {
                current.Init();
                current.AfterInit();
                return;
            }

            if (next == null || isBlock) return;

            Block();

            if (current != null)
            {
                next.previous = current;

                current.Hide(next.anim);

                if (current.blinds && !next.blinds)
                    blinds.Hide();

                if (current.header && !next.header)
                    header.Hide();
            }

            StartCoroutine(PopupShowFinalize(next));
        }
        IEnumerator PopupShowFinalize(Popup next)
        {
            if (current != null && (current.anim == Popup.Animation.ScaleInOut || (next.anim == Popup.Animation.ScaleInOut && current.anim != Popup.Animation.None)))
                yield return new WaitForSeconds(next.animationTime);

            if ((current == null || !current.blinds) && next.blinds)
                blinds.Show(next.blindsTransparent);

            if ((current == null || !current.header) && next.header)
                header.Show();

            current = next;

            next.Show(PopupAfterShow);

            Log.Debug("UI - Open - " + next.name);
        }
        void PopupAfterShow()
        {
            Unblock();

            GC.Collect();
        }

        public void PopupClose()
        {
            if (current == null || isBlock)
                return;

            Block();

            Popup next = current.previous;
            current.previous = null;

            if (current.blinds && (next == null || !next.blinds)) blinds.Hide();

            if (current.header && (next == null || !next.header)) header.Hide();

            current.HideBack();

            StartCoroutine(PopupCloseFinalize(next));
        }
        IEnumerator PopupCloseFinalize(Popup next)
        {
            if (current.anim == Popup.Animation.ScaleInOut)
                yield return new WaitForSeconds(current.animationTime);

            if (next != null)
            {
                if (!current.blinds && next.blinds)
                    blinds.Show(next.blindsTransparent);

                if (!current.header && next.header)
                    header.Show();

                next.ShowBack(current.anim, PopupAfterShow);

                current = next;

                Log.Debug("UI - Open - " + current.name);
            }
            else
            {
                current = null;
                PopupAfterShow();
            }
        }
        #endregion

        [Space(10)]
        public Vector2 canvasScale = new Vector2(576, 1024);
        public Vector2 canvasScaleMAX = new Vector2(576, 1200);

        public void Init()
        {
            canvas[0].gameObject.SetActive(false);
            header.rectTransform.anchoredPosition = ui.header.hidePosition;
            header.gameObject.SetActive(false);

            splash.gameObject.SetActive(true);
            splash.SetText(Localization.Get("loadLocal"));

            SetCanvasScale();
        }

        [ContextMenu("SetCanvasScale")]
        public void SetCanvasScale()
        {
            var scale = canvasScale;
            if (SG_Utils.aspectRatio < SG_Utils.aspectRatio_9x16)
            {
                var k = (SG_Utils.aspectRatio_9x16 - SG_Utils.aspectRatio) / (SG_Utils.aspectRatio_9x16 - SG_Utils.aspectRatio_iPhoneX);
                scale = new Vector2(scale.x, scale.y + k * (canvasScaleMAX.y - scale.y));
            }

            foreach (var c in canvas)
                c.GetComponent<CanvasScaler>().referenceResolution = scale;
        }

        void Update()
        {
            // SetCanvasScale();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (tutorial.gameObject.activeSelf)
                    tutorial.currentBubble.Hide();
                else if (current != null && !isBlock)
                    current.OnEscapeKey();
            }
            else if (current == promocode)
            {
                if (Input.GetKeyDown(KeyCode.Return)) promocode.EnterCode();
            }

            if (build.cheats || platform == Platform.Editor)
            {
                if (Input.GetKeyDown(KeyCode.M)) ui.options.MusicToggle();
                // else if (Input.GetKeyDown(KeyCode.L)) ui.options.LanguageToggle();
                else if (Input.GetKeyDown(KeyCode.S)) ui.options.SoundToggle();
#if UNITY_EDITOR
                else if (Input.GetKeyDown(KeyCode.P)) UnityEditor.EditorApplication.isPaused = false;
#endif
                else if (Input.GetKeyDown(KeyCode.S)) SG_Utils.TakeScreenshot();
                else if (current == prepare)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow)) LevelUpNow();
                    else if (Input.GetKeyDown(KeyCode.DownArrow)) LevelDownNow();
                }
                else if (current == collection)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && collection.bigCatSlot.catItem != null && !collection.bigCatSlot.catItem.isMaxLevel)
                    {
                        user.GetCat(collection.bigCatSlot.catItem.type);

                        collection.bigLevelText.text = collection.bigCatSlot.catItem.level.ToString();
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow) && collection.bigCatSlot.catItem != null && collection.bigCatSlot.catItem.level > 1)
                    {
                        collection.bigCatSlot.catItem.level--;
                        collection.bigLevelText.text = collection.bigCatSlot.catItem.level.ToString();
                        user.CollectionSave(true);
                    }
                }
                else if (current == options)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow)) ProgressMAX();
                    else if (Input.GetKeyDown(KeyCode.DownArrow)) ProgressReset();

                }
                else if (current == game)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8)) gameplay.seconds += 90;
                    else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2)) TimeOut();
                    else if (Input.GetKeyDown(KeyCode.O)) gameplay.score += 100000;
                }
            }

            if (!string.IsNullOrEmpty(build.updateUrl) && build.isCriticalUpdateNeeded) SG_Utils.OpenLink(build.updateUrl, "Update");
        }

        public void ProgressMAX()
        {
            Missions.UnlockAll();

            header.UpdateAll();

            foreach (var cat in gameplay.superCats) user.GetCat(cat);

            // if (user.level < 1) user.level = 1;
        }

        public void ProgressReset()
        {
            PlayerPrefs.DeleteAll();
            user = new User();

            Missions.LockAll();

            header.UpdateAll();
        }

        public void LevelUpNow()
        {
            if (user.level >= Missions.MAX_LEVEL) return;

            user.LevelUp();
            PopupShow(levelUp);
        }

        public void LevelDownNow()
        {
            if (user.level < 1) return;

            Level l = Missions.LEVELS[user.level];

            foreach (Mission mission in l.missions)
                if (mission.clear != null) mission.clear();

            user.UpdateLevel(user.level - 1, true);
        }

        public void TimeOut()
        {
            if (gameplay.seconds > 3) gameplay.seconds = 3;
        }

        public void Block() { blocker.SetActive(true); }
        public void Unblock() { blocker.SetActive(false); }
        public bool isBlock { get { return blocker.activeSelf; } }

        public void LoadingShow() { loading.SetActive(true); }
        public void LoadingHide() { loading.SetActive(false); }


        public void FacebookLogin()
        {
            if (build.parentGate && parentGate != null)
            {
                parentGate.Show(() =>
                {
                    ui.LoadingShow();
                    server.CheckConnection(isConnection =>
                    {
                        if (!isConnection)
                        {
                            ui.LoadingHide();
                            return;
                        }

#if FACEBOOK
                    StartCoroutine(FacebookLoginCoroutine());
#endif
                    });
                });

                return;
            }

            ui.LoadingShow();
            server.CheckConnection(isConnection =>
            {
                if (!isConnection)
                {
                    ui.LoadingHide();
                    return;
                }

#if FACEBOOK
            StartCoroutine(FacebookLoginCoroutine());
#endif
            });
        }

#if FACEBOOK
    enum Status { Trying, Success, Fail }
    IEnumerator FacebookLoginCoroutine()
    {
        var facebookStatus = Status.Trying;

        fb.ProceedLogin(facebookData =>
        {
            user.SetFacebookData(facebookData);
            facebookStatus = facebookData != null ? Status.Success : Status.Fail;
        });

        var startTime = Time.time;
        while (Time.time - startTime < 10f && facebookStatus == Status.Trying) yield return null;

        if (facebookStatus == Status.Success)
            Analytic.EventImportant("Facebook", "Login");

        user.SyncServer(true, FacebookLoginCallback);
    }

    void FacebookLoginCallback()
    {
        ui.LoadingHide();

        if (fb.isLogin)
        {
            Analytic.EventPropertiesImportant("Facebook", "Login", "Success");

            if (Missions.isTournament || Missions.isChampionship)
                ui.PopupShow(ui.main);
            else if (user.level > 0)
                ui.PopupShow(ui.prepare);
            else
                ui.PopupShow(ui.intro);
        }
        else
        {
            Analytic.EventProperties("Facebook", "Login", "Failed");
        }
    }


#endif
        public void FacebookInviteFriends(Vector3 point, Action callback = null)
        {
            ui.Block();
            server.CheckConnection(succeess =>
            {
                if (succeess)
                {
#if FACEBOOK
                fb.InviteFriends(Localization.Get("inviteFriendMessage"), Localization.Get("inviteFriendTitle"),
                    facebookIDs =>
                    {
                        if (facebookIDs != null && facebookIDs.Count > 0 && user.AddInvitedFriends(facebookIDs) > 0 && point != Vector3.zero)
                            ui.header.ShowCoinsIn(point, 6, canvas[3].transform, shift: 0.4f, delay: 0.5f);

                        ui.Unblock();
                        if (callback != null) callback();
                    });
#endif
                }
                else
                {
                    ui.Unblock();
                    if (callback != null) callback();
                }
            });
        }
    }
}