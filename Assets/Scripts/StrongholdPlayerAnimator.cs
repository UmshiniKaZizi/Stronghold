using UnityEngine;

public class StrongholdPlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Animation Settings")]
    [SerializeField] private float movementDampTime = 0.1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (playerMovement == null)
            return;

        Rigidbody rb = playerMovement.GetComponent<Rigidbody>();

        if (rb == null)
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
}