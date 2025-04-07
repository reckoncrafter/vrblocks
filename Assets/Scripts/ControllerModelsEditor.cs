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
            controllerModels.EnableControllerModel(false, true);
            controllerModels.EnableControllerModel(false, false);
        }
        if (GUILayout.Button("Disable Hands"))
        {
            controllerModels.EnableControllerModel(true, true);
            controllerModels.EnableControllerModel(true, false);
        }
    }
}
#endif
