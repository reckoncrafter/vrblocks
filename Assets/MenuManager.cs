using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button movementTab;
    public Button controlTab;
    public Button functionTab;

    void Start()
    {
        int levelProgress = SceneTransitionStates.GetSelectedLevel(); // Get current level
        UpdateTabLocks(levelProgress);
    }

    public void UpdateTabLocks(int level)
    {
        movementTab.interactable = true;
        controlTab.interactable = level >= 1;
        functionTab.interactable = level >= 3;
    }
}
