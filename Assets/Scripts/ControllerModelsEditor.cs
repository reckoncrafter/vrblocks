#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(ControllerModels))]
public class ControllerModelsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ControllerModels controllerModels = (ControllerModels)target;
        if (GUILayout.Button("Enable Hands"))
        {
            controllerModels.EnableControllerHands(true);
        }
        if (GUILayout.Button("Disable Hands"))
        {
            controllerModels.EnableControllerHands(false);
        }
    }
}
#endif
