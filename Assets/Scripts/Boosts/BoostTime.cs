namespace SG.RSC
{
    public class BoostTime : Boost
    {
        void Start()
        {
            tutorialPart = Tutorial.Part.BoostTime;
            power = 10;
            avalible = () => Missions.isBoostTime;
        }
    }
}