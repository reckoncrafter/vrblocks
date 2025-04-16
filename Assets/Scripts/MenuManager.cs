using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button movementTab;
    public Button controlTab;
    public Button functionTab;
    private LevelStatesManager levelStatesManager;
    private int levelProgress;

    void Start()
    {
        levelProgress = SceneTransitionStates.GetSelectedLevel(); // Get current level
        levelStatesManager = FindObjectOfType<LevelStatesManager>();
        UpdateTabLocks(levelProgress);
    }

    public void UpdateTabLocks(int level)
    {
        void set_interactable(Button button, bool enabled)
        {
            Transform tmPro = button.transform.Find("Text (TMP)");
            Transform lockSprite = button.transform.Find("LockSprite");
            button.interactable = enabled;
            tmPro.gameObject.SetActive(enabled);
            lockSprite.gameObject.SetActive(!enabled);
        }
        if(levelStatesManager)
        {
            LevelMetadataScriptableObject levelMetadata = levelStatesManager.levelMetadataScriptables[levelProgress];
            set_interactable(movementTab, levelMetadata.enableMovementMenu);
            set_interactable(controlTab,  levelMetadata.enableControlMenu);
            set_interactable(functionTab, levelMetadata.enableFunctionMenu);
        }
        else
        {
            set_interactable(movementTab, true);
            set_interactable(controlTab,  true);
            set_interactable(functionTab, true);
        }
    }
}
