using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class LeaderboardSlot : Core
    {
        public Text placeText;
        public RawImage picImage;
        public Image frameImage;
        public Image levelImage;
        public Image crownImage;
        public Text nameText;
        public Text recordText;
        public Text levelText;
        public Button inviteButton;
        public Image inviteDoneImage;
        public Button profileButton;

        public GameObject loading;

        public GameObject highlight;

        [HideInInspector]
        public string facebookID;

        public void Setup(Rival rival)
        {
            facebookID = rival.facebookID;

            recordText.text = rival.record.SpaceFormat();

            nameText.text = rival.name;
            if (rival.isPlayer) nameText.text += " (" + Localization.Get("you") + ")";

            highlight.SetActive(rival.isPlayer);

            if (rival.level > 0) levelText.text = rival.level.ToString();
            else levelImage.gameObject.SetActive(false);

            if (rival.userPicTexture != null) picImage.texture = rival.userPicTexture;
            else picImage.texture = pics.emptyUserPic;

            if (rival.userPicTextureRect != zeroRect) picImage.uvRect = rival.userPicTextureRect;

            if (picImage.texture == pics.emptyUserPic && user.socialPic != null) picImage.texture = user.socialPic;
        }

        public void OpenProfile()
        {
            sound.PlaySoundButton();
            ui.PopupShow(ui.profile);
        }

        public void Invite()
        {
            sound.PlaySoundButton();
            ui.Block();
            inviteButton.gameObject.SetActive(false);
            loading.SetActive(true);

            server.CheckConnection(succeess =>
            {
                if (succeess)
                {
#if FACEBOOK
                if (facebookID.Length < 20)
                    fb.InviteFriend(Localization.Get("challengeFriendMessage", user.record.SpaceFormat()),
                        Localization.Get("challengeFriendTitle"), InviteCallback, facebookID);
                else
                    fb.InviteFriend(Localization.Get("inviteFriendMessage"),
                        Localization.Get("inviteFriendTitle"), InviteCallback, facebookID);
#endif
                Notifications.SendPush(Localization.Get("notificationInvite", user.fullName), facebookId: facebookID, expirationHours: 12);
                }
                else
                {
                    inviteButton.gameObject.SetActive(true);
                    loading.SetActive(false);
                    ui.Unblock();
                }
            });
        }

        public void InviteCallback(List<string> facebookIDs)
        {
            loading.SetActive(false);
            ui.Unblock();

            if (facebookIDs != null && facebookIDs.Count > 0)
            {
                if (user.AddInvitedFriends(facebookIDs) > 0)
                    ui.header.ShowCoinsIn(inviteButton.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.5f);

                inviteDoneImage.gameObject.SetActive(true);
            }
            else
            {
                inviteButton.gameObject.SetActive(true);
            }
        }
    }
}