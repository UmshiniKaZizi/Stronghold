using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 20f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    // Input
    private Vector2 moveInput;

    // State
    private bool isGrounded;
    private bool canMove = true;
    private bool canJump = true;

    // Public read-only information for other systems
    public bool IsGrounded => isGrounded;
    public bool CanMove => canMove;
    public bool CanJump => canJump;

    private void Awake()
    {
        // Automatically find Rigidbody if one wasn't assigned.
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            StopHorizontalMovement();
            return;
        }

        MovePlayer();
    }

    // ============================================================
    // INPUT
    // ============================================================

    public void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        TryJump();
    }

    // ============================================================
    // MOVEMENT
    // ============================================================

    private void MovePlayer()
    {
        Vector3 direction =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        // Prevent diagonal movement from being faster.
        direction = Vector3.ClampMagnitude(direction, 1f);

        Vector3 targetVelocity = direction * moveSpeed;

        Vector3 currentVelocity = rb.linearVelocity;

        float currentAcceleration =
            direction.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        Vector3 horizontalVelocity = Vector3.MoveTowards(
            new Vector3(currentVelocity.x, 0f, currentVelocity.z),
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(
            horizontalVelocity.x,
            currentVelocity.y,
            horizontalVelocity.z
        );
    }

    private void StopHorizontalMovement()
    {
        Vector3 velocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            0f,
            velocity.y,
            0f
        );
    }

    // ============================================================
    // JUMP
    // ============================================================

    private void TryJump()
    {
        if (!canJump || !isGrounded || !canMove)
            return;

        // Remove existing downward velocity so jump height
        // remains consistent.
        Vector3 velocity = rb.linearVelocity;

        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    // ============================================================
    // GROUND DETECTION
    // ============================================================

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    // ============================================================
    // CONTROL
    // ============================================================

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled)
            StopHorizontalMovement();
    }

    public void SetJumpEnabled(bool enabled)
    {
        canJump = enabled;
    }

    // ============================================================
    // DEBUGGING
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundDistance
        );
    }
}