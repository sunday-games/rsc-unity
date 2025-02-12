using UnityEngine;
using System;
using DG.Tweening;

namespace SG.RSC
{
    // EaseType http://www.robertpenner.com/easing/easing_demo.html
    public abstract class Popup : Core
    {
        public Canvas canvas;
        public bool header = false;
        public bool blinds = false;
        public bool blindsCloseByTap = false;
        public float blindsTransparent = 0.5f;

        [HideInInspector]
        public Popup previous = null;

        public virtual void Init() { }
        public virtual void AfterInit() { }
        public virtual void PreReset() { }
        public virtual void Reset() { }

        public virtual void OnEscapeKey() { ui.PopupClose(); }

        // SetActive(true) >> Init >> AnimationShow >> AfterInit >> ... >> PreReset >> AnimationHide >> Reset >> SetActive(false)

        public void Show(Action callback = null)
        {
            if (canvas != null) canvas.gameObject.SetActive(true);
            else gameObject.SetActive(true);

            Init();

            if (anim != Animation.None) AnimationShow(callback);
            else ShowEnd(callback);
        }
        public void ShowBack(Animation currentAnim, Action callback = null)
        {
            if (canvas != null) canvas.gameObject.SetActive(true);
            else gameObject.SetActive(true);

            Init();

            if (anim == Animation.None) ShowEnd(callback);
            else if (currentAnim == Animation.ScaleInOut) AnimationShowBack(Animation.ScaleInOut, callback);
            else if (currentAnim == Animation.MoveUpDown || currentAnim == Animation.MoveDownUp) AnimationShowBack(Animation.ScaleInOut, callback);
            else if (currentAnim == Animation.MoveLeftRight) AnimationShowBack(Animation.MoveLeftRight, callback);
            else if (currentAnim == Animation.MoveRightLeft) AnimationShowBack(Animation.MoveRightLeft, callback);
            else AnimationShowBack(anim, callback);
        }
        public void ShowEnd(Action callback = null)
        {
            if (callback != null) callback();

            AfterInit();
        }

        public void Hide()
        {
            PreReset();

            if (anim != Animation.None) AnimationHide(anim);
            else HideEnd();
        }
        public void Hide(Animation nextAnim)
        {
            PreReset();

            if (anim == Animation.None) HideEnd();
            else if (nextAnim == Animation.ScaleInOut) AnimationHide(Animation.ScaleInOut);
            else if (nextAnim == Animation.MoveUpDown || nextAnim == Animation.MoveDownUp) AnimationHide(Animation.ScaleInOut);
            else if (nextAnim == Animation.MoveLeftRight) AnimationHide(Animation.MoveLeftRight);
            else if (nextAnim == Animation.MoveRightLeft) AnimationHide(Animation.MoveRightLeft);
            else AnimationHide(anim);
        }
        public void HideBack()
        {
            PreReset();

            if (anim != Animation.None) AnimationHideBack();
            else HideEnd();
        }
        public void HideEnd()
        {
            Reset();

            if (canvas != null) canvas.gameObject.SetActive(false);
            else gameObject.SetActive(false);

            (transform as RectTransform).anchoredPosition = Vector2.zero;
            transform.localScale = Vector3.one;
        }

        #region ANIMATIONS
        public enum Animation { None, MoveUpDown, ScaleInOut, MoveLeftRight, MoveDownUp, MoveRightLeft }
        public Animation anim = Animation.None;
        public float animationTime = 0.4f;

        static float width;
        static float height;
        static Vector3 scaleMin = new Vector3(0.1f, 0.1f, 1f);
        static Vector3 scaleMax = new Vector3(1.5f, 1.5f, 1f);
        static Vector2 positionUp;
        static Vector2 positionBottom;
        static Vector2 positionRight;
        static Vector2 positionLeft;

        protected RectTransform rectTransform;
        void Awake()
        {
            rectTransform = transform as RectTransform;

            height = 1024;
            width = height * SG_Utils.aspectRatio;

            positionUp = new Vector2(0, height);
            positionBottom = new Vector2(0, -height);
            positionRight = new Vector2(width, 0);
            positionLeft = new Vector2(-width, 0);
        }

        void AnimationShow(Action callback)
        {
            if (anim == Animation.ScaleInOut)
            {
                transform.localScale = scaleMax;
                transform.DOScale(Vector3.one, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveUpDown)
            {
                rectTransform.anchoredPosition = positionUp;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveDownUp)
            {
                rectTransform.anchoredPosition = positionBottom;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rectTransform.anchoredPosition = positionRight;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.InOutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rectTransform.anchoredPosition = positionLeft;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.InOutQuad).OnComplete(() => ShowEnd(callback));
            }
        }
        void AnimationShowBack(Animation anim, Action callback)
        {
            if (anim == Animation.ScaleInOut)
            {
                transform.localScale = scaleMin;
                transform.DOScale(Vector3.one, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveUpDown)
            {
                rectTransform.anchoredPosition = positionBottom;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveDownUp)
            {
                rectTransform.anchoredPosition = positionUp;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.OutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rectTransform.anchoredPosition = positionLeft;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.InOutQuad).OnComplete(() => ShowEnd(callback));
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rectTransform.anchoredPosition = positionRight;
                rectTransform.DOAnchorPos(Vector2.zero, animationTime).SetEase(Ease.InOutQuad).OnComplete(() => ShowEnd(callback));
            }
        }
        void AnimationHide(Animation anim)
        {
            if (anim == Animation.ScaleInOut)
            {
                transform.DOScale(scaleMin, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveUpDown)
            {
                rectTransform.DOAnchorPos(positionBottom, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveDownUp)
            {
                rectTransform.DOAnchorPos(positionUp, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rectTransform.DOAnchorPos(positionLeft, animationTime).SetEase(Ease.InOutQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rectTransform.DOAnchorPos(positionRight, animationTime).SetEase(Ease.InOutQuad).OnComplete(HideEnd);
            }
        }
        void AnimationHideBack()
        {
            if (anim == Animation.ScaleInOut)
            {
                transform.DOScale(scaleMax, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveUpDown)
            {
                rectTransform.DOAnchorPos(positionUp, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveDownUp)
            {
                rectTransform.DOAnchorPos(positionBottom, animationTime).SetEase(Ease.InQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveLeftRight)
            {
                rectTransform.DOAnchorPos(positionRight, animationTime).SetEase(Ease.InOutQuad).OnComplete(HideEnd);
            }
            else if (anim == Animation.MoveRightLeft)
            {
                rectTransform.DOAnchorPos(positionLeft, animationTime).SetEase(Ease.InOutQuad).OnComplete(HideEnd);
            }
        }
        #endregion
    }
}