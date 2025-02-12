using UnityEngine;
using UnityEngine.UI;
using System;

namespace SG.RSC
{
    public class CatSlot : Core
    {
        public Image slotImage;
        public Transform itemViewParent;
        public Vector3 scale = Vector3.one;

        public Image fullBack;
        public Image full;
        int _mana;
        public int mana
        {
            get { return _mana; }
            set
            {
                _mana = value;

                full.fillAmount = _mana / catItem.mana;

                if (_mana != 0) iTween.PunchScale(fullBack.gameObject, new Vector3(-0.2f, 0, 0), 0.5f);
            }
        }
        [HideInInspector]
        public CatItemView itemView;
        public CatItem catItem;

        public void Init(CatItem catItem, Action<CatItemView> action = null)
        {
            if (itemView != null) Clear();

            this.catItem = catItem;
            itemView = Instantiate(catItem.type.itemViewPrefab) as CatItemView;
            itemView.transform.SetParent(itemViewParent, false);
            itemView.transform.localScale = scale;
            if (action != null) itemView.Init(catItem, action);
            itemView.footer.SetActive(false);
        }

        public CatType type;
        public void AddMana(MonoBehaviour target)
        {
            if (catItem == null) return;

            if (!gameplay.isPlaying) return;

            int amount = 1;

            if (!catItem.isMaxLevel) catItem.expGame += amount;

            if (mana + amount >= catItem.mana)
            {
                if (catItem.type == CatType.GetCatType(Cats.Raiden))
                    Game.factory.CreateCatSuper(catItem, (int)catItem.power);
                else
                    Game.factory.CreateCatSuper(catItem);

                catItem.used++;
                mana = 0;
            }
            else
            {
                mana += amount;
            }
        }

        public void Clear()
        {
            if (catItem != null)
            {
                catItem.used = 0;
                catItem.expGame = 0;
                catItem = null;
            }

            if (itemView != null) Destroy(itemView.gameObject);

            _mana = 0;
            if (full != null) full.fillAmount = 0;
        }
    }
}