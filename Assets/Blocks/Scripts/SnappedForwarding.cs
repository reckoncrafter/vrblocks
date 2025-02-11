using UnityEngine;

public class SnappedForwarding : MonoBehaviour
{
    public bool IsRootBlock { get; set; } = true;
    public bool IsSnapped { get; private set; } = false;  // Flag to prevent further snapping
    public GameObject ConnectedBlock { get; set; }

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

        // Count the current block
        blockCount++;

        // Check if there is a connected block and read the next one
        SnappedForwarding snappedForwarding = currentBlock.GetComponentInChildren<SnappedForwarding>();
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

    public GameObject FindRootBlock(GameObject startingBlock)
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
}
