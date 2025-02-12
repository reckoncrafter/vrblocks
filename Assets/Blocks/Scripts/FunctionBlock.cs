using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionBlock : MonoBehaviour
{
    QueueReading queueReading = gameObject.GetComponent<QueueReading>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FunctionBegin(){}
    public void FunctionEnd(){}

    void onActivation(){
        Queue<string> blockQueue = queueReading.GetBlockQueue();
    }
}
