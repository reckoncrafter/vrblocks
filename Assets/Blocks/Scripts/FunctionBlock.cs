using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class FunctionBlock : MonoBehaviour
{
    public InputActionReference primaryButton;
    public GameObject functionCallPrefab;
    public Vector3 spawnOffset;
    public int FunctionID;

    private QueueReading queueReading;
    private bool isHovered = false;
    // Start is called before the first frame update
    void Start()
    {
      primaryButton.action.started += onPrimaryButton;
      queueReading = gameObject.GetComponent<QueueReading>();
      FunctionID = gameObject.GetInstanceID();
    }

    void onActivation(){
        Queue<UnityEvent> blockQueue = queueReading.GetBlockQueueOfUnityEvents();
    }

    public void OnHoverEntered(){
      isHovered = true;
      //Debug.Log("FunctionDefinintionBlock: Hovered");
    }

    public void OnHoverExited(){
      isHovered = false;
      //Debug.Log("FunctionDefinintionBlock: Not Hovered");
    }

    void onPrimaryButton(InputAction.CallbackContext context){
      if(isHovered){
        spawnCallBlock();
      }
    }

    void spawnCallBlock(){
      var transform = GetComponent<Transform>();
      var blockEntity = GameObject.Find("MoveableEntities/BlockEntity").GetComponent<Transform>();
      GameObject newFunctionCall = Instantiate(functionCallPrefab, transform.position + spawnOffset, transform.rotation, blockEntity);
      newFunctionCall.name = "Block (FunctionCall)";
      FunctionCallBlock fcb = newFunctionCall.AddComponent(typeof(FunctionCallBlock)) as FunctionCallBlock;
      fcb.functionDefinition = gameObject;
      var FCLabel = newFunctionCall.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshProUGUI>();
      FCLabel.text = "Call " + FunctionID.ToString();
      fcb.GetComponent<FunctionCallBlock>().FunctionID = FunctionID;
    }
}

/*
 * There are two blocks, the Function Definition and the Function Call.
 * The Function Definition begins a separate stack of blocks from the main stack.
 * It will generate an associated Function Call, which contains a reference to the
 * Function Definition.
 * The Function Call will be included in the main stack.
 * Logic:
 * - QueueReading encounters a Function Call.
 * - Function Call contains a reference to it's associated Function Definition
 * - QueueReading uses this reference to execute the queue reader of the Function Definition.
 * - QueueReading pushes the resulting list onto the queue.
 *
 * The only problem left is how to have multiple functions and have their blocks be
 * distinct to the user.
*/
