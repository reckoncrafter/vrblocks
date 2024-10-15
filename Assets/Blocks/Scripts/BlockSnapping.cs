using System.Collections;
using System.Collections.Generic;
//using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class BlockSnapping : MonoBehaviour
{
    private bool hasSnapped = false; // Flag to check if snapped, snap would loop over and over sometimes.

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this.gameObject.name + " entered by: " + other.name);

        // Check if SnapTriggerBottom was entered by SnapTriggerTop
        if (this.gameObject.name == "SnapTriggerBottom" && other.gameObject.name == "SnapTriggerTop" && !hasSnapped)
        {
            Debug.Log("SnapTriggerBottom met SnapTriggerTop. Snapping blocks!");
            SnapToBlock(this.transform.parent.gameObject, other.transform.parent.gameObject); // block1 is bottom (SnapTriggerBottom), block2 is top (SnapTriggerTop)
            hasSnapped = true;
        }
    }

    private void SnapToBlock(GameObject block1, GameObject block2)
    {
        Rigidbody rb2 = block2.GetComponent<Rigidbody>();
        Collider col2 = block2.GetComponent<Collider>();

        // Block2 collider disabled to prevent clipping through plane.
        if (col2 != null)
        {
            col2.enabled = false;
        }

        // Calculate the center of the bottom of block1
        Vector3 bottomCenterBlock1 = block1.GetComponent<Collider>().bounds.center;
        bottomCenterBlock1.y = block1.GetComponent<Collider>().bounds.min.y;

        // Move block2 to match block1 x and z and snap to bottom of block1
        block2.transform.position = new Vector3(
            block1.transform.position.x,
            bottomCenterBlock1.y + (block2.GetComponent<Collider>().bounds.extents.y),
            block1.transform.position.z 
        );

        // Set the rotation of block2 to match block1's
        block2.transform.rotation = block1.transform.rotation;

        // Disable physics block2 physics to get them to stick better (to fix a bug, may not need anymore.)
        {
            rb2.isKinematic = true;
        }

        // Parent block2 to block1 to make them move together (Doesn't work well, need to read into XR Interactable Documentation)
        block2.transform.SetParent(block1.transform);
        Debug.Log(block2.name + " parented to " + block1.name);

        // Re-enable physics for block2
        if (rb2 != null)
        {
            rb2.isKinematic = false;
            rb2.velocity = Vector3.zero; // To prevent clipping through plane.
        }

        // Re-enable the collider
        if (col2 != null)
        {
            col2.enabled = true;
        }

        Rigidbody rb1 = block1.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            rb1.isKinematic = false;
            Debug.Log("Physics re-enabled on: " + block1.name);
        }

        Debug.Log("Block2 (" + block2.name + ") snapped to Block1 (" + block1.name + ")");
    }

    //TODO: Create function to unsnap blocks on grab. Need to read further XR interaction documentation.
}
