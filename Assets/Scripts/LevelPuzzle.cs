using UnityEngine;

/// <summary>
/// One level's skeleton, locked door, and pressure plate.
/// </summary>
public class LevelPuzzle : MonoBehaviour
{
    [SerializeField] string levelName = "Level";
    [SerializeField] LockedDoor lockedDoor;
    [SerializeField] PressurePlate pressurePlate;
    [SerializeField] ThirdPersonMotor skeleton;
    [SerializeField] Transform interactionPoint;
    [SerializeField] float interactionDistance = 2f;

    public string LevelName => levelName;
    public ThirdPersonMotor Skeleton => skeleton;
    public float InteractionDistance => interactionDistance;

    public bool SkeletonAvailable => skeleton != null && skeleton.gameObject.activeInHierarchy;

    void Awake()
    {
        if (lockedDoor == null || pressurePlate == null || skeleton == null || interactionPoint == null)
            Debug.LogWarning($"{name} is missing a puzzle reference.", this);
    }

    public float DistanceToDoor(Vector3 witchPosition)
    {
        if (interactionPoint == null)
            return float.MaxValue;

        Vector3 door = interactionPoint.position;
        witchPosition.y = 0f;
        door.y = 0f;
        return Vector3.Distance(witchPosition, door);
    }

    public bool IsWitchInRange(Vector3 witchPosition)
    {
        return DistanceToDoor(witchPosition) <= interactionDistance;
    }

    void OnDrawGizmosSelected()
    {
        if (interactionPoint == null)
            return;

        Gizmos.color = new Color(0.6f, 0.3f, 1f, 0.8f);
        Gizmos.DrawWireSphere(interactionPoint.position, interactionDistance);
    }
}
