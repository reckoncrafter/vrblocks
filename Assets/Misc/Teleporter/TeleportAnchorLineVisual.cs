using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportAnchorLineVisual : MonoBehaviour
{
    private Gradient defaultGradient;
    public Gradient teleportAnchorLineColorGradient;

    void Start()
    {
        defaultGradient = GetComponent<XRInteractorLineVisual>().validColorGradient;
    }

    public void OnHoverEnter(HoverEnterEventArgs hoverEnter)
    {
        if(hoverEnter.interactableObject.GetType() == typeof(TeleportationAnchor))
        {
            GetComponent<XRInteractorLineVisual>().validColorGradient = teleportAnchorLineColorGradient;
        }
    }

    public void OnHoverExit(HoverExitEventArgs hoverExit)
    {
        if(hoverExit.interactableObject.GetType() == typeof(TeleportationAnchor))
        {
            GetComponent<XRInteractorLineVisual>().validColorGradient = defaultGradient;
        }
    }
}
