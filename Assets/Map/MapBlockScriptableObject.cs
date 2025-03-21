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

    public string goalPrefabName;
    public Vector3 goalSpawnPoint;
    public Vector3 goalScale;
    public Vector3 goalPositionOffset = new Vector3(0.0f, -0.35f, 0.0f);
    public Vector3 goalRotation = new Vector3(0.0f, -90.0f, 0.0f);

    public Vector3[] spawnPoints;
}
