using UnityEngine;

/// <summary>
/// The witch wins when she walks into this trigger. A possessed skeleton does not.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class WinTrigger : MonoBehaviour
{
    [SerializeField] ThirdPersonMotor witch;
    [SerializeField] PossessionController possession;

    BoxCollider area;
    bool awarded;

    void Awake()
    {
        area = GetComponent<BoxCollider>();
        if (area != null)
            area.isTrigger = true;

        if (witch == null)
            Debug.LogWarning($"{name} has no witch assigned.", this);
        if (possession == null)
            Debug.LogWarning($"{name} has no possession controller assigned.", this);
    }

    void FixedUpdate()
    {
        if (awarded || area == null || witch == null || possession == null || possession.IsGameOver)
            return;

        CharacterController body = witch.GetComponent<CharacterController>();
        if (body == null || !area.bounds.Intersects(body.bounds))
            return;

        awarded = true;
        possession.Win();
    }
}
