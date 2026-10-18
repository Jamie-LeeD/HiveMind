using UnityEngine;

/// <summary>
/// Lets a possessed skeleton pick up a box and place it, including on a pressure plate.
/// </summary>
public class SkeletonCarrier : MonoBehaviour
{
    [SerializeField] ThirdPersonMotor motor;
    [SerializeField] GameplayInput input;
    [SerializeField] float pickupRange = 1.6f;
    [SerializeField] float holdHeight = 1.15f;
    [SerializeField] float holdForward = 0.75f;
    [SerializeField] float placeDistance = 0.9f;

    CarryableBox carried;
    Transform holdPoint;

    public string Prompt { get; private set; } = "";

    void Awake()
    {
        if (motor == null)
            motor = GetComponent<ThirdPersonMotor>();

        holdPoint = new GameObject("BoxHold").transform;
        holdPoint.SetParent(transform, false);
        holdPoint.localPosition = new Vector3(0f, holdHeight, holdForward);
        holdPoint.localRotation = Quaternion.identity;
    }

    void Update()
    {
        bool controlled = motor != null && motor.enabled && input != null && input.Carry != null;
        if (!controlled)
        {
            if (carried != null)
                Drop();
            Prompt = "";
            return;
        }

        if (carried != null)
        {
            Prompt = "Press E to place";
            if (input.Carry.WasPressedThisFrame())
                Drop();
            return;
        }

        CarryableBox nearby = FindNearbyBox();
        Prompt = nearby != null ? "Press E to pick up" : "";
        if (nearby != null && input.Carry.WasPressedThisFrame())
            Pickup(nearby);
    }

    void Pickup(CarryableBox box)
    {
        carried = box;
        box.PickUp(holdPoint);
    }

    void Drop()
    {
        if (carried == null)
            return;

        Vector3 ahead = transform.position + transform.forward * placeDistance;
        PressurePlate plate = PressurePlate.FindPlateAt(ahead);
        Vector3 position = plate != null
            ? plate.PlacementPoint(carried.Height)
            : PlaceOnGround(ahead, carried.Height);

        CarryableBox box = carried;
        carried = null;
        box.Place(position);
    }

    Vector3 PlaceOnGround(Vector3 ahead, float height)
    {
        Vector3 origin = ahead + Vector3.up * 2f;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 6f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        Vector3 point = new Vector3(ahead.x, transform.position.y, ahead.z);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;
            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                point = hit.point;
            }
        }

        point.y += height * 0.5f;
        return point;
    }

    CarryableBox FindNearbyBox()
    {
        CarryableBox nearest = null;
        float bestDistance = pickupRange;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f)
            forward.Normalize();

        foreach (CarryableBox box in CarryableBox.Active)
        {
            if (box == null || box.IsCarried)
                continue;

            Vector3 toBox = box.transform.position - origin;
            toBox.y = 0f;
            float distance = toBox.magnitude;
            if (distance > bestDistance)
                continue;

            if (distance > 0.25f && Vector3.Dot(forward, toBox.normalized) < 0.15f)
                continue;

            bestDistance = distance;
            nearest = box;
        }

        return nearest;
    }
}
