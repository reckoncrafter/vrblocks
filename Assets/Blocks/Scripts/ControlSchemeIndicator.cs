using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSchemeIndicator : MonoBehaviour
{
    public enum Controls
    {
        Bumper_Right,
        HomeButton_Right,
        Trigger_Right,
        AButton_Right,
        BButton_Right,
        ThumbStick_Right,

        Bumper_Left,
        HomeButton_Left,
        Trigger_Left,
        AButton_Left,
        BButton_Left,
        ThumbStick_Left
    }

    public ControllerModels controllerModels;
    public List<Controls> controlsToIndicate;
    public Material indicationMaterial;
    public Material submeshDefaultMaterial;

    public List<GameObject> XRControllerSubmeshesToIndicate;
    public List<Renderer> submeshRenderers;

    public void Initialize(ControllerModels cm){
       //indicationMaterial = Resources.Load("Assets/Materials/Red", typeof(Material)) as Material;
       //submeshDefaultMaterial = Resources.Load("Assets/Samples/XR Interaction Toolkit/2.6.3/Starter Assets/Models/XRControllerRight.fbx/No Name", typeof(Material)) as Material;
       //controllerModels = GameObject.Find("XR Origin (XR Rig)").GetComponent<ControllerModels>();
        controllerModels = cm;
        Transform leftControllerTransform = controllerModels.XRControllerLeft.GetComponent<Transform>();
        Transform rightControllerTransform = controllerModels.XRControllerRight.GetComponent<Transform>();

        Func<Transform, string, GameObject> _Find = (Transform tr, string str) => tr.Find(str).gameObject;
        Func<Controls, GameObject> ControlSwitch = (Controls ctl)=>
        {
            return ctl switch
            {
                Controls.Bumper_Right       =>_Find(rightControllerTransform, "Bumper"),
                Controls.HomeButton_Right   =>_Find(rightControllerTransform, "Button_Home"),
                Controls.Trigger_Right      =>_Find(rightControllerTransform, "Trigger"),
                Controls.AButton_Right      =>_Find(rightControllerTransform, "XRController_LT_Thumbstick_Buttons/Button_A"),
                Controls.BButton_Right      =>_Find(rightControllerTransform, "XRController_LT_Thumbstick_Buttons/Button_B"),
                Controls.ThumbStick_Right   =>_Find(rightControllerTransform, "XRController_LT_Thumbstick_Buttons/ThumbStick"),

                Controls.Bumper_Left        =>_Find(leftControllerTransform, "Bumper"),
                Controls.HomeButton_Left    =>_Find(leftControllerTransform, "Button_Home"),
                Controls.Trigger_Left       =>_Find(leftControllerTransform, "Trigger"),
                Controls.AButton_Left       =>_Find(leftControllerTransform, "XRController_LT_Thumbstick_Buttons/Button_A"),
                Controls.BButton_Left       =>_Find(leftControllerTransform, "XRController_LT_Thumbstick_Buttons/Button_B"),
                Controls.ThumbStick_Left    =>_Find(leftControllerTransform, "XRController_LT_Thumbstick_Buttons/ThumbStick"),
                _ => null

            };
        };

        foreach(Controls ctl in controlsToIndicate)
        {
            XRControllerSubmeshesToIndicate.Add(ControlSwitch(ctl));
        }

        foreach(GameObject submesh in XRControllerSubmeshesToIndicate)
        {
            submeshRenderers.Add(submesh.GetComponent<Renderer>());
        }
    }



    public void OnHoverEntered(){
        controllerModels.EnableControllerHands(false);
        foreach(Renderer r in submeshRenderers)
        {
            r.material = indicationMaterial;
        }
    }

    public void OnHoverExited(){
        controllerModels.EnableControllerHands(true);
        foreach(Renderer r in submeshRenderers)
        {
            r.material = submeshDefaultMaterial;
        }

    }
}
