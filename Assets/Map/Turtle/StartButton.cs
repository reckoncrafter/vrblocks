using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public TurtleMovement turtleMovement;
    public BlockWalker blockWalker;


    public void SetGlowEffect(bool isGlow){
        Renderer renderer = GetComponent<Renderer>();
        if(renderer != null){
            Material material = renderer.material;

            if(isGlow){
                material.SetColor("_EmissionColor", Color.green);
                material.EnableKeyword("_EMISSION");
            }
            else{
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
            }
        }
        else{
            Debug.LogWarning("Renderer not found on GameObject");
        }
    }

    public void CaptureQueueAndExecute(){
        blockWalker.onActivation();
        foreach(var unityEvent in blockWalker.ActionList) {
            unityEvent.Invoke();
        }
        turtleMovement.StartQueue();
    }
}
