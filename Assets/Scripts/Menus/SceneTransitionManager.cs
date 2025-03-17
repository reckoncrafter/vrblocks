/*
 Handle loading/transitioning scenes that are made available to the player.
*/
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LoadSceneBy
{
    AssetDirectoryOrder = 0,
    BuildSettingsOrder = 1
}
public class SceneTransitionManager : MonoBehaviour
{
    public int waitDuration;
    public static SceneTransitionManager singleton;

    public static FileInfo[] scenes;

    public void Start()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Scenes/PlayableLevels");
        scenes = dir.GetFiles("*.unity");
    }

    private void Awake()
    {
        if (singleton && singleton != this)
        {
            Destroy(singleton);
        }

        singleton = this;
    }

    public void GoToScene(int sceneIndex, LoadSceneBy loadOption = LoadSceneBy.AssetDirectoryOrder)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex, loadOption));
    }
    public void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneRoutine(sceneName));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex, LoadSceneBy loadOption)
    {
        yield return new WaitForSeconds(waitDuration);
        if (loadOption == LoadSceneBy.AssetDirectoryOrder)
        {
            SceneManager.LoadScene(scenes[sceneIndex].Name.Replace(".unity", ""));
        }
        else if (loadOption == LoadSceneBy.BuildSettingsOrder)
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
    IEnumerator GoToSceneRoutine(string sceneName)
    {
        yield return new WaitForSeconds(waitDuration);
        SceneManager.LoadScene(sceneName);
    }

    public void GoToSceneAsync(int sceneIndex, LoadSceneBy loadOption = LoadSceneBy.AssetDirectoryOrder)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex, loadOption));
    }
    public void GoToSceneAsync(string sceneName)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneName));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex, LoadSceneBy loadOption)
    {
        AsyncOperation operation;
        if (loadOption == LoadSceneBy.AssetDirectoryOrder)
        {
            operation = SceneManager.LoadSceneAsync(scenes[sceneIndex].Name.Replace(".unity", ""));
        }
        else // LoadSceneBy.BuildSettingsOrder
        {
            operation = SceneManager.LoadSceneAsync(sceneIndex);
        }

        operation.allowSceneActivation = false;
        float timer = 0;
        while (timer <= waitDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        operation.allowSceneActivation = true;
    }
    IEnumerator GoToSceneAsyncRoutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        float timer = 0;
        while (timer <= waitDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        operation.allowSceneActivation = true;
    }
}
