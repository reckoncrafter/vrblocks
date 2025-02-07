#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(QueueReading))]
public class QueueReadingMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        QueueReading queueReading = (QueueReading)target;
        if (GUILayout.Button("Read Queue"))
        {
            queueReading.ReadQueue();
        }
    }
}
#endif
