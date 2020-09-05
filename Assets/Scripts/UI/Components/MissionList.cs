using UnityEngine;
using System;

using CodeStage.AntiCheat.ObscuredTypes;

public class MissionList : Core
{
    public MissionView[] views;
    public GameObject maxLevel;
    public GameObject viewsParent;
    public bool showSkipButton = false;
    public GameObject skipButton;

    int sessionWhenAlmostLevelUp
    {
        get { return ObscuredPrefs.GetInt("sessionWhenAlmostLevelUp", -1); }
        set { ObscuredPrefs.SetInt("sessionWhenAlmostLevelUp", value); }
    }

    public static DateTime lastSkipMission
    {
        get { return new DateTime(ObscuredPrefs.GetLong("lastSkipMission", DateTime.Now.Ticks)); }
        set { ObscuredPrefs.SetLong("lastSkipMission", value.Ticks); }
    }

    void OnEnable()
    {
        if (!user.isLevelOK) // Достигли максимального уровня
        {
            if (viewsParent != null) viewsParent.SetActive(false);
            if (maxLevel != null) maxLevel.SetActive(true);
            return;
        }

        if (viewsParent != null) viewsParent.SetActive(true);
        if (maxLevel != null) maxLevel.SetActive(false);

        ui.header.UpdateLevel();

        int missionsDone = 0;
        for (int i = 0; i < 3; i++)
        {
            views[i].Setup(Missions.LEVELS[user.level].missions[i]);
            if (views[i].mission.isDone) ++missionsDone;
        }

        if (!showSkipButton) return;

        if (missionsDone != 2)
        {
            sessionWhenAlmostLevelUp = -1;
            skipButton.SetActive(false);
            return;
        }

        if (sessionWhenAlmostLevelUp < 0)
        {
            sessionWhenAlmostLevelUp = user.gameSessions;
        }
        else if (user.gameSessions - sessionWhenAlmostLevelUp > 2 &&
            iapManager.isInitialized && user.IsTutorialShown(Tutorial.Part.BuySkipMission))
        {
            if (!build.premium || (DateTime.Now - lastSkipMission).TotalDays > 3)
                skipButton.SetActive(true);
        }
    }

    void OnDisable()
    {
        foreach (var view in views)
        {
            iTween.Stop(view.checkImage.gameObject);
            view.checkImage.transform.localScale = Vector3.one;
        }
    }

    public void UpdateCheckboxes()
    {
        if (user.isLevelOK)
        {
            for (int i = 0; i < 3; i++)
                if (!views[i].checkImage.gameObject.activeSelf && Missions.LEVELS[user.level].missions[i].isDone)
                {
                    views[i].checkImage.gameObject.SetActive(true);
                    iTween.PunchScale(views[i].checkImage.gameObject, new Vector3(2f, 2f, 0), 1);
                }
        }
    }

    public void ShowSkipMission()
    {
        skipButton.SetActive(false);
        ui.tutorial.Show(Tutorial.Part.BuySkipMission, new Transform[] { transform });
    }

    public void TutorialBuySkipMission()
    {
        if (sessionWhenAlmostLevelUp > 0 && user.gameSessions - sessionWhenAlmostLevelUp > 2 &&
        iapManager.isInitialized && !user.IsTutorialShown(Tutorial.Part.BuySkipMission))
            ui.tutorial.Show(Tutorial.Part.BuySkipMission, new Transform[] { transform });
    }

    public void TutorialMissions()
    {
        if (user.isLevelOK) ui.tutorial.Show(Tutorial.Part.Missions, new Transform[] { transform });
    }
}
