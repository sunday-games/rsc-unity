namespace SG.RSC
{
    public class BoostMultiplier : Boost
    {
        void Start()
        {
            tutorialPart = Tutorial.Part.BoostMultiplier;
            avalible = () => Missions.isBoostMultiplier;
        }
    }
}