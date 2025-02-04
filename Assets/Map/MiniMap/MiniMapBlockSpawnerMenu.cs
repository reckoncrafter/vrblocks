using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MiniMapBlockSpawner))]
public class MiniMapBlockSpawnerMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MiniMapBlockSpawner miniMapBlockSpawner = (MiniMapBlockSpawner)target;

        if (GUILayout.Button("Spawn Entities"))
        {
            if (miniMapBlockSpawner.transform.childCount == 0)
            {
                miniMapBlockSpawner.SpawnEntities();
            }
            else 
            {
                Debug.Log("MiniMap GameObject contains children. Skiping Spawn");
            }
        }
        if (GUILayout.Button("Clear Children"))
        {
            miniMapBlockSpawner.ClearMap();
        }
    }
}