using UnityEngine;

[CreateAssetMenu(fileName = "MapScriptableObject", menuName = "ScriptableObjects/MapScriptableObject", order = 1)]
public class MapBlockScriptableObject : ScriptableObject
{
    public string prefabName;

    public Vector3 blockScale;
    public Vector3[] spawnPoints;
}
