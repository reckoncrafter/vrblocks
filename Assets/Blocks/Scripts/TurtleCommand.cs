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
        renderer.material.SetColor("_EmissionColor", renderer.material.GetColor("_Color"));
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


    // public UnityEvent onMove;
    // public TurtleMovement turtleMovement;
    public Command commandEnum;

    // UnityAction AssignCommand()
    // {
    //     return commandEnum switch
    //     {
    //         Command.MoveForward => turtleMovement.WalkForward,
    //         Command.RotateRight => turtleMovement.RotateRight,
    //         Command.RotateLeft => turtleMovement.RotateLeft,
    //         Command.Jump => turtleMovement.Jump,
    //         Command.IfBegin => turtleMovement.EnqueueIfStatementBegin,
    //         Command.ElseIf => turtleMovement.EnqueueElseIfStatement,
    //         Command.Else => turtleMovement.EnqueueElseStatement,
    //         Command.IfEnd => turtleMovement.EnqueueIfStatementEnd,
    //         Command.WhileBegin => turtleMovement.EnqueueWhileStatementBegin,
    //         Command.WhileEnd => turtleMovement.EnqueueWhileStatementEnd,
    //         Command.ConditionTrue => turtleMovement.setConditionTrue,
    //         Command.ConditionFalse => turtleMovement.setConditionFalse,
    //         Command.Other => () => { },
    //         _ => () => { }
    //         ,
    //     };
    // }
    //
    // void Start(){
    //     turtleMovement = GameObject.Find("/MapSpawner/Turtle").GetComponent<TurtleMovement>();
    //
    //     if(turtleMovement != null){
    //         onMove.AddListener(AssignCommand());
    //     }
    //     else{
    //         Debug.Log("Block did not connect to Turtle.");
    //     }
    // }
    // Start is called before the first frame update
    // public void onTrigger(){
    //     onMove.Invoke();
    // }

}
