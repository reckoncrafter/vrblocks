using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class QueueReading : MonoBehaviour
{
    // Queue to hold the block types (FIFO order)

    private readonly Queue<string> blockQueue = new();
    private readonly Queue<UnityEvent> eventQueue = new();
    private readonly Stack<GameObject> incompleteConditionals = new();
    private FunctionBlock? functionBlock;

    public void ReadQueue()
    {
        Debug.Log("Starting Queue Reading...");

        functionBlock = gameObject.GetComponent<FunctionBlock>();

        // Clear previous readings
        blockQueue.Clear();
        eventQueue.Clear();

        // set textures of incomplete conditional blocks back to normal before clearing the stack.
        foreach (GameObject b in incompleteConditionals)
        {
            b.GetComponent<IncompleteConditionalHandler>().SetOffendingState(false);
        }

        incompleteConditionals.Clear();

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

            // check for remaining incomplete statments
            if (incompleteConditionals.Count > 0)
            {
                incompleteConditionals.Pop().GetComponent<IncompleteConditionalHandler>().SetOffendingState(true);
            }
            return;
        }

        // Get the connected block from the SnappedForwarding component
        GameObject connectedBlock = snappedForwarding.ConnectedBlock;

        // Get the block type from the connected block's name
        string blockType = connectedBlock.name;

        Debug.Log($"Detected connected block: {connectedBlock.name} with type: {blockType}");

        // incomplete conditional statement detection
        if (blockType == "Block (IfBegin)" || blockType == "Block (WhileBegin)")
        {
            incompleteConditionals.Push(connectedBlock);
        }

        Action<string, string, bool> CheckEnds = (string EndBlockName, string BeginBlockName, bool peek) =>
        {
            if (blockType == EndBlockName)
            {
                try
                {
                    GameObject b = peek ? incompleteConditionals.Peek() : incompleteConditionals.Pop();
                    if (b.name != BeginBlockName)
                    {
                        incompleteConditionals.Push(connectedBlock);
                    }
                }
                catch (Exception e) when (e is InvalidOperationException)
                {
                    incompleteConditionals.Push(connectedBlock);
                }
            }
        };

        CheckEnds("Block (IfEnd)", "Block (IfBegin)", false);
        CheckEnds("Block (WhileEnd)", "Block (WhileBegin)", false);
        CheckEnds("Block (Else)", "Block (IfBegin)", true);
        CheckEnds("Block (ElseIf)", "Block (IfBegin)", true);

        // if block is function, get function contents
        if (blockType == "Block (FunctionCall)")
        {
            if (functionBlock != null)
            {
                int connectedID = connectedBlock.GetComponent<FunctionCallBlock>().FunctionID;
                int thisID = functionBlock.FunctionID;
                if (connectedID == thisID)
                {
                    Debug.Log("Block (Function): QueueReading: Recursion Detected! Aborting!");
                    return;
                }
            }
            Queue<UnityEvent> functionQueue = connectedBlock.GetComponent<FunctionCallBlock>().getFunction();
            while (functionQueue.Count > 0)
            {
                eventQueue.Enqueue(functionQueue.Dequeue());
            }
        }
        else
        {
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

    public Queue<UnityEvent> GetBlockQueueOfUnityEvents()
    {
        return new Queue<UnityEvent>(eventQueue);
    }
}
