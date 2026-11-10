using UnityEngine;

public class PulseTextUI : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.08f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float pulse =
            1f +
            Mathf.Sin(Time.unscaledTime * pulseSpeed) *
            pulseAmount;

        transform.localScale =
            originalScale * pulse;
    }
}