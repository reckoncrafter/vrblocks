using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionCallBlock : MonoBehaviour
{
    public FunctionBlock functionDefinition;
    public int FunctionID;

    void Start(){
        //FunctionID = functionDefinition.GetComponent<FunctionBlock>().FunctionID;
        FunctionID = functionDefinition.gameObject.GetInstanceID();
    }

    // public Queue<UnityEvent> getFunction(){
    //     QueueReading functionDefinitionQueueReading = functionDefinition.GetComponent<QueueReading>();
    //     functionDefinitionQueueReading.ReadQueue();
    //     return functionDefinitionQueueReading.GetBlockQueueOfUnityEvents();
    // }
}
