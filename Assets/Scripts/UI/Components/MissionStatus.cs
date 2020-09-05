using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MissionStatus : Core
{
    public Text descriptionText;

    public GameObject check;

    public RectTransform progress;
    float progressMAX = 485;

    public Text progressText;

    Mission currentMission = null;

    void OnEnable()
    {
        currentMission = null;
        (transform as RectTransform).anchoredPosition = Vector2.zero;

        foreach (Mission mission in Missions.LEVELS[user.level].missions)
        {
            mission.status = new bool[5];
            float p = (float)mission.current() / (float)mission.target;
            for (int i = 0; i < mission.status.Length; i++)
                mission.status[i] = mission.atOneGame && !mission.isDone ? false : p - (i + 1) * 0.2f > -0.01f;
        }
    }

    void Update()
    {
        if (currentMission != null) UpdateProgress();
        else
            foreach (Mission mission in Missions.LEVELS[user.level].missions)
            {
                float p = (float)mission.current() / (float)mission.target;

                bool needShow = false;
                for (int i = 0; i < mission.status.Length; i++)
                    if (!mission.status[i] && p - (i + 1) * 0.2f > -0.01f)
                    {
                        needShow = true;
                        mission.status[i] = true;

                        // Костылек, чтобы не показывать мультиплаерные миссии лишний раз
                        if (mission is GetMultiplier && mission.current() == 1) needShow = false;
                    }

                if (needShow)
                {
                    StartCoroutine(Show(mission));
                    return;
                }
            }
    }

    void UpdateProgress()
    {
        check.SetActive(currentMission.isDone);
        progressText.text = currentMission.current().SpaceFormat() + " / " + currentMission.target.SpaceFormat();
        progress.sizeDelta = new Vector2(Mathf.Clamp((float)currentMission.current() / (float)currentMission.target, 0f, 1f) * progressMAX, progress.sizeDelta.y);
    }

    IEnumerator Show(Mission mission)
    {
        currentMission = mission;
        descriptionText.text = currentMission.description;
        UpdateProgress();

        (transform as RectTransform).anchoredPosition = Vector2.zero;
        iTween.MoveTo(gameObject, iTween.Hash("y", -7.5f, "easeType", "easeOutBack", "time", 0.8f));

        yield return new WaitForSeconds(4f);

        iTween.MoveTo(gameObject, iTween.Hash("y", -10f, "easeType", "easeInBack", "time", 0.8f));

        yield return new WaitForSeconds(1.2f);

        currentMission = null;
    }
}
