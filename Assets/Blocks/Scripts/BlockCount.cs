using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockCount : MonoBehaviour
{
    [SerializeField] private QueueReading queueReading; // Attach object that contains this script
    [SerializeField] private TextMeshProUGUI blockCountText; // Attach TMP text in BlockCount prefab

    void Start()
    {
        queueReading = GameObject.Find("Block (StartQueue)").GetComponent<QueueReading>();

        if (queueReading != null)
        {
            queueReading.OnQueueUpdated += UpdateBlockCount;
            UpdateBlockCount();
        }
        else
        {
            Debug.LogWarning("QueueReading component not found!");
        }
    }

    private void OnDestroy()
    {
        if (queueReading != null)
        {
            queueReading.OnQueueUpdated -= UpdateBlockCount;
        }
    }

    private void UpdateBlockCount()
    {
        if (queueReading == null || blockCountText == null)
        {
            Debug.LogWarning("QueueReading or Text component is not assigned.");
            return;
        }

        int blockCount = queueReading.GetBlockQueue().Count;
        blockCountText.text = $"Block Count: {blockCount}";
    }
}
