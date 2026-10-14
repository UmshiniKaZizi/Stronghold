
using UnityEngine;

public class ForceFieldVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Stronghold stronghold;
    [SerializeField] private Renderer fieldRenderer;

    [Header("Visual Settings")]
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.25f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.02f;

    private Material fieldMaterial;
    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty =
        Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (stronghold == null)
            stronghold = GetComponentInParent<Stronghold>();

        if (fieldRenderer == null)
            fieldRenderer = GetComponent<Renderer>();

        if (fieldRenderer == null)
        {
            Debug.LogError(
                name + ": No Renderer assigned to ForceFieldVisual.",
                this
            );
            enabled = false;
            return;
        }

        // Create a per-renderer material instance.
        fieldMaterial = fieldRenderer.material;
    }

    private void Update()
    {
        if (stronghold == null || fieldMaterial == null)
            return;

        UpdateFieldVisual();
    }

    private void UpdateFieldVisual()
    {
        float strengthPercent = stronghold.maxForceField > 0f
            ? Mathf.Clamp01(
                stronghold.currentForceField /
                stronghold.maxForceField
            )
            : 0f;

        bool hasStrength =
            strengthPercent > 0f && !stronghold.IsDestroyed;

        fieldRenderer.enabled = hasStrength;

        if (!hasStrength)
            return;

        float alpha = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            strengthPercent
        );

        SetAlpha(BaseColor, alpha);
        SetAlpha(ColorProperty, alpha);
    }

    private void SetAlpha(int property, float alpha)
    {
        if (!fieldMaterial.HasProperty(property))
            return;

        Color color = fieldMaterial.GetColor(property);
        color.a = alpha;
        fieldMaterial.SetColor(property, color);
    }

    private void OnDestroy()
    {
        if (fieldMaterial != null)
            Destroy(fieldMaterial);
    }
}
