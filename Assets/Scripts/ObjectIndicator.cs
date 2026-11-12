using UnityEngine;

public class CriticalObjectIndicator : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;

    [Header("Floating")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmount = 0.1f;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        float pulse =
            1f +
            Mathf.Sin(Time.time * pulseSpeed) *
            pulseAmount;

        transform.localScale =
            originalScale * pulse;

        float floatOffset =
            Mathf.Sin(Time.time * floatSpeed) *
            floatAmount;

        transform.localPosition =
            originalPosition +
            Vector3.up * floatOffset;
    }
}