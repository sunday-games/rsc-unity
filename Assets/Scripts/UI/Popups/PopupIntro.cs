using UnityEngine;

public class PopupIntro : Popup
{
    public override void OnEscapeKey()
    {
        Application.Quit();
    }
}
