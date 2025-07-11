using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ResolutionManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullScreenToggle;

        private Resolution[] resolutions;
        private List<Resolution> filteredResolutions = new();

        private RefreshRate currentRefreshRate;
        private int currentResolutionIndex;

        void Start()
        {
            fullScreenToggle.isOn = Screen.fullScreen;

            resolutionDropdown.ClearOptions();

            InitializeResolutionOptions();
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
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
}