using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockCount : MonoBehaviour
{
    [SerializeField] private QueueReading? queueReading; // Attach object that contains this script
    [SerializeField] private TextMeshProUGUI? blockCountText; // Attach TMP text in BlockCount prefab

    void Start()
    {
        queueReading = GameObject.Find("Block (StartQueue)").GetComponent<QueueReading>();
    }

    private void Update()
    {
        if (queueReading == null || blockCountText == null)
        {
            Debug.LogWarning("QueueReading or Text component is not assigned.");
            return;
        }

        Queue<string> blockQueue = queueReading.GetBlockQueue();
        int blockCount = blockQueue.Count;

        blockCountText.text = $"Block Count: {blockCount}";
    }
}
