using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    
    [Header("UI Pages")]
    public GameObject UIContainer;
    public GameObject pauseMenu;
    public GameObject confirmationWindow;
    public GameObject options;
    // public GameObject endScreen;

    [Header("Start Menu Buttons")]
    public Button resumeGameButton;
    public Button optionsButton;
    public Button returnToMenuButton;
    public Button returnToMenuYButton;
    public Button returnToMenuNButton;
    public List<Button> pauseMenuReturnButtons;

    //Animations
    public float animationSpeed = .3f;
    
    void Start()
    {
        pauseMenu.SetActive(true);
        // pauseMenu.LeanScale(Vector3.zero, 0f);
        confirmationWindow.SetActive(true);
        confirmationWindow.LeanScale(Vector3.zero, 0f);
        options.SetActive(true);
        options.LeanScale(Vector3.zero, 0f);
        // endScreen.SetActive(true);
        // endScreen.LeanScale(Vector3.zero, 0f); 

        resumeGameButton.onClick.AddListener(ClosePauseMenu);
        optionsButton.onClick.AddListener(EnableOptionsMenu);
        returnToMenuButton.onClick.AddListener(OpenConfirmationWindow);
        returnToMenuNButton.onClick.AddListener(CloseConfirmationWindow);
        returnToMenuYButton.onClick.AddListener(ReturnToLevelSelector);

        foreach (var button in pauseMenuReturnButtons){
            button.onClick.AddListener(EnablePauseMenu);
        }

        StartCoroutine(ClearUIShaderChannels());
    }

    IEnumerator ClearUIShaderChannels(){
        while(true){
            UIContainer.GetComponent<Canvas>().additionalShaderChannels = AdditionalCanvasShaderChannels.None;      // For some reason cannot be set in stone before runtime. This helps fix text disappearing
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void EnablePauseMenu(){
        pauseMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void ClosePauseMenu(){
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        confirmationWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void OpenConfirmationWindow(){
        confirmationWindow.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    } 
    public void CloseConfirmationWindow(){
        confirmationWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void EnableOptionsMenu(){
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void ReturnToLevelSelector(){
        // SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.AssetDirectoryOrder);
        SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.BuildSettingsOrder);
    }

}
