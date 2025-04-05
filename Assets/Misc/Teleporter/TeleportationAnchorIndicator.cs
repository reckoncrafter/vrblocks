using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationAnchorIndicator : MonoBehaviour
{
    public GameObject arrow;
    public TeleportationAnchor teleportationAnchor;
    public float disableArrowDistance;

    void Start()
    {
        if(!teleportationAnchor)
        {
            teleportationAnchor = GetComponent<TeleportationAnchor>();
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs hoverEnter)
    {
        if(Vector3.Distance(hoverEnter.interactorObject.transform.position, transform.position) > disableArrowDistance)
        {
            arrow.SetActive(true);
        }
    }

    public void OnHoverExited(HoverExitEventArgs hoverExit)
    {
        arrow.SetActive(false);
    }
}
