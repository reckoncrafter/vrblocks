using System.Linq;
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
            // Find the jointObject child
            Transform jointObject = currentBlock.transform.Find("FixedJointObject");

            if (jointObject != null)
            {
                FixedJoint joint = jointObject.GetComponent<FixedJoint>();

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
                    Debug.Log($"No FixedJoint found within FixedJointObject for {currentBlock.name}. Stopping traversal.");
                    break;
                }
            }
            else
            {
                Debug.Log($"No FixedJointObject found for {currentBlock.name}. Stopping traversal.");
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
            // Find the FixedJointObject and destroy it to allow block realignment
            Transform jointObject = rb.transform.Find("FixedJointObject");
            Rigidbody parentRb = null;

            if (jointObject != null)
            {
                FixedJoint joint = jointObject.GetComponent<FixedJoint>();

                if (joint != null && joint.connectedBody != null && !joint.connectedBody.name.Contains("Wire"))
                {
                    parentRb = joint.connectedBody;  // Store the parent Rigidbody for later joint recreation
                    Destroy(jointObject.gameObject); // Destroy the entire FixedJointObject
                    Debug.Log("FixedJointObject destroyed, ready for realignment.");
                }
                else
                {
                    Debug.Log("No valid joint or parent found.");
                }
            }

            // Get the parent position (you can adjust this as needed)
            Vector3 parentPosition = parentRb.transform.position;

            // Update position to match the X and Z of the parent block, leaving Y unchanged
            rb.transform.position = new Vector3(parentPosition.x, rb.transform.position.y, parentPosition.z);

            // Reset rotation to zero (you can adjust if needed)
            rb.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Realign the SnapPointTop with SnapPointBottom
            Transform snapPointTop = rb.transform.Find("SnapPointTop");
            Transform snapPointBottom = parentRb.transform.Find("SnapPointBottom");

            if (snapPointTop != null && snapPointBottom != null)
            {
                Vector3 offset = snapPointTop.position - rb.transform.position;
                Vector3 realignPosition = snapPointBottom.position - offset;
                rb.transform.position = new Vector3(parentPosition.x, realignPosition.y, parentPosition.z);
            }

            // Create a new FixedJointObject to properly organize the joint
            GameObject newJointObject = new GameObject("FixedJointObject");
            newJointObject.transform.SetParent(rb.transform);
            newJointObject.transform.localPosition = Vector3.zero;
            newJointObject.transform.localRotation = Quaternion.identity;

            // Create a new joint on the new joint holder object
            if (parentRb != null)
            {
                FixedJoint newJoint = newJointObject.AddComponent<FixedJoint>();
                newJoint.connectedBody = parentRb;
                newJoint.breakForce = Mathf.Infinity;
                newJoint.breakTorque = Mathf.Infinity;
                Debug.Log("New joint created and connected to parent.");
            }
            else
            {
                Debug.Log("No parent Rigidbody found, no joint recreated.");
            }
        }
    }
}