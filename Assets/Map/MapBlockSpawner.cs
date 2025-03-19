using UnityEngine;

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
    private void SpawnEntities()
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
        GameObject generatedGoalObject = Instantiate(goalObject, goalCoords, Quaternion.identity);
        generatedGoalObject.transform.SetParent(gameObject.transform, false);
        generatedGoalObject.transform.localScale = mapValues.goalScale;
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
}
