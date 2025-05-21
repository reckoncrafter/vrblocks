using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class FunctionBlock : MonoBehaviour
{
    public InputActionReference primaryButtonRight;
    public InputActionReference primaryButtonLeft;

    public InputActionReference secondaryButtonRight;
    public InputActionReference secondaryButtonLeft;
    public GameObject functionCallPrefab;
    public Vector3 spawnOffset;
    public int FunctionID;
    public Material selectedForSpawnMaterial;
    public Material defaultMaterial;
    private PlayerUIManager playerUIManager;
    private bool isHovered = false;

    [Space (10)]
    public string functionName = "";
    private TextMeshProUGUI textMesh;
    public UnityEvent<string> OnNameChanged = new UnityEvent<string>();
    void Start()
    {
      primaryButtonRight.action.started += OnPrimaryButton;
      primaryButtonLeft.action.started += OnPrimaryButton;
      secondaryButtonRight.action.started += OnSecondaryButton;
      secondaryButtonLeft.action.started += OnSecondaryButton;

      textMesh = GetComponentInChildren<TextMeshProUGUI>();

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
      playerUIManager.blockMenuAction.action.Disable();
      //controllerModels.EnableControllerHands(false);
      //Debug.Log("FunctionDefinintionBlock: Hovered");
    }

    public void OnHoverExited(HoverExitEventArgs hoverExit){
      isHovered = false;
      playerUIManager.blockMenuAction.action.Enable();
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

    public void OnSecondaryButton(InputAction.CallbackContext ctx)
    {
      if(isHovered)
      {
        VRKeys.Keyboard keyboard = FindObjectOfType<VRKeys.Keyboard>();
        keyboard.Enable();
        keyboard.SetText(functionName);
        ControllerModels cm = FindObjectOfType<ControllerModels>();
        cm.EnableControllerModel(true, true);
        cm.EnableControllerModel(true, false);

        void disable_keyboard(){
          cm.EnableControllerModel(false, true);
          cm.EnableControllerModel(false, false);
          keyboard.Disable();
          keyboard.OnCancel.RemoveAllListeners();
          keyboard.OnSubmit.RemoveAllListeners(); 
        };

        keyboard.OnCancel.AddListener( () => {
          disable_keyboard();
        } );
        
        keyboard.OnSubmit.AddListener( (submitText) => {
          functionName = submitText;
          textMesh.text = functionName;
          if (functionName == ""){ textMesh.text = FunctionID.ToString(); }
          OnNameChanged.Invoke(submitText);
          disable_keyboard();
        } );
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
