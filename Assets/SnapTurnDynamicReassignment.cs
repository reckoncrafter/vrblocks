using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapTurnDynamicReassignment : MonoBehaviour
{
    public InputActionProperty leftHandSnapTurnAction;
    public InputActionProperty rightHandSnapTurnAction;
    public InputActionReference rightHandGrabAction;
    public InputActionReference leftHandGrabAction;
    private InputActionProperty emptyInputAction = new InputActionProperty();
    private ActionBasedSnapTurnProvider snapTurnProvider;
    private bool rightIsGrabbing = false;
    private bool leftIsGrabbing = false;

    void Start()
    {
        snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();  
        AssignTurnActions();

        rightHandGrabAction.action.started += (ctx) => {rightIsGrabbing=true;AssignTurnActions();};
        rightHandGrabAction.action.canceled += (ctx) => {rightIsGrabbing=false;AssignTurnActions();};
        leftHandGrabAction.action.started += (ctx) => {leftIsGrabbing=true;AssignTurnActions();};
        leftHandGrabAction.action.canceled += (ctx) => {leftIsGrabbing=false;AssignTurnActions();};
    }
    private void AssignTurnActions()
    {
        snapTurnProvider.rightHandSnapTurnAction = rightIsGrabbing? emptyInputAction: rightHandSnapTurnAction;
        snapTurnProvider.leftHandSnapTurnAction = leftIsGrabbing? emptyInputAction: leftHandSnapTurnAction;
    }
}
