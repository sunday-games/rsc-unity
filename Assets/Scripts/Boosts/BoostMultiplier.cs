using UnityEngine;
using System.Collections;

public class BoostMultiplier : Boost
{
    void Start()
    {
        tutorialPart = Tutorial.Part.BoostMultiplier;
        avalible = () => Missions.isBoostMultiplier;
    }
}