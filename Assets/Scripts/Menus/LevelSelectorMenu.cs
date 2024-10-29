using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorMenu : MonoBehaviour
{
    public Button playLevelButton;
    public Button leftNavigateButton;
    public Button rightNavigateButton;
    public GameObject middleLevelView;
    public GameObject leftLevelView;
    public GameObject rightLevelView;

    public int selectedLevelIndex = 1;
    private Sprite[] levelThumbnails;

    void Start()
    {
        DirectoryInfo thumbnailDir = new DirectoryInfo(Application.dataPath + "/Scenes/Thumbnails");
        FileInfo[] thumbnailFiles = thumbnailDir.GetFiles("*.png");
        // levelThumbnails 

        playLevelButton.onClick.AddListener(GoToLevel);
        leftNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Max(0, selectedLevelIndex - 1); 
            UpdateDisplayView();
        });
        rightNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Min(selectedLevelIndex + 1, 5);               //TODO: Change the max value to the # of scenes in the build
            UpdateDisplayView();
        });
        
        UpdateDisplayView();
    }

    public void GoToLevel(){
        SceneTransitionManager.singleton.GoToSceneAsync(selectedLevelIndex, LoadSceneBy.BuildSettingsOrder);
    }

    public void UpdateDisplayView(){
        // TODO: For error handling, show nothing if there are no levels
        GameObject middleThumbnail = middleLevelView.transform.Find("LevelThumbnail").GetComponent<GameObject>();

        // Display Left View
        if(selectedLevelIndex > 0){
            leftLevelView.SetActive(true);
        }
        else{
            leftLevelView.SetActive(false);
        }
        
        // Display Right View
        if(selectedLevelIndex < 5){                   //TODO: Change the max value to the # of scenes in the build
            rightLevelView.SetActive(true);
        }
        else{
            rightLevelView.SetActive(false);
        }
    }
}
