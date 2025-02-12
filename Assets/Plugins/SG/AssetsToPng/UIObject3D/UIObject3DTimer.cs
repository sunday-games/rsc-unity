using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace ThreeDimensional
{
    internal class DelayedEditorAction
    {
        internal double TimeToExecute;
        internal Action Action;
        internal MonoBehaviour ActionTarget;
        internal bool ForceEvenIfTargetIsGone;

        public DelayedEditorAction(double timeToExecute, Action action, MonoBehaviour actionTarget, bool forceEvenIfTargetIsGone = false)
        {
            TimeToExecute = timeToExecute;
            Action = action;
            ActionTarget = actionTarget;
            ForceEvenIfTargetIsGone = forceEvenIfTargetIsGone;
        }
    }

    public static class UIObject3DTimer
    {
        private static UIObject3DTimerComponent _timerComponent;
        private static UIObject3DTimerComponent timerComponent
        {
            get
            {
                if (_timerComponent == null)
                {
                    _timerComponent = GameObject.FindFirstObjectByType<UIObject3DTimerComponent>();

                    if (_timerComponent == null)
                    {
                        var timerGO = new GameObject("UIObject3DTimer");
                        _timerComponent = timerGO.AddComponent<UIObject3DTimerComponent>();
                        timerGO.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                return _timerComponent;
            }
        }

        public static void DestroyGO()
        {
            if (_timerComponent != null)
                GameObject.Destroy(_timerComponent.gameObject);
            _timerComponent = null;
        }

#if UNITY_EDITOR
        static List<DelayedEditorAction> delayedEditorActions = new List<DelayedEditorAction>();

        static UIObject3DTimer()
        {
            //if (!Application.isPlaying) UnityEditor.EditorApplication.update += EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }
#endif

        static void EditorUpdate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            var actionsToExecute = delayedEditorActions.Where(dea => UnityEditor.EditorApplication.timeSinceStartup >= dea.TimeToExecute).ToList();

            if (!actionsToExecute.Any()) return;

            foreach (var actionToExecute in actionsToExecute)
            {
                try
                {
                    if (actionToExecute.ActionTarget != null || actionToExecute.ForceEvenIfTargetIsGone) // don't execute if the target is gone
                    {
                        actionToExecute.Action.Invoke();
                    }
                }
                finally
                {
                    delayedEditorActions.Remove(actionToExecute);
                }
            }
#endif
        }

        /// <summary>
        /// Call Action 'action' after the specified delay, provided the 'actionTarget' is still present and active in the scene at that time.
        /// Can be used in both edit and play modes.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        /// <param name="actionTarget"></param>
        public static void DelayedCall(float delay, Action action, MonoBehaviour actionTarget, bool forceEvenIfObjectIsInactive = false)
        {
            if (Application.isPlaying)
            {                
                if (forceEvenIfObjectIsInactive) timerComponent.StartCoroutine(_DelayedCall(delay, action));
                else if (actionTarget != null && actionTarget.gameObject.activeInHierarchy) actionTarget.StartCoroutine(_DelayedCall(delay, action));                
            }
#if UNITY_EDITOR
            else
            {
                delayedEditorActions.Add(new DelayedEditorAction(UnityEditor.EditorApplication.timeSinceStartup + delay, action, actionTarget, forceEvenIfObjectIsInactive));
            }
#endif
        }

        private static IEnumerator _DelayedCall(float delay, Action action)
        {
            if (delay == 0f) yield return null;
            else yield return new WaitForSeconds(delay);

            action.Invoke();
        }

        /// <summary>
        /// Shorthand for DelayedCall(0, action, actionTarget)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="actionTarget"></param>
        public static void AtEndOfFrame(Action action, MonoBehaviour actionTarget, bool forceEvenIfObjectIsInactive = false)
        {
            DelayedCall(0, action, actionTarget, forceEvenIfObjectIsInactive);
        }
    }
}
