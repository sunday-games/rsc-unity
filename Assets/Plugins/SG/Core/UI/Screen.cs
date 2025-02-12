using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public static class ScreenParams // TODO enum
    {
        public const string AUTHOR = "author";
        public const string INSTIGATOR = "instigator";
        public const string TARGET = "target";
        public const string TOKEN = "token";
        public const string BLOCKCHAIN = "blockchain";
        public const string SHIP = "ship";
        public const string ITEM = "item";
        public const string NFT = "nft";
        public const string ORDER = "order";
        public const string TYPE = "type";
        public const string MESSAGE = "message";
        public const string CONTRACT = "contract";
        public const string TEMPLATE = "template";
        public const string AMOUNT = "amount";
        public const string DIALOGUE = "dialogue";
        public const string CREDITS_LIMIT = "limit ";
        public const string MODE = "mode";
        public const string CALLBACK = "callback";
        public const string IS_SIMPLE_MODE = "issm";
        public const string SLOT_PARAMS = "slot";
        public const string TO = "to";
        public const string FROM = "from";
        public const string USE = "use";
        public const string TRANSFER = "transfer";
        public const string TRADE = "trade";
    }

    public abstract class Screen : MonoBehaviour
    {
        public Settings settings;

        [Serializable]
        public class Settings
        {
            public bool modal = true;

            [Space(5)]
            public bool header = false;

#if SG_UI_BLUR
            [Space(5)]
            public bool blur = false;
#endif

#if SG_UI_OVERLAY
            [Space(5)]
            public OverlayLevel overlay = OverlayLevel.None;
            public bool isOverlay => overlay != OverlayLevel.None;
#endif
            [Space(5)]
            public bool blinds = true;

            [Space(5)]
            public Text TitleText;
            public Button CloseButton;
            public Button MainButton;

            [Space(5)]
            public Animation anim = Animation.None;

            public float animationTime = 0.4f;

            [Space(5)]
            public CanvasGroup canvasGroup;

            [Space(5)]
            public Selectable firstSelected;

            [Space(5)]
            public Screen fixedPrevious;
        }

        [NonSerialized] public Screen previous = null;

        public Action onOpen;

        protected DictSO Params;

        protected RectTransform rt;

        public bool IsCurrent => UI.Instance.current == this;

        protected bool TryGetOpenParam<T>(out T obj) where T : class
        {
            if (Params != null)
            {
                obj = Params.Values.FirstOrDefault() as T;
                return true;
            }

            obj = null;
            return false;
        }

        public virtual void Setup()
        {
            rt = transform as RectTransform;

            if (settings.CloseButton != null)
                settings.CloseButton.SetCallback(OnEscapeKey);
        }

        [ContextMenu("Show")]
        public void Show(DictSO openParams, Screen previous = null) => 
            UI.Instance.ScreenShow(this, openParams, previous);
        public void Show() => 
            Show(openParams: null);
        public void Show(string openParamName, object openParamValue) => 
            Show(new DictSO { [openParamName] = openParamValue });
        public void Show(string openParamName, object openParamValue, Screen previous) => 
            Show(new DictSO { [openParamName] = openParamValue }, previous);
        public void Show(Action callback) => 
            Show("callback", callback);

        public void Hide(DictSO openParams) => 
            UI.Instance.ScreenHide(openParams);
        public void Hide() => 
            Hide(openParams: null);
        public void Hide(string openParamName, object openParamValue) =>
            Hide(new DictSO { [openParamName] = openParamValue });

        /// <summary>
        /// SetActive(true) >> Open >> AnimationShow
        /// </summary>
        public virtual void Open(DictSO openParams)
        {
            this.Params = openParams;
            Open();
        }

        /// <summary>
        /// Called when this screen has become the Current:
        /// A. It was closed (inactive) and now became open (active)
        /// B. It was in the background and now returned to the foreground
        /// </summary>
        public virtual void Open() { }

        /// <summary>
        /// Called after Open event has been called and the accompanying animation has completed
        /// </summary>
        public virtual void AfterOpen() { }

        /// <summary>
        /// Called when this screen has moved into the background but has not been closed (still active)
        /// </summary>
        public virtual void Background() { }

        /// <summary>
        /// Called when this screen begins to close (before the screen closing animation has begun)
        /// </summary>
        public virtual void PreClose() { }

        /// <summary>
        /// Called when the closing animation has completed, just before this screen becomes inactive
        /// </summary>
        public virtual void Close() { }

        public virtual void OnEscapeKey() => Hide();

        public virtual void OnKeyEnter()
        {
            if (settings.MainButton != null && settings.MainButton.gameObject.activeSelf && settings.MainButton.interactable)
                settings.MainButton.onClick.Invoke();
        }

        public virtual void OnKeyTab() { }

        public void AnimOpen(DictSO openParams, Screen previous = null)
        {
            if (gameObject.activeSelf)
            {
                Open(openParams);
                return;
            }

            if(settings.blinds)
                UI.Instance.Blinds.Show();

            gameObject.SetActive(true);

            if (settings.firstSelected && Utils.IsPlatform(Platform.tvOS))
                settings.firstSelected.Select();

            if (settings.fixedPrevious)
                this.previous = settings.fixedPrevious;

            Open(openParams);
            onOpen?.Invoke();

            // TODO
            if (previous && previous.settings.anim != Animation.None)
                AnimationShowBack(previous.settings.anim).Start();
            else if (settings.anim != Animation.None)
                AnimationShow(settings.anim).Start();

            AfterOpen();

            //Log.Debug("UI - Open - " + name + (openParams == null ? "" : ". Params: " + openParams.ToText()));

            // SG.AnalyticsPlugin.AnalyticsManager.View(name);
        }

        public void AnimClose(Screen next = null)
        {
            if (next && !next.settings.modal && settings.modal)
            {
                Background();
                return;
            }

            PreClose();

            // TODO
            if (next && next.settings.anim != Animation.None)
                AnimationHide(next.settings.anim).Start();
            else if (settings.anim != Animation.None)
                AnimationHideBack(settings.anim).Start();

            Close();

            if (settings.blinds)
                UI.Instance.Blinds.Hide();

            gameObject.SetActive(false);

            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        #region ANIMATIONS

        public enum Animation { None, MoveUpDown, MoveDownUp, MoveLeftRight, MoveRightLeft, ScaleInOut, Fade }

        public static void AnimationsSetup(float height, float aspectRatio)
        {
            positionUp = new Vector2(0f, height);
            positionBottom = new Vector2(0f, -height);
            positionRight = new Vector2(height * aspectRatio, 0f);
            positionLeft = new Vector2(-height * aspectRatio, 0f);
        }

        static Vector3 scaleMin = new Vector3(0.1f, 0.1f, 1f);
        static Vector3 scaleMax = new Vector3(1.5f, 1.5f, 1f);
        static Vector2 positionUp;
        static Vector2 positionBottom;
        static Vector2 positionRight;
        static Vector2 positionLeft;

        IEnumerator AnimationShow(Animation anim)
        {
            if (anim == Animation.ScaleInOut)
            {
                rt.localScale = scaleMax;
                yield return rt.DOScale(Vector3.one, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveUpDown)
            {
                rt.anchoredPosition = positionUp;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveDownUp)
            {
                rt.anchoredPosition = positionBottom;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rt.anchoredPosition = positionRight;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rt.anchoredPosition = positionLeft;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();
            }
            else if (anim == Animation.Fade)
            {
                settings.canvasGroup.alpha = 0f;
                yield return settings.canvasGroup.DOFade(1f, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
        }

        IEnumerator AnimationShowBack(Animation currentAnim)
        {
            var anim = settings.anim;

            if (currentAnim == Animation.ScaleInOut)
                anim = Animation.ScaleInOut;
            //else if (currentAnim == Animation.MoveUpDown && settings.anim != Animation.MoveUpDown) anim = Animation.ScaleInOut;
            //else if (currentAnim == Animation.MoveDownUp && settings.anim != Animation.MoveDownUp) anim = Animation.ScaleInOut;
            else if (currentAnim == Animation.MoveLeftRight)
                anim = Animation.MoveLeftRight;
            else if (currentAnim == Animation.MoveRightLeft)
                anim = Animation.MoveRightLeft;
            //else if (currentAnim == Animation.Fade) anim = Animation.None;

            if (anim == Animation.ScaleInOut)
            {
                rt.localScale = scaleMin;
                yield return rt.DOScale(Vector3.one, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveUpDown)
            {
                rt.anchoredPosition = positionBottom;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveDownUp)
            {
                rt.anchoredPosition = positionUp;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rt.anchoredPosition = positionLeft;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rt.anchoredPosition = positionRight;
                yield return rt.DOAnchorPos(Vector2.zero, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();
            }
            else if (anim == Animation.Fade)
            {
                settings.canvasGroup.alpha = 0f;
                yield return settings.canvasGroup.DOFade(1f, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
            }
        }

        IEnumerator AnimationHide(Animation nextAnim)
        {
            var anim = settings.anim;

            if (nextAnim == Animation.ScaleInOut)
                anim = Animation.ScaleInOut;
            //else if (nextAnim == Animation.MoveUpDown && settings.anim != Animation.MoveUpDown) anim = Animation.ScaleInOut;
            //else if (nextAnim == Animation.MoveDownUp && settings.anim != Animation.MoveDownUp) anim = Animation.ScaleInOut;
            else if (nextAnim == Animation.MoveLeftRight)
                anim = Animation.MoveLeftRight;
            else if (nextAnim == Animation.MoveRightLeft)
                anim = Animation.MoveRightLeft;
            //else if (nextAnim == Animation.Fade) anim = Animation.None;

            if (anim == Animation.ScaleInOut)
                yield return rt.DOScale(scaleMin, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveUpDown)
                yield return rt.DOAnchorPos(positionBottom, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveDownUp)
                yield return rt.DOAnchorPos(positionUp, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveLeftRight)
                yield return rt.DOAnchorPos(positionLeft, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();

            else if (anim == Animation.MoveRightLeft)
                yield return rt.DOAnchorPos(positionRight, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();

            else if (anim == Animation.Fade)
                yield return settings.canvasGroup.DOFade(0f, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
        }

        IEnumerator AnimationHideBack(Animation anim)
        {
            if (anim == Animation.ScaleInOut)
                yield return rt.DOScale(scaleMax, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveUpDown)
                yield return rt.DOAnchorPos(positionUp, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveDownUp)
                yield return rt.DOAnchorPos(positionBottom, settings.animationTime).SetEase(Ease.InQuad).WaitForCompletion();

            else if (anim == Animation.MoveLeftRight)
                yield return rt.DOAnchorPos(positionRight, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();

            else if (anim == Animation.MoveRightLeft)
                yield return rt.DOAnchorPos(positionLeft, settings.animationTime).SetEase(Ease.InOutQuad).WaitForCompletion();

            else if (anim == Animation.Fade)
                yield return settings.canvasGroup.DOFade(0f, settings.animationTime).SetEase(Ease.OutQuad).WaitForCompletion();
        }

        #endregion
    }
}