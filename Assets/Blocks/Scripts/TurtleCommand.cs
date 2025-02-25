using System;
using UnityEngine;
using UnityEngine.Events;

public class TurtleCommand : MonoBehaviour
{

    public UnityEvent onMove;

    public void onTrigger(){
        onMove.Invoke();
    }

}
