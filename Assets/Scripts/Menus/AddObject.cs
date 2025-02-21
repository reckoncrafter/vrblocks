using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddObject : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab; // Assign corresponding block in inspector
    [SerializeField] private Transform spawnParent; // Assign "MoveableEntities" as spawn position in hierarchy if that's how we're moving with it.
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0); // Offset to avoid overlap

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(SpawnBlock);
        }
        else
        {
            Debug.LogError("AddObject script must be attached to a UI Button.");
        }
    }

    private void SpawnBlock()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("No blockPrefab assigned to " + gameObject.name);
            return;
        }

        Transform buttonTransform = GetComponent<Transform>();
        Transform parentTransform = spawnParent != null ? spawnParent : null;

        GameObject newBlock = Instantiate(
            blockPrefab,
            buttonTransform.position + spawnOffset, // Spawning based on button position
            buttonTransform.rotation,
            parentTransform
        );

        newBlock.name = blockPrefab.name; // Because I use strings for block queue.

        Debug.Log("Spawned: " + newBlock.name + " at " + newBlock.transform.position);
    }
}
