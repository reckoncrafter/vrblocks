using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionBlock : MonoBehaviour
{
    QueueReading queueReading;
    // Start is called before the first frame update
    void Start()
    {
      queueReading = gameObject.GetComponent<QueueReading>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void onActivation(){
        Queue<UnityEvent> blockQueue = queueReading.GetBlockQueueOfUnityEvents();
    }
}

/*
 * There are two blocks, the Function Definition and the Function Call.
 * The Function Definition begins a separate stack of blocks from the main stack.
 * It will generate an associated Function Call, which contains a reference to the
 * Function Definition.
 * The Function Call will be included in the main stack.
 * Logic:
 * - QueueReading encounters a Function Call.
 * - Function Call contains a reference to it's associated Function Definition
 * - QueueReading uses this reference to execute the queue reader of the Function Definition.
 * - QueueReading pushes the resulting list onto the queue.
 *
 * The only problem left is how to have multiple functions and have their blocks be
 * distinct to the user.
*/
