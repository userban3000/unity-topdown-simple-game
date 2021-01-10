using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public GameObject levelSelectMenuHolder;

    public Slider[] volumeSliders;
    public Button[] levelButtons;

    private void Start() {
        optionsMenuHolder.SetActive(false);
        levelSelectMenuHolder.SetActive(false);
        mainMenuHolder.SetActive(true);
    }

    public void Play() {
        mainMenuHolder.SetActive(false);
        levelSelectMenuHolder.SetActive(true);
        LevelSelectMenu();
    }

    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void MainMenu() {
        optionsMenuHolder.SetActive(false);
        levelSelectMenuHolder.SetActive(false);
        mainMenuHolder.SetActive(true);
    }

    public void StartLevel( int lvl ) {
        string levelName = "Level" + lvl.ToString();
        SceneManager.LoadScene(levelName);
    }

    public void LevelSelectMenu() {
        int levelCount = levelButtons.Length;

        int reachedLevel = PlayerPrefs.GetInt("reachedLevel", 1);

        for ( int i = 0; i < levelCount; i++ ) {
            if ( i > reachedLevel ) {
                levelButtons[i].interactable = false;
            }
        }

    }

    public void SetMasterVolume() {
        
    }

    public void SetMusicVolume() {
        
    }

    public void SetSFXVolume() {
        
    }

}
