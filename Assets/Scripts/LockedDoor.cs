using UnityEngine;

/// <summary>
/// Rotates the door panel 90 degrees while its pressure plate stays pressed.
/// </summary>
public class LockedDoor : MonoBehaviour
{
    [SerializeField] Transform doorPanel;
    [Tooltip("Local Y rotation while the plate is held. Use -90 if the door swings the wrong way.")]
    [SerializeField] float openAngle = 90f;
    [SerializeField] float rotateSpeed = 180f;
    [SerializeField] bool requireContinuousPressure = true;

    Vector3 closedLocalPosition;
    Quaternion closedLocalRotation;
    bool pressed;
    bool isOpen;
    bool ready;
    Rigidbody panelBody;

    void Awake()
    {
        if (doorPanel == null)
        {
            Debug.LogWarning($"{name} has no door panel assigned.", this);
            return;
        }

        closedLocalPosition = doorPanel.localPosition;
        closedLocalRotation = doorPanel.localRotation;
        PreparePanelCollider();
        ready = true;
    }

    public void SetPressed(bool value)
    {
        pressed = value;
        if (pressed)
            isOpen = true;
        else if (requireContinuousPressure)
            isOpen = false;
    }

    void Update()
    {
        if (!ready)
            return;

        Quaternion targetLocal = isOpen
            ? closedLocalRotation * Quaternion.Euler(0f, openAngle, 0f)
            : closedLocalRotation;

        doorPanel.localPosition = closedLocalPosition;
        doorPanel.localRotation = Quaternion.RotateTowards(
            doorPanel.localRotation,
            targetLocal,
            rotateSpeed * Time.deltaTime);

        if (panelBody == null)
            return;

        panelBody.position = doorPanel.position;
        panelBody.rotation = doorPanel.rotation;
    }

    void PreparePanelCollider()
    {
        MeshCollider mesh = doorPanel.GetComponent<MeshCollider>();
        if (mesh != null)
            DestroyImmediate(mesh);

        BoxCollider box = doorPanel.GetComponent<BoxCollider>();
        if (box == null)
            box = doorPanel.gameObject.AddComponent<BoxCollider>();

        Renderer renderer = doorPanel.GetComponent<Renderer>();
        if (renderer != null)
            FitBoxToRenderer(box, renderer);

        panelBody = doorPanel.GetComponent<Rigidbody>();
        if (panelBody == null)
            panelBody = doorPanel.gameObject.AddComponent<Rigidbody>();

        panelBody.isKinematic = true;
        panelBody.useGravity = false;
    }

    void FitBoxToRenderer(BoxCollider box, Renderer renderer)
    {
        Bounds bounds = renderer.bounds;
        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector3 extents = bounds.extents;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = bounds.center + Vector3.Scale(extents, new Vector3(x, y, z));
                    Vector3 local = doorPanel.InverseTransformPoint(corner);
                    min = Vector3.Min(min, local);
                    max = Vector3.Max(max, local);
                }
            }
        }

        box.center = (min + max) * 0.5f;
        Vector3 size = max - min;
        box.size = new Vector3(
            Mathf.Clamp(size.x, 0.05f, 3f),
            Mathf.Clamp(size.y, 0.05f, 3f),
            Mathf.Clamp(size.z, 0.05f, 3f));
    }
}
