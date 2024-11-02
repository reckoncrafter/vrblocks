using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockUIToObject : MonoBehaviour
{
    
    public GameObject lockTo;
    public GameObject playerUI;
    public bool lockPosition;
    public bool lockRotation;


    void Update()
    {
        if(lockPosition){
            playerUI.transform.position = lockTo.transform.position;    
        }
        if(lockRotation){
            playerUI.transform.rotation = lockTo.transform.rotation;
        }
    }
}
