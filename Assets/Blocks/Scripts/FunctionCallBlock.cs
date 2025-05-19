using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FunctionCallBlock : MonoBehaviour
{
    public FunctionBlock functionDefinition;
    public int FunctionID;
    private TextMeshProUGUI textMesh;

    void Start(){
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        //FunctionID = functionDefinition.GetComponent<FunctionBlock>().FunctionID;
        FunctionID = functionDefinition.gameObject.GetInstanceID();
        functionDefinition.OnNameChanged.AddListener( (newName) => {
            textMesh.text = $"Call: {newName}";
        });
    }

    // public Queue<UnityEvent> getFunction(){
    //     QueueReading functionDefinitionQueueReading = functionDefinition.GetComponent<QueueReading>();
    //     functionDefinitionQueueReading.ReadQueue();
    //     return functionDefinitionQueueReading.GetBlockQueueOfUnityEvents();
    // }
}
