using UnityEngine;

public class MapBlockSpawner : MonoBehaviour
{
    public GameObject entityToSpawn;

    public MapBlockScriptableObject mapBlockValues;

    void Start()
    {
        SpawnEntities();
    }

    void SpawnEntities()
    {
        // Vector3 startPositionOffset = gameObject.transform.position + mapBlockValues.blockScale / 2;
        Vector3 startPositionOffset = mapBlockValues.blockScale / 2;

        for (int i = 0; i < mapBlockValues.spawnPoints.Length; i++)
        {
            Vector3 coords = startPositionOffset + Vector3.Scale(mapBlockValues.spawnPoints[i], mapBlockValues.blockScale);
            GameObject currentEntity = Instantiate(entityToSpawn, coords, Quaternion.identity);

            currentEntity.transform.SetParent(gameObject.transform, false);

            currentEntity.name = mapBlockValues.prefabName + i;
            currentEntity.transform.localScale = mapBlockValues.blockScale;
        }
    }
}