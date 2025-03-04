using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BlockSnapping : MonoBehaviour
{
    public bool hasSnapped = false; // Flag to prevent repeated snapping
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
            SnappedForwarding otherSnappedForwarding = other.gameObject.GetComponent<SnappedForwarding>();
            SnappedForwarding thisSnappedForwarding = GetComponentInChildren<SnappedForwarding>();

            // Check if the bottom trigger is already snapped
            if (otherSnappedForwarding != null && otherSnappedForwarding.CanSnap())
            {
                GameObject parentObject = other.transform.parent?.gameObject;
                if (parentObject != null)
                {
                    Debug.Log($"{sender.name} collided with {other.name}. Attempting to snap.");
                    SnapToBlock(this.gameObject, other.transform.parent.gameObject);

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
    }

    private Coroutine resetSnapStatusCoroutine;

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
            //UpdatePhysics(otherRb);
        }

        // Start the coroutine and store reference for OnRelease()
        resetSnapStatusCoroutine = StartCoroutine(ResetSnapStatusAfterDelay());

        SnappedForwarding snappedForwarding = gameObject.GetComponentInChildren<SnappedForwarding>();
        if (snappedForwarding != null)
        {
            snappedForwarding.IsRootBlock = true;
        }
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

        CheckColumnSize();
        queueReading?.ReadQueue();
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
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds (change as needed)
        queueReading?.ReadQueue(); // Update Block Queue on unsnap.
        hasSnapped = false; // Allow snapping again after a delay
        Debug.Log($"Snapping re-enabled on: {gameObject.name}");
    }

    private GameObject GetNthBlock(GameObject startingBlock, int n) // Function created to find blocks for CheckColumnSize()
    {
        int count = 1;
        GameObject current = startingBlock;
        while (current != null && count < n)
        {
            // Get the SnappedForwarding component on the current block
            SnappedForwarding sf = current.GetComponentInChildren<SnappedForwarding>();
            if (sf != null && sf.ConnectedBlock != null)
            {
                current = sf.ConnectedBlock;
                count++;
            }
            else
            {
                break;
            }
        }
        return (count == n) ? current : null;
    }

    private void CheckColumnSize()
    {
        // Get the SnappedForwarding component
        SnappedForwarding thisSnappedForwarding = GetComponentInChildren<SnappedForwarding>();
        if (thisSnappedForwarding == null)
        {
            Debug.LogWarning("SnappedForwarding component not found on this block.");
            return;
        }

        GameObject startingBlock = transform.gameObject;

        // Find the root block
        GameObject rootBlock = thisSnappedForwarding.FindRootBlock(startingBlock);
        if (rootBlock == null)
        {
            Debug.LogWarning("Root block not found.");
            return;
        }

        // Count the number of blocks attached to the root block (including the root itself)
        int columnCount = thisSnappedForwarding.CountBlocks(rootBlock);
        Debug.Log($"Column count: {columnCount}");

        // If the column count exceeds 5, reposition the 6th block and then create a new joint between block 5 and block 6.
        if (columnCount > 5)
        {
            // Retrieve the 6th block in the chain
            GameObject sixthBlock = GetNthBlock(rootBlock, 6);
            if (sixthBlock != null)
            {
                // Break the joint connection on the sixth block if it exists to allow repositioning
                FixedJoint joint = sixthBlock.GetComponent<FixedJoint>();
                if (joint != null)
                {
                    Destroy(joint);
                    Debug.Log($"FixedJoint on {sixthBlock.name} destroyed.");
                }

                // Reposition the 6th block: set its position to rootBlock's position plus 1 unit along the X axis. (Will adjust to make it dynamic.)
                Vector3 newPosition = rootBlock.transform.position + new Vector3(1f, 0f, 0f);
                sixthBlock.transform.position = newPosition;
                Debug.Log($"Sixth block {sixthBlock.name} repositioned to new column at {newPosition}");

                Rigidbody rbSixth = sixthBlock.GetComponent<Rigidbody>();
                if (rbSixth != null)
                {
                    rbSixth.useGravity = false;
                    rbSixth.velocity = Vector3.zero;
                    rbSixth.angularVelocity = Vector3.zero;
                }
                else
                {
                    Debug.LogWarning($"Rigidbody not found on {sixthBlock.name}");
                }

                // Mark the 6th block as a new root block.
                SnappedForwarding sixthSF = sixthBlock.GetComponentInChildren<SnappedForwarding>();
                if (sixthSF != null)
                {
                    sixthSF.IsRootBlock = true;
                }

                //Create a new joint between the 5th and 6th blocks
                GameObject fifthBlock = GetNthBlock(rootBlock, 5);
                if (fifthBlock != null)
                {
                    Rigidbody rbFifth = fifthBlock.GetComponent<Rigidbody>();
                    if (rbFifth != null && rbSixth != null)
                    {
                        FixedJoint newJoint = sixthBlock.AddComponent<FixedJoint>();
                        newJoint.connectedBody = rbFifth;
                        newJoint.breakForce = Mathf.Infinity;
                        newJoint.breakTorque = Mathf.Infinity;
                        Debug.Log($"New joint created between {sixthBlock.name} and {fifthBlock.name}.");
                    }
                    else
                    {
                        Debug.LogWarning("Could not find Rigidbody on the 5th or 6th block for joint creation.");
                    }

                    // Spawn wire between two blocks.
                    SpawnWire(fifthBlock, sixthBlock);
                }
                else
                {
                    Debug.LogWarning("Could not locate the fifth block in the column.");
                }

            }
            else
            {
                Debug.LogWarning("Could not locate the sixth block in the column.");
            }
        }
    }

    public void SpawnWire(GameObject block5, GameObject block6)
    {
        GameObject wirePrefab = Resources.Load<GameObject>("Prefabs/Wire2");

        if (wirePrefab == null || block5 == null || block6 == null)
        {
            Debug.LogError("Missing wirePrefab or blocks.");
            return;
        }

        // Get the snap points
        Transform snapPointRight = block5.transform.Find("SnapPointRight");
        Transform snapPointLeft = block6.transform.Find("SnapPointLeft");

        if (snapPointRight == null)
        {
            Debug.LogError("SnapPointRight is null.");
            return;
        }

        if (snapPointLeft == null)
        {
            Debug.LogError("SnapPointLeft is null.");
            return;
        }

        // Spawn wire at the correct position
        GameObject newWire = Instantiate(wirePrefab, snapPointRight.position, wirePrefab.transform.rotation);

        // Get the wire's snap points
        Transform wireSnapLeft = newWire.transform.Find("SnapPointLeft");
        Transform wireSnapRight = newWire.transform.Find("SnapPointRight");

        if (wireSnapLeft == null || wireSnapRight == null)
        {
            Debug.LogError("Wire prefab is missing snap points.");
            return;
        }

        // Adjust position so wireSnapLeft aligns with snapPointRight of block5
        Vector3 wireOffset = snapPointRight.position - wireSnapLeft.position;
        newWire.transform.position += wireOffset;

        // Attach the joints to keep the wire locked between blocks
        AttachJoint(newWire, snapPointRight, snapPointLeft);
    }

    private void AttachJoint(GameObject wire, Transform snapRight, Transform snapLeft)
    {
        Rigidbody wireRb = wire.GetComponent<Rigidbody>();
        if (wireRb == null)
        {
            wireRb = wire.AddComponent<Rigidbody>();
            wireRb.isKinematic = false;
            wireRb.useGravity = false;
            wireRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            wireRb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Rigidbody blockBottomRb = snapRight.GetComponentInParent<Rigidbody>();
        Rigidbody blockTopRb = snapLeft.GetComponentInParent<Rigidbody>();

        if (blockBottomRb != null)
        {
            FixedJoint joint1 = blockBottomRb.gameObject.AddComponent<FixedJoint>();
            joint1.connectedBody = wireRb;
            joint1.breakForce = Mathf.Infinity;
            joint1.breakTorque = Mathf.Infinity;
        }
        else
        {
            Debug.LogError($"No Rigidbody found in parent of {snapRight.name}");
        }

        if (blockTopRb != null)
        {
            FixedJoint joint2 = blockTopRb.gameObject.AddComponent<FixedJoint>();
            joint2.connectedBody = wireRb;
            joint2.breakForce = Mathf.Infinity;
            joint2.breakTorque = Mathf.Infinity;
        }
        else
        {
            Debug.LogError($"No Rigidbody found in parent of {snapLeft.name}");
        }
    }
}
