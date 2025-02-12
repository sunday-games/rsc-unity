using System.Linq;
using UnityEngine;
using Dropdown = TMPro.TMP_Dropdown;
using Slider = SG.UI.Slider;
using Button = SG.UI.Button;
using Toggle = SG.UI.Toggle;

namespace SG
{
    public class SettingsScreen : SG.UI.Screen
    {
        [Header("Sound")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        [Header("Graphics")]
        public Dropdown displaysDropdown;
        public Dropdown refreshRateDropdown;
        public Dropdown renderScaleDropdown;
        public Dropdown shadowsDropdown;
        public Dropdown antiAliasingDropdown;
        public Toggle fullScreenToggle;
        [Header("Other")]
        public Dropdown languageDropdown;
        public Toggle showRoofAlwaysToggle; // TODO
        public Toggle monitorSyncToggle;
        public Toggle analyticsCollectionToggle;
        public Button deletePlayerPrefsButton;

        protected SG.Settings _data => Configurator.Instance.Settings;

        public override void Setup()
        {
            base.Setup();

            languageDropdown?.ClearOptions();
            languageDropdown?.AddOptions(Localization.languages.Select(l => l.ToString()).ToList());
            languageDropdown?.onValueChanged.AddListener(value =>
            {
                Localization.SetLanguage(Localization.languages[value]);
            });

            refreshRateDropdown?.onValueChanged.AddListener(value =>
            {
                _data.refreshRate = _data.refreshRateValues[value];
                _data.ApplyAndSave();
            });

            displaysDropdown?.onValueChanged.AddListener(value =>
            {
                _data.display = value;
                _data.ApplyAndSave();
            });

            renderScaleDropdown?.onValueChanged.AddListener(value =>
            {
                _data.renderScale = _data.renderScaleValues[value];
                _data.ApplyAndSave();
            });

            shadowsDropdown?.onValueChanged.AddListener(value =>
            {
                _data.shadows = _data.shadowsValues[value];
                _data.ApplyAndSave();
            });

            antiAliasingDropdown?.onValueChanged.AddListener(value =>
            {
                _data.antiAliasing = _data.antiAliasingValues[value];
                _data.ApplyAndSave();
            });

            masterVolumeSlider?.onValueChanged.AddListener(value =>
            {
                _data.masterVolume = value;
                _data.ApplyAndSave();
            });
            musicVolumeSlider?.onValueChanged.AddListener(value =>
            {
                _data.musicVolume = value;
                _data.ApplyAndSave();
            });

            showRoofAlwaysToggle?.onValueChanged.AddListener(value =>
            {
                _data.showRoofAlways = value;
                _data.ApplyAndSave();
            });

            monitorSyncToggle?.onValueChanged.AddListener(value =>
            {
                _data.monitorSync = value;
                _data.ApplyAndSave();
            });

            analyticsCollectionToggle?.onValueChanged.AddListener(value =>
            {
                _data.analyticsCollection = value;
                _data.ApplyAndSave();
            });

            fullScreenToggle?.onValueChanged.AddListener(value =>
            {
#if UNITY_WEBGL
                Screen.fullScreen = !Screen.fullScreen;
#else
                _data.fullScreen = value;
                _data.ApplyAndSave();

                if (!_data.fullScreen)
                    Screen.SetResolution(Screen.width / 2, Screen.height / 2, fullscreen: false);
#endif
            });

            deletePlayerPrefsButton?.onClick.AddListener(Configurator.DeletePlayerPrefs);
        }

        public override void Open()
        {
            if (displaysDropdown != null && displaysDropdown.transform.parent.ActivateIf(Display.displays.Length > 1))
            {
                displaysDropdown.ClearOptions();
                for (int i = 0; i < Display.displays.Length; i++)
                    displaysDropdown.options.Add(
                        new Dropdown.OptionData($"{i + 1}: {Display.displays[i].systemWidth}x{Display.displays[i].systemHeight}"));

                displaysDropdown.SetValueWithoutNotify(_data.display);
                displaysDropdown.RefreshShownValue();
            }

            if (refreshRateDropdown != null)
            {
                Log.Info("targetFrameRate: " + Application.targetFrameRate);
                Log.Info("vSyncCount: " + QualitySettings.vSyncCount);
                Log.Info($"refreshRateRatio: {Screen.currentResolution.refreshRateRatio.numerator} / {Screen.currentResolution.refreshRateRatio.denominator} = {Screen.currentResolution.refreshRateRatio.value}");

                if (Screen.currentResolution.refreshRateRatio.numerator > 0)
                    for (int i = 0; i < refreshRateDropdown.options.Count - 1; i++)
                        refreshRateDropdown.options[i].text = Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value / _data.refreshRateValues[i]) + " FPS";
                else
                    for (int i = 0; i < refreshRateDropdown.options.Count - 1; i++)
                        refreshRateDropdown.options[i].text = _data.refreshRateDefaults[i] + " FPS";

                refreshRateDropdown.SetValueWithoutNotify(_data.refreshRateValues.IndexOf(_data.refreshRate));
                refreshRateDropdown.RefreshShownValue();
            }

            masterVolumeSlider?.SetValueWithoutNotify(_data.masterVolume);
            musicVolumeSlider?.SetValueWithoutNotify(_data.musicVolume);
            languageDropdown?.SetValueWithoutNotify(Localization.languageIndex);
            renderScaleDropdown?.SetValueWithoutNotify(_data.renderScaleValues.IndexOf(_data.renderScale));
            shadowsDropdown?.SetValueWithoutNotify(_data.shadowsValues.IndexOf(_data.shadows));
            antiAliasingDropdown?.SetValueWithoutNotify(_data.antiAliasingValues.IndexOf(_data.antiAliasing));
            showRoofAlwaysToggle?.SetValue(_data.showRoofAlways, invokeEvent: false);
            monitorSyncToggle?.SetValue(_data.monitorSync, invokeEvent: false);
            analyticsCollectionToggle?.SetValue(_data.analyticsCollection, invokeEvent: false);

#if UNITY_WEBGL
            fullScreenToggle?.SetValue(Screen.fullScreen, invokeEvent: false);
#else
            fullScreenToggle?.SetValue(_data.fullScreen, invokeEvent: false);
#endif
        }

        public virtual void Restart()
        {
            Configurator.DeletePlayerPrefs();
            UnityEngine.SceneManagement.SceneManager.LoadScene(Utils.ActiveSceneName);
        }
    }
}
