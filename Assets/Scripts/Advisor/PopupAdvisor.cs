using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using IngameAdvisor;

namespace SG.RSC
{
    public class PopupAdvisor : Popup
    {
        public Animator catAnimator;
        public AudioSource catAudioSource;

        [Space(10)]
        public Bubble replyBubble;
        public Vector2 replyBubbleHeight = new Vector2(210, 730f);

        [Space(10)]
        public Bubble askBubble;

        [Space(10)]
        public InputField askInputField;
        public GameObject micImage;
        public GameObject keyboardImage;

        [Space(10)]
        public AskButton askButtonPrefab;
        List<AskButton> askButtons = new List<AskButton>();
        public GridLayoutGroup askButtonsGrid;
        public Vector2 askButtonsPositionKeyboardNone;
        public Vector2 askButtonsPositionKeyboardIOS;
        public Vector2 askButtonsPositionKeyboardAndroid;
        Vector2 askButtonsPositionKeyboard;

        // Сколько раз подряд Советник не понял, чего от него хотят
        int misunderstandingsCount = 0;

        public override void Init()
        {
            askButtonsRectTransform = askButtonsGrid.transform as RectTransform;

            if (Utils.IsPlatform(Platform.iOS)) askButtonsPositionKeyboard = askButtonsPositionKeyboardIOS;
            else if (Utils.IsPlatform(Platform.Android)) askButtonsPositionKeyboard = askButtonsPositionKeyboardAndroid;
            else askButtonsPositionKeyboard = askButtonsPositionKeyboardNone;

            Reset();

            Analytic.Event("Advisor", "Open");
        }

        public override void AfterInit()
        {
            ReplyBubbleShow(Localization.Get("advisorReplyDefault"));
        }

        public void Tap()
        {
            catAnimator.SetTrigger("Tap");
            catAudioSource.pitch = Random.Range(0.8f, 1.2f);
            if (sound.ON) catAudioSource.Play();
        }

        RectTransform askButtonsRectTransform;
        bool lastState = false;
        public void Update()
        {
            micImage.SetActive(string.IsNullOrEmpty(askInputField.text));
            keyboardImage.SetActive(string.IsNullOrEmpty(askInputField.text));

            if (lastState != askInputField.isFocused)
            {
                lastState = askInputField.isFocused;
                askButtonsGrid.DOKill();

                Vector2 to = askButtonsPositionKeyboardNone;

                if (askInputField.isFocused && Utils.IsPlatform(Platform.Mobile))
                    to = askButtonsPositionKeyboard
                        - (askButtonsRectTransform.childCount - 2) * new Vector2(0f, askButtonsGrid.cellSize.y + askButtonsGrid.spacing.y);

                askButtonsRectTransform.DOAnchorPos(to, 0.4f);
            }
        }

        public void Reset(string ask = null)
        {
            foreach (var replyButton in askButtons) Destroy(replyButton.gameObject);
            askButtons.Clear();

            replyBubble.Hide();

            askBubble.Hide();
            if (!string.IsNullOrEmpty(ask)) askBubble.Show(ask, replyBubbleHeight);

            if (!Utils.IsPlatform(Platform.Mobile))
            {
                askInputField.Select();
                askInputField.ActivateInputField();
            }
        }

        public void AskFromInputField()
        {
            if (string.IsNullOrEmpty(askInputField.text) || !advisor) return;

            Ask(askInputField.text);
            askInputField.text = "";
        }

        public void Ask(string text)
        {
            Reset(text);

            var request = new Advisor.Request(text);
            request.user = user.isId ? user.id : user.deviceId;
            request.vars = new List<Advisor.Var>() {
            new Advisor.Var("platform", platform),
            new Advisor.Var("misunderstandingsCount", misunderstandingsCount),
            new Advisor.Var("facebookLogin", fb.isLogin),
            new Advisor.Var("musicOn", music.ON),
        };

            advisor.Ask(request, Localization.language, OnGetReply);
        }

        public void OnGetReply(Advisor.Status status, Advisor.Response response, Advisor.Request request)
        {
            if (status == Advisor.Status.NoConnection)
            {
                ReplyBubbleShow(Localization.Get("advisorReplyNoConnection"));
                return;
            }

            if (status != Advisor.Status.Success || response == null)
            {
                ReplyBubbleShow(Localization.Get("advisorReplyError"));
                return;
            }

            Analytic.EventProperties("Advisor", response.input, request.text);

            ReplyBubbleShow(response.output);

            if (!string.IsNullOrEmpty(response.imageUrl))
                server.DownloadPic(replyBubble.bubbleContentImage, response.imageUrl);

            if (!string.IsNullOrEmpty(response.url))
                SG_Utils.OpenLink(response.url, response.output);

            var replies = response.replies;
            while (replies.Count > 3)
                replies.Remove(replies[Random.Range(0, replies.Count)]);
            foreach (var reply in replies)
                askButtons.Add(askButtonPrefab.Copy(reply));

            misunderstandingsCount = response.input == "All" ? misunderstandingsCount + 1 : 0;
        }

        public void ReplyBubbleShow(string text)
        {
            replyBubble.Show(text,
                replyBubbleHeight - (askButtonsRectTransform.childCount - 2) * new Vector2(0f, askButtonsGrid.cellSize.y + askButtonsGrid.spacing.y));
        }

        public void OnAskButtonClick(Advisor.Reply reply)
        {
            if (!string.IsNullOrEmpty(reply.url))
            {
                SG_Utils.OpenLink(reply.url, reply.name);

                Reset(reply.name);
                if (!string.IsNullOrEmpty(reply.answer)) ReplyBubbleShow(reply.answer);
            }
            else if (!string.IsNullOrEmpty(reply.email))
            {
                // Utils.Email(reply.email, reply.subject, reply.body);

                ui.options.SendEmail();

                Reset(reply.name);
                if (!string.IsNullOrEmpty(reply.answer)) ReplyBubbleShow(reply.answer);
            }
            else
            {
                Ask(reply.name);
            }
        }
    }
}