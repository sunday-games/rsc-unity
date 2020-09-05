using UnityEngine;
using System;
using System.Collections;

public class Boost : Core
{
    public Sprite sprite;

    public int price = 0;

    public int power = 0;

    [HideInInspector]
    public int count = 0;

    [HideInInspector]
    public bool ON = false;

    [HideInInspector]
    public Tutorial.Part tutorialPart;

    public Func<bool> avalible = null;
}
