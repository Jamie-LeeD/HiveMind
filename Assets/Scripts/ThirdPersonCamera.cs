using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Follows one target with the scene's only camera. Possession changes the target.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameplayInput input;
    [SerializeField] float distance = 2.2f;
    [SerializeField] float focusHeight = 1.2f;
    [SerializeField] float minDistance = 0.6f;
    [SerializeField] float collisionRadius = 0.2f;
    [SerializeField] float mouseSensitivity = 0.1f;
    [SerializeField] float stickSensitivity = 160f;
    [SerializeField] float minPitch = -10f;
    [SerializeField] float maxPitch = 55f;
    [SerializeField] float followSharpness = 14f;
    [SerializeField] float pitch = 12f;

    float yaw;
    bool snapPending = true;
    bool lookEnabled = true;

    public bool LookEnabled
    {
        get => lookEnabled;
        set => lookEnabled = value;
    }

    public void SetTarget(Transform newTarget, bool snap)
    {
        target = newTarget;
        if (snap)
            snapPending = true;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (lookEnabled && input != null && input.Look != null && Cursor.lockState == CursorLockMode.Locked)
            ApplyLook(input.Look.ReadValue<Vector2>());

        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * focusHeight;
        Vector3 desiredPosition = ResolveCollision(focus, focus - orbit * Vector3.forward * distance);

        if (snapPending)
        {
            transform.SetPositionAndRotation(desiredPosition, orbit);
            snapPending = false;
            return;
        }

        float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        transform.rotation = Quaternion.Slerp(transform.rotation, orbit, blend);
    }

    Vector3 ResolveCollision(Vector3 focus, Vector3 desired)
    {
        Vector3 offset = desired - focus;
        float length = offset.magnitude;
        if (length < 0.05f)
            return desired;

        Vector3 direction = offset / length;
        RaycastHit[] hits = Physics.SphereCastAll(
            focus,
            collisionRadius,
            direction,
            length,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        float nearest = length;
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == target || hit.transform.IsChildOf(target))
                continue;

            if (hit.distance < nearest)
                nearest = hit.distance;
        }

        float allowed = Mathf.Clamp(nearest - collisionRadius, minDistance, length);
        return focus + direction * allowed;
    }

    void ApplyLook(Vector2 look)
    {
        InputDevice device = input.Look.activeControl != null ? input.Look.activeControl.device : null;
        bool pointer = device is Pointer;
        float scale = pointer ? mouseSensitivity : stickSensitivity * Time.deltaTime;
        yaw += look.x * scale;
        pitch = Mathf.Clamp(pitch - look.y * scale, minPitch, maxPitch);
    }
}
