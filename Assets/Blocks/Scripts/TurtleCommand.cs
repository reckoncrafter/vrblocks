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
        Else,
        IfEnd,
        WhileBegin,
        WhileEnd,
        ConditionTrue,
        ConditionFalse
    };


    public UnityEvent onMove;
    public TurtleMovement turtleMovement;
    public Command commandEnum;

    UnityAction AssignCommand(){
        switch(commandEnum)
        {
            case Command.MoveForward:
                return turtleMovement.WalkForward;

            case Command.RotateRight:
                return turtleMovement.RotateRight;

            case Command.RotateLeft:
                return turtleMovement.RotateLeft;

            case Command.Jump:
                return turtleMovement.Jump;

            case Command.IfBegin:
                return turtleMovement.EnqueueIfStatementBegin;

            case Command.Else:
                return turtleMovement.EnqueueElseStatement;

            case Command.IfEnd:
                return turtleMovement.EnqueueIfStatementEnd;

            case Command.WhileBegin:
                return turtleMovement.EnqueueWhileStatementBegin;

            case Command.WhileEnd:
                return turtleMovement.EnqueueWhileStatementEnd;

            case Command.ConditionTrue:
                return turtleMovement.setConditionTrue;

            case Command.ConditionFalse:
                return turtleMovement.setConditionFalse;

        }
        return () => {};
    }

    void Start(){
        turtleMovement = GameObject.Find("/MapSpawner/Turtle").GetComponent<TurtleMovement>();

        if(turtleMovement != null){
            Debug.Log("Block connected successfully to Turtle.");
            onMove.AddListener(AssignCommand());
        }
        else{
            Debug.Log("Block did not connect to Turtle.");
        }
    }
    // Start is called before the first frame update
    public void onTrigger(){
        onMove.Invoke();
    }

}
