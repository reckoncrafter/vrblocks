#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptManagement : Editor
{
[MenuItem("DevTools/Script Management/Delete All Missing Scripts!")]
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

[MenuItem("/DevTools/Script Management/Delete All Duplicate Components!")]
static void FindAndDeleteDuplicateScripts()
{
    foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>())
    {
        List<System.Type> typesAlreadySeen = new List<System.Type>();
        foreach(Component component in gameObject.GetComponents<Component>())
        {
            if(typesAlreadySeen.Contains(component.GetType()))
            {
                DestroyImmediate(component);
            }
            else
            {
                typesAlreadySeen.Add(component.GetType());
            }
        }    
    }
}
}
#endif