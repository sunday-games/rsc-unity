using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BasicCatRikiImage : Core
{
    public GameObject idle;
    public GameObject candy;
    public GameObject multiplier;
    public Text multiplierText;

    public void Hide()
    {
        gameObject.SetActive(false);

        idle.SetActive(false);
        candy.SetActive(false);
        multiplier.SetActive(false);
        multiplierText.gameObject.SetActive(false);
    }
}
