using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorMenu : MonoBehaviour
{
    public Button playLevelButton;

    void Start()
    {
        playLevelButton.onClick.AddListener(GoToLevel);
    }

    public void GoToLevel(){
        SceneTransitionManager.singleton.GoToSceneAsync(1);
    }
}
