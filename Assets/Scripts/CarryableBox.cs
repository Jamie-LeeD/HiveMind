using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A box a skeleton can carry. While it is not being carried, it can hold a pressure plate down.
/// </summary>
public class CarryableBox : MonoBehaviour
{
    public static readonly List<CarryableBox> Active = new List<CarryableBox>();

    Collider body;

    public bool IsCarried { get; private set; }
    public float Height { get; private set; }

    public Bounds WorldBounds
    {
        get
        {
            if (body != null && body.enabled)
                return body.bounds;

            return new Bounds(transform.position, new Vector3(0.75f, Height, 0.75f));
        }
    }

    void Awake()
    {
        body = GetComponent<Collider>();
        Height = body != null ? Mathf.Max(0.2f, body.bounds.size.y) : 0.75f;
    }

    void OnEnable()
    {
        if (!Active.Contains(this))
            Active.Add(this);
    }

    void OnDisable()
    {
        Active.Remove(this);
    }

    public void PickUp(Transform holdPoint)
    {
        IsCarried = true;
        if (body != null)
            body.enabled = false;

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Place(Vector3 worldPosition)
    {
        transform.SetParent(null, true);
        transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
        IsCarried = false;

        if (body != null)
            body.enabled = true;
    }
}
