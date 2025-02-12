using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupProgress : Popup
    {
        public Text ProgressText;

        public Text thisLevelText;
        public Text thisCoinsText;
        public Text thisCollectionText;
        public Text thisRecordText;

        public Text otherLevelText;
        public Text otherCoinsText;
        public Text otherCollectionText;
        public Text otherRecordText;

        public override void Init()
        {
            ui.LoadingHide();

            ProgressText.text = Localization.Get("progressDescription" + user.gender);
            isChoice = false;
        }

        public void Setup(int otherLevel, int otherCoins, int otherCollection, int otherRecord)
        {
            thisLevelText.text = Localization.Get("progressLevel", user.level);
            thisCoinsText.text = Localization.Get("progressGoldfishes", user.coins.SpaceFormat());
            thisCollectionText.text = Localization.Get("progressCollection", user.collection.Count, gameplay.superCats.Length);
            thisRecordText.text = Localization.Get("progressRecord", user.permanentRecord.SpaceFormat());

            otherLevelText.text = Localization.Get("progressLevel", otherLevel);
            otherCoinsText.text = Localization.Get("progressGoldfishes", otherCoins.SpaceFormat());
            otherCollectionText.text = Localization.Get("progressCollection", otherCollection, gameplay.superCats.Length);
            otherRecordText.text = Localization.Get("progressRecord", otherRecord.SpaceFormat());
        }
        public override void OnEscapeKey() { }

        bool isChoice = false;
        public void Apply()
        {
            if (isChoice) return;
            isChoice = true;

            //#if PARSE
            //        user.ParseToLocal();
            //#endif
        }

        public void Ignore()
        {
            if (isChoice) return;
            isChoice = true;

            //#if PARSE
            //        user.LocalToParse();
            //#endif
        }
    }
}