using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Assign the prefab or object you want to spawn
    public Transform spawnLocation; // Where the object will be spawned

    public void Start()
    {
        // Find the button and add an onClick listener
        Button spawnButton = GameObject.Find("SpawnButton").GetComponent<Button>();
        spawnButton.onClick.AddListener(SpawnObject);
    }

    public void SpawnObject()
    {
        if (objectToSpawn != null && spawnLocation != null)
        {
            Instantiate(objectToSpawn, spawnLocation.position, spawnLocation.rotation);
            Debug.Log("Object spawned at: " + spawnLocation.position);
        }
        else
        {
            Debug.LogWarning("Missing objectToSpawn or spawnLocation reference.");
        }
    }
}