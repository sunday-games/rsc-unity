using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour
{
    public static UnityEvent OnResolutionOrOrientationChanged = new UnityEvent();

    static List<CanvasHelper> helpers = new List<CanvasHelper>();
    static bool screenChangeVarsInitialized = false;
    static ScreenOrientation lastOrientation = ScreenOrientation.Landscape;
    static Vector2 lastResolution = Vector2.zero;
    static Rect lastSafeArea = Rect.zero;

    public RectTransform safeAreaTransform;

    Canvas canvas;
    RectTransform rectTransform;

    void Awake()
    {
        if (!helpers.Contains(this))
            helpers.Add(this);

        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        if (!screenChangeVarsInitialized)
        {
            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
            lastSafeArea = Screen.safeArea;

            screenChangeVarsInitialized = true;
        }

        ApplySafeArea();
    }

    //void Update()
    //{
    //    if (helpers[0] != this)
    //        return;

    //    if (Application.isMobilePlatform && Screen.orientation != lastOrientation)
    //        OrientationChanged();

    //    if (Screen.safeArea != lastSafeArea)
    //        SafeAreaChanged();

    //    if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
    //        ResolutionChanged();
    //}

    void ApplySafeArea()
    {
        if (safeAreaTransform == null)
            return;

        var safeArea = Screen.safeArea;

        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        safeAreaTransform.anchorMin = anchorMin;
        safeAreaTransform.anchorMax = anchorMax;
    }

    void OnDestroy()
    {
        if (helpers != null && helpers.Contains(this))
            helpers.Remove(this);
    }

    static void OrientationChanged()
    {
        //Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

        lastOrientation = Screen.orientation;
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    static void ResolutionChanged()
    {
        //Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    static void SafeAreaChanged()
    {
        // Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

        lastSafeArea = Screen.safeArea;

        for (int i = 0; i < helpers.Count; i++)
        {
            helpers[i].ApplySafeArea();
        }
    }
}