using UnityEngine;
using System.Collections;

public class BoostTime : Boost
{
    void Start()
    {
        tutorialPart = Tutorial.Part.BoostTime;
        power = 10;
        avalible = () => Missions.isBoostTime;
    }
}