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
        if (GUILayout.Button("Start Queue"))
        {
            turtleMovement.StartQueue();
        }
    }
}
