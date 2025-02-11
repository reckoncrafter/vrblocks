using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSpawner : MonoBehaviour, IPointerClickHandler
{
    public GameObject blockPrefab; // Assign the specific block prefab in Inspector
    public float spawnDistance = 1f; // Distance in front of the player

    public void OnPointerClick(PointerEventData eventData)
    {
        SpawnObject();
    }

    private void SpawnObject()
    {
        if (blockPrefab != null && Camera.main != null)
        {
            // Calculate spawn position in front of the player
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;

            // Instantiate the block at the calculated position
            Instantiate(blockPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Spawned: " + blockPrefab.name + " at " + spawnPos);
        }
        else
        {
            Debug.LogWarning("Missing blockPrefab or Camera.main is not set.");
        }
    }
}

