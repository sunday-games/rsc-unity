using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupGetGoldfishes : Popup
    {
        public Image coinsImage;
        public Text coinsText;

        public override void Init()
        {
            previous = ui.prepare;

            coinsText.text = (ui.shop.aquarium * ui.shop.xGoldfishes).SpaceFormat();

            sound.Play(sound.winPrize);
        }

        public void Get()
        {
            ui.header.ShowCoinsIn(coinsImage.transform.position, 15, ui.canvas[3].transform, shift: 0.4f, delay: 0.6f);
            ui.PopupClose();
        }

        public override void OnEscapeKey() { Get(); }
    }
}