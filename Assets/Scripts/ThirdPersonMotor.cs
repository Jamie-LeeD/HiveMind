using UnityEngine;

/// <summary>
/// Camera-relative movement for the witch or a possessed skeleton.
/// Only the enabled motor reads movement input.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMotor : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4.5f;
    [SerializeField] float rotationSpeed = 12f;
    [SerializeField] float gravity = -20f;
    [SerializeField] GameplayInput input;
    [SerializeField] Transform viewCamera;

    static readonly int IsMovingId = Animator.StringToHash("IsMoving");

    CharacterController controller;
    Animator animator;
    float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (controller == null)
        {
            Debug.LogWarning($"{name} had no CharacterController. One was added.", this);
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
        }

        // Child capsules were static stand-ins. They would collide with this controller.
        foreach (CapsuleCollider capsule in GetComponentsInChildren<CapsuleCollider>(true))
        {
            if (capsule.gameObject != gameObject)
                capsule.enabled = false;
        }
    }

    void OnEnable()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    public void SetControlEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            verticalVelocity = 0f;
            SetMoving(false);
        }
    }

    void OnDisable()
    {
        SetMoving(false);
    }

    void Update()
    {
        if (controller == null || input == null || input.Move == null)
            return;

        Vector2 moveInput = input.Move.ReadValue<Vector2>();
        Vector3 move = CameraRelativeMove(moveInput);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        bool moving = move.sqrMagnitude > 0.001f;
        SetMoving(moving);

        if (moving)
        {
            Quaternion facing = Quaternion.LookRotation(move);
            float turn = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turn);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    Vector3 CameraRelativeMove(Vector2 moveInput)
    {
        Transform view = viewCamera;
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        Vector3 right = view != null ? view.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();
        right.Normalize();
        return forward * moveInput.y + right * moveInput.x;
    }

    void SetMoving(bool moving)
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator == null)
            return;

        animator.SetBool(IsMovingId, moving);
    }
}
