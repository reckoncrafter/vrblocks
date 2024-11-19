using UnityEngine;

public class CollisionForwarding : MonoBehaviour
{
    public BlockSnapping parentBlockSnapping;

    private void OnTriggerEnter(Collider other)
    {
        if (parentBlockSnapping != null)
        {
            parentBlockSnapping.OnChildTriggerEnter(this, other);
        }
    }
}
