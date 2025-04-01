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
    public int currentColumn = 0;

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
                    Debug.Log($"SnapToBlock: {other.name} connected block set to {otherSnappedForwarding.ConnectedBlock.name}.");
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

        CalculateBlockPositions(columnSize);
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
            Debug.LogWarning("CheckColumnSize: SnappedForwarding component not found on this block.");
            return -1;
        }

        GameObject startingBlock = transform.gameObject;

        // Find the root block
        GameObject? rootBlock = thisSnappedForwarding.FindRootBlock(startingBlock);
        if (rootBlock == null)
        {
            Debug.LogWarning("CheckColumnSize: Root block not found.");
            return -1;
        }

        // Count the number of blocks attached to the root block (including the root itself)
        int columnCount = thisSnappedForwarding.CountBlocks(rootBlock);
        return columnCount;
    }

    private void CalculateBlockPositions(int columnSize)
    {
        int blockLimit = 5; // Max size of column, can change to reference global variable at some point

        GameObject startingBlock = transform.gameObject;
        SnappedForwarding thisSnappedForwarding = startingBlock.GetComponentInChildren<SnappedForwarding>();

        if (thisSnappedForwarding == null)
        {
            Debug.LogError("SnappedForwarding component missing from the starting block.");
            return;
        }

        GameObject currentBlock = thisSnappedForwarding.FindRootBlock(startingBlock);

        if (currentBlock == null)
        {
            Debug.LogError("Root block not found.");
            return;
        }

        // Capture initial root block position
        Vector3 initialRootBlockPosition = currentBlock.transform.position;

        // Debug.Log($"UpdateBlockPositions: Root block is {currentBlock}");
        Rigidbody currentRb = currentBlock.GetComponent<Rigidbody>();
        Rigidbody connectedRb = null;

        // Check if root block has been moved
        BlockSnapping rootBlockSnapping = currentRb.GetComponent<BlockSnapping>();
        bool rootColumnUpdate = true;

        // Adjust the rootBlockPosition based on the current and desired Y position of the root block.
        if (rootBlockSnapping.physicalPosition == rootBlockSnapping.targetPosition || columnSize <= blockLimit)
        {
            rootColumnUpdate = false;
        }
        else
        {
            int positionDifference = rootBlockSnapping.physicalPosition - rootBlockSnapping.targetPosition;

            float adjustAmount = (positionDifference % blockLimit) * 0.25f;

            initialRootBlockPosition.y += adjustAmount;
        }

        while (currentRb != null)
        {
            BlockSnapping blockSnapping = currentRb.GetComponent<BlockSnapping>();
            SnappedForwarding snappedForwarding = currentRb.GetComponentInChildren<SnappedForwarding>();

            if (blockSnapping == null || snappedForwarding == null)
            {
                Debug.LogError("UpdateBlockPositions: BlockSnapping or SnappedForwarding component not found on current block!");
                break;
            }

            GameObject connectedBlock = snappedForwarding.ConnectedBlock;
            if (connectedBlock != null)
            {
                connectedRb = connectedBlock.GetComponent<Rigidbody>(); // Get the connected block's Rigidbody
            }
            else
            {
                connectedRb = null; // End the loop if there's no connected block
            }

            int physicalPosition = blockSnapping.physicalPosition;
            int targetPosition = blockSnapping.targetPosition;
            int currentColumn = blockSnapping.currentColumn;
            int targetColumn = (targetPosition - 1) / blockLimit;
            Debug.Log($"UpdateBlockPosition: Physical Position = {physicalPosition}, Target Position = {targetPosition}.");

            if (rootColumnUpdate == false && targetPosition <= blockLimit)
            {
                // Position updating logic
                while (physicalPosition != targetPosition)
                {
                    physicalPosition = targetPosition;

                    blockSnapping.physicalPosition = targetPosition;
                }

                Rigidbody nextRb = connectedRb;  // The next block in the chain is the connected block.

                currentRb = nextRb;
            }
            else
            {
                // Position updating logic
                while (physicalPosition != targetPosition)
                {
                    int adjustPositionY = (physicalPosition - targetPosition) % blockLimit;
                    int adjustPositionX = targetColumn - currentColumn;

                    if (physicalPosition % blockLimit == 0)
                    {
                        DestroyWire(); // Define this function later
                    }

                    UpdateBlockPosition(currentRb, initialRootBlockPosition, adjustPositionX, adjustPositionY);

                    physicalPosition = targetPosition;

                    if (physicalPosition % blockLimit == 0 && connectedRb != null)
                    {
                        //SpawnWire(currentRb, connectedRb); // SpawnWire if new position is bottom of the column
                    }

                    blockSnapping.physicalPosition = targetPosition;
                }

                Rigidbody nextRb = connectedRb;  // The next block in the chain is the connected block.

                currentRb = nextRb;
            }
        }
    }

    private void UpdateBlockPosition(Rigidbody currentRb, Vector3 initialRootBlockPosition, int adjustPositionX, int adjustPositionY)
    {
        Debug.Log("UpdateBlockPosition: Called!");

        // Define block offsets
        float yBlockOffset = 0.25f; // Width (Y) of block, consider changing to reference global variable
        float xBlockOffset = 1.0f; // X Offset of columns, consider changing to reference global variable

        // Find parent block through the FixedJoint and destroy the joint to allow block realignment
        FixedJoint[] joints = currentRb.GetComponents<FixedJoint>();
        Rigidbody parentRb = null;

        //Debug.Log("UpdateBlockPosition: Initial number of joints = " + joints.Length);

        // Find the parent and destroy the joint
        foreach (FixedJoint joint in joints)
        {
            //Debug.Log("UpdateBlockPosition: Checking joint connected to " + joint.connectedBody?.name);

            if (joint.connectedBody != null && !joint.connectedBody.name.Contains("Wire"))
            {
                parentRb = joint.connectedBody;  // Store the parent Rigidbody for later joint
                Destroy(joint); // Destroy the joint to allow repositioning
                //Debug.Log("UpdateBlockPosition: Joint Destroyed!");
                break;
            }
            else
            {
                //Debug.Log("UpdateBlockPosition: No parent found or joint connected to a wire.");
            }
        }

        // Log the number of joints after the joint destruction
        joints = currentRb.GetComponents<FixedJoint>();
        //Debug.Log("UpdateBlockPosition: Number of joints after destruction = " + joints.Length);

        // Capture the initial position before making changes (for debug)
        Vector3 initialPosition = currentRb.transform.position;
        Debug.Log("UpdateBlockPosition: Initial Position - X: " + initialPosition.x + ", Y: " + initialPosition.y + ", Z: " + initialPosition.z);

        // Now calculate the new position based on initial root position and offsets
        Vector3 adjustedPosition = new Vector3(
            initialRootBlockPosition.x + (adjustPositionX * xBlockOffset),
            initialRootBlockPosition.y + (adjustPositionY * yBlockOffset),
            initialRootBlockPosition.z
        );

        Debug.Log("UpdateBlockPosition: Adjusted Position - X: " + adjustedPosition.x + ", Y: " + adjustedPosition.y + ", Z: " + adjustedPosition.z);

        // Apply the new position to the block
        currentRb.transform.position = adjustedPosition;
        //Debug.Log("UpdateBlockPosition: Block Position Updated!");

        // Reset rotation (probably unnecessary)
        currentRb.transform.rotation = Quaternion.Euler(0, 0, 0);
        //Debug.Log("UpdateBlockPosition: Block Rotation Reset!");

        // Recreate the joint to reattach the block to its parent
        if (parentRb != null)
        {
            FixedJoint newJoint = currentRb.gameObject.AddComponent<FixedJoint>();
            newJoint.connectedBody = parentRb;
            newJoint.breakForce = Mathf.Infinity;
            newJoint.breakTorque = Mathf.Infinity;
            //Debug.Log("UpdateBlockPosition: New Joint Created and Connected to Parent.");
        }
        else
        {
            //Debug.Log("UpdateBlockPosition: No parent Rigidbody found, no joint recreated.");
        }

        // Debug to confirm position before/after update
        Debug.Log($"UpdateBlockPosition: Block '{currentRb.name}' moved from " +
                  $"Position X: {initialPosition.x}, Y: {initialPosition.y}, Z: {initialPosition.z} " +
                  $"to New Position X: {currentRb.transform.position.x}, Y: {currentRb.transform.position.y}, Z: {currentRb.transform.position.z}.");
    }


    public void SpawnWire(Rigidbody rb, Rigidbody connectedRb)
    {
        // Pretty much the same as the original SpawnWire function
        GameObject wirePrefab = Resources.Load<GameObject>("Prefabs/Wire2");

        if (wirePrefab == null || rb == null || connectedRb == null)
        {
            Debug.LogError("Missing wirePrefab or Rigidbody.");
            return;
        }

        // Find the snap points on the current block
        Transform snapPointRight = rb.gameObject.transform.Find("SnapPointRight");
        if (snapPointRight == null)
        {
            Debug.LogError("SnapPointRight not found on the current block.");
            return;
        }

        // Find the snap point on the connected block
        Transform snapPointLeft = connectedRb.transform.Find("SnapPointLeft");
        if (snapPointLeft == null)
        {
            Debug.LogError("SnapPointLeft not found on the connected block.");
            return;
        }

        // Spawn wire at the correct position
        GameObject newWire = Instantiate(wirePrefab, snapPointRight.position, wirePrefab.transform.rotation);

        // Find the wire's snap points
        Transform wireSnapLeft = newWire.transform.Find("SnapPointLeft");
        Transform wireSnapRight = newWire.transform.Find("SnapPointRight");

        if (wireSnapLeft == null || wireSnapRight == null)
        {
            Debug.LogError("Wire prefab is missing snap points.");
            return;
        }

        // Adjust position so wireSnapLeft aligns with snapPointRight of the original block
        Vector3 wireOffset = snapPointRight.position - wireSnapLeft.position;
        newWire.transform.position += wireOffset;

        Rigidbody wireRb = newWire.GetComponent<Rigidbody>();
        if (wireRb == null)
        {
            wireRb = newWire.AddComponent<Rigidbody>();
            wireRb.isKinematic = false;
            wireRb.useGravity = false;
            wireRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            wireRb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void DestroyWire()
    {
        Debug.Log("DestroyWire: called!");
    }
}
