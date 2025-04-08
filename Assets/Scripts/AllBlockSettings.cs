using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class AllBlockSettings : MonoBehaviour
{

    public bool KinematicInteraction;
    public bool smoothPostion;
    public bool throwOnDetach;
    void Awake()
    {
        XRGrabInteractable[] xRGrabInteractables = GetComponentsInChildren<XRGrabInteractable>();
        for(int i = 0; i < xRGrabInteractables.Length; i++)
        {
            xRGrabInteractables[i].movementType = KinematicInteraction? XRBaseInteractable.MovementType.Kinematic : XRBaseInteractable.MovementType.Instantaneous;
            xRGrabInteractables[i].smoothPosition = smoothPostion;
            xRGrabInteractables[i].throwOnDetach = throwOnDetach;
        }
    }
}
