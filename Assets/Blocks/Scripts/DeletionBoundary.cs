using UnityEngine;
using UnityEngine.Events;

public class DeletionBoundary : MonoBehaviour
{
    public UnityEvent objectDestroyedEvent = new UnityEvent();
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered.");

        // something similar might be needed for start queue
        // turtle cannot be destroyed
        if (other.gameObject.CompareTag("Player") ||
            other.gameObject.CompareTag("MiniMap") ||
            other.gameObject.CompareTag("StartBlock"))
        {
            //other.gameObject.GetComponent<TurtleMovement>().Reset();
            return;
        }

        if (other.gameObject.CompareTag("Block"))
        {
            Joint joint = other.GetComponent<Joint>();
            if (joint != null && joint.connectedBody != null)
            {
                GameObject parentBlock = joint.connectedBody.gameObject;
                BlockSnapping parentSnapping = parentBlock.GetComponent<BlockSnapping>();
                if (parentSnapping != null)
                {
                    parentSnapping.ResetSnapStatusOnOtherBlock(parentBlock);
                    Rigidbody parentRb = parentBlock.GetComponent<Rigidbody>();
                    parentSnapping.DestroyWire(parentRb);
                }
            }
        }

        Destroy(other.gameObject);
        objectDestroyedEvent.Invoke();
    }

    void OnTriggerExit()
    {
        Debug.Log("Trigger Exited.");
    }
}
