
using UnityEngine;

public class StrongholdPlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Animation Settings")]
    [SerializeField] private float movementDampTime = 0.1f;

    [Tooltip("Playback speed while moving. Use 1 for normal speed.")]
    [SerializeField, Range(0.1f, 1f)]
    private float movementPlaybackSpeed = 0.7f;

    private Rigidbody rb;
    private float originalAnimatorSpeed = 1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        if (playerMovement != null)
            rb = playerMovement.GetComponent<Rigidbody>();

        if (animator != null)
            originalAnimatorSpeed = animator.speed;
    }

    private void Update()
    {
        UpdateMovementAnimation();
        UpdateAnimationPlaybackSpeed();
    }

    private void UpdateMovementAnimation()
    {
        if (playerMovement == null || rb == null || animator == null)
            return;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float movementSpeed = horizontalVelocity.magnitude;

        animator.SetFloat(
            "Movement",
            movementSpeed,
            movementDampTime,
            Time.deltaTime
        );
    }

    private void UpdateAnimationPlaybackSpeed()
    {
        if (animator == null || rb == null)
            return;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        bool isMoving = horizontalVelocity.magnitude > 0.1f;

        animator.speed = isMoving
            ? originalAnimatorSpeed * movementPlaybackSpeed
            : originalAnimatorSpeed;
    }

    private void OnDisable()
    {
        if (animator != null)
            animator.speed = originalAnimatorSpeed;
    }
}
