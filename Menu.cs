using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public Text bestScore;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    int activeScreenResIndex;


    public void Start()
    {
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullscreen = (PlayerPrefs.GetInt("fullscreen") == 1)?true:false;
        volumeSliders [0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders [1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders [2].value = AudioManager.instance.sfxVolumePercent;

        for (int i=0; i<resolutionToggles.Length; i++)
        {
            resolutionToggles [i].isOn = i == activeScreenResIndex;
        }
        fullscreenToggle.isOn = isFullscreen;
        bestScore.text = "Best Score : " + PlayerPrefs.GetInt("BestScore", ScoreKeeper.score).ToString("D6");
    }

    public void Play()
    {
        SceneManager.LoadScene ("Game");
    }

    public void Quit()
    {
        Application.Quit ();
    }
    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles [i].isOn)
        {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9;
            Screen.SetResolution (screenWidths [i], (int)(screenWidths [i] / aspectRatio), false);
            PlayerPrefs.SetInt ("screen res index", activeScreenResIndex);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        for (int i=0; i<resolutionToggles.Length; i++)
        {
            resolutionToggles [i].interactable = !isFullscreen;
        }
        if (isFullscreen)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions [allResolutions.Length - 1];
            Screen.SetResolution (maxResolution.width, maxResolution.height, true);
        }
        else
        {
            SetScreenResolution(activeScreenResIndex);
        }
        PlayerPrefs.SetInt ("fullscreen", ((isFullscreen)?1:0));
        PlayerPrefs.Save ();
    }
//audio volume 세팅은 audio manager에서 저장했음
    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Master);
    }
    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Music);
    }
    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Sfx);
    }

}
