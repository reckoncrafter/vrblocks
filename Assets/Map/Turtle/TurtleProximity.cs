using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleProximity : MonoBehaviour
{
    private SphereCollider sphereCollider;

    public bool isTouchingMapBlock = false;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    public void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"TurtleProximity: Collided with {collision.gameObject.tag}");
        if(collision.gameObject.tag == "MapBlock")
        {
            isTouchingMapBlock = true;
        }
    }

    public void OnTriggerExit()
    {
        isTouchingMapBlock = false;
    }
}
