using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BlockSnapping : MonoBehaviour
{
    private bool hasSnapped = false; // Flag to prevent repeated snapping

    private void Awake()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this.gameObject.name + " entered by: " + other.name);

        // Check if SnapTriggerTop was entered by SnapTriggerBottom
        if (this.gameObject.name == "SnapTriggerTop" && other.gameObject.name == "SnapTriggerBottom" && !hasSnapped)
        {
            Debug.Log("SnapTriggerTop met SnapTriggerBottom. Snapping blocks!");
            SnapToBlock(this.transform.parent.gameObject, other.transform.parent.gameObject);
            hasSnapped = true;
            //Debug.Log("hasSnapped set to true for: " + gameObject.name);
        }

        if (this.gameObject.name == "SnapTriggerTop" && other.gameObject.name == "SnapTriggerBottom" && hasSnapped)
        {
            //Debug.Log("Snap possible but currently disabled.");
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

        // Reset X and Z rotations for both blocks
        block1.transform.rotation = Quaternion.Euler(0, block1.transform.rotation.eulerAngles.y, 0);
        block2.transform.rotation = Quaternion.Euler(0, block1.transform.rotation.eulerAngles.y, 0);

        // Sync the Y position of block2 to block1
        block2.transform.position = new Vector3(block2.transform.position.x, snapPointTop.position.y, block2.transform.position.z);

        // Snap block2's SnapPointBottom to block1's SnapPointTop
        block2.transform.position = snapPointTop.position - (snapPointBottom.position - block2.transform.position);

        // Add FixedJoint to lock blocks together properly
        Rigidbody rb2 = block2.GetComponent<Rigidbody>();
        if (rb2 != null)
        {
            FixedJoint joint = block1.AddComponent<FixedJoint>();
            joint.connectedBody = rb2;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            //rb2.isKinematic = false;
        }

        // Re-enable physics on the bottom block
        Rigidbody rb1 = block1.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            rb1.isKinematic = false;
            //Debug.Log("Physics re-enabled on: " + block1.name);
        }

        Debug.Log(block2.name + " snapped to " + block1.name);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Block grabbed: " + gameObject.name);

        // Check for joints connected to objects
        FixedJoint[] joints = GetComponents<FixedJoint>();
        foreach (var joint in joints)
        {
            Destroy(joint); // Destroy the joint to unsnap
            //Debug.Log(gameObject.name + " unsnapped from " + joint.connectedBody.name);
        }

        StartCoroutine(ResetSnapStatusAfterDelay()); // Wait arbitrary amount of time to prevent block resnapping immediately.
    }

    private IEnumerator ResetSnapStatusAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds (change as needed)

        Transform snapTriggerTop = transform.Find("SnapTriggerTop");
        if (snapTriggerTop != null)
        {
            BlockSnapping topBlockSnapping = snapTriggerTop.GetComponent<BlockSnapping>();
            if (topBlockSnapping != null)
            {
                topBlockSnapping.hasSnapped = false; // Reset the snap status
                //Debug.Log("hasSnapped reset to false for: " + snapTriggerTop.name);
            }
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        //Debug.Log("Block released: " + gameObject.name);
    }
}
