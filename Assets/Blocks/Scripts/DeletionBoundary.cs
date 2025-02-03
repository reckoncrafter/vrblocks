using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletionBoundary : MonoBehaviour
{
    void OnTriggerEnter(Collider other){
        Debug.Log("Trigger Entered.");
        Destroy(other.gameObject);
    }

    void OnTriggerExit(Collider other){
        Debug.Log("Trigger Exited.");
    }
}
