using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IncompleteConditionalHandler : MonoBehaviour
{
    public Material offendingStateMaterial;
    private Material defaultMaterial;
    private Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
        defaultMaterial = renderer.material;
    }

    public void SetOffendingState(bool status)
    {
        if(status)
        {
            renderer.material = offendingStateMaterial;
        }
        else
        {
            renderer.material = defaultMaterial;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IncompleteConditionalHandler))]
public class IncompleteConditionalHandlerMenu : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        IncompleteConditionalHandler ich = (IncompleteConditionalHandler)target;
        if (GUILayout.Button("Set Offending State"))
        {
            ich.SetOffendingState(true);
        }
    }
}
#endif
