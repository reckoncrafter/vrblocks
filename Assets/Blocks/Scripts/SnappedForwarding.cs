using UnityEngine;

public class SnappedForwarding : MonoBehaviour
{
    public bool IsSnapped { get; private set; } = false;  // Flag to prevent further snapping

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
}
