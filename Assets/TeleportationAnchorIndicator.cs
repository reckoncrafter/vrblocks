using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationAnchorIndicator : MonoBehaviour
{
    public GameObject arrow;
    public TeleportationAnchor teleportationAnchor;

    void Start()
    {
        if(!teleportationAnchor)
        {
            teleportationAnchor = GetComponent<TeleportationAnchor>();
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs hoverEnter)
    {
        arrow.SetActive(true);
    }

    public void OnHoverExited(HoverExitEventArgs hoverExit)
    {
        arrow.SetActive(false);
    }
}
