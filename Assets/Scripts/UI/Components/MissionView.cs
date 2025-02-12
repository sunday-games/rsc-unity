using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class MissionView : Core
    {
        public Text descriptionText;
        public Vector2 descriptionTextFullSize;
        public Vector2 descriptionTextWithHelpButton;

        [Space(10)]
        public Image checkImage;

        [Space(10)]
        public GameObject helpButton;

        public Mission mission;

        public void Setup(Mission mission)
        {
            this.mission = mission;

            descriptionText.text = mission.description;
            if (mission.current() != 0) descriptionText.text += " (" + mission.current().SpaceFormat() + ")";

            if (!mission.isDone) checkImage.gameObject.SetActive(false);

            if (helpButton != null && mission.tipTutorial != null && !mission.isDone)
            {
                descriptionText.rectTransform.sizeDelta = descriptionTextWithHelpButton;
                if (helpButton != null) helpButton.SetActive(true);
            }
            else
            {
                descriptionText.rectTransform.sizeDelta = descriptionTextFullSize;
                if (helpButton != null) helpButton.SetActive(false);
            }
        }

        public void Help()
        {
            ui.tutorial.Show(mission.tipTutorial, param: mission.target.SpaceFormat());
        }
    }
}