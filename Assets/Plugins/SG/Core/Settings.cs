using UnityEngine;
using System;

namespace SG
{
    [CreateAssetMenu(menuName = "Sunday/Settings")]
    public class Settings : ScriptableObject
    {
        public static bool GoodMode = false;
        public static bool CinemaMode = false;
        
        public float masterVolume = 0.5f;
        public float musicVolume = 0.5f;

        public int display = 0;

        public int refreshRate = 1;
        [NonSerialized] public int[] refreshRateDefaults = new int[] { 20, 25, 30, 60, -1 };
        [NonSerialized] public int[] refreshRateValues = new int[] { 4, 3, 2, 1, 0 };

        public float renderScale = 1f;
        [NonSerialized] public float[] renderScaleValues = new float[] { 0.5f, 0.7f, 1f, 1.5f, 2f };

        public int shadows = 0;
        [NonSerialized] public int[] shadowsValues = new int[] { 0, 600 }; // TODO: 600 is shadowDistance

        public int antiAliasing = 4;
        [NonSerialized] public int[] antiAliasingValues = new int[] { 0, 2, 4, 8 };

        public bool showRoofAlways = false;
        public bool monitorSync = false;
        public bool analyticsCollection = true;
        public bool fullScreen = true;

        public void ApplyAndSave()
        {
            Apply();
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetString("settings", JsonUtility.ToJson(this));
        }

        public bool Load()
        {
            if (PlayerPrefs.HasKey("settings"))
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("settings"), this);
                Apply();
                return true;
            }

            return false;
        }

        public virtual void Apply()
        {
            AudioListener.pause = masterVolume <= 0f;
            AudioListener.volume = masterVolume;

            FindAnyObjectByType<Music>()?.SetVolume(musicVolume);

#if UNITY_STANDALONE && !UNITY_EDITOR
            QualitySettings.vSyncCount = refreshRate;
            Log.Info("vSyncCount: " + QualitySettings.vSyncCount);
#else
            if (refreshRate == 0)
                Application.targetFrameRate = -1;
            else if (Screen.currentResolution.refreshRateRatio.numerator > 0)
                Application.targetFrameRate = Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value / refreshRate);
            else
                Application.targetFrameRate = refreshRateDefaults[refreshRateValues.IndexOf(refreshRate)];
            Log.Info("targetFrameRate: " + Application.targetFrameRate);
#endif

            SG.Analytics.AnalyticsManager.SetDataCollection(analyticsCollection);

#if UNITY_WEBGL
#else
            if (fullScreen)
                Screen.SetResolution(Display.displays[display].systemWidth, Display.displays[display].systemHeight, fullscreen: true);
            else
                Screen.fullScreen = false;
#endif

            if (Configurator.Instance.renderPipeline == RenderPipeline.Universal)
            {
#if URP
                if (QualitySettings.renderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
                {
                    // Workaround because there are no urp.supportsMainLightShadows setter
                    urp.shadowDistance = shadows;

                    // Workaround because Disabled is 1 in URP
                    urp.msaaSampleCount = antiAliasing == 0 ? 1 : antiAliasing;

                    urp.renderScale = renderScale;
                }
#endif
            }
            else
            {
                QualitySettings.shadows = shadows == 0 ? ShadowQuality.Disable : ShadowQuality.All;

                QualitySettings.antiAliasing = antiAliasing;

                // QualitySettings.renderScale = renderScale;
            }
        }
    }
}