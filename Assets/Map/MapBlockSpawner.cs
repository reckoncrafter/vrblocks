using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(MapBlockSpawner))]
public class MapBlockSpawnerMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapBlockSpawner mapBlockSpawner = (MapBlockSpawner)target;

        if (GUILayout.Button("Spawn Entities"))
        {
            if (mapBlockSpawner.transform.childCount == 0)
            {
                mapBlockSpawner.SpawnEntities();
            }
            else
            {
                Debug.Log("Map GameObject contains children. Skiping Spawn");
            }
        }
        if (GUILayout.Button("Clear Children"))
        {
            mapBlockSpawner.ClearMap();
        }
    }
}
#endif

[ExecuteInEditMode]
public class MapBlockSpawner : MonoBehaviour
{
    public AudioClip turtleSuccessAudio;
    public GameObject mapBlock;
    public GameObject goalObject;
    public TurtleMovement turtle;
    public MapBlockScriptableObject mapValues;

    private readonly DetectTurtle detectTurtle = new();

    void Start()
    {
        detectTurtle.TurtleSuccessAudio = turtleSuccessAudio;

        // delete children to spawn again
        if (transform.childCount == 0)
        {
            SpawnEntities();
        }
        else
        {
            detectTurtle.FindTurtle();
        }
    }
    public void SpawnEntities()
    {
        Vector3 startPositionOffset = mapValues.blockScale / 2;

        GameObject? currentEntity = null;
        for (int i = 0; i < mapValues.spawnPoints.Length; i++)
        {
            Vector3 coords = startPositionOffset + Vector3.Scale(mapValues.spawnPoints[i], mapValues.blockScale);
            currentEntity = Instantiate(mapBlock, coords, Quaternion.identity);

            currentEntity.transform.SetParent(gameObject.transform, false);

            currentEntity.name = mapValues.blockPrefabName + i;
            currentEntity.transform.localScale = mapValues.blockScale;
        }

        // Triggerable Goal
        Vector3 goalCoords = startPositionOffset + Vector3.Scale(mapValues.goalSpawnPoint, mapValues.blockScale);
        GameObject generatedGoalObject = Instantiate(goalObject, goalCoords, Quaternion.Euler(mapValues.goalRotation));
        generatedGoalObject.transform.SetParent(gameObject.transform, false);
        generatedGoalObject.transform.localScale = mapValues.goalScale;
        generatedGoalObject.transform.position += mapValues.goalPositionOffset;
        generatedGoalObject.name = mapValues.goalPrefabName;

        Vector3 turtleCoords = startPositionOffset + Vector3.Scale(mapValues.turtleSpawnPoint, mapValues.blockScale);
        TurtleMovement turtleEntity = Instantiate(turtle, turtleCoords, Quaternion.Euler(0, mapValues.turtleRotation, 0));

        turtleEntity.transform.SetParent(gameObject.transform, false);

        turtleEntity.name = mapValues.turtlePrefabName;

        turtleEntity.movementDuration = mapValues.movementDuration;
        turtleEntity.animationSpeed = mapValues.animationSpeed;


        if (currentEntity != null)
        {
            turtleEntity.moveDistance = Vector3.Scale(currentEntity.GetComponent<BoxCollider>().bounds.size, mapValues.blockScale);
        }

        detectTurtle.SetTurtleAndGoal(turtleEntity, generatedGoalObject);
    }

    public void ClearMap()
    {
        // https://discussions.unity.com/t/why-does-foreach-work-only-1-2-of-a-time/91068/2
        // forgot you dont mutate the same list you're looping through
        foreach (Transform child in transform.Cast<Transform>().ToList())
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (SphereCollider sCollider in transform.gameObject.GetComponents<SphereCollider>().ToList())
        {
            DestroyImmediate(sCollider);
        }
    }
}
