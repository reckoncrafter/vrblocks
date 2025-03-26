using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockCount : MonoBehaviour
{
    //[SerializeField] private QueueReading? queueReading; // Attach object that contains this script
    [SerializeField] private ExecutionDirector executionDirector;
    [SerializeField] private TextMeshProUGUI? blockCountText; // Attach TMP text in BlockCount prefab

    void Start()
    {
        //queueReading = GameObject.Find("Block (StartQueue)").GetComponent<QueueReading>();
        executionDirector = GameObject.Find("/ExecutionDirector").GetComponent<ExecutionDirector>();
        BlockSnapping.blockSnapEvent.AddListener(UpdateBlockCount);
    }
    private void UpdateBlockCount()
    {
        if (executionDirector == null || blockCountText == null)
        {
            Debug.LogWarning("ExecutionDirector or Text component is not assigned.");
            return;
        }

        blockCountText.text = $"Block Count: {executionDirector.GetMainListLength()}";
    }
}
