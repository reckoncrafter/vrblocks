using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BlockSnapping : MonoBehaviour
{
    private bool hasSnapped = false; // Flag to prevent repeated snapping
    private QueueReading queueReading;

    private void Awake()
    {
        // Automatic detection of QueueReading component
        queueReading = FindObjectOfType<QueueReading>();
        if (queueReading == null)
        {
            Debug.LogWarning("QueueReading component not found in the scene. Ensure there is one active in the hierarchy.");
        }

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
            SnappedForwarding snappedForwarding = other.gameObject.GetComponent<SnappedForwarding>();

            // Check if the bottom trigger is already snapped
            if (snappedForwarding != null && snappedForwarding.CanSnap())
            {
                Debug.Log($"{sender.name} collided with {other.name}. Attempting to snap.");
                SnapToBlock(this.gameObject, other.transform.parent.gameObject);

                hasSnapped = true;
                snappedForwarding.ConnectedBlock = this.gameObject;

                // Set other block to snapped
                snappedForwarding.SetSnapped(true);
            }
            else
            {
                Debug.Log($"Cannot snap to {other.name} as it is already snapped.");
            }
        }
    }


    private void SnapToBlock(GameObject block1, GameObject block2)
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

        // Reset X and Z rotations for both blocks
        block1.transform.rotation = Quaternion.Euler(0, block1.transform.rotation.eulerAngles.y, 0);
        block2.transform.rotation = Quaternion.Euler(0, block1.transform.rotation.eulerAngles.y, 0);

        // Snap bottom block's SnapPointBottom to top block's SnapPointTop
        block2.transform.position = snapPointTop.position - (snapPointBottom.position - block2.transform.position);

        // Add FixedJoint to lock blocks together
        if (bottomRb != null)
        {
            FixedJoint joint = block1.AddComponent<FixedJoint>();
            joint.connectedBody = bottomRb;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
        }

        Debug.Log($"{block2.name} snapped to {block1.name}.");
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log($"Block grabbed: {gameObject.name}");

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.None;

        // Check for joints connected to objects
        FixedJoint[] joints = GetComponents<FixedJoint>();
        foreach (FixedJoint joint in joints)
        {
            // Find the other block connected to this block through the FixedJoint
            Rigidbody otherRb = joint.connectedBody;
            if (otherRb != null)
            {
                // Get the other block
                GameObject otherBlock = otherRb.gameObject;

                // Reset the snap status on the other block's SnappedForwarding component
                ResetSnapStatusOnOtherBlock(otherBlock);
            }
            Destroy(joint); // Destroy the joint to unsnap
        }

        StartCoroutine(ResetSnapStatusAfterDelay()); // Wait arbitrary amount of time to prevent block resnapping immediately.
    }

    private void ResetSnapStatusOnOtherBlock(GameObject otherBlock)
    {
        // Find the SnapTriggerBottom on the other block
        Transform otherSnapTriggerBottom = otherBlock.transform.Find("SnapTriggerBottom");
        if (otherSnapTriggerBottom != null)
        {
            // Get the SnappedForwarding component of the other block
            SnappedForwarding otherSnappedForwarding = otherSnapTriggerBottom.GetComponent<SnappedForwarding>();
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
        yield return new WaitForSeconds(0.1f); // Wait for 0.5 seconds (change as needed)
        queueReading?.ReadQueue(); // Update Block Queue on unsnap.
        hasSnapped = false; // Allow snapping again after a delay
        Debug.Log($"Snapping re-enabled on: {gameObject.name}");
    }

   private void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log($"Block released: {gameObject.name}");

        Rigidbody rb = GetComponent<Rigidbody>();
        SnappedForwarding snappedForwarding = GetComponent<SnappedForwarding>();

        if (rb == null)
        {
            Debug.LogWarning("Rigidbody component not found on this block.");
            return;
        }

        bool canSnap = snappedForwarding != null && snappedForwarding.CanSnap();

        Debug.Log($"CanSnap: {canSnap}, hasSnapped: {hasSnapped}");


        if (gameObject.name == "Block (StartQueue)")
        {
            Debug.Log("OnRelease: Block is ReadQueue.");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }
        // Case 1: Block IS snapped on top
        else if (hasSnapped == true)
        {
            Debug.Log($"CASE 1");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            // Do NOT change rb.constraints
        }
        // Case 2: Block IS NOT snapped on top or bottom
        else if (canSnap == false && hasSnapped == false)
        {
            Debug.Log($"CASE 2");
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
        // Default case
        else
        {
            Debug.Log($"CASE DEFAULT");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }

        queueReading?.ReadQueue();
    }
}
