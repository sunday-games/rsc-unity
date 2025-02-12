using UnityEngine;

namespace SG.RSC
{
    public class CatType : Core
    {
        public static CatType GetCatType(Cats cat) { return GetCatType(cat.ToString()); }
        public static CatType GetCatType(string name)
        {
            foreach (CatType type in gameplay.basicCats)
                if (type.name == name) return type;

            foreach (CatType type in gameplay.superCats)
                if (type.name == name) return type;

            return null;
        }

        public string localizedName { get { return Localization.Get("nameCat" + name); } }

        public int id;

        [Space(10)]
        public Sprite spriteNormal;
        public Sprite spritePicked;

        [Space(6)]
        public Sprite spriteFish;
        public Sprite spriteFishPicked;

        [Space(6)]
        public Sprite spriteMultiplier;

        [Space(6)]
        public Sprite spriteHeart;
        public Sprite spriteHeartPicked;

        [Space(6)]
        public Sprite spriteBat;
        public Sprite spriteBatPicked;

        [Space(10)]
        public GameObject onFreeFX;
        public AudioClip onFreeFXSound;
        public BasicCatAnimation onFreeAnimation;
        public GameObject onEndFX;

        [Space(10)]
        public CatItemView itemViewPrefab;
        public Stuff gamePrefab;

        [Space(10)]
        public Vector3 scale = Vector3.one;
        public Color color = Color.white;
        public int baseCost = 0;
        public float catCostRise = 0.8f;
        public int baseMana = 0;
        public float catManaRise = 0.1f;

        [Space(10)]
        public bool isEventCat = false;

        [Space(10)]
        public float[] levelPower;
    }
}