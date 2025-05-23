#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class ThumbnailScreenshots : MonoBehaviour
{

    [MenuItem("DevTools/Take Thumbnail Screenshot for Scene")]

    static void TakeSS()
    {
        Camera? mainCam = null;
        Camera? thumbnailCam = null;
        string fileName = SceneManager.GetActiveScene().name.ToString();

        foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (cam.name == "Main Camera") { mainCam = cam; }
            if (cam.name == "Thumbnail Camera") { thumbnailCam = cam; }
        }
        Debug.Assert(mainCam, "No \"Main Camera\" found. Did you do something wrong?");
        Debug.Assert(thumbnailCam, "No \"Thumbnail Camera\" found. Please create one to capture the thumbnail of the scene");
        if (mainCam != null) { mainCam.gameObject.SetActive(false); }
        if (thumbnailCam != null) { thumbnailCam.gameObject.SetActive(true); }

        Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        EditorWindow.GetWindow(gameViewType);
        Directory.CreateDirectory(Application.dataPath + "/LevelData/Thumbnails");
        WaitForSecondsRoutine();
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/LevelData/Thumbnails/" + fileName + ".png");
        //TODO: Convert the .png into a Sprite (2D and UI)

        Debug.Log("Screenshot saved to " + fileName + ".png");
        //TODO: properly set active and inactive 

        // new WaitForSeconds(1);
        // mainCam.gameObject.SetActive(true);
        // thumbnailCam.gameObject.SetActive(false);

    }

    private static IEnumerable WaitForSecondsRoutine()
    {
        yield return new WaitForSeconds(1);
    }
}
#endif