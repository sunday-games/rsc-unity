using UnityEngine;

namespace SG.RSC
{
    public class CatItem
    {
        public CatType type;
        public int level = 1;
        public int exp = 0;
        public int expGame = 0;
        public int used = 0;

        public bool isMaxLevel => level >= type.levelPower.Length;
        public int cost => (int)((type.baseCost + type.catCostRise * type.baseCost * (level - 1)) * Core.achievements.catSale);
        public float mana => (type.baseMana + used * type.baseMana * 0.3f) * Core.achievements.easyGetCat;
        public float power => type.levelPower[level - 1];

        [HideInInspector]
        public int isInstalled = -1;

        public CatItem(CatType type, int level, int exp)
        {
            this.type = type;
            this.level = level;
            this.exp = exp;
        }
        public CatItem(string data)
        {
            var splited = data.Split(new char[] { ':' });

            if (splited.Length != 3) return;

            this.type = CatType.GetCatType(splited[0]);
            this.level = int.Parse(splited[1]);
            this.exp = int.Parse(splited[2]);
        }

        public override string ToString() { return type.name + ":" + level + ":" + exp; }

        public string localizedDescription
        {
            get
            {
                if (type.levelPower.Length > 0) return Localization.Get("powerCat" + type.name, type.levelPower[level - 1]);
                else return Localization.Get("powerCat" + type.name);
            }
        }

        public void LevelUp(bool resetExp = true)
        {
            if (isMaxLevel)
            {
                Debug.LogError(type.name + " - max " + level + " level reached!");
                return;
            }

            level++;
            exp = resetExp ? 0 : (int)((float)exp * (float)Game.balance.catLevelsExp[level - 1] / (float)Game.balance.catLevelsExp[level - 2]);

            Debug.Log(type.name + " get level up to " + level + " level");

            Core.achievements.OnCatLevelUp();
        }
    }
}