using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupPause : Popup
    {
        [Space(10)]

        public RectTransform window;

        [Space(10)]

        public Text soundText;
        public Text voiceText;
        public Text musicText;

        [Space(10)]

        public GameObject restartButton;

        [Space(10)]

        public MissionList missionList;

        [Space(10)]

        public Vector2 sizeCompact = new Vector2(500f, 320f);
        public Vector2 sizeWithRestart = new Vector2(500f, 463f);
        public Vector2 sizeWithMissions = new Vector2(500f, 700f);

        public override void Init()
        {
            gameplay.isPause = true;

            music.SetVolume(0.1f, 1f);

            soundText.text = Localization.Get(sound.ON ? "on" : "off");
            voiceText.text = Localization.Get(sound.voiceON ? "on" : "off");
            musicText.text = Localization.Get(music.ON ? "on" : "off");

            if (user.level == 0)
            {
                window.sizeDelta = sizeCompact;
                restartButton.SetActive(false);
                missionList.gameObject.SetActive(false);
            }
            else if (!user.isLevelOK)
            {
                window.sizeDelta = sizeWithRestart;
                restartButton.SetActive(true);
                missionList.gameObject.SetActive(false);
            }
            else
            {
                window.sizeDelta = sizeWithMissions;
                restartButton.SetActive(true);
                missionList.gameObject.SetActive(true);
            }
        }

        public override void AfterInit()
        {
            missionList.UpdateCheckboxes();
        }

        public override void Reset()
        {
            gameplay.isPause = false;

            music.SetVolume(music.volumeNormal, 1f);
        }

        public override void OnEscapeKey()
        {
            if (platform == Platform.Tizen) gameplay.RestartGame();
            else ui.PopupClose();
        }

        public void SoundToggle()
        {
            sound.ON = !sound.ON;
            soundText.text = Localization.Get(sound.ON ? "on" : "off");
        }

        public void VoiceToggle()
        {
            sound.voiceON = !sound.voiceON;
            voiceText.text = Localization.Get(sound.voiceON ? "on" : "off");
        }

        public void MusicToggle()
        {
            music.ON = !music.ON;
            if (music.ON) music.Switch(music.game);
            else music.TurnOff();
            musicText.text = Localization.Get(music.ON ? "on" : "off");
        }
    }
}