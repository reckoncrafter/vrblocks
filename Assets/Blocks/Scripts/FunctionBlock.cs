using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class FunctionBlock : MonoBehaviour
{
    public InputActionReference primaryButtonRight;
    public InputActionReference primaryButtonLeft;
    public GameObject functionCallPrefab;
    public Vector3 spawnOffset;
    public int FunctionID;
    public Material selectedForSpawnMaterial;
    public Material defaultMaterial;
    private PlayerUIManager playerUIManager;
    private bool isHovered = false;
    // Start is called before the first frame update
    void Start()
    {
      primaryButtonRight.action.started += OnPrimaryButton;
      primaryButtonLeft.action.started += OnPrimaryButton;
      GetComponent<XRGrabInteractable>().hoverEntered.AddListener(OnHoverEntered);
      GetComponent<XRGrabInteractable>().hoverExited.AddListener(OnHoverExited);
      FunctionID = gameObject.GetInstanceID();
      var FCLabel = transform.Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshProUGUI>();
      FCLabel.text = "Function " + FunctionID.ToString();
      playerUIManager = FindObjectOfType<PlayerUIManager>();
    }

    // void onActivation(){
    //     Queue<UnityEvent> blockQueue = queueReading.GetBlockQueueOfUnityEvents();
    // }

    public void OnHoverEntered(HoverEnterEventArgs hoverEnter){
      isHovered = true;
      //controllerModels.EnableControllerHands(false);
      //Debug.Log("FunctionDefinintionBlock: Hovered");
    }

    public void OnHoverExited(HoverExitEventArgs hoverExit){
      isHovered = false;
      //controllerModels.EnableControllerHands(true);
      //Debug.Log("FunctionDefinintionBlock: Not Hovered");
    }

    public void OnPrimaryButton(InputAction.CallbackContext ctx)
    {
      if(isHovered && playerUIManager != null)
      {
        if(playerUIManager.selectedFunctionBlock != this && playerUIManager.selectedFunctionBlock != null) // other function selected
        {
          playerUIManager.selectedFunctionBlock.SetSelectedVisual(false);
          playerUIManager.selectedFunctionBlock = this;
          playerUIManager.SetFunctionCallButtonStatus(true);
          SetSelectedVisual(true);
        }
        else if(playerUIManager.selectedFunctionBlock == null) // no function selected
        {
          playerUIManager.selectedFunctionBlock = this;
          playerUIManager.SetFunctionCallButtonStatus(true);
          SetSelectedVisual(true);
        }
        else // this function selected
        {
          playerUIManager.selectedFunctionBlock = null;
          playerUIManager.SetFunctionCallButtonStatus(false);
          SetSelectedVisual(false);
        }
      }
    }

    public void SetSelectedVisual(bool status){ GetComponent<Renderer>().material = status ? selectedForSpawnMaterial : defaultMaterial; }
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
