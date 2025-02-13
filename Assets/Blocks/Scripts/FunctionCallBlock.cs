using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionCallBlock : MonoBehaviour
{
    public GameObject functionDefinition;

    Queue<UnityEvent> getFunction(){
        QueueReading functionDefinitionQueueReading = functionDefinition.GetComponent<QueueReading>();

        return functionDefinitionQueueReading.GetBlockQueueOfUnityEvents();
    }
}
