using UnityEngine;

public class StrongholdPerimeter : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private StrongholdState strongholdState;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Perimeter Settings")]
    [SerializeField, Min(3)] private int arrowCount = 12;
    [SerializeField, Min(1f)] private float radius = 12f;
    [SerializeField] private float heightOffset = 0.2f;

    [Header("Arrow Appearance")]
    [SerializeField] private float arrowScale = 1f;

    private GameObject[] arrows;
    private Renderer[][] arrowRenderers;
    private StrongholdStatus previousStatus;

    private void Start()
    {
        if (strongholdState == null)
            strongholdState = GetComponent<StrongholdState>();

        if (strongholdState == null || arrowPrefab == null)
        {
            Debug.LogError(
                name + ": Assign StrongholdState and Arrow Prefab."
            );
            enabled = false;
            return;
        }

        CreatePerimeter();
        previousStatus = strongholdState.CurrentStatus;
        UpdateArrowColors();
    }

    private void Update()
    {
        // Only update materials when the gameplay state changes.
        if (strongholdState.CurrentStatus != previousStatus)
        {
            previousStatus = strongholdState.CurrentStatus;
            UpdateArrowColors();
        }
    }

    private void CreatePerimeter()
    {
        arrows = new GameObject[arrowCount];
        arrowRenderers = new Renderer[arrowCount][];

        for (int i = 0; i < arrowCount; i++)
        {
            // Calculate an evenly spaced position around the ring.
            float angle = i * (360f / arrowCount);

            float radians = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Sin(radians),
                0f,
                Mathf.Cos(radians)
            );

            Vector3 position =
                transform.position +
                direction * radius +
                Vector3.up * heightOffset;

            // Point each arrow toward the Stronghold centre.
            Quaternion rotation =
                Quaternion.LookRotation(-direction);

            GameObject arrow = Instantiate(
                arrowPrefab,
                position,
                rotation,
                transform
            );

            arrow.name = "Perimeter Arrow " + (i + 1);
            arrow.transform.localScale *= arrowScale;

            arrows[i] = arrow;

            // Support meshes with more than one Renderer.
            arrowRenderers[i] =
                arrow.GetComponentsInChildren<Renderer>();
        }
    }

    private void UpdateArrowColors()
    {
        Color statusColor = strongholdState.GetStatusColor();

        foreach (Renderer[] renderers in arrowRenderers)
        {
            foreach (Renderer item in renderers)
            {
                if (item == null)
                    continue;

                // Use a property block so we do not create
                // a separate material instance per arrow.
                MaterialPropertyBlock block =
                    new MaterialPropertyBlock();

                item.GetPropertyBlock(block);

                block.SetColor("_BaseColor", statusColor);
                block.SetColor("_Color", statusColor);
                block.SetColor("_EmissionColor", statusColor * 2f);

                item.SetPropertyBlock(block);
            }
        }
    }
}

