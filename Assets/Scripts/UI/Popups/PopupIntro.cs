using UnityEngine;

namespace SG.RSC
{
    public class PopupIntro : Popup
    {
        public override void OnEscapeKey()
        {
            Application.Quit();
        }
    }
}