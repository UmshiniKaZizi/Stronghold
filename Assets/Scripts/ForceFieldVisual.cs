using UnityEngine;

public class ForceFieldVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Stronghold stronghold;
    [SerializeField] private Renderer fieldRenderer;

    [Header("Visual Settings")]
    [SerializeField] private float maxAlpha = 0.25f;
    [SerializeField] private float minAlpha = 0.02f;

    private Material fieldMaterial;

    private void Awake()
    {
        if (stronghold == null)
            stronghold = GetComponentInParent<Stronghold>();

        if (fieldRenderer == null)
            fieldRenderer = GetComponent<Renderer>();

        fieldMaterial = fieldRenderer.material;
    }

    private void Update()
    {
        if (stronghold == null)
            return;

        UpdateFieldVisual();
    }

    private void UpdateFieldVisual()
    {
        float strengthPercent =
            stronghold.currentForceField / stronghold.maxForceField;

        float alpha = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            strengthPercent
        );

        Color color = fieldMaterial.color;
        color.a = alpha;
        fieldMaterial.color = color;

        if (strengthPercent <= 0f)
        {
            fieldRenderer.enabled = false;
        }
        else
        {
            fieldRenderer.enabled = true;
        }
    }
}