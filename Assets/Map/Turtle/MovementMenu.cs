#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TurtleMovement))]
public class MovementMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TurtleMovement turtleMovement = (TurtleMovement)target;


        if (GUILayout.Button("Reset"))
        {
            turtleMovement.Reset();
        }
        if (GUILayout.Button("Move Forward"))
        {
            turtleMovement.PerformWalkForward();
        }
        if (GUILayout.Button("Turn Left"))
        {
            turtleMovement.PerformRotateLeft();
        }
        if (GUILayout.Button("Turn Right"))
        {
            turtleMovement.PerformRotateRight();
        }
        if (GUILayout.Button("Jump"))
        {
            turtleMovement.shouldJump = true;
            turtleMovement.FixedUpdate();
        }

        if (GUILayout.Button("Jump + Move Forward"))
        {
            turtleMovement.shouldJump = true;
            turtleMovement.afterJumpAction = turtleMovement.PerformWalkForward;
            turtleMovement.FixedUpdate();
        }
        if (GUILayout.Button("Jump + Turn Left"))
        {
            turtleMovement.shouldJump = true;
            turtleMovement.afterJumpAction = turtleMovement.PerformRotateLeft;
            turtleMovement.FixedUpdate();
        }
        if (GUILayout.Button("Jump + Turn Right"))
        {
            turtleMovement.shouldJump = true;
            turtleMovement.afterJumpAction = turtleMovement.PerformRotateRight;
            turtleMovement.FixedUpdate();
        }
    }
}
#endif
