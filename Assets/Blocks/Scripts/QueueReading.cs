using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QueueReading : MonoBehaviour
{
    // Queue to hold the block types (FIFO order)
    private readonly Queue<string> blockQueue = new Queue<string>();
    private readonly Queue<UnityEvent> eventQueue = new Queue<UnityEvent>();
    private FunctionBlock functionBlock;
    public void ReadQueue()
    {
        Debug.Log("Starting Queue Reading...");

        functionBlock = gameObject.GetComponent<FunctionBlock>();

        // Clear previous readings
        blockQueue.Clear();
        eventQueue.Clear();

        // Start reading from the Queue Block
        ReadBlocks(gameObject);

        // Used for debugging, will remove later.
        if (blockQueue.Count == 0)
        {
            Debug.LogWarning("No blocks were detected in the queue.");
        }
        else
        {
            Debug.Log("Queue Reading Complete. Queue contents:");
            foreach (var blockType in blockQueue)
            {
                Debug.Log(blockType);
            }
        }
    }

    private void ReadBlocks(GameObject currentBlock)
    {
        // Get the SnappedForwarding component from the current block
        SnappedForwarding snappedForwarding = currentBlock.GetComponentInChildren<SnappedForwarding>();
        if (snappedForwarding == null || snappedForwarding.ConnectedBlock == null)
        {
            Debug.Log($"No SnappedForwarding component or no connected block found for {currentBlock.name}. Stopping traversal.");
            return;
        }

        // Get the connected block from the SnappedForwarding component
        GameObject connectedBlock = snappedForwarding.ConnectedBlock;

        // Get the block type from the connected block's name
        string blockType = connectedBlock.name;

        Debug.Log($"Detected connected block: {connectedBlock.name} with type: {blockType}");

        // if block is function, get function contents
        if(blockType == "Block (FunctionCall)"){
            if(functionBlock){
                int connectedID = connectedBlock.GetComponent<FunctionCallBlock>().FunctionID;
                int thisID = functionBlock.FunctionID;
                if (connectedID == thisID){
                    Debug.Log("Block (Function): QueueReading: Recursion Detected! Aborting!");
                    return;
                }
            }
            Queue<UnityEvent> functionQueue = connectedBlock.GetComponent<FunctionCallBlock>().getFunction();
            while(functionQueue.Count > 0){
                eventQueue.Enqueue(functionQueue.Dequeue());
            }
        }
        else{
            eventQueue.Enqueue(connectedBlock.GetComponent<TurtleCommand>().onMove);
        }
        // Add the block type to the queue
        blockQueue.Enqueue(blockType);

        // Recursively read the next block in the queue
        ReadBlocks(connectedBlock);
    }

    // Public method to retrieve the queue for execution
    public Queue<string> GetBlockQueue()
    {
        return new Queue<string>(blockQueue);
    }

    public Queue<UnityEvent> GetBlockQueueOfUnityEvents(){
        return new Queue<UnityEvent>(eventQueue);
    }
}
