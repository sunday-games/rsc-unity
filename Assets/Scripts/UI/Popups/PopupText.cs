using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupText : Popup
{
    public Text mainText;

    public override void Init()
    {
        mainText.text = "";
    }

    public override void Reset() { }

    public override void OnEscapeKey() { }
}