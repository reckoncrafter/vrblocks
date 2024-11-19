using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurtleCommand : MonoBehaviour
{

    public UnityEvent onMove;
    // Start is called before the first frame update
    public void onTrigger(){
        onMove.Invoke();
    }
}
