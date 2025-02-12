using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SG.RSC
{
    public class CatPromote : Core
    {
        public CatSlot catSlot;

        public Text nameText;

        public Image levelImage;
        public Text levelText;
        public Image expSlider;

        [HideInInspector]
        public bool isDone = false;

        public void Init(CatItem catItem)
        {
            catItem.expGame = (int)(catItem.expGame * achievements.moreExpCat * balance.expMultiplier);
            if (gameplay.boosts.experience.ON) catItem.expGame *= gameplay.boosts.experience.power;

            catSlot.Init(catItem);

            nameText.text = catItem.type.localizedName;
            levelText.text = catItem.level.ToString();
            StartCoroutine(ShowGettingExp(catItem.level, catItem.type.levelPower.Length, catItem.exp, catItem.expGame));

            GetExp(catItem);
        }

        IEnumerator ShowGettingExp(int level, int levelMAX, int exp, int expGame)
        {
            while (expGame > 0)
            {
                exp++;
                expGame--;

                if (exp >= balance.catLevelsExp[level - 1] && level < levelMAX)
                {
                    level++;
                    exp = 0;

                    var fx = Instantiate(factory.catLevelUpFXPrefab) as GameObject;
                    fx.transform.SetParent(catSlot.itemViewParent, false);
                    Invoke("PunchCat", 0.3f);
                    iTween.PunchScale(levelImage.gameObject, Vector3.one, 1);
                    levelText.text = level.ToString();

                    if (!user.IsTutorialShown(Tutorial.Part.CatLevelUp))
                        ui.tutorial.Show(Tutorial.Part.CatLevelUp, new Transform[] { ui.promoteCats.addWindow, ui.promoteCats.window });
                }
                expSlider.fillAmount = (float)exp / (float)balance.catLevelsExp[level - 1];

                yield return new WaitForEndOfFrame();
            }

            isDone = true;
        }

        public void PunchCat() { iTween.PunchScale(catSlot.itemView.gameObject, Vector3.one, 1); }

        void GetExp(CatItem catItem)
        {
            Debug.Log(catItem.type.name + " Cat get " + catItem.expGame + " exp");

            while (catItem.expGame > 0)
            {
                catItem.exp++;
                catItem.expGame--;

                if (catItem.exp >= Game.balance.catLevelsExp[catItem.level - 1]) catItem.LevelUp();
            }
        }
    }
}