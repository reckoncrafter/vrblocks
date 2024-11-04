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

    // Animations
    private float levelNavAnimationSpeed = 0.1f;
    private Vector3 middleLevelViewPos;
    private Vector3 leftLevelViewPos;
    private Vector3 rightLevelViewPos;
    private Quaternion middleLevelViewRot;
    private Quaternion leftLevelViewRot;
    private Quaternion rightLevelViewRot;
    private Vector2 middleLevelViewScale;

    private Vector2 leftLevelViewScale;
    private Vector2 rightLevelViewScale;

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
                new Rect(0.0f, 0.0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        playLevelButton.onClick.AddListener(GoToLevel);
        leftNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Max(0, selectedLevelIndex - 1); 
            UpdateDisplayView();
            AnimateNavigateLeft();
        });
        rightNavigateButton.onClick.AddListener(() => {
            selectedLevelIndex = Math.Min(selectedLevelIndex + 1, levelThumbnails.Length - 1);
            UpdateDisplayView();
            AnimateNavigateRight();
        });
        UpdateDisplayView();

        // Animations
        middleLevelViewPos = middleLevelView.transform.position;
        middleLevelViewRot = middleLevelView.transform.localRotation;
        middleLevelViewScale = middleLevelView.transform.localScale;
        leftLevelViewPos = leftLevelView.transform.position;
        leftLevelViewRot = leftLevelView.transform.localRotation;
        leftLevelViewScale = leftLevelView.transform.localScale;
        rightLevelViewPos = rightLevelView.transform.position;
        rightLevelViewRot = rightLevelView.transform.localRotation;
        rightLevelViewScale = rightLevelView.transform.localScale;
    }

    public void GoToLevel(){
        SceneTransitionManager.singleton.GoToSceneAsync(selectedLevelIndex, LoadSceneBy.AssetDirectoryOrder);
        // SceneTransitionManager.singleton.GoToSceneAsync(selectedLevelIndex, LoadSceneBy.BuildSettingsOrder);
    }

    public void AnimateNavigateLeft(){
        middleLevelView.transform.position = leftLevelViewPos;
        middleLevelView.transform.rotation = leftLevelViewRot;
        middleLevelView.transform.localScale = leftLevelViewScale;
        middleLevelView.LeanMove(middleLevelViewPos, levelNavAnimationSpeed).setEaseOutCubic();
        middleLevelView.LeanRotate(middleLevelViewRot.eulerAngles, levelNavAnimationSpeed).setEaseOutCubic();
        middleLevelView.LeanScale(middleLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        rightLevelView.transform.position = middleLevelViewPos;
        rightLevelView.transform.rotation = middleLevelViewRot;
        rightLevelView.transform.localScale = middleLevelViewScale;
        rightLevelView.LeanMove(rightLevelViewPos, levelNavAnimationSpeed).setEaseOutCubic();
        rightLevelView.LeanRotate(rightLevelViewRot.eulerAngles, levelNavAnimationSpeed).setEaseOutCubic();
        rightLevelView.LeanScale(rightLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        leftLevelView.LeanScale(Vector3.zero, 0f);
        if(selectedLevelIndex > 0){
            leftLevelView.LeanScale(leftLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        }
    }

    public void AnimateNavigateRight(){
        middleLevelView.transform.position = rightLevelViewPos;
        middleLevelView.transform.rotation = rightLevelViewRot;
        middleLevelView.transform.localScale = rightLevelViewScale;
        middleLevelView.LeanMove(middleLevelViewPos, levelNavAnimationSpeed).setEaseOutCubic();
        middleLevelView.LeanRotate(middleLevelViewRot.eulerAngles, levelNavAnimationSpeed).setEaseOutCubic();
        middleLevelView.LeanScale(middleLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        leftLevelView.transform.position = middleLevelViewPos;
        leftLevelView.transform.rotation = middleLevelViewRot;
        leftLevelView.transform.localScale = middleLevelViewScale;
        leftLevelView.LeanMove(leftLevelViewPos, levelNavAnimationSpeed).setEaseOutCubic();
        leftLevelView.LeanRotate(leftLevelViewRot.eulerAngles, levelNavAnimationSpeed).setEaseOutCubic();
        leftLevelView.LeanScale(leftLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        rightLevelView.LeanScale(Vector3.zero, 0f);
        if(selectedLevelIndex < levelThumbnails.Length - 1){
            rightLevelView.LeanScale(rightLevelViewScale, levelNavAnimationSpeed).setEaseOutCubic();
        }
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
            Image lThumbnail = leftLevelView.transform.Find("LevelThumbnail").GetComponent<Image>();
            lThumbnail.sprite = levelThumbnails[selectedLevelIndex - 1];
        }
        else{
            leftNavigateButton.gameObject.SetActive(false);
            leftLevelView.LeanScale(Vector3.zero, 0f);
        }
        
        // Display Right View
        if(selectedLevelIndex < levelThumbnails.Length - 1){
            rightNavigateButton.gameObject.SetActive(true);
            Image rThumbnail = rightLevelView.transform.Find("LevelThumbnail").GetComponent<Image>();
            rThumbnail.sprite = levelThumbnails[selectedLevelIndex + 1];
        }
        else{
            rightNavigateButton.gameObject.SetActive(false);
            rightLevelView.LeanScale(Vector3.zero, 0f);
        }
    }
}
