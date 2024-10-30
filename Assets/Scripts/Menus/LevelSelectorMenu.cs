using System;
using System.IO;
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
        // Long winded way of grabbing the thumbnails. Wished Resource.LoadAll worked
        DirectoryInfo thumbnailDir = new DirectoryInfo(Application.dataPath + "/Scenes/Thumbnails");
        FileInfo[] thumbnailFiles = thumbnailDir.GetFiles("*.png");
        Debug.Log(thumbnailFiles.Length);
        levelThumbnails = new Sprite[thumbnailFiles.Length];
        for(int i = 0; i < thumbnailFiles.Length; i++){
            Texture2D tex = new Texture2D(2, 2);                                // Texture size should not matter (https://discussions.unity.com/t/how-to-load-a-texture2d-from-path-without-resources-load/796449/6)
            tex.LoadImage(File.ReadAllBytes(thumbnailFiles[i].FullName));
            levelThumbnails[i] = Sprite.Create(
                tex, 
                new Rect(0.0f, 0.0f, tex.width, tex.height),                            // Mask; use the whole image for the sprite
                new Vector2(0.5f, 0.5f)                                                 // Center point; define it as the very middle
            );
        }

        playLevelButton.onClick.AddListener(GoToLevel);
        leftNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Max(0, selectedLevelIndex - 1); 
            UpdateDisplayView();
        });
        rightNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Min(selectedLevelIndex + 1, levelThumbnails.Length - 1);
            UpdateDisplayView();
        });
        UpdateDisplayView();
    }

    public void GoToLevel(){
        SceneTransitionManager.singleton.GoToSceneAsync(selectedLevelIndex, LoadSceneBy.AssetDirectoryOrder);
        // SceneTransitionManager.singleton.GoToSceneAsync(selectedLevelIndex, LoadSceneBy.BuildSettingsOrder);
    }

    public void UpdateDisplayView(){
        // For error handling, show nothing if there are no levels
        if(levelThumbnails.Length > 0){
            middleLevelView.SetActive(true);
            Image mThumbnail = middleLevelView.transform.Find("LevelThumbnail").GetComponent<Image>();
            mThumbnail.sprite = levelThumbnails[selectedLevelIndex];
        }
        else{
            middleLevelView.SetActive(false);
        }

        // Display Left View
        if(selectedLevelIndex > 0){
            leftNavigateButton.gameObject.SetActive(true);
            leftLevelView.SetActive(true);
            Image lThumbnail = leftLevelView.transform.Find("LevelThumbnail").GetComponent<Image>();
            lThumbnail.sprite = levelThumbnails[selectedLevelIndex - 1];
        }
        else{
            leftNavigateButton.gameObject.SetActive(false);
            leftLevelView.SetActive(false);
        }
        
        // Display Right View
        if(selectedLevelIndex < levelThumbnails.Length - 1){
            rightNavigateButton.gameObject.SetActive(true);
            rightLevelView.SetActive(true);
            Image rThumbnail = rightLevelView.transform.Find("LevelThumbnail").GetComponent<Image>();
            rThumbnail.sprite = levelThumbnails[selectedLevelIndex + 1];
        }
        else{
            rightNavigateButton.gameObject.SetActive(false);
            rightLevelView.SetActive(false);
        }
    }
}
