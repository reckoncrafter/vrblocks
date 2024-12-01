/*
 Code for handling level to level Player UI + animations
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public InputActionProperty pauseMenuAction = new InputActionProperty(new InputAction("Pause Menu Input", expectedControlType: "bool"));
    public InputActionProperty blockMenuAction = new InputActionProperty(new InputAction("Block Menu Input", expectedControlType: "bool")); // New action for Block Menu

    [Header("UI Pages")]
    public GameObject UIContainer;
    public GameObject pauseMenu;
    public GameObject confirmationWindow;
    public GameObject options;
    public GameObject blockMenu; // New Block Menu
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
    private bool isBlockMenuOpen = false; // Track block menu state

    void Start()
    {
        pauseMenu.SetActive(true);
        pauseMenu.LeanScale(Vector3.zero, 0f);
        confirmationWindow.SetActive(true);
        confirmationWindow.LeanScale(Vector3.zero, 0f);
        options.SetActive(true);
        options.LeanScale(Vector3.zero, 0f);
        blockMenu.SetActive(true); // Ensure blockMenu is ready
        blockMenu.LeanScale(Vector3.zero, 0f); // Initially hidden

        pauseMenuAction.action.started += context => { EnablePauseMenu(); };
        blockMenuAction.action.started += context => { ToggleBlockMenu(); }; // Handle block menu toggle
        resumeGameButton.onClick.AddListener(ClosePauseMenu);
        optionsButton.onClick.AddListener(EnableOptionsMenu);
        returnToMenuButton.onClick.AddListener(OpenConfirmationWindow);
        returnToMenuNButton.onClick.AddListener(CloseConfirmationWindow);
        returnToMenuYButton.onClick.AddListener(ReturnToLevelSelector);

        foreach (var button in pauseMenuReturnButtons)
        {
            button.onClick.AddListener(EnablePauseMenu);
        }

        StartCoroutine(ClearUIShaderChannels());
    }

    void OnEnable()
    {
        pauseMenuAction.action.Enable();
        blockMenuAction.action.Enable(); // Enable block menu action
    }

    void OnDisable()
    {
        pauseMenuAction.action.Disable();
        blockMenuAction.action.Disable(); // Disable block menu action
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
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void ClosePauseMenu()
    {
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        confirmationWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void OpenConfirmationWindow()
    {
        confirmationWindow.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void CloseConfirmationWindow()
    {
        confirmationWindow.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
    }

    public void EnableOptionsMenu()
    {
        pauseMenu.LeanScale(Vector3.zero, animationSpeed).setEaseInOutCubic();
        options.LeanScale(Vector3.one, animationSpeed).setEaseInOutCubic();
    }

    public void ReturnToLevelSelector()
    {
        // SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.AssetDirectoryOrder);
        SceneTransitionManager.singleton.GoToSceneAsync(0, LoadSceneBy.BuildSettingsOrder);
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
}