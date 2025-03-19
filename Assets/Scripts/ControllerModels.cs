using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerModels : MonoBehaviour
{
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject XRControllerLeft;
    public GameObject XRControllerRight;

    void Start(){
        // LeftHand = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Left Controller/LeftHand");
        // RightHand = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Right Controller/RightHand");
        // XRControllerLeft = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Left Controller/XRControllerLeft");
        // XRControllerRight = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Right Controller/XRControllerRight");

        EnableControllerHands(true);
    }

    public void EnableControllerHands(bool status)
    {
        if(status)
        {
            LeftHand.SetActive(true);
            RightHand.SetActive(true);
            XRControllerLeft.SetActive(false);
            XRControllerRight.SetActive(false);
        }
        else
        {
            XRControllerLeft.SetActive(true);
            XRControllerRight.SetActive(true);
            LeftHand.SetActive(false);
            RightHand.SetActive(false);
        }
    }

}
