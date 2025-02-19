using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace SG.UI
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager instance;

        public RectTransform window;
        public Notification notificationPrefab;
        public float animTime = 0.6f;
        public int maxNotifications = 5;

        public Color progressPendingColor;
        public Color progressFailedColor;
        public Color progressCompletedColor;

        private List<Notification> notifications = new List<Notification>();
        private Queue<Notification> showQueue = new Queue<Notification>();
        private Queue<Notification> hideQueue = new Queue<Notification>();

        private Vector2 _notificationWidth;
        private bool _isAnimating = false;
        private bool _updatePositionsNeeded = false;

        public virtual void Setup()
        {
            instance = this;

            _notificationWidth = new Vector2(notificationPrefab.Size.x + window.anchoredPosition.x, 0f);
            notificationPrefab.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        public Notification NotificationCreate(string name, string title = null, string description = null,
            float progress = -1f, Color progressColor = default, float progressTime = default, string progressText = null, bool progressAnim = true,
            Action mainAction = null, ButtonData buttonData1 = null, ButtonData buttonData2 = null, float autoHideTime = default)
        {
            Notification Setup(Notification notification)
            {
                notification
                    .SetName(name)
                    .SetTitle(title)
                    .SetDescription(description)
                    .SetProgress(progress, progressColor, progressTime, progressAnim, progressText)
                    .SetMainButton(mainAction)
                    .SetAutoHide(autoHideTime);

                if (notification.ActionButtons.Length > 0)
                {
                    notification.SetActionButton(notification.ActionButtons[0], buttonData1);

                    if (notification.ActionButtons.Length > 1)
                        notification.SetActionButton(notification.ActionButtons[1], buttonData2);

                    notification.ActionButtons[0].transform.parent.gameObject.SetActive(buttonData1 != null || buttonData2 != null);
                }

                return notification;
            }

            Notification existingNotification = null;
            if (notifications.TryFind(n => n.name == name, out existingNotification) ||
                showQueue.TryFind(n => n.name == name, out existingNotification))
            {
                Setup(existingNotification).Show();
                return existingNotification;
            }

            var notification = Setup(Instantiate(notificationPrefab, notificationPrefab.transform.parent));

            showQueue.Enqueue(notification);

            RunAnimation();

            return notification;
        }

        public bool NotificationHide(string name)
        {
            if (notifications.TryFind(n => n.name == name, out Notification notification))
            {
                NotificationHide(notification);
                return true;
            }
            return false;
        }
        public void NotificationHide(Notification notification)
        {
            if (hideQueue.Contains(notification))
                return;

            hideQueue.Enqueue(notification);

            RunAnimation();
        }

        public void Clear()
        {
            if (notifications.Count == 0)
                return;

            foreach (var notification in notifications)
                hideQueue.Enqueue(notification);

            RunAnimation();
        }

        private void RunAnimation()
        {
            if (!_isAnimating)
                StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                _isAnimating = true;
                while (showQueue.Count > 0 || hideQueue.Count > 0)
                {
                    if (hideQueue.Count > 0)
                    {
                        var notification = hideQueue.Dequeue();

                        if (notification != null)
                        {
                            notification.StopAllCoroutines();

                            var notificationIndex = notifications.IndexOf(notification);
                            notifications.Remove(notification);
                            notification.Window.DOAnchorPos(notification.Position - _notificationWidth, animTime)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() =>
                                {
                                    if (notification != null && notification.gameObject != null)
                                    {
                                        notification.Window.DOKill();
                                        Destroy(notification.gameObject);
                                    }
                                });

                            for (int i = 0; i < notificationIndex; i++)
                                notifications[i].Window.DOAnchorPos(
                                        new Vector2(notifications[i].Position.x, notifications[i].Position.y - notification.Size.y), animTime)
                                    .SetEase(Ease.InBack);
                        }
                    }
                    else if (showQueue.Count > 0)
                    {
                        if (notifications.Count >= maxNotifications)
                        {
                            hideQueue.Enqueue(notifications[0]);
                            continue;
                        }

                        var notification = showQueue.Dequeue();

                        if (notification != null)
                        {
                            notification.Show();
                            notifications.Add(notification);

                            // wait until canvas will recalculate, before it notification.Size.y == 0
                            yield return new WaitUntil(() => notification.Size.y > 0f);
                            notification.Position = new Vector2(0f, -notification.Size.y);

                            for (int i = 0; i < notifications.Count; i++)
                                notifications[i].Window.DOAnchorPos(
                                    new Vector2(notifications[i].Position.x, notifications[i].Position.y + notification.Size.y), animTime)
                                    .SetEase(Ease.OutQuad);
                        }
                    }
                    yield return new WaitForSeconds(animTime);
                }

                while (_updatePositionsNeeded)
                {
                    _updatePositionsNeeded = false;

                    var height = 0f;
                    for (int i = notifications.Count - 1; i >= 0; i--)
                    {
                        notifications[i].Position = new Vector2(notifications[i].Position.x, height);
                        height += notifications[i].Size.y;
                    }
                }

                _isAnimating = false;
            }
        }

        public IEnumerator OnNotificationChanged(Notification notification)
        {
            var stopWaitingTime = Time.time + 0.2f;
            var oldSizeY = notification.Size.y;
            while (stopWaitingTime > Time.time)
            {
                if (notification.Size.y != oldSizeY)
                {
                    _updatePositionsNeeded = true;
                    RunAnimation();
                    break;
                }
                yield return null;
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]

        private readonly string _testTitle = "Test title";

        private string GetRandomDescription() =>
            new List<string> { "alpha", "alpha\nbeta", "alpha\nbeta\ngamma" }.GetRandom();

        [Button("AddSimpleNotification")] public bool addSimpleNotification;
        public void AddSimpleNotification()
        {
            NotificationCreate("SimpleNotification" + Utils.RandomRange(1000, 9999), _testTitle, GetRandomDescription());
        }

        [Button("AddSimpleButtonNotification")] public bool addSimpleButtonNotification;
        public void AddSimpleButtonNotification()
        {
            var buttonData = new ButtonData("Button text", () => Debug.Log("Text optional action"));
            NotificationCreate("SimpleButtonNotification" + Utils.RandomRange(1000, 9999),
                title: _testTitle,
                description: GetRandomDescription(),
                buttonData1: buttonData);
        }

        [Button("UpdateNotification")] public bool updateNotification;
        public void UpdateNotification()
        {
            if (notifications.Count > 0)
                NotificationCreate(notifications.GetRandom().name, _testTitle, GetRandomDescription());
        }

        [Button("AddProgressNotification")] public bool addProgressNotification;
        public void AddProgressNotification()
        {
            StartCoroutine(AddProgressNotificationCoroutine());
            IEnumerator AddProgressNotificationCoroutine()
            {
                string notificationName = "ProgressNotification" + Utils.RandomRange(1000, 9999);
                for (int i = 0; i < 10; i++)
                {
                    NotificationCreate(notificationName, _testTitle, GetRandomDescription(), (i + 1) * 0.1f, progressPendingColor);
                    yield return new WaitForSeconds(1f);
                    NotificationCreate(notificationName, _testTitle, GetRandomDescription(), (i + 1) * 0.1f, progressCompletedColor);
                }
            }
        }
#endif
    }
}