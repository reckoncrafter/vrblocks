using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerModels : MonoBehaviour
{
    public enum Controls
    {
        Bumper,
        HomeButton,
        Trigger,
        AButton,
        BButton,
        ThumbStick,
    };
    public Material TransparentHandMaterial;
    public Material ControllerIndicationMaterial;
    public float DelayPeriod;

    private Material DefaultHandMaterial;
    private Material DefaultControllerMaterial;
    private GameObject RightController;
    private GameObject LeftController;
    private GameObject LeftHand;
    private GameObject RightHand;
    private GameObject XRControllerLeft;
    private GameObject XRControllerRight;
    private Dictionary<Controls, Renderer> LeftSubmeshRenderers;
    private Dictionary<Controls, Renderer> RightSubmeshRenderers;

    void Start(){
        RightController = transform.Find("Camera Offset/Right Controller").gameObject;
        LeftController = transform.Find("Camera Offset/Left Controller").gameObject;

        LeftHand = LeftController.transform.Find("LeftHand").gameObject;
        RightHand = RightController.transform.Find("RightHand").gameObject;
        XRControllerLeft = LeftController.transform.Find("XR Controller Left").gameObject;
        XRControllerRight = RightController.transform.Find("XR Controller Right").gameObject;

        DefaultHandMaterial = LeftHand.GetComponentInChildren<Renderer>().material;
        DefaultControllerMaterial = XRControllerLeft.GetComponentInChildren<Renderer>().material;

        RightSubmeshRenderers = new Dictionary<Controls, Renderer>()
        {
            {Controls.Bumper,     XRControllerRight.transform.Find("Bumper").GetComponent<Renderer>() },
            {Controls.HomeButton, XRControllerRight.transform.Find("Button_Home").GetComponent<Renderer>() },
            {Controls.Trigger,    XRControllerRight.transform.Find("Trigger").GetComponent<Renderer>() },
            {Controls.AButton,    XRControllerRight.transform.Find("XRController_LT_Thumbstick_Buttons/Button_A").GetComponent<Renderer>() },
            {Controls.BButton,    XRControllerRight.transform.Find("XRController_LT_Thumbstick_Buttons/Button_B").GetComponent<Renderer>() },
            {Controls.ThumbStick, XRControllerRight.transform.Find("XRController_LT_Thumbstick_Buttons/ThumbStick").GetComponent<Renderer>() }
        };
        LeftSubmeshRenderers = new Dictionary<Controls, Renderer>()
        {
            {Controls.Bumper,      XRControllerLeft.transform.Find("Bumper").GetComponent<Renderer>()  },
            {Controls.HomeButton,  XRControllerLeft.transform.Find("Button_Home").GetComponent<Renderer>()  },
            {Controls.Trigger,     XRControllerLeft.transform.Find("Trigger").GetComponent<Renderer>()  },
            {Controls.AButton,     XRControllerLeft.transform.Find("XRController_LT_Thumbstick_Buttons/Button_A").GetComponent<Renderer>()  },
            {Controls.BButton,     XRControllerLeft.transform.Find("XRController_LT_Thumbstick_Buttons/Button_B").GetComponent<Renderer>()  },
            {Controls.ThumbStick,  XRControllerLeft.transform.Find("XRController_LT_Thumbstick_Buttons/ThumbStick").GetComponent<Renderer>()  }
        };

        var foundCSIObjects = FindObjectsByType<ControlSchemeIndicator>(FindObjectsSortMode.None);
        foreach(ControlSchemeIndicator csi in foundCSIObjects)
        {
            XRSimpleInteractable interactable;
            if(csi.gameObject.TryGetComponent<XRSimpleInteractable>(out interactable))
            {
                interactable.hoverEntered.AddListener(OnHoverEnter);
                interactable.hoverExited.AddListener(OnHoverExit);
            }
        }

        EnableControllerModel(false, true);
        EnableControllerModel(false, false);
    }

    public void EnableControllerModel(bool enabled, bool right) // true -> right, false -> left
    {
        if(right)
        {
            XRControllerRight.SetActive(enabled);
            RightHand.SetActive(!enabled);
        }
        else
        {
            XRControllerLeft.SetActive(enabled);
            LeftHand.SetActive(!enabled);
        }
    }

    bool leftInterrupt = false;
    bool rightInterrupt = false;
    IEnumerator DelaySwap(bool right)
    {
        yield return new WaitForSeconds(DelayPeriod);
        if( (right && !rightInterrupt) || (!right && !leftInterrupt) )
        {
            EnableControllerModel(true, right);
        }
    }

    public void OnHoverEnter(HoverEnterEventArgs hoverEnter)
    {
        if(hoverEnter.interactorObject.transform.parent == RightController.transform)
        {
            ControlSchemeIndicator controlScheme;
            if(hoverEnter.interactableObject.transform.TryGetComponent<ControlSchemeIndicator>(out controlScheme))
            {
                foreach(Controls c in controlScheme.rightControls)
                {
                    RightSubmeshRenderers[c].material = ControllerIndicationMaterial;
                }
            }
            RightHand.GetComponentInChildren<Renderer>().material = TransparentHandMaterial;
            rightInterrupt = false;
            StartCoroutine(DelaySwap(true));
        }
        else if(hoverEnter.interactorObject.transform.parent == LeftController.transform)
        {
            ControlSchemeIndicator controlScheme;
            if(hoverEnter.interactableObject.transform.TryGetComponent<ControlSchemeIndicator>(out controlScheme))
            {
                foreach(Controls c in controlScheme.leftControls)
                {
                    LeftSubmeshRenderers[c].material = ControllerIndicationMaterial;
                }
            }
            LeftHand.GetComponentInChildren<Renderer>().material = TransparentHandMaterial;
            leftInterrupt = false;
            StartCoroutine(DelaySwap(false));
        }
    }

    public void OnHoverExit(HoverExitEventArgs hoverExit)
    {
        if(hoverExit.interactorObject.transform.parent == RightController.transform)
        {
            foreach(Renderer r in RightSubmeshRenderers.Values)
            {
                r.material = DefaultControllerMaterial;
            }
            EnableControllerModel(false, true);
            RightHand.GetComponentInChildren<Renderer>().material = DefaultHandMaterial;
            rightInterrupt = true;
        }
        else if(hoverExit.interactorObject.transform.parent == LeftController.transform)
        {
            foreach(Renderer r in LeftSubmeshRenderers.Values)
            {
                r.material = DefaultControllerMaterial;
            }
            EnableControllerModel(false, false);
            LeftHand.GetComponentInChildren<Renderer>().material = DefaultHandMaterial;
            leftInterrupt = true;
        }
    }
}
