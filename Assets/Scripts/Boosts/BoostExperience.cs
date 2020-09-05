using UnityEngine;
using System.Collections;

public class BoostExperience : Boost
{
    void Start()
    {
        power = 2;
        tutorialPart = Tutorial.Part.BoostExperience;
        avalible = () => Missions.isBoostExperience;
    }
}