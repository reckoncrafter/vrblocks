using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class TurtleCommand : MonoBehaviour
{

    public UnityEvent onMove;
    private Material defaultMaterial;
    private Renderer renderer;
    // Start is called before the first frame update
    void Start(){
        renderer = gameObject.GetComponent<Renderer>();
        defaultMaterial = renderer.material;

    }

    public void onTrigger(){
        onMove.Invoke();
    }

    public void setOffendingState(bool status){
        // TODO: Fix material fetch
        Material offendingStateMaterial = Resources.Load("OffendingState", typeof(Material)) as Material;
        if(status){
            renderer.material = offendingStateMaterial;
        }
        else{
            renderer.material = defaultMaterial;
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(TurtleCommand))]
public class TurtleCommandMenu : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        TurtleCommand turtleCommand = (TurtleCommand)target;
        if (GUILayout.Button("Set Offending State"))
        {
            turtleCommand.setOffendingState(true);
        }
    }
}
#endif
