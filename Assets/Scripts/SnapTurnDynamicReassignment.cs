using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapTurnDynamicReassignment : MonoBehaviour
{
    public enum TurningMode
    {
        SnapTurn,
        ContinuousTurn
    }    

    public TurningMode turningMode = TurningMode.SnapTurn;
    public InputActionProperty leftHandTurnAction;
    public InputActionProperty rightHandTurnAction;
    public InputActionProperty leftHandSnapTurnAction;
    public InputActionProperty rightHandSnapTurnAction;
    public InputActionReference rightHandGrabAction;
    public InputActionReference leftHandGrabAction;
    private InputActionProperty emptyInputAction = new InputActionProperty();
    private ActionBasedSnapTurnProvider snapTurnProvider;
    private ActionBasedContinuousTurnProvider continuousTurnProvider;
    private bool rightIsGrabbing = false;
    private bool leftIsGrabbing = false;

    void Start()
    {
        snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        AssignTurnActions();

        rightHandGrabAction.action.started += (ctx) => {rightIsGrabbing=true;AssignTurnActions();};
        rightHandGrabAction.action.canceled += (ctx) => {rightIsGrabbing=false;AssignTurnActions();};
        leftHandGrabAction.action.started += (ctx) => {leftIsGrabbing=true;AssignTurnActions();};
        leftHandGrabAction.action.canceled += (ctx) => {leftIsGrabbing=false;AssignTurnActions();};
    }
    public void AssignTurnActions()
    {
        if(turningMode == TurningMode.SnapTurn)
        {
            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;

        }
        else if(turningMode == TurningMode.ContinuousTurn)
        {
            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }

        snapTurnProvider.rightHandSnapTurnAction = rightIsGrabbing? emptyInputAction: rightHandSnapTurnAction;
        snapTurnProvider.leftHandSnapTurnAction = leftIsGrabbing? emptyInputAction: leftHandSnapTurnAction;
        continuousTurnProvider.rightHandTurnAction = rightIsGrabbing? emptyInputAction: rightHandTurnAction;
        continuousTurnProvider.leftHandTurnAction = leftIsGrabbing? emptyInputAction: leftHandTurnAction;
    }
}
