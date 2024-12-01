using UnityEngine;

[ExecuteInEditMode]
public class MapBlockSpawner : MonoBehaviour
{
    public GameObject mapBlock;
    public GameObject goalSphere;
    public TurtleMovement turtle;
    public MapBlockScriptableObject mapValues;

    void Start()
    {
        // delete children to spawn again
        if (transform.childCount == 0)
        {
            SpawnEntities();
        }
    }
    private void SpawnEntities()
    {
        Vector3 startPositionOffset = mapValues.blockScale / 2;

        GameObject currentEntity = null;
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
        GameObject generatedGoalSphere = Instantiate(goalSphere, goalCoords, Quaternion.identity);
        generatedGoalSphere.transform.SetParent(gameObject.transform, false);
        generatedGoalSphere.transform.localScale = mapValues.goalScale;
        generatedGoalSphere.name = mapValues.goalPrefabName;
        //

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
    }
}
