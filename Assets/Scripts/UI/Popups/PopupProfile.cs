using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class PopupProfile : Popup
    {
        public RawImage avatarImage;
        public Text levelText;
        public Text nameText;
        public Text leagueText;
        public Text recordText;
        public Text gameEndsText;
        public Text catScaredText;
        public Text friendWinsText;

        public GridLayoutGroup medalsGrid;

        public GameObject loginButton;
        public GameObject loginBonus;
        public Text loginBonusText;

        public GameObject bubble;
        public GameObject bubbleTexts;
        public Text bubbleGetText;
        public Text bubbleBonusText;

        public GameObject achievementsButton;
        public Image achievementsImage;
        public Sprite gameCenter;
        public Sprite googleGames;

        List<Medal> medals = new List<Medal>();

        public override void Init()
        {
            if (avatarImage.texture == pics.emptyUserPic)
            {
                if (!string.IsNullOrEmpty(user.facebookId))
                    server.DownloadPic(avatarImage, fb.GetPicURL(user.facebookId));
                else if (user.socialPic != null) avatarImage.texture = user.socialPic;
            }
            levelText.text = user.level.ToString();
            nameText.text = !string.IsNullOrEmpty(user.nameToView) ? user.nameToView : Localization.Get("nameNotSet");
            leagueText.text = Localization.Get("league" + user.league.name) + " " + Localization.Get("league");
            recordText.text = user.permanentRecord.SpaceFormat();
            gameEndsText.text = user.gameSessions.SpaceFormat();
            catScaredText.text = user.getCatsHistory.SpaceFormat();
            friendWinsText.text = user.allWonFriends.SpaceFormat();

            loginButton.SetActive(!fb.isLogin && build.facebook);

            loginBonusText.text = balance.reward.coinsForFacebookLogin.SpaceFormat();
            loginBonus.SetActive(Missions.isGoldfishes && !user.isId);

            foreach (Medal medal in medals) Destroy(medal.gameObject);
            medals.Clear();
            foreach (var achieve in achievements.list)
            {
                if ((achieve is Achievements.TournamentWonFriends || achieve is Achievements.InviteFriends) && !build.facebook)
                    continue;

                medals.Add(Medal.Create(medalsGrid.transform, achieve));
            }

            if (platform == Platform.iOS)
            {
                achievementsButton.SetActive(true);
                achievementsImage.sprite = gameCenter;
            }
            else if (platform == Platform.Android)
            {
                achievementsButton.SetActive(true);
                achievementsImage.sprite = googleGames;
            }
            else
            {
                achievementsButton.SetActive(false);
            }
        }

        public override void AfterInit()
        {
            if (!user.IsTutorialShown(Tutorial.Part.Login) && !fb.isLogin && build.facebook)
            {
                if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Login1, Tutorial.Part.Login }, new Transform[] { loginButton.transform });
                else ui.tutorial.Show(Tutorial.Part.Login, new Transform[] { loginButton.transform });
            }
            else if (!user.IsTutorialShown(Tutorial.Part.Medals))
                ui.tutorial.Show(Tutorial.Part.Medals, new Transform[] { medalsGrid.transform });
        }

        static List<int> leftSide = new List<int> { 4, 5, 9, 10, 14, 15, 19, 20 };
        public void BubbleShow(Medal medal)
        {
            bubble.transform.position = medal.transform.position;
            bubbleGetText.text = medal.achievement.getText;
            if (medal.achievement.current() > 0)
            {
                if (medal.achievement is Achievements.League) bubbleGetText.text += " (" + Localization.Get("league" + user.league.name) + ")";
                else bubbleGetText.text += " (" + medal.achievement.current().SpaceFormat() + ")";
            }
            bubbleBonusText.text = medal.achievement.bonusText;

            if (leftSide.IndexOf(medals.IndexOf(medal) + 1) < 0)
            {
                bubble.transform.rotation = Quaternion.Euler(Vector3.zero);
                bubbleTexts.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                bubble.transform.rotation = Quaternion.Euler(0, 180, 0);
                bubbleTexts.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }

            bubble.SetActive(true);
        }

        public void BubbleHide()
        {
            bubble.SetActive(false);
        }
    }
}