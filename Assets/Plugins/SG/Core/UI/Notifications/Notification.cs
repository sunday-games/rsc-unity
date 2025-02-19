using UnityEngine;
using System;
using System.Collections;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public class Notification : MonoBehaviour
    {
        private const float DELAY = 1f;

        public RectTransform Window;
        public Text TitleText;
        public Text DescriptionText;
        public Progress Progress;
        public Button MainButton;
        public Button[] ActionButtons;

        private float _autoHideTime;
        private float _progressTime;

        public Vector2 Position
        {
            get => Window.anchoredPosition;
            set => Window.anchoredPosition = value;
        }
        public Vector2 Size => Window.sizeDelta;

        public Notification SetName(string name)
        {
            this.name = name;
            return this;
        }
        public Notification SetTitle(string text)
        {
            if (TitleText && TitleText.ActivateIf(text.IsNotEmpty()))
                TitleText.text = text;
            return this;
        }

        public Notification SetDescription(string text)
        {
            if (DescriptionText && DescriptionText.ActivateIf(text.IsNotEmpty()))
                DescriptionText.text = text;
            return this;
        }

        public Notification SetProgress(float value, Color color, float time, bool animation = true, string progressText = null)
        {
            _progressTime = time;
            Progress.ValueText?.SetText(progressText ?? string.Empty);

            if (Progress.ActivateIf(value >= 0f))
            {
                Progress.SetColor(color);

                if (animation)
                    Progress.SetValueWithAnimation(value, DELAY);
                else
                    Progress.SetValue(value);
            }
            return this;
        }

        public Notification SetMainButton(Action callback)
        {
            if (callback != null)
                MainButton.SetCallback(callback);
            return this;
        }

        public Notification SetActionButton(Button button, ButtonData buttonData)
        {
            if (button.ActivateIf(buttonData != null))
            {
                button.SetText(buttonData.Text);

                button.SetCallback(() =>
                {
                    if (buttonData.HideButton && !buttonData.CloseWindow)
                    {
                        button.transform.parent.gameObject.SetActive(false);
                        StartCoroutine(UI.Instance.Notifications.OnNotificationChanged(this));
                    }

                    buttonData.Action?.Invoke();

                    if (buttonData.CloseWindow)
                        Hide();
                });
            }
            return this;
        }

        public Notification SetAutoHide(float autoHideTime)
        {
            _autoHideTime = autoHideTime;
            return this;
        }

        public void Show()
        {
            StopAllCoroutines();

            gameObject.SetActive(true);

            if (_autoHideTime != default)
                StartCoroutine(AutoHider());

            if (Progress.ValueText)
            {
                if (_progressTime != default)
                    StartCoroutine(ProgressTimeUpdater());
                else if (Progress.ValueText.text.Contains("..."))
                    StartCoroutine(DotUpdater(Progress.ValueText));
            }

            if (TitleText && TitleText.text.Contains("..."))
                StartCoroutine(DotUpdater(TitleText));

            StartCoroutine(UI.Instance.Notifications.OnNotificationChanged(this));
        }

        public void Hide() => NotificationManager.instance.NotificationHide(this);

        private IEnumerator AutoHider()
        {
            yield return new WaitForSeconds(_autoHideTime);
            Hide();
        }

        private IEnumerator DotUpdater(Text component)
        {
            while (true)
            {
                yield return new WaitForSeconds(DELAY);

                if (component.text.Contains("..."))
                    component.text = component.text.Replace("...", ".");
                else if (component.text.Contains(".."))
                    component.text = component.text.Replace("..", "...");
                else if (component.text.Contains("."))
                    component.text = component.text.Replace(".", "..");
                else
                    yield break;
            }
        }

        private IEnumerator ProgressTimeUpdater()
        {
            var timeoutSeconds = _progressTime;

            do
            {
                var timeLeft = TimeSpan.FromSeconds(timeoutSeconds);
                Progress.ValueText.text = timeLeft.ToString("mm':'ss");
                Progress.SetValueWithAnimation(timeoutSeconds / _progressTime, DELAY);
                yield return new WaitForSeconds(DELAY);
                timeoutSeconds -= DELAY;
            }
            while (timeoutSeconds > 0);
        }
    }
}