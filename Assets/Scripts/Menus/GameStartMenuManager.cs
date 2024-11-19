/*
 Code for Start Menu when game begins + animations
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject UIContainer;
    public GameObject startMenu;
    public GameObject options;
    public GameObject levelSelector;

    [Header("Start Menu Buttons")]
    public Button playGameButton;
    public Button optionsButton;
    public Button quitButton;
    public List<Button> startMenuReturnButtons;

    // Animations
    public float animationSpeed = .3f;

    void Start(){
        if(SceneTransitionStates.IsGameStart()){                                    // Show the Game Start Menu on boot up
            startMenu.SetActive(true);
            options.SetActive(true);
            options.LeanScale(Vector3.zero, 0f);
            levelSelector.SetActive(true);
            levelSelector.LeanScale(Vector3.zero, 0f);
            SceneTransitionStates.SetGameStart(false);
        }
        else{                                                                       //TODO: For non-level returns to the main menu, create more conditional branches from here
            startMenu.SetActive(true);
            startMenu.LeanScale(Vector3.zero, 0f);
            options.SetActive(true);
            options.LeanScale(Vector3.zero, 0f);
            levelSelector.SetActive(true);
        }
        playGameButton.onClick.AddListener(EnableLevelSelectorMenu);
        optionsButton.onClick.AddListener(EnableOptionsMenu);
        quitButton.onClick.AddListener(QuitGame);

        foreach (var button in startMenuReturnButtons){
            button.onClick.AddListener(EnableStartMenu);
        }

        StartCoroutine(ClearUIShaderChannels());
    }

    IEnumerator ClearUIShaderChannels(){
        while(true){
            UIContainer.GetComponent<Canvas>().additionalShaderChannels = AdditionalCanvasShaderChannels.None;      // For some reason cannot be set in stone before runtime. This helps fix text disappearing
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void EnableStartMenu(){
        startMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        levelSelector.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void EnableOptionsMenu(){
        startMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        levelSelector.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void EnableLevelSelectorMenu(){
        startMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        levelSelector.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    } 

    public void QuitGame(){
        Application.Quit();
    }
}
