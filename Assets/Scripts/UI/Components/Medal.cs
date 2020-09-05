using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Medal : Core
{
    public static Medal Create(Transform parent, Achievements.Achievement achievement)
    {
        Medal medal = Instantiate(factory.medalPrefab) as Medal;
        medal.transform.SetParent(parent, false);

        medal.achievement = achievement;
        medal.iconImage.sprite = achievement.icon;
        medal.mainImage.sprite = factory.medalSprites[achievement.rank];
        if (medal.iconOutline != null) medal.iconOutline.effectColor = factory.medalColors[achievement.rank];

        if (medal.hideImage != null)
        {
            medal.hideImage.sprite = medal.mainImage.sprite;
            medal.hideImage.fillAmount = 1f - medal.achievement.progress;
        }

        return medal;
    }

    public GameObject lightImage;
    public Image mainImage;
    public Image hideImage;
    public Image iconImage;
    public Outline iconOutline;

    public Achievements.Achievement achievement;

    static Vector3 scale = new Vector3(1.1f, 1.1f, 1f);
    public void Down()
    {
        transform.localScale = scale;
        lightImage.SetActive(true);

        ui.profile.BubbleShow(this);
    }

    public void Up()
    {
        transform.localScale = Vector3.one;
        lightImage.SetActive(false);

        ui.profile.BubbleHide();
    }
}
