using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MapBlockSpawner : MonoBehaviour
{
    public GameObject mapBlock;
    public TurtleMovement turtle;

    public MapBlockScriptableObject mapValues;

    void Start()
    {
        SpawnEntities();
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