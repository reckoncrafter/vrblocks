using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class BlockSnapping : MonoBehaviour
{
    // blockSnapEvent is a static (global) event that objects can use to respond to changes in the block queue.
    public static UnityEvent blockSnapEvent = new UnityEvent();

    public bool hasSnapped = false; // Flag to prevent repeated snapping
    //private QueueReading? queueReading;

    public int physicalPosition = 1;
    public int targetPosition = 1;

    public AudioClip snapSound;
    private AudioSource audio;



    private void Awake()
    {
        // Collision Forwarding active
        Transform snapTriggerTop = transform.Find("SnapTriggerTop");
        AttachCollisionForwarding(snapTriggerTop);

        // XR Grab Interactable listeners
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }

        audio = GetComponent<AudioSource>();
        if (audio == null)
        {
            audio = gameObject.AddComponent<AudioSource>(); // Add if not found
        }
    }

    private void AttachCollisionForwarding(Transform trigger)
    {
        if (trigger != null)
        {
            CollisionForwarding forwarding = trigger.GetComponent<CollisionForwarding>();
            if (forwarding == null)
            {
                forwarding = trigger.gameObject.AddComponent<CollisionForwarding>();
            }
            forwarding.parentBlockSnapping = this;
        }
        else
        {
            Debug.LogWarning("SnapTriggerTop is missing on: " + gameObject.name);
        }
    }

    public void OnChildTriggerEnter(CollisionForwarding sender, Collider other)
    {
        // Check if SnapTriggerTop was entered by SnapTriggerBottom
        if (sender.gameObject.name == "SnapTriggerTop" && other.gameObject.name == "SnapTriggerBottom" && !hasSnapped)
        {
            // Get SnappedForwarding from other.SnapTriggerBottom
            SnappedForwarding otherSnappedForwarding = other.gameObject.GetComponent<SnappedForwarding>();
            SnappedForwarding thisSnappedForwarding = GetComponentInChildren<SnappedForwarding>();

            // Check if the bottom trigger is already snapped
            if (otherSnappedForwarding != null && otherSnappedForwarding.CanSnap())
            {
                GameObject? parentObject = other.transform.parent?.gameObject;
                if (parentObject != null)
                {
                    Debug.Log($"{sender.name} collided with {other.name}. Attempting to snap.");
                    PlaySnapSound();
                    SnapToBlock(this.gameObject, parentObject);

                    hasSnapped = true;
                    otherSnappedForwarding.ConnectedBlock = this.gameObject;
                    thisSnappedForwarding.IsRootBlock = false;

                    // Set other block to snapped
                    otherSnappedForwarding.SetSnapped(true);
                }
                else
                {
                    Debug.LogError("Parent object is null!");
                }
            }
            else
            {
                Debug.Log($"Cannot snap to {other.name} as it is already snapped.");
            }
        }
    }

    private void SnapToBlock(GameObject block1, GameObject block2) //block2 is top block, block1 is bottom block
    {
        Transform snapPointTop = block1.transform.Find("SnapPointTop");
        Transform snapPointBottom = block2.transform.Find("SnapPointBottom");

        if (snapPointTop == null || snapPointBottom == null)
        {
            Debug.LogError("Snap points not found on one or both blocks. Ensure SnapPointTop and SnapPointBottom are correctly named and attached.");
            return;
        }

        Rigidbody topRb = block1.GetComponent<Rigidbody>();
        Rigidbody bottomRb = block2.GetComponent<Rigidbody>();

        // Allow both blocks to react to physics
        topRb.constraints = RigidbodyConstraints.None;
        bottomRb.constraints = RigidbodyConstraints.None;

        // Reset X and Z rotations for both blocks IF blocks are NOT wires
        if (!block1.name.Contains("Wire"))
        {
            block1.transform.rotation = Quaternion.Euler(0, block1.transform.rotation.eulerAngles.y, 0);
        }
        if (!block2.name.Contains("Wire"))
        {
            block2.transform.rotation = Quaternion.Euler(0, block2.transform.rotation.eulerAngles.y, 0);
        }

        // Snap bottom block's SnapPointBottom to top block's SnapPointTop IF blocks are NOT wires
        if (!block2.name.Contains("Wire"))
        {
            block2.transform.position = snapPointTop.position - (snapPointBottom.position - block2.transform.position);
        }

        // Add FixedJoint to lock blocks together
        if (bottomRb != null)
        {
            FixedJoint joint = block1.AddComponent<FixedJoint>();
            joint.connectedBody = bottomRb;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
        }

        Debug.Log($"{block2.name} snapped to {block1.name}.");

        // Get BlockSnapping components
        BlockSnapping block1Snapping = block1.GetComponent<BlockSnapping>();
        BlockSnapping block2Snapping = block2.GetComponent<BlockSnapping>();

        if (block1Snapping != null && block2Snapping != null)
        {
            // Set targetPosition values
            block1Snapping.targetPosition = block2Snapping.targetPosition + 1;
            Debug.Log($"{block1.name} targetPosition set to {block1Snapping.targetPosition}");

            // Set targetPosition values of children (if any)
            UpdateChildBlockPositions(block1);
        }
    }

    private void UpdateChildBlockPositions(GameObject parentBlock)
    {
        // Check if the parent block has a connected child
        SnappedForwarding parentForwarding = parentBlock.GetComponentInChildren<SnappedForwarding>();
        if (parentForwarding == null || parentForwarding.ConnectedBlock == null)
        {
            Debug.LogWarning($"Parent block {parentBlock.name} has no connected child.");
            return;
        }

        // Get components for the parent and child blocks
        BlockSnapping parentSnapping = parentBlock.GetComponent<BlockSnapping>();
        GameObject childBlock = parentForwarding.ConnectedBlock;
        SnappedForwarding childForwarding = childBlock.GetComponentInChildren<SnappedForwarding>();
        BlockSnapping childSnapping = childBlock.GetComponent<BlockSnapping>();

        // Ensure both parent and child have BlockSnapping components
        if (parentSnapping == null || childSnapping == null)
        {
            Debug.LogWarning($"Either parent {parentBlock.name} or child {childBlock.name} is missing BlockSnapping component.");
            return;
        }

        // Set targetPosition for the child to parent + 1
        childSnapping.targetPosition = parentSnapping.targetPosition + 1;
        Debug.Log($"{childBlock.name} targetPosition set to {childSnapping.targetPosition}");

        // Recursively update connected child blocks if they exist
        if (childForwarding != null && childForwarding.ConnectedBlock != null)
        {
            UpdateChildBlockPositions(childForwarding.ConnectedBlock);
        }
    }

    private void PlaySnapSound()
    {
        if (audio != null && snapSound != null)
        {
            audio.PlayOneShot(snapSound);
        }
        else
        {
            Debug.LogError("Audio source or sound is null.");
        }
    }

    private Coroutine? resetSnapStatusCoroutine;

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log($"Block grabbed: {gameObject.name}");

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;

        FixedJoint[] joints = GetComponents<FixedJoint>();
        foreach (FixedJoint joint in joints)
        {
            Rigidbody otherRb = joint.connectedBody;
            if (otherRb != null)
            {
                GameObject otherObject = otherRb.gameObject;

                // If the connected object is a wire, despawn it
                if (otherObject.name.Contains("Wire"))
                {
                    WireDespawn wireDespawn = otherObject.GetComponent<WireDespawn>();
                    if (wireDespawn != null)
                    {
                        wireDespawn.Despawn();
                    }
                }
                else
                {
                    ResetSnapStatusOnOtherBlock(otherObject);
                }
            }

            Destroy(joint);

            SnappedForwarding sf = otherRb.GetComponentInChildren<SnappedForwarding>();
            if (sf != null)
            {
                sf.UpdatePhysics(otherRb);
            }
        }

        // Start the coroutine and store reference for OnRelease()
        resetSnapStatusCoroutine = StartCoroutine(ResetSnapStatusAfterDelay());

        // Set this block as root block (MAY BE DEPRECATED)
        SnappedForwarding snappedForwarding = gameObject.GetComponentInChildren<SnappedForwarding>();
        if (snappedForwarding != null)
        {
            snappedForwarding.IsRootBlock = true;
        }

        // Set this block's targetPosition to 1 (root position)
        this.targetPosition = 1;
        Debug.Log($"{gameObject.name} targetPosition set to {this.targetPosition}");

        // Update all child blocks recursively
        UpdateChildBlockPositions(gameObject);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log($"Block released: {gameObject.name}");

        // Check if ResetSnapStatusAfterDelay is complete to prevent physics bugs.
        if (resetSnapStatusCoroutine != null)
        {
            StartCoroutine(WaitForCoroutineToComplete(resetSnapStatusCoroutine));
        }
        else
        {
            ProceedWithRelease();
        }
    }

    private IEnumerator WaitForCoroutineToComplete(Coroutine coroutine)
    {
        // Wait until the coroutine has finished
        yield return coroutine;

        // Once the coroutine finishes, proceed with the release actions
        ProceedWithRelease();
    }

    private void ProceedWithRelease()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Rigidbody component not found on this block.");
            return;
        }

        int columnSize = CheckColumnSize();

        UpdateBlockPositions();
        blockSnapEvent.Invoke();
        //queueReading?.ReadQueue();
    }

    private void ResetSnapStatusOnOtherBlock(GameObject otherBlock)
    {
        // Find the SnapTriggerBottom on the other block
        Transform otherSnapTriggerBottom = otherBlock.transform.Find("SnapTriggerBottom");
        if (otherSnapTriggerBottom != null)
        {
            // Get the SnappedForwarding component of the other block
            SnappedForwarding? otherSnappedForwarding = otherSnapTriggerBottom.GetComponent<SnappedForwarding>();
            if (otherSnappedForwarding != null)
            {
                // Reset the snap status of the other block to false
                otherSnappedForwarding.SetSnapped(false);
                otherSnappedForwarding.ConnectedBlock = null;
            }
            else
            {
                Debug.LogWarning($"No SnappedForwarding component found on {otherSnapTriggerBottom.gameObject.name}.");
            }
        }
        else
        {
            Debug.LogWarning("SnapTriggerBottom not found on the other block.");
        }
    }

    private IEnumerator ResetSnapStatusAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds (change as needed)
        //queueReading?.ReadQueue(); // Update Block Queue on unsnap.
        hasSnapped = false; // Allow snapping again after a delay

        blockSnapEvent.Invoke();
        Debug.Log($"Snapping re-enabled on: {gameObject.name}");
    }

    private int CheckColumnSize()
    {
        // Get the SnappedForwarding component
        SnappedForwarding thisSnappedForwarding = GetComponentInChildren<SnappedForwarding>();
        if (thisSnappedForwarding == null)
        {
            Debug.LogWarning("SnappedForwarding component not found on this block.");
            return -1;
        }

        GameObject startingBlock = transform.gameObject;

        // Find the root block
        GameObject? rootBlock = thisSnappedForwarding.FindRootBlock(startingBlock);
        if (rootBlock == null)
        {
            Debug.LogWarning("Root block not found.");
            return -1;
        }

        // Count the number of blocks attached to the root block (including the root itself)
        int columnCount = thisSnappedForwarding.CountBlocks(rootBlock);
        return columnCount;
    }

    private void UpdateBlockPositions()
    {
        Debug.Log("UpdateBlockPositions() called");
        //WIP
    }
}
