using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BlockGrabInteractable : XRGrabInteractable
{
    public enum DetachMode
    {
        Primary,
        Secondary
    };
    public bool waitForPullInput = false;
    public InputActionReference rPrimaryDetachAction;
    public InputActionReference lPrimaryDetachAction;
    public InputActionReference rSecondaryDetachAction;
    public InputActionReference lSecondaryDetachAction;
    private bool isWaiting = false;
    private string interactorName = "";
    private Transform OriginalSceneParent;
    private BlockSnapping blockSnapping;


    public UnityEvent<DetachMode> detachTriggered;
    public UnityEvent<bool> onGrabChanged; // true = grabbed, false = released


    private void Start()
    {
        rPrimaryDetachAction.action.started += (ctx) => { Detach(ctx, true, DetachMode.Primary); };
        lPrimaryDetachAction.action.started += (ctx) => { Detach(ctx, false, DetachMode.Primary); };
        rSecondaryDetachAction.action.started += (ctx) => { Detach(ctx, true, DetachMode.Secondary); };
        lSecondaryDetachAction.action.started += (ctx) => { Detach(ctx, false, DetachMode.Secondary); };
        // add more detach actions here

        OriginalSceneParent = transform.parent;
        blockSnapping = GetComponent<BlockSnapping>();

        Transform attachPoint = transform.Find("AttachPoint");
        if (attachPoint != null)
        {
            attachTransform = attachPoint;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        onGrabChanged?.Invoke(true);
        if (blockSnapping && blockSnapping.hasSnapped)
        {
            transform.SetParent(OriginalSceneParent);
            if (waitForPullInput && args.interactorObject is XRRayInteractor)
            {
                isWaiting = true;
                trackPosition = false;
                trackRotation = false;
                interactorName = args.interactorObject.transform.gameObject.name;
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        onGrabChanged?.Invoke(false);
        if (isWaiting && waitForPullInput && args.interactorObject is XRRayInteractor)
        {
            isWaiting = false;
            trackPosition = true;
            trackRotation = true;
            interactorName = "";
        }
    }

    private void Detach(InputAction.CallbackContext ctx, bool isRightHand, DetachMode detachMode)
    {
        if (isWaiting && ((isRightHand && interactorName == "RRayInteractor") || (!isRightHand && interactorName == "LRayInteractor")))
        {
            detachTriggered.Invoke(detachMode);
            transform.SetParent(null);
            isWaiting = false;
            trackPosition = true;
            trackRotation = true;
        }
    }
}
