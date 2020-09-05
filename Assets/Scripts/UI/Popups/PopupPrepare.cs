using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupPrepare : Popup
{
    [Space(10)]

    public MissionList missionList;

    [Space(10)]

    public GameObject placeholderImage;

    [Space(10)]

    public GameObject prepareAdd;
    public Image catItemsBackImageAdd;
    public GameObject[] catSlotsAdd;

    [Space(10)]

    public GameObject cats;
    public GameObject costImage;
    public Image catItemsBackImage;
    public GridLayoutGroup catItemGrid;
    public CatSlot[] catSlots;

    [Space(10)]

    public GameObject costCoin;
    public Text costCoinText;

    [Space(10)]

    public GameObject backButton;

    [Space(10)]

    public GameObject levelUpButton;

    List<CatItemView> catItemList = new List<CatItemView>();

    public int cost
    {
        get
        {
            int _cost = 0;
            foreach (CatItem catItem in user.collection)
                if (catItem.isInstalled > -1) _cost += catItem.cost;
            return _cost;
        }
    }

    public CatSlot freeCatSlot
    {
        get
        {
            for (int i = 0; i < Missions.maxCatSlot; i++)
                if (catSlots[i].catItem == null) return catSlots[i];

            return null;
        }
    }

    public override void Init()
    {
        previous = ui.main;

        backButton.SetActive(Missions.isChampionship);

        levelUpButton.SetActive(build.cheats);

        if (Missions.maxCatSlot > 0)
        {
            placeholderImage.SetActive(false);
            cats.SetActive(true);

            foreach (CatSlot slot in catSlots) slot.Clear();

            for (int i = 0; i < catSlots.Length; i++) catSlots[i].gameObject.SetActive(i < Missions.maxCatSlot);

            foreach (CatItem catItem in user.collection)
                if (catItem.isInstalled > -1) Install(catItem);
                else AddToList(catItem);
        }
        else
        {
            cats.SetActive(false);
            placeholderImage.SetActive(true);
        }

        costCoinText.text = cost.ToString();
    }

    public override void OnEscapeKey()
    {
        if (Missions.isChampionship) ui.PopupClose();
        else Application.Quit();
    }

    public override void AfterInit()
    {
        user.blockSave = false;

        if (isTNT && !user.IsTutorialShown(Tutorial.Part.Missions))
        {
            ui.tutorial.Show(new List<Tutorial.Part>() { Tutorial.Part.Missions1, Tutorial.Part.Missions2, Tutorial.Part.Missions3, Tutorial.Part.Missions4, Tutorial.Part.Missions5, Tutorial.Part.Missions6, Tutorial.Part.Missions7, Tutorial.Part.Missions8 });
            user.TutorialShown(Tutorial.Part.Missions);
        }
        else
        {
            if (!user.IsTutorialShown(Tutorial.Part.Missions)) missionList.TutorialMissions();
        }

        if (Missions.maxCatSlot > 0)
        {
            prepareAdd.SetActive(true);

            foreach (CatSlot slot in catSlots) slot.slotImage.enabled = false;

            for (int i = 0; i < catSlots.Length; i++) catSlotsAdd[i].SetActive(catSlots[i].gameObject.activeSelf);

            catItemsBackImage.enabled = false;
            catItemsBackImageAdd.enabled = true;
            if (costImage != null) costImage.SetActive(false);

            if (user.level == Missions.BOX_UNLOCK_LEVELS[0] && catItemList.Count > 0 && catSlots[0].catItem == null && user.useCats == 0)
                ui.tutorial.Show(Tutorial.Part.CatBox, new Transform[] { catSlotsAdd[0].transform, catItemList[0].transform });
        }

        if (user.isLevelOK)
        {
            missionList.UpdateCheckboxes();

            missionList.TutorialBuySkipMission();

            if (Missions.LEVELS[user.level].isDone) StartCoroutine(LevelUp());
        }

        foreach (CatItemView catItemView in catItemList)
            if (catItemView.particleController != null) catItemView.particleController.ON(true);

        if (0 < Missions.maxCatSlot) ui.header.adLabel.TryShow();
    }

    IEnumerator LevelUp()
    {
        ui.Block();
        yield return new WaitForSeconds(0.7f);
        ui.Unblock();

        user.LevelUp();
        ui.PopupShow(ui.levelUp);
    }
    public override void PreReset()
    {
        ui.header.adLabel.Hide();

        if (Missions.maxCatSlot > 0)
        {
            foreach (CatSlot slot in catSlots) slot.slotImage.enabled = true;
            for (int i = 0; i < catSlots.Length; i++) catSlotsAdd[i].SetActive(false);

            catItemsBackImage.enabled = true;
            catItemsBackImageAdd.enabled = false;
            if (costImage != null) costImage.SetActive(true);

            prepareAdd.SetActive(false);
        }

        foreach (CatItemView catItemView in catItemList)
            if (catItemView.particleController != null) catItemView.particleController.OFF();
    }
    public override void Reset()
    {
        foreach (CatItemView catItemUI in catItemList) Destroy(catItemUI.gameObject);
        catItemList.Clear();
    }

    public void TutorialPrepareBox()
    {
        ui.tutorial.Show(Tutorial.Part.PrepareBox, new Transform[] { placeholderImage.transform }, Missions.BOX_UNLOCK_LEVELS[0].ToString());
    }

    Vector3 catListScale = new Vector3(0.9f, 0.9f, 1f);
    public void AddToList(CatItem catItem)
    {
        CatItemView itemView = Instantiate(catItem.type.itemViewPrefab) as CatItemView;
        itemView.transform.localScale = catListScale;
        itemView.transform.SetParent(catItemGrid.transform, false);
        itemView.Init(catItem, catItemView => { Install(catItemView.catItem, showCoins: true); });
        catItemList.Add(itemView);

        if (itemView.particleController != null) itemView.particleController.OFF();

        (catItemGrid.transform as RectTransform).sizeDelta = new Vector2(
            catItemList.Count * (catItemGrid.cellSize.x + catItemGrid.spacing.x) + catItemGrid.spacing.x * 2,
            (catItemGrid.transform as RectTransform).sizeDelta.y);
    }
    void RemoveFromList(CatItem catItem)
    {
        foreach (CatItemView catItemView in catItemList)
            if (catItemView.catItem == catItem)
            {
                catItemList.Remove(catItemView);
                Destroy(catItemView.gameObject);
                break;
            }

        (catItemGrid.transform as RectTransform).sizeDelta = new Vector2(
            catItemList.Count * (catItemGrid.cellSize.x + catItemGrid.spacing.x) + catItemGrid.spacing.x * 2,
            (catItemGrid.transform as RectTransform).sizeDelta.y);
    }

    public void Install(CatItem catItem, bool showCoins = false)
    {
        foreach (CatSlot slot in catSlots)
            if (slot.catItem == catItem) return;

        if (catItem.isInstalled < 0)
            for (int i = 0; i < catSlots.Length; i++)
                if (catSlots[i].gameObject.activeSelf && catSlots[i].catItem == null)
                {
                    catItem.isInstalled = i;
                    break;
                }

        if (catItem.isInstalled < 0) return;

        catSlots[catItem.isInstalled].Init(catItem, catItemView => { Uninstall(catItemView.catItem); });

        RemoveFromList(catItem);

        if (!user.IsTutorialShown(Tutorial.Part.CatGoldfishes))
            ui.tutorial.Show(Tutorial.Part.CatGoldfishes, new Transform[] { catSlots[catItem.isInstalled].transform, costCoin.transform });

        costCoinText.text = cost.ToString();
        iTween.PunchScale(costCoin.gameObject, new Vector3(0.3f, 0.5f, 0), 1);

        if (showCoins) ui.header.ShowCoinsOut(catSlots[catItem.isInstalled].itemViewParent, 8, missionList.transform, scale: 0.8f);
    }

    public void Uninstall(CatItem catItem)
    {
        CatSlot slot = catSlots[catItem.isInstalled];
        slot.Clear();
        catItem.isInstalled = -1;

        costCoinText.text = cost.ToString();
        iTween.PunchScale(costCoin.gameObject, new Vector3(-0.3f, -0.5f, 0), 1);

        ui.header.ShowCoinsIn(slot.itemViewParent.position, 8, missionList.transform, scale: 0.8f);

        AddToList(catItem);
    }

    public void StartGame()
    {
        if (gameplay.isPlaying) return;

        if (user.coins < cost && !build.premium)
        {
            ui.notEnoughGoldfishes.mode = PopupNotEnoughGoldfishes.Mode.Cats;
            ui.PopupShow(ui.notEnoughGoldfishes);
        }
        else if (user.coins < cost && build.premium)
        {
            iTween.Stop(ui.header.coinsText.gameObject);
            ui.header.coinsText.transform.localScale = Vector3.one;
            iTween.PunchScale(ui.header.coinsText.gameObject, Vector3.one, 1f);
        }
        else if (Missions.isBoosts)
        {
            ui.PopupShow(ui.boosts);
        }
        else
        {
            user.UpdateCoins(-cost, false);

            gameplay.Play();
        }
    }
}
