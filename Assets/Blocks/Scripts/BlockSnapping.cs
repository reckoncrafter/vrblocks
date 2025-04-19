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
    public bool hasWire = false;

    public AudioClip snapSound;
    private AudioSource audioSource;



    private void Awake()
    {
        // Collision Forwarding active
        Transform snapTriggerTop = transform.Find("SnapTriggerTop");
        AttachCollisionForwarding(snapTriggerTop);

        // XR Grab Interactable listeners
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            if (grabInteractable is BlockGrabInteractable)
            {
                (grabInteractable as BlockGrabInteractable).detachTriggered.AddListener(OnGrab);
                grabInteractable.selectExited.AddListener(OnRelease);
            }
            else
            {
                grabInteractable.selectEntered.AddListener(OnGrab);
                grabInteractable.selectExited.AddListener(OnRelease);
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Add if not found
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
            SnappedForwarding otherSnappedForwarding = other.gameObject.GetComponent<SnappedForwarding>();
            SnappedForwarding thisSnappedForwarding = GetComponentInChildren<SnappedForwarding>();
            GameObject thisChildBlock = thisSnappedForwarding.ConnectedBlock; // Get ConnectedBlock to check for looped snapping.

            if (thisSnappedForwarding.snappingEnabled == true && otherSnappedForwarding.snappingEnabled == true)
            {
                // Prevent looped snapping
                if (!thisSnappedForwarding.IsLoopedBlock(thisChildBlock, other.transform.parent?.gameObject))
                {
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
                else
                {
                    Debug.LogError($"Looped snap prevented: {other.transform.parent?.name} is already connected to {this.name}");
                }
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
        if (parentBlock == null) return;

        // Attempt to get the SnappedForwarding component; proceed even if null
        SnappedForwarding parentForwarding = parentBlock.GetComponentInChildren<SnappedForwarding>();

        if (parentForwarding?.ConnectedBlock == null)
        {
            Debug.LogWarning($"Parent block {parentBlock.name} has no connected child.");
            return;
        }

        GameObject childBlock = parentForwarding.ConnectedBlock;

        // Attempt to get BlockSnapping components, but do not force them
        BlockSnapping parentSnapping = parentBlock.GetComponent<BlockSnapping>();
        BlockSnapping childSnapping = childBlock.GetComponent<BlockSnapping>();

        if (parentSnapping != null && childSnapping != null)
        {
            // Adjust child's position based on parent's position (Customize your logic as needed)
            childSnapping.targetPosition = parentSnapping.targetPosition + 1;
            Debug.Log($"Updated {childBlock.name} targetPosition to {childSnapping.targetPosition}");
        }

        // Continue recursively without enforcing BlockSnapping presence
        UpdateChildBlockPositions(childBlock);
    }

    private void PlaySnapSound()
    {
        if (audioSource != null && snapSound != null)
        {
            audioSource.PlayOneShot(snapSound);
        }
        else
        {
            Debug.LogError("Audio source or sound is null.");
        }
    }

    private Coroutine? resetSnapStatusCoroutine;
    private Coroutine? disableSnapOnGrab;

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Disable snapping for a moment to prevent snapping during teleportation (Moved up to ensure it's called before anything else)
        disableSnapOnGrab = StartCoroutine(DisableSnapOnGrab());
        OnGrab(BlockGrabInteractable.DetachMode.Primary);
    }
    private void OnGrab(BlockGrabInteractable.DetachMode detachMode)
    {
        SnappedForwarding snappedForwarding = gameObject.GetComponentInChildren<SnappedForwarding>();

        if (detachMode == BlockGrabInteractable.DetachMode.Primary) // Primary grab logic
        {
            Debug.Log($"Block grabbed (primary): {gameObject.name}");

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;

            FixedJoint[] joints = GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints)
            {
                Rigidbody otherRb = joint.connectedBody;
                if (otherRb != null)
                {
                    GameObject otherObject = otherRb.gameObject;
                    DestroyWire(otherRb);
                    ResetSnapStatusOnOtherBlock(otherObject);
                }

                Destroy(joint);

                SnappedForwarding sf = otherRb.GetComponentInChildren<SnappedForwarding>();
                if (sf != null)
                {
                    sf.UpdatePhysics(otherRb);
                    ResnapBlocks(otherRb);
                }
            }
        }
        else if (detachMode == BlockGrabInteractable.DetachMode.Secondary)
        {
            Debug.Log($"Block grabbed (secondary): {gameObject.name}");

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;

            GameObject childBlock = snappedForwarding?.ConnectedBlock;
            GameObject topBlock = null;

            // Destroy joints in block
            FixedJoint[] joints = GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints)
            {
                Rigidbody otherRb = joint.connectedBody;
                if (otherRb != null)
                {
                    GameObject otherObject = otherRb.gameObject;
                    topBlock = otherObject; // Save as parent
                    DestroyWire(otherRb);
                    Destroy(joint);
                }
            }

            // Destroy joints in child block
            if (childBlock != null)
            {
                FixedJoint[] childJoints = childBlock.GetComponents<FixedJoint>();
                Rigidbody childRb = childBlock.GetComponent<Rigidbody>();
                DestroyWire(childRb);
                Debug.Log($"Secondary Grab: Found {childJoints.Length} joint(s) on childBlock: {childBlock.name}");

                foreach (FixedJoint childJoint in childJoints)
                {
                    Debug.Log($"Secondary Grab: Destroying joint on childBlock: {childJoint.name} (connected to: {childJoint.connectedBody?.gameObject.name})");

                    Destroy(childJoint);
                }
            }
            else
            {
                Debug.Log("Secondary grab: childBlock is null!");
            }

            // Snap top block to child block
            if (topBlock != null && childBlock != null) // CASE 1: Block has parent and child block
            {
                FixedJoint newJoint = childBlock.AddComponent<FixedJoint>();
                newJoint.connectedBody = topBlock.GetComponent<Rigidbody>();
                newJoint.breakForce = Mathf.Infinity;
                newJoint.breakTorque = Mathf.Infinity;

                SnappedForwarding topSnap = topBlock.GetComponentInChildren<SnappedForwarding>();
                if (topSnap != null)
                {
                    topSnap.ConnectedBlock = null;
                    snappedForwarding.ConnectedBlock = null;
                    snappedForwarding.SetSnapped(false);
                    topSnap.ConnectedBlock = childBlock;

                    // Update physics
                    Rigidbody topRb = topBlock.GetComponent<Rigidbody>();

                    UpdateChildBlockPositions(topBlock);
                    topSnap.UpdatePhysics(topRb);

                    BlockSnapping topBlockSnapping = topBlock.GetComponent<BlockSnapping>();
                    if (topBlockSnapping != null)
                        topBlockSnapping.ProceedWithRelease();
                }
            }
            else if (childBlock != null) // CASE 2: Block has NO parent block but has child block
            {
                snappedForwarding.ConnectedBlock = null;
                snappedForwarding.SetSnapped(false);

                Rigidbody childRb = childBlock.GetComponent<Rigidbody>();
                BlockSnapping childBlockSnapping = childBlock.GetComponent<BlockSnapping>();
                SnappedForwarding childSnappedForwarding = childBlock.GetComponentInChildren<SnappedForwarding>();

                childBlockSnapping.targetPosition = 1;
                childBlockSnapping.hasSnapped = false;

                UpdateChildBlockPositions(childBlock);
                childSnappedForwarding.UpdatePhysics(childRb);

                childSnappedForwarding.IsRootBlock = true;
                childBlockSnapping.ProceedWithRelease();
            }
            else if (topBlock != null) // Case 3: Block has parent block but NO child block
            {
                SnappedForwarding topSnap = topBlock.GetComponentInChildren<SnappedForwarding>();
                Rigidbody otherRb = topBlock.GetComponent<Rigidbody>();

                ResetSnapStatusOnOtherBlock(topBlock);
                topSnap.UpdatePhysics(otherRb);
                ResnapBlocks(otherRb);
            }
            else // Case 4: Block has NO parent and NO child block
            {
                // Add logic if necessary (don't think it is, will delete on cleanup)
            }
        }
        else
        {
            Debug.LogWarning("detachMode is null!");
        }

        // Start the coroutine and store reference for OnRelease()
        resetSnapStatusCoroutine = StartCoroutine(ResetSnapStatusAfterDelay());

        // Set this block as root block
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

    private IEnumerator DisableSnapOnGrab()
    {
        SnappedForwarding snappedForwarding = GetComponentInChildren<SnappedForwarding>();
        if (snappedForwarding != null)
        {
            snappedForwarding.snappingEnabled = false;
            yield return new WaitForSeconds(0.2f);
            snappedForwarding.snappingEnabled = true;
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

    public void ProceedWithRelease()
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

            Debug.Log($"UpdateBlockPosition: Block '{currentRb.name}' at Physical Position = {physicalPosition}, Target Position = {targetPosition}.");


            // Position updating logic
            int positionDifference = 1 - targetPosition;
            int adjustPositionY = Mathf.Abs(positionDifference) % blockLimit;
            if (positionDifference < 0) adjustPositionY = -adjustPositionY;

            int adjustPositionX = targetColumn;

            if ((blockSnapping.hasWire == true && targetPosition % blockLimit != 0) ||
            (blockSnapping.hasWire == true && snappedForwarding.ConnectedBlock == null))
            {
                Debug.Log("UpdateBlockPosition: DestroyWire Called!");
                DestroyWire(currentRb);
            }

            UpdateBlockPosition(currentRb, initialRootBlockPosition, adjustPositionX, adjustPositionY);

            physicalPosition = targetPosition;
            Debug.Log($"SpawnWire: Physical position = {physicalPosition}, Evaluation = {physicalPosition % blockLimit}");

            if (blockSnapping.hasWire == false && physicalPosition % blockLimit == 0 && connectedRb != null)
            {
                Debug.Log("UpdateBlockPosition: SpawnWire called!");
                SpawnWire(currentRb, connectedRb); // SpawnWire if new position is bottom of the column
            }

            blockSnapping.physicalPosition = targetPosition;
            blockSnapping.currentColumn = targetColumn;

            Rigidbody nextRb = connectedRb;  // The next block in the chain is the connected block.

            currentRb = nextRb;

        }
    }

    private void UpdateBlockPosition(Rigidbody currentRb, Vector3 initialRootBlockPosition, int adjustPositionX, int adjustPositionY)
    {
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
            if (joint.connectedBody != null && !joint.connectedBody.name.Contains("Wire"))
            {
                parentRb = joint.connectedBody;
                GameObject parentObject = parentRb.gameObject;

                // Destroy the joint
                Destroy(joint);
                Debug.Log($"Joint destroyed between {gameObject.name} and {parentObject.name}");

                // Only break if it's a valid parent-child relationship
                SnappedForwarding parentSnap = parentObject.GetComponentInChildren<SnappedForwarding>();
                if (parentSnap != null && parentSnap.ConnectedBlock == this.gameObject)
                {
                    break;
                }
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

    public void ResnapBlocks(Rigidbody currentRb)
    {
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
    }

    public void SpawnWire(Rigidbody rb, Rigidbody connectedRb)
    {
        Debug.Log("SpawnWire: Called!");
        GameObject wirePrefab = Resources.Load<GameObject>("Prefabs/WireLine");

        if (wirePrefab == null || rb == null || connectedRb == null)
        {
            Debug.LogError("Missing wirePrefab or Rigidbody.");
            return;
        }

        Transform snapPointRight = rb.transform.Find("SnapPointRight");
        Transform snapPointLeft = connectedRb.transform.Find("SnapPointLeft");

        if (snapPointRight == null || snapPointLeft == null)
        {
            Debug.LogError("Snap points not found.");
            return;
        }

        GameObject newWire = Instantiate(wirePrefab);
        WireLine wireLine = newWire.GetComponent<WireLine>();

        if (wireLine != null)
        {
            wireLine.startPoint = snapPointRight;
            wireLine.endPoint = snapPointLeft;
            BlockSnapping blockSnapping = rb.GetComponent<BlockSnapping>();
            blockSnapping.hasWire = true;
        }
        else
        {
            Debug.LogError("WireLine script missing from prefab!");
        }

        Debug.Log("LineRenderer wire successfully spawned.");
    }

    public void DestroyWire(Rigidbody rb)
    {
        Debug.Log("DestroyWire: Called!");

        if (rb == null)
        {
            Debug.LogError("DestroyWire: Rigidbody is null.");
            return;
        }

        // Get the SnapPointRight of this block
        Transform snapPointRight = rb.transform.Find("SnapPointRight");
        if (snapPointRight == null)
        {
            Debug.LogError("DestroyWire: SnapPointRight not found on the Rigidbody.");
            return;
        }

        // Find the wire connected to SnapPointRight
        foreach (var wire in FindObjectsOfType<WireLine>())
        {
            if (wire.startPoint == snapPointRight || wire.endPoint == snapPointRight)
            {
                Destroy(wire.gameObject); // Destroy the wire
                Debug.Log("DestroyWire: Wire connected to SnapPointRight destroyed.");
                BlockSnapping blockSnapping = rb.GetComponent<BlockSnapping>();
                blockSnapping.hasWire = false;
                return;
            }
        }

        Debug.LogWarning("DestroyWire: No wire found connected to SnapPointRight.");
    }
}
