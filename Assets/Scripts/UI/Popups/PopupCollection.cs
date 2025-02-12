using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SG.RSC
{
    public class PopupCollection : Popup
    {
        public CatSlot bigCatSlot;
        public Text bigNameText;
        public Text bigLevelText;
        public Text bigCoinCost;
        public GameObject bigExpSliderBack;
        public Image bigExpSlider;
        public GameObject maximum;
        public Text bigDescriptionText;

        public GameObject collectionAdd;

        public GameObject catsBackground;
        public GameObject catsAddBackground;
        public GameObject catsGrid;

        public GameObject buyCatboxButton;

        public List<CatItemView> catViews = new List<CatItemView>();

        public override void Init()
        {
            bigCatSlot.catItem = null;

            foreach (var catView in catViews)
            {
                var catItem = user.GetItem(catView.catType);

                if (catItem != null)
                {
                    catView.Init(catItem, catItemView => { ShowBig(catItemView.catItem); });

                    catView.cat.SetActive(true);
                    catView.catEmpty.SetActive(false);
                    catView.footer.SetActive(true);

                    if (catView.particleController != null) catView.particleController.OFF();

                    if (bigCatSlot.catItem == null)
                    {
                        ShowBig(catItem);
                        if (bigCatSlot.itemView.particleController != null) bigCatSlot.itemView.particleController.OFF();
                    }
                }
                else
                {
                    catView.cat.SetActive(false);
                    catView.catEmpty.SetActive(true);
                    catView.footer.SetActive(false);

                    if (catView.catType == CatType.GetCatType(Cats.Santa))
                        catView.ActivateButton(item => { ui.tutorial.Show(Tutorial.Part.CollectionSanta, new Transform[] { item.catEmpty.transform }); });
                    else if (catView.catType == CatType.GetCatType(Cats.Lady))
                        catView.ActivateButton(item => { ui.tutorial.Show(Tutorial.Part.CollectionLady, new Transform[] { item.catEmpty.transform }); });
                    else if (catView.catType == CatType.GetCatType(Cats.Jack))
                        catView.ActivateButton(item => { ui.tutorial.Show(Tutorial.Part.CollectionJack, new Transform[] { item.catEmpty.transform }); });
                    else if (catView.catType == CatType.GetCatType(Cats.Mix))
                        catView.ActivateButton(item => { ui.tutorial.Show(Tutorial.Part.CollectionMix, new Transform[] { item.catEmpty.transform }); });
                }
            }

            buyCatboxButton.SetActive(!build.premium && user.isCanGetPremiumBox);
        }

        public override void AfterInit()
        {
            collectionAdd.SetActive(true);
            if (bigCatSlot.slotImage != null) bigCatSlot.slotImage.enabled = false;
            catsBackground.SetActive(false);

            if (isTNT && !user.IsTutorialShown(Tutorial.Part.Collection))
                ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Collection1, Tutorial.Part.Collection2, Tutorial.Part.Collection }, new Transform[] { catsAddBackground.transform, catsGrid.transform });
            if (!user.IsTutorialShown(Tutorial.Part.Collection))
                ui.tutorial.Show(Tutorial.Part.Collection, new Transform[] { catsAddBackground.transform, catsGrid.transform });
            else if (!user.IsTutorialShown(Tutorial.Part.BuyCatbox) && !build.premium)
            {
                if (isTNT) ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.BuyCatbox1, Tutorial.Part.BuyCatbox }, new Transform[] { buyCatboxButton.transform });
                else ui.tutorial.Show(Tutorial.Part.BuyCatbox, new Transform[] { buyCatboxButton.transform });
            }

            foreach (var catView in catViews)
                if (catView.particleController != null) catView.particleController.ON(false);

            if (bigCatSlot.itemView != null && bigCatSlot.itemView.particleController != null) bigCatSlot.itemView.particleController.ON(false);
        }

        public override void PreReset()
        {
            collectionAdd.SetActive(false);
            if (bigCatSlot.slotImage != null) bigCatSlot.slotImage.enabled = true;
            catsBackground.SetActive(true);

            foreach (var catView in catViews)
                if (catView.particleController != null) catView.particleController.OFF();

            if (bigCatSlot.itemView != null && bigCatSlot.itemView.particleController != null) bigCatSlot.itemView.particleController.OFF();
        }

        public void ShowBig(CatItem catItem)
        {
            if (catItem.type.onFreeFX != null)
                bigCatSlot.Init(catItem,
                    сatItemView => { Instantiate(сatItemView.catItem.type.onFreeFX, bigCatSlot.itemView.catButton.transform.position, Quaternion.identity); });
            else
                bigCatSlot.Init(catItem);
            bigNameText.text = catItem.type.localizedName;
            bigLevelText.text = catItem.level.ToString();
            if (catItem.isMaxLevel)
            {
                bigExpSliderBack.SetActive(false);
                maximum.SetActive(true);
            }
            else
            {
                maximum.SetActive(false);
                bigExpSliderBack.SetActive(true);
                bigExpSlider.fillAmount = (float)catItem.exp / (float)balance.catLevelsExp[catItem.level - 1];
            }
            bigDescriptionText.text = catItem.localizedDescription;
            bigCoinCost.text = catItem.cost.ToString();
        }
    }
}