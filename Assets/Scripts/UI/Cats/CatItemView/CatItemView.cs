using UnityEngine;
using UnityEngine.UI;
using System;

namespace SG.RSC
{
    public class CatItemView : MonoBehaviour
    {
        public GameObject footer;
        public Text nameText;
        public Text levelText;
        public CatItem catItem;
        public Button catButton;
        public Image catImage;

        public ParticleVisibleController particleController;

        // Костылек для реализации статичного создания в коллекции
        public CatType catType;
        public GameObject cat;
        public GameObject catEmpty;

        Action<CatItemView> action;

        public void Init(CatItem catItem, Action<CatItemView> action)
        {
            this.catItem = catItem;

            nameText.text = catItem.type.localizedName;
            levelText.text = catItem.level.ToString();

            ActivateButton(action);
        }

        public void ActivateButton(Action<CatItemView> action)
        {
            this.action = action;

            catButton.interactable = true;
        }

        public void Activate()
        {
            action?.Invoke(this);
        }
    }
}