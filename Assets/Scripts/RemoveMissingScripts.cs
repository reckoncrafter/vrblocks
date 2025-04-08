using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts : Editor
{
[MenuItem("DevTools/Delete All Missing Scripts!")]
static void FindAndDeleteMissingScripts()
{
    foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>())
    {
        foreach (Component component in gameObject.GetComponentsInChildren<Component>())
        {
            if (component == null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                break;
            }
        }
    }
}
}
