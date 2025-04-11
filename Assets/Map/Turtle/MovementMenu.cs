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
            turtleMovement.canReset = true;
            turtleMovement.Reset();
        }
        if (GUILayout.Button("Move Forward"))
        {
            turtleMovement.canFail = true;
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
    }
}
#endif
