using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class QueueReading : MonoBehaviour
{
    // Queue to hold the block types (FIFO order)
    private Queue<string> blockQueue = new Queue<string>();
    private Queue<UnityEvent> eventQueue = new Queue<UnityEvent>();
    public void ReadQueue()
    {
        Debug.Log("Starting Queue Reading...");

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

    private Stack<GameObject> incompleteIfStatements = new Stack<GameObject>();
    private Stack<GameObject> incompleteWhileStatements = new Stack<GameObject>();
    private void ReadBlocks(GameObject currentBlock)
    {
        // Get the SnappedForwarding component from the current block
        SnappedForwarding snappedForwarding = currentBlock.GetComponentInChildren<SnappedForwarding>();
        if (snappedForwarding == null || snappedForwarding.ConnectedBlock == null)
        {
            Debug.Log($"No SnappedForwarding component or no connected block found for {currentBlock.name}. Stopping traversal.");

            // TODO: setOffendingState not being triggered. Invalid references in stack?
            if(incompleteIfStatements.Count > 0){
                Debug.Log($"Found {incompleteIfStatements.Count} incomplete If Statements");
                foreach(GameObject block in incompleteIfStatements){
                    block.GetComponent<TurtleCommand>().setOffendingState(true);
                }
            }
            if(incompleteWhileStatements.Count > 0){
                Debug.Log($"Found {incompleteWhileStatements.Count} incomplete While Statements");
                foreach(GameObject block in incompleteWhileStatements){
                    block.GetComponent<TurtleCommand>().setOffendingState(true);
                }
            }
            return;
        }

        // Get the connected block from the SnappedForwarding component
        GameObject connectedBlock = snappedForwarding.ConnectedBlock;

        // Get the block type from the connected block's name
        string blockType = connectedBlock.name;

        Debug.Log($"Detected connected block: {connectedBlock.name} with type: {blockType}");

        // if block is if or while, check if it is complete

        // TODO: Fix currentBlock being null
        if(blockType == "Block (IfBegin)"){
            incompleteIfStatements.Push(currentBlock);
        }
        if(blockType == "Block (IfEnd)"){
            try{
                incompleteIfStatements.Pop();
            }
            catch(Exception e) when (e is InvalidOperationException){
                currentBlock.GetComponent<TurtleCommand>().setOffendingState(true);
                Debug.Log("Lone IfEnd Detected");
            }
        }

        if(blockType == "Block (WhileBegin)"){
            incompleteWhileStatements.Push(currentBlock);
        }
        if(blockType == "Block (WhileEnd)"){
            try{
                incompleteWhileStatements.Pop();
            }
            catch(Exception e) when (e is InvalidOperationException){
                currentBlock.GetComponent<TurtleCommand>().setOffendingState(true);
                Debug.Log("Lone WhileEnd Detected");
            }
        }

        // if block is function, get function contents
        if(blockType == "Block (FunctionCall)"){
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
