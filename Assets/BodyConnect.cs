using UnityEngine;

public class BodyConnect : MonoBehaviour
{
    public GameObject objectB;

    void OnDrawGizmos()
    {
        // Check if both GameObjects are set
        if (objectB != null)
        {
            // Set the color of the Gizmo
            Gizmos.color = Color.red;

            // Draw a line between the two GameObjects
            Gizmos.DrawLine(transform.position, objectB.transform.position);
        }
    }
}