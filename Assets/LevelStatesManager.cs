using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelStatesManager : MonoBehaviour
{
    public LevelMetadataScriptableObject[] levelMetadataScriptables;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);    
    }

    void Start()
    {
        // Initializing LevelStates...
        LevelStates.setNumStates(levelMetadataScriptables.Length);
        LevelStates.initializeStatesAndMetadata(levelMetadataScriptables);
        LevelStates.triggerPrerequisiteLevelUnlock("");
    }
}
