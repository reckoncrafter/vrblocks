using UnityEditor.VersionControl;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelMetadataScriptableObject", menuName = "ScriptableObjects/LevelMetadataScriptableObject", order = 2)]
public class LevelMetadataScriptableObject : ScriptableObject
{
    public int maxStarCount = 3;
    public Asset[] prerequisiteLevelsPlaceholder;          //placeholder for now
    public string[] levelTutorialEvents;

    public string[] levelHints;
}
