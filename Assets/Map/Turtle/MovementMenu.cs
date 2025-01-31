using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TurtleMovement))]
public class MovementMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TurtleMovement turtleMovement = (TurtleMovement)target;

        if (GUILayout.Button("Start Queue"))
        {
            turtleMovement.StartQueue();
        }
        if (GUILayout.Button("Move Forward"))
        {
            turtleMovement.WalkForward();
        }
        if (GUILayout.Button("Turn Left"))
        {
            turtleMovement.RotateLeft();
        }
        if (GUILayout.Button("Turn Right"))
        {
            turtleMovement.RotateRight();
        }
        if (GUILayout.Button("Jump"))
        {
            turtleMovement.Jump();
        }

        if (GUILayout.Button("Insert IfStatementBegin"))
        {
            turtleMovement.EnqueueConditional("IfStatementBegin");
        }
        if (GUILayout.Button("Insert IfStatementEnd"))
        {
            turtleMovement.EnqueueConditional("IfStatementEnd");
        }
        if (GUILayout.Button("Insert setConditionTrue"))
        {
            turtleMovement.EnqueueConditional("setConditionTrue");
        }
        if (GUILayout.Button("Insert setConditionFalse"))
        {
            turtleMovement.EnqueueConditional("setConditionFalse");
        }
        if (GUILayout.Button("Insert WhileStatementBegin"))
        {
            turtleMovement.EnqueueConditional("WhileStatementBegin");
        }
        if (GUILayout.Button("Insert WhileStatmentEnd"))
        {
            turtleMovement.EnqueueConditional("WhileStatementEnd");
        }

        if(GUILayout.Button("Directly setConditionTrue")){
            turtleMovement.setConditionTrue();
        }
        if(GUILayout.Button("Directly setConditionFalse")){
            turtleMovement.setConditionFalse();
        }
    }
}
