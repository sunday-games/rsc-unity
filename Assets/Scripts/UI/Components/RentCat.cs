using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class RentCat : Core
{
    public Transform catParent;
    public Text descriptionText;

    public CatItem catItem;
    CatItemView catView;

    public int rentCatNextSession
    {
        get { return ObscuredPrefs.GetInt("rentCatNextSession", balance.rentCat.frequency); }
        set { ObscuredPrefs.SetInt("rentCatNextSession", value); }
    }

    public bool isTimeToRentCat
    {
        get
        {
            return user.level > balance.rentCat.startLevel &&
                user.gameSessions > rentCatNextSession &&
                ui.prepare.freeCatSlot != null;
        }
    }

    public void Show(CatSlot slot)
    {
        if (!AddCat()) return;

        rentCatNextSession = user.gameSessions + balance.rentCat.frequency +
            rentCats[catItem.type] * (int)((float)balance.rentCat.frequency * 0.3f);

        descriptionText.text = Localization.Get("rentCat", catItem.type.localizedName, catItem.level);

        catView = Instantiate(catItem.type.itemViewPrefab);
        catView.transform.SetParent(catParent, false);
        catView.transform.localScale = 2 * Vector3.one;
        catView.footer.SetActive(false);

        gameObject.SetActive(true);

        Analytic.EventProperties("RentCat", catItem.type.name, "RENT");
    }

    public void Hide()
    {
        Destroy(catView.gameObject);

        gameObject.SetActive(false);
    }

    public bool AddCat()
    {
        if (rentCats == null) LoadRentCats();

        CatType catType = null;
        int min = int.MaxValue;
        foreach (var rentCat in rentCats)
            if (!user.isOwned(rentCat.Key) && min > rentCat.Value)
            {
                catType = rentCat.Key;
                min = rentCat.Value;
            }

        if (catType == null) return false;

        catItem = new CatItem(catType, Random.Range(5, 8), 0);

        // TODO Кажется можно просто вызвать ui.prepare.freeCatSlot
        for (int i = 0; i < ui.prepare.catSlots.Length; i++)
            if (ui.prepare.catSlots[i].gameObject.activeSelf && ui.prepare.catSlots[i].catItem == null)
            {
                catItem.isInstalled = i;
                ui.prepare.catSlots[i].Init(catItem);
                return true;
            }

        return false;
    }

    public void RemoveCat()
    {
        if (catItem == null) return;

        ++rentCats[catItem.type];
        SaveRentCats();

        ui.prepare.catSlots[catItem.isInstalled].Clear();
        catItem = null;
    }

    Dictionary<CatType, int> rentCats = null;
    void LoadRentCats()
    {
        var rentCatsDict = Json.Deserialize(ObscuredPrefs.GetString("rentCatsDict", "{}")) as Dictionary<string, object>;

        if (rentCatsDict.Count == 0)
            foreach (var cat in balance.rentCat.cats) rentCatsDict.Add(cat.ToString(), 0);

        rentCats = new Dictionary<CatType, int>();
        foreach (var pair in rentCatsDict)
            rentCats.Add(CatType.GetCatType(pair.Key), System.Convert.ToInt32(pair.Value));
    }
    void SaveRentCats()
    {
        if (rentCats == null) return;

        var dict = new Dictionary<string, object>();

        foreach (var rentCat in rentCats) dict.Add(rentCat.Key.name, rentCat.Value);

        ObscuredPrefs.SetString("rentCatsDict", Json.Serialize(dict));
    }
}
