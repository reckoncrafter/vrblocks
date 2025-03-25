using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockCount : MonoBehaviour
{
<<<<<<< HEAD
    //[SerializeField] private QueueReading? queueReading; // Attach object that contains this script
    [SerializeField] private ExecutionDirector executionDirector;
    [SerializeField] private TextMeshProUGUI? blockCountText; // Attach TMP text in BlockCount prefab

    void Start()
    {
        //queueReading = GameObject.Find("Block (StartQueue)").GetComponent<QueueReading>();
        executionDirector = GameObject.Find("ExecutionDirector").GetComponent<ExecutionDirector>();
=======
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
>>>>>>> main
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
        if (executionDirector == null || blockCountText == null)
        {
            Debug.LogWarning("ExecutionDirector or Text component is not assigned.");
            return;
        }

<<<<<<< HEAD
        blockCountText.text = $"Block Count: {executionDirector.GetMainListLength()}";
=======
        int blockCount = queueReading.GetBlockQueue().Count;
        blockCountText.text = $"Block Count: {blockCount}";
>>>>>>> main
    }
}
