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
            TurtleMovement tm;
            if(other.gameObject.TryGetComponent<TurtleMovement>(out tm))
            {
                tm.canReset = true;
                tm.Reset();
            }
            return;
        }

        Destroy(other.gameObject);
    }

    void OnTriggerExit()
    {
        Debug.Log("Trigger Exited.");
    }
}
