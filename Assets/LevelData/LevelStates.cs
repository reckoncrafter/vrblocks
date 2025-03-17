using System;
using System.IO;
using UnityEngine;

public class LevelState
{
    private bool isLockedLevel = true;
    private int starCount = 0;

    public bool getIsLockedLevel(){ return isLockedLevel; }
    public void setIsLockedLevel(bool isLocked){ isLockedLevel = isLocked; }
    public int getStarCount(){ return starCount; }
    public void setStartCount(int stars){ starCount = stars; }
}

public static class LevelStates
{
    private static LevelState[] states = new LevelState[Directory.GetDirectories(Application.dataPath + "/LevelData/MetaData").Length];

    static LevelStates()
    {
        for (int i = 0; i < states.Length; i++)
        {
            states[i] = new LevelState();
        }
    }

    public static int getNumStates(){ return states.Length; }
    public static bool getIsLockedLevel(int levelIndex){ return states[levelIndex].getIsLockedLevel(); }
    public static void setIsLockedLevel(int levelIndex, bool isLocked){ states[levelIndex].setIsLockedLevel(isLocked); }
    public static void unlockLevel(int levelIndex){ states[levelIndex].setIsLockedLevel(false); }
}