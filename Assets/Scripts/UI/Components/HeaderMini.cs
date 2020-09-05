using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeaderMini : Core
{
    public GameObject cats;
    public Text catsText;

    public GameObject coins;
    public Text coinsText;

    public GameObject spins;
    public Text spinsText;

    public Vector2 positionNormal = Vector2.zero;
    public Vector2 positionNewYear = new Vector2(80f, 0f);

    void OnEnable()
    {
        (transform as RectTransform).anchoredPosition = Events.newYear.isActive ? positionNewYear : positionNormal;

        coins.SetActive(Missions.isGoldfishes);
        coinsText.text = user.coins.SpaceFormat();

        cats.SetActive(user.collection.Count > 0);
        catsText.text = user.collection.Count + "/" + gameplay.superCats.Length;

        spins.SetActive(Missions.isLuckyWheel);
        spinsText.text = user.TotalSpins(System.DateTime.UtcNow).ToString();
    }
}
