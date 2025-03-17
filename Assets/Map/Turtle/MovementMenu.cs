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

        if (GUILayout.Button("Start Queue"))
        {
            turtleMovement.StartQueue();
        }
        if (GUILayout.Button("Reset"))
        {
            turtleMovement.Reset();
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
            turtleMovement.EnqueueIfStatementBegin();
        }
        if (GUILayout.Button("Insert ElseStatement"))
        {
            turtleMovement.EnqueueElseStatement();
        }
        if (GUILayout.Button("Insert IfStatementEnd"))
        {
            turtleMovement.EnqueueIfStatementEnd();
        }
        if (GUILayout.Button("Insert setConditionTrue"))
        {
            turtleMovement.setConditionTrue();
        }
        if (GUILayout.Button("Insert setConditionFalse"))
        {
            turtleMovement.setConditionFalse();
        }
        if (GUILayout.Button("Insert WhileStatementBegin"))
        {
            turtleMovement.EnqueueWhileStatementBegin();
        }
        if (GUILayout.Button("Insert WhileStatementEnd"))
        {
            turtleMovement.EnqueueWhileStatementEnd();
        }
        if (GUILayout.Button("Insert ConditionFacingWall"))
        {
            turtleMovement.ConditionFacingWall();
        }
    }
}
#endif
