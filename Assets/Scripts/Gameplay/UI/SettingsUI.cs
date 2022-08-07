using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class SettingsUI : MonoBehaviour
    {
        public GameObject buttons;
        public GameObject settingsMenu;
        
        public AudioMixer audioMixer;
        Resolution[] resolutions;
        public TMP_Dropdown resolutionDropdown;
        public Slider volumeSlider;
        public Toggle fullscreen;

        private GameModel model;
        
        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            GameSaveData data = model.saveGame.current;
            
            
            Screen.SetResolution(data.width, data.height, data.fullscreen);
            audioMixer.SetFloat("volume", data.volume);

            int currentResolutionIndex = 0;
            resolutions = Screen.resolutions;
            resolutions = resolutions.Reverse().ToArray();

            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.width &&
                    resolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            audioMixer.GetFloat("volume", out float vol);

            volumeSlider.SetValueWithoutNotify(vol);

            fullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
        }
        
        public void SetResolution(int resolutionIndex)
        {
            GameSaveData data = model.saveGame.current;
            Resolution resolution = resolutions[resolutionIndex];
            data.width = resolution.width;
            data.height = resolution.height;
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            model.saveGame.Save();
        }

        public void SetVolume(float volume)
        {
            GameSaveData data = model.saveGame.current;
            audioMixer.SetFloat("volume", volume);
            data.volume = volume;
            model.saveGame.Save();
        }
        


        public void SetFullscreen(bool isFullscreen)
        {
            GameSaveData data = model.saveGame.current;
            Screen.fullScreen = isFullscreen;
            data.fullscreen = isFullscreen;
            model.saveGame.Save();
        }

        public void OpenMenu()
        {
            settingsMenu.SetActive(true);
            buttons.SetActive(false);
        }

        public void CloseMenu()
        {
            settingsMenu.SetActive(false);
            buttons.SetActive(true);
        }
        
        public void OnExit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}