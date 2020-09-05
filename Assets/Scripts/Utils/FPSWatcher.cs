using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSWatcher : Core
{
    public Text fpsText;
    public float updateInterval = 0.5f;

    float accum = 0; // FPS accumulated over the interval
    int frames = 0; // Frames drawn over the interval
    float timeleft; // Left time for current interval

    void Start()
    {
        fpsText.gameObject.SetActive(true);
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            float fps = accum / frames;

            if (fps < 20) fpsText.color = Color.red;
            else if (fps < 30) fpsText.color = Color.yellow;
            else fpsText.color = Color.green;

            fpsText.text = ((int)fps).ToString();

            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }
}