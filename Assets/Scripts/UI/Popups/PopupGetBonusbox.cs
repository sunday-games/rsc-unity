using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class PopupGetBonusbox : Popup
    {
        [Space(10)]
        public Catbox bonusbox;

        [Space(10)]
        public Image[] bonusImages = new Image[3];

        [Space(10)]
        public GameObject collectButton;

        public override void Init()
        {
            previous = ui.prepare;

            collectButton.SetActive(false);

            for (int i = 0; i < bonusImages.Length; ++i)
                bonusImages[i].sprite = user.lastGetBoosts[i].sprite;

            Invoke("ShowButton", 1.5f);
        }

        void ShowButton()
        {
            sound.Play(sound.winPrize);
            collectButton.SetActive(true);
        }
    }
}