using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleTeleportationInteractor : MonoBehaviour
{
    [SerializeField] public XRRayInteractor defaultRayInteractor;
    [SerializeField] public XRRayInteractor teleportRayInteractor;
    [SerializeField] public InputActionReference teleportationToggleAction;

    private bool teleportEnabled = false;

    void Start()
    {
        teleportationToggleAction.action.performed += (ctx) => {ToggleInteractors();};
    }

    void ToggleInteractors()
    {
        teleportEnabled = !teleportEnabled;
        if(teleportEnabled)
        {
            defaultRayInteractor.gameObject.SetActive(false);
            teleportRayInteractor.gameObject.SetActive(true);
        }
        else
        {
            defaultRayInteractor.gameObject.SetActive(true);
            teleportRayInteractor.gameObject.SetActive(false);
        }
    }
}
