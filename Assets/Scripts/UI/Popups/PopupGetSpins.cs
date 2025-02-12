using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupGetSpins : Popup
    {
        public Text spinsText;

        public override void Init()
        {
            previous = ui.luckyWheel;
            ui.luckyWheel.previous = ui.main;

            spinsText.text = (balance.shop.sausages * ui.shop.xSausages).SpaceFormat();

            sound.Play(sound.winPrize);
        }
    }
}