using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionCallBlock : MonoBehaviour
{
    public GameObject functionDefinition;
    public int FunctionID;

    public Queue<UnityEvent> getFunction(){
        QueueReading functionDefinitionQueueReading = functionDefinition.GetComponent<QueueReading>();
        functionDefinitionQueueReading.ReadQueue();
        return functionDefinitionQueueReading.GetBlockQueueOfUnityEvents();
    }
}
