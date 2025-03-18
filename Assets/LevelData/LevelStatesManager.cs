using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelStatesManager: MonoBehaviour
{
    public static LevelStatesManager singleton;
    private bool init = false;
    private int numStates = Directory.GetDirectories(Application.dataPath + "/LevelData/MetaData").Length;
    private LevelState[] states;
    private LevelMetadataScriptableObject[] metadata;

    private void Awake()
    {
        if (singleton && singleton != this)
        {
            Destroy(singleton);
        }

        singleton = this;
    }

    public void InitializeLevelStates()
    {
        if (!init)
        {
            states = new LevelState[numStates];
            metadata = new LevelMetadataScriptableObject[numStates];

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
            }

            init = true;
        }
    }

    public bool getIsLockedLevel(int levelIndex){ return states[levelIndex].getIsLockedLevel(); }
    public void setIsLockedLevel(int levelIndex, bool isLocked){ states[levelIndex].setIsLockedLevel(isLocked); }
    public void unlockLevel(int levelIndex){ states[levelIndex].setIsLockedLevel(false); }
    public void triggerPrerequisiteLevelUnlock(string prerequisiteLevelName)
    {
        for (int i = 0; i < numStates; i++)
        {
            if (metadata[i].prerequisiteLevel.Trim() == prerequisiteLevelName.Trim())
            {
                unlockLevel(i);
            }
        }
    }
    public void triggerPrerequisiteLevelUnlock(int prerequisiteLevelIndex)
    {
        string prerequisiteLevelName = states[prerequisiteLevelIndex].getName();
        triggerPrerequisiteLevelUnlock(prerequisiteLevelName);
    }
}