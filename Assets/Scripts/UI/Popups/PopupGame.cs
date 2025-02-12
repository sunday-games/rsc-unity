using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupGame : Popup
    {
        public GameObject add;

        public Text scoreText;
        public Text multiplierText;

        public Text timeText;
        public Image timeImage;
        public Image freezeImage;

        public CatSlot[] catSlots;
        public GameObject[] catLockedSlots;
        public GameObject[] catLockedParentSlots;

        public Image feverLightImage;

        public Mover coinPrefab;
        public Mover manaPrefab;

        public GameObject missionStatus;

        public Image alertLeft;
        public Image alertRight;

        public Transform stuffBack;
        public Transform stuffFront;
        public Transform stuffFrontFront;

        public Transform spawnPointLeft;
        public Transform spawnPointRight;

        public ShowCombo showCombo;

        public GameObject timeOutButton;

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) ui.PopupShow(ui.pause);
        }

        public override void Init()
        {
            timeOutButton.SetActive(build.cheats);

            ui.header.gameObject.SetActive(false);

            add.SetActive(true);

            if (Missions.isGoldfishes) gameplay.level.coinSlot.SetActive(true);
            else gameplay.level.coinSlot.SetActive(false);

            missionStatus.SetActive(user.isLevelOK);

            freezeImage.color = new Color(1, 1, 1, gameplay.isTimeFreezed ? 1 : 0);

            if (gameplay.seconds < 9) AlertON();
            else AlertOFF();

            if (gameplay.isFever) FeverON();
        }

        public void InitSlots()
        {
            foreach (CatSlot slot in catSlots) slot.Clear();

            for (int i = 0; i < catSlots.Length; i++)
                if (i < Missions.maxCatSlot)
                {
                    catSlots[i].gameObject.SetActive(true);
                    catLockedParentSlots[i].SetActive(false);
                    if (ui.prepare.catSlots[i].catItem != null) catSlots[i].Init(ui.prepare.catSlots[i].catItem);
                }
                else
                {
                    catSlots[i].gameObject.SetActive(false);
                    catLockedParentSlots[i].SetActive(true);
                }
        }

        public override void Reset()
        {
            ui.header.gameObject.SetActive(true);
            missionStatus.SetActive(false);
            iTween.Stop(missionStatus);
            add.SetActive(false);
            HideFeverLightImage();
        }

        public void TutorialClosedGameBox(int i)
        {
            ui.tutorial.Show(Tutorial.Part.GameClosedBox, new Transform[] { catLockedSlots[i].transform }, Missions.BOX_UNLOCK_LEVELS[i].ToString());
        }

        public override void OnEscapeKey()
        {
            ui.PopupShow(ui.pause);
        }

        public void FeverON()
        {
            if (music.ON) iTween.AudioTo(music.game.gameObject, music.game.volume, 1.4f, 1);
            feverLightImage.gameObject.SetActive(true);
            iTween.ScaleTo(feverLightImage.gameObject, iTween.Hash("x", 1, "y", 1, "easeType", "easeOutBack", "time", 1));
            StartCoroutine("FlashFeverImage");
            UpdateMultiplier();
        }
        public void FeverOFF()
        {
            if (music.ON) iTween.AudioTo(music.game.gameObject, music.game.volume, 1, 1);
            StopCoroutine("FlashFeverImage");
            iTween.ScaleTo(feverLightImage.gameObject,
                iTween.Hash("x", 0.2f, "y", 0.2f, "easeType", "easeInBack", "time", 1, "oncomplete", "HideFeverLightImage", "oncompletetarget", gameObject));

            UpdateMultiplier();
        }
        public void HideFeverLightImage()
        {
            feverLightImage.gameObject.SetActive(false);
        }
        IEnumerator FlashFeverImage()
        {
            while (gameplay.isFever)
            {
                while (gameplay.level.feverImage.color.a > 0.05f)
                {
                    gameplay.level.feverImage.color = new Color(
                    gameplay.level.feverImage.color.r,
                    gameplay.level.feverImage.color.g,
                    gameplay.level.feverImage.color.b,
                    gameplay.level.feverImage.color.a - 2 * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                while (gameplay.level.feverImage.color.a < 0.95f)
                {
                    gameplay.level.feverImage.color = new Color(
                        gameplay.level.feverImage.color.r,
                        gameplay.level.feverImage.color.g,
                        gameplay.level.feverImage.color.b,
                        gameplay.level.feverImage.color.a + 2 * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public void UpdateMultiplier()
        {
            multiplierText.text = gameplay.multiplier > 1 ? "x" + gameplay.multiplier : "";
            multiplierText.text += gameplay.isFever ? " x3" : "";
            iTween.PunchScale(multiplierText.gameObject, Vector3.one, 1f);
        }

        public void FreezeShow()
        {
            StartCoroutine(FreezingShow());
        }
        IEnumerator FreezingShow()
        {
            while (freezeImage.color.a < 1f)
            {
                freezeImage.color = new Color(1, 1, 1, freezeImage.color.a + 3 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        public void FreezeHide()
        {
            StartCoroutine(FreezingHide());
        }
        IEnumerator FreezingHide()
        {
            while (freezeImage.color.a > 0f)
            {
                freezeImage.color = new Color(1, 1, 1, freezeImage.color.a - 3 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        public bool isAlertON { get { return alertLeft.gameObject.activeSelf; } }
        public void AlertON()
        {
            sound.PlayVoice(sound.voiceClips.hurryUp[Random.Range(0, sound.voiceClips.hurryUp.Length)]);
            alertLeft.gameObject.SetActive(true);
            alertRight.gameObject.SetActive(true);
        }
        public void AlertOFF()
        {
            alertLeft.gameObject.SetActive(false);
            alertRight.gameObject.SetActive(false);
        }
    }
}