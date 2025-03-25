using System;
using UnityEngine;
using UnityEngine.Events;

public class TurtleCommand : MonoBehaviour
{
    public enum Command
    {
        MoveForward,
        RotateRight,
        RotateLeft,
        Jump,
        IfBegin,
        ElseIf,
        Else,
        IfEnd,
        WhileBegin,
        WhileEnd,
        ConditionTrue,
        ConditionFalse,
        ConditionFacingWall,
        ConditionFacingCliff,
        ConditionFacingStepDown,
        CommandError
    };

    private Renderer renderer;
    public Material defaultMaterial;
    public Material? offendingStateMaterial;

    public void Start()
    {
        renderer = GetComponent<Renderer>();
        defaultMaterial = renderer.material;
        offendingStateMaterial = Resources.Load("Materials/OffendingState") as Material;
    }

    public void SetOffendingState(bool state)
    {
        if(state)
        {
            renderer.material = offendingStateMaterial;
        }
        else
        {
            renderer.material = defaultMaterial;
        }
    }

    public Command commandEnum;

}
