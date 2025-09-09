using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;        // Assign the player here
    public Vector3 offset = new Vector3(0, 0, -10); // Default offset for 2D camera

    [Header("Smooth Follow")]
    public float smoothSpeed = 5f;  // How fast the camera follows

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired camera position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate from current position to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Call this after spawning or repositioning the player
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // Optional: instantly move camera to target when assigned
        if (target != null)
            transform.position = target.position + offset;
    }
}
