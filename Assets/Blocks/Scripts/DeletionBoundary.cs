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

        Destroy(other.gameObject);
        objectDestroyedEvent.Invoke();
    }

    void OnTriggerExit()
    {
        Debug.Log("Trigger Exited.");
    }
}
