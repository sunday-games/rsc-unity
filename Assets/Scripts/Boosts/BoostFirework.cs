using UnityEngine;
using System.Collections;

public class BoostFirework : Boost
{
    void Start()
    {
        tutorialPart = Tutorial.Part.BoostFirework;
        avalible = () => Missions.isBoostFirework;
    }
}