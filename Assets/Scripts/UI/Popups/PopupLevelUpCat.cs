using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class PopupLevelUpCat : Popup
    {
        public Text levelUpCatText;
        public CatSlot catSlot;
        public GameObject levelImage;
        public Text levelText;

        public override void Init()
        {
            levelUpCatText.text = Localization.Get("levelUpCat", user.lastGetCat.type.localizedName);
            catSlot.Init(user.lastGetCat);
            levelText.text = (user.lastGetCat.level - 1).ToString();
            StartCoroutine(LevelUp());
        }
        public override void Reset()
        {
            catSlot.Clear();
        }

        IEnumerator LevelUp()
        {
            iTween.RotateAdd(levelImage.gameObject, iTween.Hash("y", 360, "easeType", "easeOutBack", "time", 1f));

            yield return new WaitForSeconds(0.5f);

            levelText.text = (user.lastGetCat.level).ToString();

            iTween.PunchScale(levelImage.gameObject, Vector3.one, 1f);
        }
    }
}