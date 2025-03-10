using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SourceCodePanel : MonoBehaviour
{
    public XRController? controller;
    public InputHelpers.Button toggleButton;
    public float activationThreshold = 0.1f;

    private Renderer? objectRenderer;
    private bool isVisible = false;

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        SetVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsButtonPressed())
        {
            isVisible = !isVisible;
            SetVisibility(isVisible);

        }
    }

    private void SetVisibility(bool visible)
    {
        if (objectRenderer != null)
        {
            objectRenderer.enabled = visible;
        }
    }
    private bool IsButtonPressed()
    {
        if (controller == null)
        {
            return false;
        }

        InputHelpers.IsPressed(controller.inputDevice, toggleButton, out bool IsPressed, activationThreshold);
        return IsPressed;
    }
}
