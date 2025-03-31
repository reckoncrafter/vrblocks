using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelState
{
    private string name;
    //private bool isLockedLevel = true;
    //private int starCount = 0;

    /*
    * Explanation on PlayerPrefs.
    * PlayerPrefs is a key-value database that persists outside of Unity.
    * I figured we didn't need anything super fancy or secure to keep track of progress.
    * I don't think anyone would care if our game allowed you to just unlock all the levels by editing a file.
    * I've replaced the static variables here with calls to the database, that way progress is saved between runs.
    */

    public LevelState(string name)
    {
        this.name = name;
        PlayerPrefs.SetInt(name + "_locked", 1);
        PlayerPrefs.SetInt(name + "_stars", 0);
    }

    public string getName(){ return name; }
    public bool getIsLockedLevel(){ return PlayerPrefs.GetInt(name + "_locked") == 1; }
    public void setIsLockedLevel(bool isLocked){PlayerPrefs.SetInt(name + "_locked", isLocked? 1:0); }
    public int getStarCount(){ return PlayerPrefs.GetInt(name + "_stars"); }
    public void setStartCount(int stars){ PlayerPrefs.SetInt(name + "_stars", stars); }
}

public static class LevelStates
{
    private static int numStates = Directory.GetDirectories(Application.dataPath + "/LevelData/MetaData").Length;
    private static LevelState[] states = new LevelState[numStates];
    private static LevelMetadataScriptableObject[] metadata = new LevelMetadataScriptableObject[numStates];

    static LevelStates()
    {
        string[] levelDirs = Directory.GetDirectories(Application.dataPath + "/LevelData/MetaData");

        for (int i = 0; i < numStates; i++)
        {
            // Init LevelState objects, name them based on folder name
            states[i] = new LevelState(new DirectoryInfo(levelDirs[i]).Name);
        //     // Load Level Metadata using the first LevelMetadataScriptableObject found in the folder
        //     string levelDir = Path.Combine("Assets/LevelData/MetaData", new DirectoryInfo(levelDirs[i]).Name);
        //     string[] guid = AssetDatabase.FindAssets("t:LevelMetadataScriptableObject", new[] { levelDir });
        //     if (guid.Length > 0)
        //     {
        //         metadata[i] = AssetDatabase.LoadAssetAtPath<LevelMetadataScriptableObject>(AssetDatabase.GUIDToAssetPath(guid[0]));
        //     }
        //     else
        //     {
        //         Debug.LogError("No LevelMetadataScriptableObject found within " + levelDir);
        //         metadata[i] = new LevelMetadataScriptableObject();
        //     }

        // // Unlock levels that do not have prerequisites
        // if (metadata[i].prerequisiteLevel == null || metadata[i].prerequisiteLevel.Trim() == ""){
        //     states[i].setIsLockedLevel(false);
        // }
        }
    }

    public static void setMetadataReference(int levelIndex, LevelMetadataScriptableObject levelMetadata){ metadata[levelIndex] = levelMetadata; }

    public static int getNumStates(){ return states.Length; }
    public static bool getIsLockedLevel(int levelIndex){ return states[levelIndex].getIsLockedLevel(); }
    public static void setIsLockedLevel(int levelIndex, bool isLocked){ states[levelIndex].setIsLockedLevel(isLocked); }
    public static void unlockLevel(int levelIndex){ states[levelIndex].setIsLockedLevel(false); }
    public static void triggerPrerequisiteLevelUnlock(string prerequisiteLevelName)
    {
        for (int i = 0; i < numStates; i++)
        {
            if (metadata[i].prerequisiteLevel == null){ metadata[i].prerequisiteLevel = ""; }           // Thank you @reckoncrafter
            if (metadata[i].prerequisiteLevel.Trim() == prerequisiteLevelName.Trim())
            {
                unlockLevel(i);
            }
        }
    }
    public static void triggerPrerequisiteLevelUnlock(int prerequisiteLevelIndex)
    {
        string prerequisiteLevelName = states[prerequisiteLevelIndex].getName();
        triggerPrerequisiteLevelUnlock(prerequisiteLevelName);
    }
}