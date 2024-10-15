using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject startMenu;
    public GameObject options;
    public GameObject levelSelector;

    [Header("Start Menu Buttons")]
    public Button playGameButton;
    public Button optionsButton;
    public Button quitButton;
    public List<Button> startMenuReturnButtons;

    void Start(){
        EnableStartMenu();

        playGameButton.onClick.AddListener(EnableLevelSelectorMenu);
        optionsButton.onClick.AddListener(EnableOptionsMenu);
        quitButton.onClick.AddListener(QuitGame);

        foreach (var button in startMenuReturnButtons){
            button.onClick.AddListener(EnableStartMenu);
        }
    }

    public void EnableStartMenu(){
        startMenu.SetActive(true);
        options.SetActive(false);
        levelSelector.SetActive(false);
    }

    public void EnableOptionsMenu(){
        startMenu.SetActive(false);
        options.SetActive(true);
        levelSelector.SetActive(false);
    }

    public void EnableLevelSelectorMenu(){
        startMenu.SetActive(false);
        options.SetActive(false);
        levelSelector.SetActive(true);
    } 

    public void QuitGame(){
        Application.Quit();
    }
}
