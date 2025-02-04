/*
 Handle level-to-level Player UI + animations
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public InputActionProperty pauseMenuAction = new InputActionProperty(new InputAction("Pause Menu Input", expectedControlType: "bool"));
    public InputActionProperty blockMenuAction = new InputActionProperty(new InputAction("Block Menu Input", expectedControlType: "bool"));

    public InputActionProperty debug_EndScreenTriggerAction = new InputActionProperty(new InputAction("debug_EndScreenTrigger", expectedControlType: "bool"));
    //TODO: These are for development. Depending on what we do, everything that starts with "debug_" will be affected

    [Header("UI Pages")]
    public GameObject UIContainer;
    public GameObject pauseMenu;
    public GameObject mainMenuConfirmWindow;
    public GameObject optionsMenu;
    public GameObject blockMenu; 
    public GameObject endScreen;

    [Header("Player UI Buttons")]
    public Button resumeGameButton;
    public Button optionsMenuButton;
    public List<Button> pauseMenuReturnButtons;
    public List<Button> returnToMenuButtons;
    public Button returnToMenuYButton;
    public Button returnToMenuNButton;
    public Button nextLevelButton;
    public Button endScreenReturnToMenu;
    public Button endScreenReturnToMenuAlt;

    //Animations
    public float animationSpeed = .3f;
    private bool isBlockMenuOpen = false; 

    void Start()
    {
        pauseMenu.SetActive(true);
        pauseMenu.LeanScale(Vector3.zero, 0f);
        mainMenuConfirmWindow.SetActive(true);
        mainMenuConfirmWindow.LeanScale(Vector3.zero, 0f);
        optionsMenu.SetActive(true);
        optionsMenu.LeanScale(Vector3.zero, 0f);
        blockMenu.SetActive(true); 
        blockMenu.LeanScale(Vector3.zero, 0f);
        endScreen.SetActive(true);
        endScreen.LeanScale(Vector3.zero, 0f); 

        pauseMenuAction.action.started += context => { EnablePauseMenu(); };
        blockMenuAction.action.started += context => { ToggleBlockMenu(); }; 
        debug_EndScreenTriggerAction.action.started += context => { EnableEndScreen(); };
        resumeGameButton.onClick.AddListener(ClosePauseMenu);
        optionsMenuButton.onClick.AddListener(EnableOptionsMenu);

        foreach (var b in pauseMenuReturnButtons){ b.onClick.AddListener(EnablePauseMenu); }
        foreach (var b in returnToMenuButtons){ b.onClick.AddListener(OpenConfirmationWindow); }
        returnToMenuNButton.onClick.AddListener(CloseConfirmationWindow);
        returnToMenuYButton.onClick.AddListener(ReturnToLevelSelector);
        nextLevelButton.onClick.AddListener(ContinueToNextLevel);

        StartCoroutine(ClearUIShaderChannels());
    }

    void OnEnable()
    {
        pauseMenuAction.action.Enable();
        blockMenuAction.action.Enable(); 
    }

    void OnDisable()
    {
        pauseMenuAction.action.Disable();
        blockMenuAction.action.Disable();
    }

    IEnumerator ClearUIShaderChannels()
    {
        while (true)
        {
            // UIContainer.GetComponent<Canvas>().additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void EnablePauseMenu()
    {
        pauseMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void ClosePauseMenu()
    {
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        mainMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void OpenConfirmationWindow()
    {
        mainMenuConfirmWindow.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void CloseConfirmationWindow()
    {
        mainMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void EnableOptionsMenu()
    {
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void ReturnToLevelSelector()
    {
        // SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.AssetDirectoryOrder);
        SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.BuildSettingsOrder);
    }

    public void ContinueToNextLevel()
    {
        pauseMenuAction.action.Enable();
        blockMenuAction.action.Enable();
        int nextLevel = SceneTransitionStates.GetSelectedLevel() + 1;
        SceneTransitionStates.SetSelectedLevel(nextLevel);
        SceneTransitionManager.singleton.GoToSceneAsync(nextLevel, LoadSceneBy.AssetDirectoryOrder);
    }

    public void ToggleBlockMenu()
    {
        if (isBlockMenuOpen)
        {
            blockMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        }
        else
        {
            blockMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        }

        isBlockMenuOpen = !isBlockMenuOpen; // Toggle state
    }

    public void EnableEndScreen()
    {
        endScreen.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        mainMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        blockMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();

        // Show "Next Level" Button only if the current level is not the last playable level
        DirectoryInfo thumbnailDir = new DirectoryInfo(Application.dataPath + "/Scenes/Thumbnails");
        FileInfo[] fin = thumbnailDir.GetFiles("*.png");
        if(SceneTransitionStates.GetSelectedLevel() + 1 >= fin.Length){
            nextLevelButton.GameObject().LeanScale(Vector3.zero, 0f);
            endScreenReturnToMenu.GameObject().LeanScale(Vector3.zero, 0f);
        }
        else
        {
            endScreenReturnToMenuAlt.GameObject().LeanScale(Vector3.zero, 0f);
        }

        //TODO: If we want the option to allow players to play around in the level after completing, these need to go
        pauseMenuAction.action.Disable();
        blockMenuAction.action.Disable();
    }
}