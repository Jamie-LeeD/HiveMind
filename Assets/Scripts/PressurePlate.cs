using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens the linked door while the assigned skeleton or a placed box is on the plate.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PressurePlate : MonoBehaviour
{
    static readonly List<PressurePlate> Plates = new List<PressurePlate>();

    [SerializeField] ThirdPersonMotor requiredSkeleton;
    [SerializeField] LockedDoor door;
    [SerializeField] Vector3 detectionSize = new Vector3(1.2f, 1.6f, 1.2f);

    BoxCollider plate;
    bool occupied;

    void Awake()
    {
        plate = GetComponent<BoxCollider>();
        if (plate == null)
        {
            Debug.LogWarning($"{name} needs a box collider.", this);
            return;
        }

        plate.isTrigger = true;
        FitDetectionVolume();
        EnsureKinematicBody();

        if (requiredSkeleton == null)
            Debug.LogWarning($"{name} has no skeleton assigned.", this);
        if (door == null)
            Debug.LogWarning($"{name} has no door assigned.", this);
    }

    void OnEnable()
    {
        if (!Plates.Contains(this))
            Plates.Add(this);
    }

    void OnDisable()
    {
        Plates.Remove(this);
        if (!occupied || door == null)
            return;

        occupied = false;
        door.SetPressed(false);
    }

    void FixedUpdate()
    {
        if (plate == null || door == null)
            return;

        bool held = IsSkeletonOnPlate() || IsBoxOnPlate();
        if (held == occupied)
            return;

        occupied = held;
        door.SetPressed(occupied);
    }

    public static PressurePlate FindPlateAt(Vector3 worldPoint)
    {
        PressurePlate nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (PressurePlate plate in Plates)
        {
            if (plate == null || !plate.ContainsHorizontally(worldPoint))
                continue;

            float distance = HorizontalDistance(worldPoint, plate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = plate;
            }
        }

        return nearest;
    }

    public Vector3 PlacementPoint(float boxHeight)
    {
        Bounds area = plate != null ? plate.bounds : new Bounds(transform.position, Vector3.one);
        return new Vector3(area.center.x, transform.position.y + boxHeight * 0.5f, area.center.z);
    }

    public bool ContainsHorizontally(Vector3 worldPoint)
    {
        if (plate == null)
            return false;

        Bounds area = plate.bounds;
        return worldPoint.x >= area.min.x && worldPoint.x <= area.max.x
            && worldPoint.z >= area.min.z && worldPoint.z <= area.max.z;
    }

    bool IsSkeletonOnPlate()
    {
        if (requiredSkeleton == null)
            return false;

        CharacterController body = requiredSkeleton.GetComponent<CharacterController>();
        return body != null && plate.bounds.Intersects(body.bounds);
    }

    bool IsBoxOnPlate()
    {
        foreach (CarryableBox box in CarryableBox.Active)
        {
            if (box == null || box.IsCarried)
                continue;

            if (plate.bounds.Intersects(box.WorldBounds))
                return true;
        }

        return false;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void FitDetectionVolume()
    {
        Vector3 scale = transform.lossyScale;
        plate.size = new Vector3(
            detectionSize.x / Mathf.Max(0.01f, Mathf.Abs(scale.x)),
            detectionSize.y / Mathf.Max(0.01f, Mathf.Abs(scale.y)),
            detectionSize.z / Mathf.Max(0.01f, Mathf.Abs(scale.z)));
        plate.center = new Vector3(0f, plate.size.y * 0.5f, 0f);
    }

    void EnsureKinematicBody()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
    }
}
