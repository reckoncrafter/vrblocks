using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSchemeIndicator : MonoBehaviour
{
    public List<GameObject> XRControllerSubmeshesToIndicate;
    public ControllerModels controllerModels;

    public Material indicationMaterial;
    public Material submeshDefaultMaterial;
    public List<Renderer> submeshRenderers;

    void Start(){
       //indicationMaterial = Resources.Load("Assets/Materials/Red", typeof(Material)) as Material;
       //submeshDefaultMaterial = Resources.Load("Assets/Samples/XR Interaction Toolkit/2.6.3/Starter Assets/Models/XRControllerRight.fbx/No Name", typeof(Material)) as Material;
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
