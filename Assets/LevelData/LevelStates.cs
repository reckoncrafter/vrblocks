using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelState
{
    private string name;
    private bool isLockedLevel = true;
    private int starCount = 0;

    public LevelState(string name)
    {
        this.name = name;
    }

    public string getName(){ return name; }
    public bool getIsLockedLevel(){ return isLockedLevel; }
    public void setIsLockedLevel(bool isLocked){ isLockedLevel = isLocked; }
    public int getStarCount(){ return starCount; }
    public void setStartCount(int stars){ starCount = stars; }
}

public static class LevelStates
{
    private static bool init = false;
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

            // Load Level Metadata using the first LevelMetadataScriptableObject found in the folder
            string levelDir = Path.Combine("Assets/LevelData/MetaData", new DirectoryInfo(levelDirs[i]).Name);
            string[] guid = AssetDatabase.FindAssets("t:LevelMetadataScriptableObject", new[] { levelDir });
            if (guid.Length > 0)
            {
                metadata[i] = AssetDatabase.LoadAssetAtPath<LevelMetadataScriptableObject>(AssetDatabase.GUIDToAssetPath(guid[0]));
            }
            else
            {
                Debug.LogError("No LevelMetadataScriptableObject found within " + levelDir);
                metadata[i] = new LevelMetadataScriptableObject();
            }

            // Unlock levels that do not have prerequisites
            if (metadata[i].prerequisiteLevel.Trim() == ""){
                states[i].setIsLockedLevel(false);
            }
        }

        init = true;
    }

    public static bool isInit(){ return init; }
    public static int getNumStates(){ return states.Length; }
    public static bool getIsLockedLevel(int levelIndex){ return states[levelIndex].getIsLockedLevel(); }
    public static void setIsLockedLevel(int levelIndex, bool isLocked){ states[levelIndex].setIsLockedLevel(isLocked); }
    public static void unlockLevel(int levelIndex){ states[levelIndex].setIsLockedLevel(false); }
    public static void triggerPrerequisiteLevelUnlock(int prerequisiteLevelIndex)
    {
        string prerequisiteLevelName = states[prerequisiteLevelIndex].getName();
        for (int i = 0; i < numStates; i++)
        {
            if (metadata[i].prerequisiteLevel.Trim() == prerequisiteLevelName.Trim())
            {
                unlockLevel(i);
            }
        }
    }
}