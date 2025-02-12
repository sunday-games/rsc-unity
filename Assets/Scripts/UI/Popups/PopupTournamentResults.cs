using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class PopupTournamentResults : Popup
    {
        public RectTransform window;
        public LeaderboardSlot tournamentSlotPrefab;
        public GridLayoutGroup tournamentGrid;
        public Text rewardText;
        public Text coinsText;
        public Text buttonText;
        public GameObject shareButton;

        public float heightWithShare = 500f;
        public float heightWithoutShare = 640f;

        public override void Init()
        {
            sound.Play(sound.winPrize);
        }

        public override void AfterInit()
        {
            if (rivalsCount == 1)
            {
                if (!user.IsTutorialShown(Tutorial.Part.TournamentInviteToCompite)) ShowTutorial();
            }
            else if (playerPlace == rivalsCount)
            {
                if (!user.IsTutorialShown(Tutorial.Part.TournamentBetterResults)) ShowTutorial();
            }
            else
            {
                if (!user.IsTutorialShown(Tutorial.Part.TournamentInviteToMoreGoldfish)) ShowTutorial();
            }
        }

        public override void Reset()
        {
            foreach (LeaderboardSlot slot in tournamentSlots)
                Destroy(slot.gameObject);
            tournamentSlots.Clear();
        }

        int rivalsCount;
        int playerPlace;
        List<LeaderboardSlot> tournamentSlots = new List<LeaderboardSlot>();
        public void CreateTournamentSlots(List<Rival> rivals, int coins)
        {
            (tournamentGrid.transform as RectTransform).sizeDelta =
                new Vector2((tournamentGrid.transform as RectTransform).sizeDelta.x, 20 + rivals.Count * tournamentGrid.cellSize.y);

            rivalsCount = rivals.Count;
            playerPlace = 0;
            int i = 0;
            long lastRecord = long.MaxValue;
            foreach (var rival in rivals)
            {
                var slot = Instantiate(tournamentSlotPrefab) as LeaderboardSlot;
                slot.transform.SetParent(tournamentGrid.transform, false);
                tournamentSlots.Add(slot);

                slot.Setup(rival);

                if (rival.record < lastRecord) i++;
                slot.placeText.text = i.ToString();
                lastRecord = rival.record;

                if (rival.isPlayer) playerPlace = i;

                if (i > 0 && i < 4)
                {
                    slot.crownImage.gameObject.SetActive(true);
                    slot.crownImage.sprite = pics.crowns[i - 1];
                    if (slot.frameImage != null) slot.frameImage.color = pics.crownColors[i - 1];
                }
            }

            int prizeFund = 0;
            for (int j = 1; j <= rivalsCount - playerPlace; j++)
                prizeFund += j;
            coinsText.text = prizeFund > 0 ? (prizeFund * 100).ToString() : "0";

            if (rivalsCount == 1)
            {
                rewardText.text = Localization.Get("placeOnlyPlayer");
                buttonText.text = Localization.Get("next");
            }
            else if (playerPlace == rivalsCount)
            {
                rewardText.text = Localization.Get("placeLast");
                buttonText.text = Localization.Get("next");
            }
            else
            {
                rewardText.text = Localization.Get("placeNotLast", rivalsCount - playerPlace);
                buttonText.text = Localization.Get("collect");
            }

            user.AddWonTournament(playerPlace, rivalsCount);

            Analytic.EventProperties("Other", "TournamentEnd", rivalsCount.ToString());

            ShareBlock(fb.isLogin && rivalsCount - playerPlace > 0);
        }

        public void ShowTutorial()
        {
            if (rivalsCount == 1)
                ui.tutorial.Show(Tutorial.Part.TournamentInviteToCompite, new Transform[] { window });
            else if (playerPlace == rivalsCount)
                ui.tutorial.Show(Tutorial.Part.TournamentBetterResults, new Transform[] { window });
            else
                ui.tutorial.Show(Tutorial.Part.TournamentInviteToMoreGoldfish, new Transform[] { window });
        }

        public void Collect()
        {
            if (fb.isLogin && rivalsCount - playerPlace > 0) ui.header.ShowCoinsIn(coinsText.transform.position, 15, ui.canvas[3].transform, shift: 0.4f, delay: 0.8f);
            ui.PopupClose();
        }

        public override void OnEscapeKey()
        {
            Collect();
        }

        public void ShareBlock(bool show)
        {
            shareButton.SetActive(show);
            window.sizeDelta = new Vector2(window.sizeDelta.x, show ? heightWithShare : heightWithoutShare);
        }

        public void Share()
        {
            ui.Block();
            server.CheckConnection(succeess =>
            {
                if (succeess)
                {
#if FACEBOOK
                fb.Share(
                    Localization.Get("shareTournament"),
                    Localization.Get("shareTournamentDescription" + user.gender, rivalsCount - playerPlace),
                    fb.sharePicLinks.tournament, 
                    ShareResult, 
                    "ShareTournament");
#endif
            }
                else ui.Unblock();
            });
        }
        public void ShareResult(bool success)
        {
            ui.Unblock();

            if (success)
            {
                ShareBlock(false);
                user.UpdateCoins(balance.reward.coinsForShareTournamentResult, true);
                ui.header.ShowCoinsIn(shareButton.transform.position, 6, ui.canvas[3].transform, shift: 0.4f, delay: 0.7f);
            }
        }
    }
}