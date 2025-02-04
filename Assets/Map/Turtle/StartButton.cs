using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartButton : MonoBehaviour
{
    public TurtleMovement turtleMovement;
    public QueueReading queueReading;

    public void Start(){
        turtleMovement = FindObjectOfType<TurtleMovement>();
        queueReading = FindObjectOfType<QueueReading>();
    }


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
        queueReading.ReadQueue();
        Queue<UnityEvent> eventQueue = queueReading.GetBlockQueueOfUnityEvents();
        foreach(var unityEvent in eventQueue) {
            unityEvent.Invoke();
        }
        turtleMovement.StartQueue();
    }
}
