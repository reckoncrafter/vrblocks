using UnityEngine;

public class DeletionBoundary : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered.");

        // something similar might be needed for start queue
        // turtle cannot be destroyed
        if (other.gameObject.CompareTag("Player"))
        {
            //other.gameObject.GetComponent<TurtleMovement>().Reset();
            return;
        }

        Destroy(other.gameObject);
    }

    void OnTriggerExit()
    {
        Debug.Log("Trigger Exited.");
    }
}
