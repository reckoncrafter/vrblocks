using UnityEngine;

public class AntiClipping : MonoBehaviour
{
    public float floorLevel = 0f;
    public float padding = 0.1f;

    void Update()
    {
        if (transform.position.y < floorLevel + padding)
        {
            transform.position = new Vector3(transform.position.x, floorLevel + padding, transform.position.z);
        }
    }
}
