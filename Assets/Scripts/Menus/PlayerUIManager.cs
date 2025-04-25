/*
 Handle level-to-level Player UI + animations
*/
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public XROrigin xrRig;
    public GameObject pivot;

    [Header("XR Rig Actions")]
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
    public Button debug_EndscreenTriggerButton;
    public Button nextLevelButton;
    public Button endScreenReturnToMenu;
    public Button endScreenReturnToMenuAlt;

    private UISFXManager? uiSFXManager;
    private bool toggleBlockMenu = false;

    [Header("Function Call Spawning")]
    public Button functionDefinitionButton;
    public Button callFunctionButton;
    public FunctionBlock? selectedFunctionBlock;
    public GameObject functionCallPrefab;
    public GameObject functionDefPrefab;

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
        // debug_EndScreenTriggerAction.action.started += context => { EnableEndScreen(); };
        debug_EndscreenTriggerButton.onClick.AddListener(EnableEndScreen);
        resumeGameButton.onClick.AddListener(ClosePauseMenu);
        resetTurtleButton.onClick.AddListener(ResetTurtle);
        movementTab.onClick.AddListener(() => ShowBlockMenuCategory(movementPanel));
        controlTab.onClick.AddListener(() => ShowBlockMenuCategory(controlPanel));
        functionTab.onClick.AddListener(() => ShowBlockMenuCategory(functionPanel));

        foreach (var b in openPauseMenuButtons) { b.onClick.AddListener(EnablePauseMenu); }
        foreach (var b in returnToMenuWithConfirmationButtons) { b.onClick.AddListener(OpenConfirmationWindow); }
        foreach (var b in openOptionsMenuButtons) { b.onClick.AddListener(EnableOptionsMenu); }
        returnToMenuDenyButton.onClick.AddListener(CloseConfirmationWindow);
        returnToMenuConfirmButton.onClick.AddListener(ReturnToLevelSelector);
        nextLevelButton.onClick.AddListener(ContinueToNextLevel);
        callFunctionButton.onClick.AddListener(SpawnFunctionCall);
        functionDefinitionButton.onClick.AddListener(SpawnFuctionDefinition);
        
        // Open Movement Panel on Block Menu at start
        ShowBlockMenuCategory(movementPanel);

        uiSFXManager = FindObjectOfType<UISFXManager>();
        if(xrRig == null){ xrRig = FindObjectOfType<XROrigin>(); }
    }

    public float movementStrength = 2.0f;
    
    void Update()
    {
        // https://www.reddit.com/r/Unity3D/comments/cj7niq/comment/evbnl0k/
        Vector3 look = pivot.transform.position - xrRig.transform.position;
        float radians = Mathf.Atan2(look.x, look.z);
        float degrees = radians * Mathf.Rad2Deg;
        float str = Mathf.Min(movementStrength * Time.deltaTime, 1);
        Quaternion targetRotation = Quaternion.Euler(0, degrees, 0);
        pivot.transform.rotation = Quaternion.Slerp(pivot.transform.rotation, targetRotation, str);
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
        uiSFXManager?.PlayUISFX(MenuUISFX.PlayerInputActionOpen);
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
        if (turtleMovement != null)
        {
            turtleMovement.canFail = true;
            turtleMovement.canReset = true;
            turtleMovement.Fail();
        }
        else
        {
            Debug.Log("ResetTurtleButton: Can't find Turtle!");
        }
    }

    public void SetFunctionCallButtonStatus(bool status)
    {
        callFunctionButton.interactable = status;
    }

    void SpawnFunctionCall()
    {
      Vector3 spawnOffset = new (0, 0.5f, 0);
      var blockEntity = GameObject.Find("MoveableEntities/BlockEntity").GetComponent<Transform>();
      GameObject newFunctionCall = Instantiate(functionCallPrefab, callFunctionButton.transform.position + spawnOffset, callFunctionButton.transform.rotation, blockEntity);
      newFunctionCall.name = "Block (FunctionCall)";
      FunctionCallBlock fcb = newFunctionCall.AddComponent<FunctionCallBlock>();
      fcb.functionDefinition = selectedFunctionBlock;
      var FCLabel = newFunctionCall.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshProUGUI>();
      FCLabel.text = "Call " + selectedFunctionBlock.FunctionID.ToString();
      fcb.GetComponent<FunctionCallBlock>().FunctionID = selectedFunctionBlock.FunctionID;
    }

    void SpawnFuctionDefinition()
    {
        Vector3 spawnOffset = new (0, 0.5f, 0);
        var blockEntity = GameObject.Find("MoveableEntities/BlockEntity").GetComponent<Transform>();
        GameObject newFunction = Instantiate(functionDefPrefab, functionDefinitionButton.transform.position + spawnOffset, functionDefinitionButton.transform.rotation, blockEntity);
        FunctionBlock functionBlock = newFunction.GetComponent<FunctionBlock>();
        if(selectedFunctionBlock != null)
        {
            selectedFunctionBlock.SetSelectedVisual(false);
        }
        functionBlock.SetSelectedVisual(true);
        SetFunctionCallButtonStatus(true);
        selectedFunctionBlock = functionBlock;
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
        SceneTransitionManager.singleton.GoToSceneAsync(nextLevel, LoadSceneBy.LevelStatesManagerArrayOrder);
    }

    public void ToggleBlockMenu()
    {
        toggleBlockMenu = !toggleBlockMenu;
        if (toggleBlockMenu)
        {
            blockMenu.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
            uiSFXManager?.PlayUISFX(MenuUISFX.PlayerInputActionOpen);
        }
        else
        {
            blockMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
            uiSFXManager?.PlayUISFX(MenuUISFX.PlayerInputActionClose);
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
        // DirectoryInfo thumbnailDir = new DirectoryInfo(Application.dataPath + "/LevelData/Thumbnails");
        LevelMetadataScriptableObject[] levelMetadataScriptables = GameObject.Find("/LevelStatesManager").GetComponent<LevelStatesManager>().levelMetadataScriptables;
        // FileInfo[] fin = thumbnailDir.GetFiles("*.png");
        if (SceneTransitionStates.GetSelectedLevel() + 1 >= levelMetadataScriptables.Length)
        {
            nextLevelButton.GameObject().LeanScale(Vector3.zero, 0f);
            endScreenReturnToMenu.GameObject().LeanScale(Vector3.zero, 0f);
        }
        else
        {
            endScreenReturnToMenuAlt.GameObject().LeanScale(Vector3.zero, 0f);
        }

        // Unlock all other levels that require this one as a prerequisite
        Debug.Log($"PlayerUIManager.EnableEndScreen: unlocking prerequisites for {SceneTransitionStates.GetSelectedLevel()}");
        LevelStates.triggerPrerequisiteLevelUnlock(SceneTransitionStates.GetSelectedLevel());

        //TODO: If we want the option to allow players to play around in the level after completing, these need to go
        pauseMenuAction.action.Disable();
        blockMenuAction.action.Disable();
    }
}
