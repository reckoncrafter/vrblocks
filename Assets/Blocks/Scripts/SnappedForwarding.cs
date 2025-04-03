using UnityEngine;

public class SnappedForwarding : MonoBehaviour
{
    public bool IsRootBlock { get; set; } = true;
    public bool IsSnapped { get; private set; } = false;  // Flag to prevent further snapping
    public GameObject? ConnectedBlock { get; set; }

    // Get snapped status
    public bool CanSnap()
    {
        return !IsSnapped;
    }

    // Set the snap status
    public void SetSnapped(bool snapped)
    {
        IsSnapped = snapped;
    }

    // Method to count blocks attached to root
    public int CountBlocks(GameObject startingBlock)
    {
        int blockCount = 0;
        if (startingBlock != null)
        {
            ReadBlocks(startingBlock, ref blockCount);
        }
        else
        {
            Debug.LogError("Starting block is null!");
        }
        return blockCount;
    }

    private void ReadBlocks(GameObject currentBlock, ref int blockCount)
    {
        // Skip wire blocks by checking their name
        if (currentBlock == null || currentBlock.name.Contains("Wire"))
        {
            return;
        }

        SnappedForwarding snappedForwarding = currentBlock.GetComponentInChildren<SnappedForwarding>();

        // Count the current block
        blockCount++;

        // Call UpdatePhysics to help with physics issues
        Rigidbody rb = currentBlock.GetComponent<Rigidbody>();
        if (rb != null)
        {
            UpdatePhysics(rb);
        }

        // Check if there is a connected block and read the next one
        if (snappedForwarding != null && snappedForwarding.ConnectedBlock != null)
        {
            GameObject connectedBlock = snappedForwarding.ConnectedBlock;
            if (connectedBlock != null)
            {
                ReadBlocks(connectedBlock, ref blockCount);
            }
            else
            {
                Debug.LogWarning($"ConnectedBlock is null for {currentBlock.name}. Stopping traversal.");
            }
        }
        else
        {
            Debug.LogWarning($"No connected block found for {currentBlock.name}. Stopping traversal.");
        }
    }

    public GameObject? FindRootBlock(GameObject startingBlock)
    {
        // Null check for starting block
        if (startingBlock == null)
        {
            Debug.LogError("Starting block is null!");
            return null;
        }

        // Check if the starting block is the root block
        SnappedForwarding startingSnappedForwarding = startingBlock.GetComponentInChildren<SnappedForwarding>();
        if (startingSnappedForwarding != null && startingSnappedForwarding.IsRootBlock)
        {
            Debug.Log($"Starting block {startingBlock.name} is the root block.");
            return startingBlock;
        }

        GameObject currentBlock = startingBlock;

        // Loop until we find the root block
        while (currentBlock != null)
        {
            FixedJoint joint = currentBlock.GetComponent<FixedJoint>();

            if (joint != null)
            {
                Rigidbody connectedBody = joint.connectedBody;
                if (connectedBody != null)
                {
                    currentBlock = connectedBody.gameObject;  // Move to the connected block
                }
                else
                {
                    Debug.Log($"No connected body found for {currentBlock.name}. Stopping traversal.");
                    break;
                }
            }
            else
            {
                Debug.Log($"No FixedJoint found for {currentBlock.name}. Stopping traversal.");
                break;
            }

            SnappedForwarding snappedForwarding = currentBlock.GetComponentInChildren<SnappedForwarding>();
            if (snappedForwarding != null && snappedForwarding.IsRootBlock)
            {
                Debug.Log($"Root block found: {currentBlock.name}");
                return currentBlock;
            }
        }

        Debug.Log($"No root block found starting from {startingBlock.name}");
        return null;
    }

    public void UpdatePhysics(Rigidbody rb)
    {
        SnappedForwarding snappedForwarding = rb.GetComponentInChildren<SnappedForwarding>();
        BlockSnapping blockSnapping = rb.GetComponent<BlockSnapping>();
        bool canSnap = snappedForwarding != null && snappedForwarding.CanSnap();
        bool hasSnapped = blockSnapping != null && blockSnapping.hasSnapped;

        Debug.Log($"CanSnap: {canSnap}, hasSnapped: {hasSnapped}");

        if (rb.gameObject.name == "Block (StartQueue)")
        {
            Debug.Log("Physics Update: Block is StartQueue.");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }
        else if (hasSnapped)
        {
            Debug.Log($"Physics Update: CASE 1 (Snapped on top)");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.rotation = Quaternion.Euler(0, 0, 0);
            // No changes to rb.constraints
        }
        else if (canSnap && !hasSnapped)
        {
            Debug.Log($"Physics Update: CASE 2 (Can snap but not snapped)");
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
        else
        {
            Debug.Log($"Physics Update: CASE DEFAULT");
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }

        // Rotation and position update based on parent block.
        if (snappedForwarding != null && !snappedForwarding.IsRootBlock)
        {
            // Find parent block through the FixedJoint (as used in OnGrab())
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints)
            {
                GameObject otherRbParent = joint.connectedBody.gameObject;
                // Ignore wires
                if (!otherRbParent.name.Contains("Wire"))
                {
                    FixedJoint[] jointList1 = rb.GetComponents<FixedJoint>();
                    Debug.Log($"UpdatePhysics: Initial number of joints on Block: {rb.name},  (BEFORE PHYSICS UPDATE) = {jointList1.Length}");

                    // Destroy the connecting joint to allow block realignment while retaining physics reactivity
                    Destroy(joint);

                    //rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                    //rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                    //rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

                    // Update position to match the X and Z of the parent block
                    Vector3 parentPosition = otherRbParent.transform.position;
                    rb.transform.position = new Vector3(parentPosition.x, rb.transform.position.y, parentPosition.z);

                    // Reset rotation to 0
                    rb.transform.rotation = Quaternion.Euler(0, 0, 0);
                    //Debug.Log($"Physics Update: Position/Rotation reset");

                    // Update position to match the X and Z of the parent block, realign SnapPointTop with SnapPointBottom
                    Transform snapPointTop = rb.transform.Find("SnapPointTop");
                    Transform snapPointBottom = otherRbParent.transform.Find("SnapPointBottom");

                    if (snapPointTop != null && snapPointBottom != null)
                    {
                        Vector3 offset = snapPointTop.position - rb.transform.position;
                        Vector3 realignPosition = snapPointBottom.position - offset;
                        rb.transform.position = new Vector3(parentPosition.x, realignPosition.y, parentPosition.z);
                    }

                    // DO NOT Create new joint to reconnect the blocks

                    FixedJoint[] jointList2 = rb.GetComponents<FixedJoint>();
                    Debug.Log($"UpdatePhysics: Initial number of joints on Block: {rb.name},  (AFTER PHYSICS UPDATE) = {jointList2.Length}");

                    break;
                }
            }
        }
    }
}