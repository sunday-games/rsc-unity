using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using UnityEngine.UI;

namespace SG.UI
{
    public class UI : MonoBehaviour
    {
        public static UI Instance;
        [HideInInspector]
        public Screen current;

        public static GameObject selected => UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        public static bool isTablet;
        // public bool isTablet => build.scaleUI == ScaleUI.Auto ? Utils.screenSize > 6f : build.scaleUI == ScaleUI.Tablet;
        // public bool isPhone => build.scaleUI == ScaleUI.Auto ? Utils.screenSize <= 6f : build.scaleUI == ScaleUI.Phone;

        //public enum InputMode { Mouse, Touch }
        //public InputMode inputMode;
        //public bool isTouchInput => inputMode == InputMode.Touch;

        // public enum ScaleUI { Auto, Phone, Tablet }

        [Space(10)]
        public Canvas[] canvases;
        //public Camera[] cameras;
        public GraphicRaycaster[] graphicRaycasters;

        [Space(10)]
        //public ParentGate parentGate;
        //public Splash splash;
#if SG_UI_BLUR
        public Blur blur;
#endif
#if SG_UI_OVERLAY
        public Overlay overlay;
#endif

        public Transform ScreensParent;
        public Transform ScreensNonModalParent;
        public NotificationManager Notifications;
        public PopupScreen CommonPopup;
        public Loading Loading;
        public Blinds Blinds;

        #region SCREENS

        public TScreen GetInstance<TScreen>(TScreen screen) where TScreen : Screen =>
            _screens.TryGetValue(screen.name, out Screen s) ? (TScreen) s : screen;

        private Dictionary<string, Screen> _screens = new Dictionary<string, Screen>();
        public void ScreenShow(Screen next, DictSO openParams = null, Screen previous = null)
        {
            if (isBlock)
            {
                //callback?.Invoke();
                return;
            }

            if (next.transform.parent == null) // if next is prefab
            {
                if (_screens.TryGetValue(next.name, out Screen screen))
                {
                    next = screen;
                }
                else
                {
                    var parent = (next.settings.modal ? ScreensParent : ScreensNonModalParent) ?? canvases[canvases.Length - 1].transform;
                    _screens[next.name] = next = next.Copy(parent);
                    next.Setup();
                    next.gameObject.SetActive(false);
                }
            }

            if (current == next)
            {
                current.Open(openParams);
                //callback?.Invoke();
                return;
            }

            Log.UI.Info($"UI Screen: {(current != null ? current.name : "_")} >> {next.name}");

            SG.Analytics.AnalyticsManager.View(next.name);

            // Block();

            if (current)
            {
                next.previous = previous ?? current;
#if SG_UI_BLUR
                if (current.settings.blur && (!next || !next.settings.blur)) blur.OFF();
#endif
#if SG_UI_OVERLAY
                if (current.settings.isOverlay && (!next || !next.settings.isOverlay)) overlay.OFF();
#endif
                current.AnimClose(next);
            }

#if SG_UI_BLUR
            if ((!current || !current.settings.blur) && next.settings.blur) blur.ON();
#endif
#if SG_UI_OVERLAY
            if ((!current || !current.settings.isOverlay) && next.settings.isOverlay) overlay.ON(next.settings.overlay);
#endif
            current = next;

            current.AnimOpen(openParams);

            // Unblock();
            //callback?.Invoke();
        }

        public void ScreenHide(DictSO openParams = null, Action callback = null)
        {
            if (isBlock)
            {
                callback?.Invoke();
                return;
            }

            if (!current)
            {
                callback?.Invoke();
                return;
            }

            // Block();

            var next = current.previous;

            Log.UI.Info($"UI Screen: {(next != null ? next.name : "_")} << {current.name}");
#if SG_UI_BLUR
            if (current.settings.blur && (!next || !next.settings.blur)) blur.OFF();
#endif
#if SG_UI_OVERLAY
            if (current.settings.isOverlay && (!next || !next.settings.isOverlay)) overlay.OFF();
#endif
            current.AnimClose();

            var previous = current;

            current = next;

            if (current)
            {
#if SG_UI_BLUR
                if (!previous.settings.blur && current.settings.blur) blur.ON();
#endif
#if SG_UI_OVERLAY
                if (!previous.settings.isOverlay && current.settings.isOverlay) overlay.ON(current.settings.overlay);
#endif
                current.AnimOpen(openParams, previous);
            }
            // else main.Start();

            // Unblock();

            callback?.Invoke();
        }
        #endregion

        public virtual void Init()
        {
            isTablet = Helpers.aspectRatio < 1.34f;

            // inputMode = platform == Platform.iOS || platform == Platform.Android ? InputMode.Touch : InputMode.Mouse;

            Screen.AnimationsSetup(height: 1024f, aspectRatio: Helpers.aspectRatio);

            foreach (var canvas in canvases)
                foreach (var screen in canvas.GetComponentsInChildren<Screen>(includeInactive: true))
                    if (screen.transform.parent != null)
                        screen.Setup();

            Notifications?.Setup();
        }

        protected virtual void CheckKeyboardInput()
        {
#if UNITY_EDITOR
            if (IsKeyHold.command)
            {
                if (IsKeyDown.l)
                    Localization.SetNextLanguage();
                else if (IsKeyDown.p)
                    UnityEditor.EditorApplication.isPaused = true;
                else if (IsKeyDown.s)
                    Helpers.TakeScreenshot();
                return;
            }
#endif

            if (current && !isBlock && !Loading.IsShown)
            {
                if (IsKeyDown.esc)
                    current.OnEscapeKey();
                else if (IsKeyDown.enter)
                    current.OnKeyEnter();
                else if (IsKeyDown.tab)
                    current.OnKeyTab();
                return;
            }
        }

        public void Block()
        {
            foreach (var raycaster in graphicRaycasters)
                raycaster.enabled = false;
        }
        public void Unblock()
        {
            foreach (var raycaster in graphicRaycasters)
                raycaster.enabled = true;
        }
        public bool isBlock => !graphicRaycasters[0].enabled;

        public static int uiLayer;
        protected virtual void SetupLayer()
        {
            const string uiLayerName = "UI";
            uiLayer = LayerMask.NameToLayer(uiLayerName);
        }

        public void LoadScene(string sceneName)
        {
            Loading.Show();

            var scene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);


            // TODO
            //UpdateLoading();

            //Configurator.Instance.StartCoroutine(LoadScene());

            //IEnumerator LoadScene()
            //{
            //    while (!scene.isDone)
            //    {
            //        yield return null;
            //        UpdateLoading();
            //    }

            //    Loading.Hide();
            //    DontDestroyOnLoad(Loading);
            //}

            //void UpdateLoading()
            //{
            //    if (Loading != null)
            //        Loading.Show(sceneName + " scene loading " + (int) (scene.progress * 100) + "%");
            //}
        }

        public void Quit()
        {
            if (!Utils.IsPlatform(Platform.WebGL))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            };
        }
    }
}