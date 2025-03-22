/*
 Handle level-to-level Player UI + animations
*/
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
    public GameObject pauseMenu;
    public GameObject returnToMenuConfirmWindow;
    public GameObject optionsMenu;
    public GameObject blockMenu; 
    public GameObject endScreen;
    public float animationSpeed = .3f;

    [Header("Pause Menu")]
    public Button resumeGameButton;
    public Button resetTurtleButton;
    public List<Button> openPauseMenuButtons;

    [Header("Menu Return Buttons")]
    public List<Button> returnToMenuWithConfirmationButtons;
    public Button returnToMenuConfirmButton;
    public Button returnToMenuDenyButton;

    [Header("Options Menu")]
    public List<Button> openOptionsMenuButtons;

    [Header("Block Menu")]
    public GameObject movementPanel;
    public GameObject controlPanel;
    public GameObject functionPanel;
    public Button movementTab;
    public Button controlTab;
    public Button functionTab;

    [Header("End Screen Menu")]
    public Button nextLevelButton;
    public Button endScreenReturnToMenu;
    public Button endScreenReturnToMenuAlt;
    
    private bool toggleBlockMenu = false; 

    void Start()
    {
        pauseMenu.SetActive(true);
        pauseMenu.LeanScale(Vector3.zero, 0f);
        returnToMenuConfirmWindow.SetActive(true);
        returnToMenuConfirmWindow.LeanScale(Vector3.zero, 0f);
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
        resetTurtleButton.onClick.AddListener(ResetTurtle);
        movementTab.onClick.AddListener(() => ShowBlockMenuCategory(movementPanel));
        controlTab.onClick.AddListener(() => ShowBlockMenuCategory(controlPanel));
        functionTab.onClick.AddListener(() => ShowBlockMenuCategory(functionPanel));

        foreach (var b in openPauseMenuButtons){ b.onClick.AddListener(EnablePauseMenu); }
        foreach (var b in returnToMenuWithConfirmationButtons){ b.onClick.AddListener(OpenConfirmationWindow); }
        foreach (var b in openOptionsMenuButtons){ b.onClick.AddListener(EnableOptionsMenu); }
        returnToMenuDenyButton.onClick.AddListener(CloseConfirmationWindow);
        returnToMenuConfirmButton.onClick.AddListener(ReturnToLevelSelector);
        nextLevelButton.onClick.AddListener(ContinueToNextLevel);
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

    public void EnablePauseMenu()
    {
        pauseMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void ClosePauseMenu()
    {
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        returnToMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void ResetTurtle()
    {
        TurtleMovement turtleMovement = GameObject.Find("/MapSpawner/Turtle").GetComponent<TurtleMovement>();
        if(turtleMovement != null)
        {
            turtleMovement.Reset();
        }
        else
        {
            Debug.Log("ResetTurtleButton: Can't find Turtle!");
        }
    }

    public void OpenConfirmationWindow()
    {
        returnToMenuConfirmWindow.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void CloseConfirmationWindow()
    {
        returnToMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
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
        toggleBlockMenu = !toggleBlockMenu;
        if (toggleBlockMenu)
        {
            blockMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        }
        else
        {
            blockMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        }
    }

    void ShowBlockMenuCategory(GameObject category)
    {
        movementPanel.SetActive(false);
        controlPanel.SetActive(false);
        functionPanel.SetActive(false);
        category.SetActive(true);
    }

    public void EnableEndScreen()
    {
        endScreen.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        optionsMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        returnToMenuConfirmWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        blockMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        toggleBlockMenu = false;

        // Show "Next Level" Button only if the current level is not the last playable level
        DirectoryInfo thumbnailDir = new DirectoryInfo(Application.dataPath + "/LevelData/Thumbnails");
        FileInfo[] fin = thumbnailDir.GetFiles("*.png");
        if(SceneTransitionStates.GetSelectedLevel() + 1 >= fin.Length){
            nextLevelButton.GameObject().LeanScale(Vector3.zero, 0f);
            endScreenReturnToMenu.GameObject().LeanScale(Vector3.zero, 0f);
        }
        else
        {
            endScreenReturnToMenuAlt.GameObject().LeanScale(Vector3.zero, 0f);
        }

        // Unlock all other levels that require this one as a prerequisite
        LevelStates.triggerPrerequisiteLevelUnlock(SceneTransitionStates.GetSelectedLevel());

        //TODO: If we want the option to allow players to play around in the level after completing, these need to go
        pauseMenuAction.action.Disable();
        blockMenuAction.action.Disable();
    }
}
