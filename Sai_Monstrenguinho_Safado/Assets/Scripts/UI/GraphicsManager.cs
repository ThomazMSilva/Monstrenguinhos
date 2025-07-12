using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class GraphicsManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown antiAliasingDropdown;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Toggle vSyncToggle;

        private Resolution[] resolutions;
        private List<Resolution> filteredResolutions = new();

        private RefreshRate currentRefreshRate;
        private int currentResolutionIndex;

        void Start()
        {
            if (fullScreenToggle != null) fullScreenToggle.isOn = Screen.fullScreen;

            int antiAliasingIndex = 0;
            switch (QualitySettings.antiAliasing)
            {
                case 2: antiAliasingIndex = 1; break;
                case 4: antiAliasingIndex = 2; break;
                case 8: antiAliasingIndex = 3; break;
                default: break;
            }
            antiAliasingDropdown.value = antiAliasingIndex;

#if !UNITY_WEBGL
            if(vSyncToggle != null) vSyncToggle.isOn = QualitySettings.vSyncCount > 0;
            resolutionDropdown.ClearOptions();

            InitializeResolutionOptions();
#endif

        }

        private void InitializeResolutionOptions()
        {
            resolutions = Screen.resolutions;
            currentRefreshRate = Screen.currentResolution.refreshRateRatio;

            foreach (var resolution in resolutions)
            {
                if (resolution.refreshRateRatio.value == currentRefreshRate.value)
                {
                    filteredResolutions.Add(resolution);
                }
            }
            List<string> availableOptions = new();

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                Resolution filteredResolution = filteredResolutions[i];
                string option = $"{filteredResolution.width}x{filteredResolution.height}";
                availableOptions.Add(option);

                if (filteredResolution.width == Screen.width && filteredResolution.height == Screen.height)
                    currentResolutionIndex = i;
            }

            resolutionDropdown.AddOptions(availableOptions);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }


        public void SetFullscreen(bool fullscreenState)
        {
            Screen.fullScreen = fullscreenState;
//#if !UNITY_WEBGL
            if (fullscreenState)
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                //UnityEditor.PlayerSettings.resizableWindow = true;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                //UnityEditor.PlayerSettings.resizableWindow = true;
            }
//#endif
        }

        public void SetResolution(int resolutionIndex)
        {
#if !UNITY_WEBGL
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
#endif
        }
    
        public void SetAntialiasing(int value)
        {
            int antiAliasing = 0;
            switch (value)
            {
                case 1: antiAliasing = 2; break;
                case 2: antiAliasing = 4 ; break;
                case 3: antiAliasing = 8 ; break;
                default: break;
            }
            QualitySettings.antiAliasing = antiAliasing;
        }

        public void SetVSynchronization(bool active)
        {
            QualitySettings.vSyncCount = active ? 1 : 0;
        }

    }
}