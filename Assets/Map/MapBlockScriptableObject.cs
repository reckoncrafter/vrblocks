using UnityEngine;

[CreateAssetMenu(fileName = "MapScriptableObject", menuName = "ScriptableObjects/MapScriptableObject", order = 1)]
public class MapBlockScriptableObject : ScriptableObject
{
    public string blockPrefabName;
    public Vector3 blockScale;

    public string turtlePrefabName;
    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;

    public Vector3 turtleSpawnPoint;
    public float turtleRotation;

    public Vector3 goalSpawnPoint;
    public Vector3 goalScale;

    public Vector3[] spawnPoints;
}
