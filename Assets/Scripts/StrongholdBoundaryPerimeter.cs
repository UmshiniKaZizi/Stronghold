using UnityEngine;
using System.Collections.Generic;
public class StrongholdBoundaryPerimeter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StrongholdState strongholdState;
    [SerializeField] private Transform[] boundaryPoints;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Arrow Placement")]
    [SerializeField, Min(0.5f)]
    private float arrowSpacing = 2.5f;

    [SerializeField]
    private float heightOffset = 0.15f;

    [SerializeField]
    private bool pointTowardCenter = true;

    [Header("Ground Placement")]
    [SerializeField]
    private bool snapToGround = true;

    [SerializeField]
    private LayerMask groundLayers = ~0;

    [SerializeField, Min(1f)]
    private float raycastStartHeight = 10f;

    [SerializeField, Min(1f)]
    private float raycastDistance = 30f;

    [Header("Arrow Appearance")]
    [SerializeField, Min(0.1f)]
    private float arrowScale = 1f;

    private readonly List<GameObject> arrows =
        new List<GameObject>();

    private readonly List<Renderer> arrowRenderers =
        new List<Renderer>();

    private Transform generatedArrowsParent;
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

        if (boundaryPoints == null || boundaryPoints.Length < 3)
        {
            Debug.LogError(
                name + ": Assign at least 3 boundary points."
            );
            enabled = false;
            return;
        }

        // Keep generated arrows separate from the boundary points.
        GameObject container = new GameObject("Generated Arrows");
        generatedArrowsParent = container.transform;
        generatedArrowsParent.SetParent(transform, true);

        BuildPerimeter();

        previousStatus = strongholdState.CurrentStatus;
        UpdateArrowColors();
    }

    private void Update()
    {
        // Update colours only when the Stronghold state changes.
        if (strongholdState.CurrentStatus != previousStatus)
        {
            previousStatus = strongholdState.CurrentStatus;
            UpdateArrowColors();
        }
    }

    private void BuildPerimeter()
    {
        float totalLength = 0f;

        // Measure the complete closed perimeter.
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Transform start = boundaryPoints[i];
            Transform end =
                boundaryPoints[(i + 1) % boundaryPoints.Length];

            if (start == null || end == null)
            {
                Debug.LogError(
                    name + ": A boundary point is missing."
                );
                return;
            }

            totalLength += Vector3.Distance(
                start.position, end.position
            );
        }

        if (totalLength <= 0f)
        {
            Debug.LogError(name + ": Perimeter length is zero.");
            return;
        }

        // Average the boundary positions for an approximate centre.
        Vector3 centre = Vector3.zero;

        foreach (Transform point in boundaryPoints)
            centre += point.position;

        centre /= boundaryPoints.Length;

        // Walk along the full perimeter at regular intervals.
        for (float distance = 0f;
             distance < totalLength;
             distance += arrowSpacing)
        {
            Vector3 position;
            Vector3 tangent;

            if (!GetPointAlongPerimeter(
                distance, out position, out tangent))
            {
                continue;
            }

            // Snap the arrow to the terrain, if enabled.
            if (snapToGround)
            {
                Vector3 rayOrigin = position +
                    Vector3.up * raycastStartHeight;

                if (Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    raycastDistance,
                    groundLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    position = hit.point;
                }
            }

            position += Vector3.up * heightOffset;

            // Choose whether arrows follow the edge or point inward.
            Vector3 facingDirection = tangent;

            if (pointTowardCenter)
            {
                Vector3 inward = centre - position;
                inward.y = 0f;

                if (inward.sqrMagnitude > 0.001f)
                    facingDirection = inward.normalized;
            }

            facingDirection.y = 0f;

            if (facingDirection.sqrMagnitude < 0.001f)
                facingDirection = Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(
                facingDirection, Vector3.up
            );

            GameObject arrow = Instantiate(
                arrowPrefab,
                position,
                rotation,
                generatedArrowsParent
            );

            arrow.name = "Boundary Arrow " + arrows.Count;
            arrow.transform.localScale *= arrowScale;

            arrows.Add(arrow);

            // Include renderers on child meshes as well.
            arrowRenderers.AddRange(
                arrow.GetComponentsInChildren<Renderer>()
            );
        }

        Debug.Log(
            name + ": Created " + arrows.Count +
            " perimeter arrows."
        );
    }

    private bool GetPointAlongPerimeter(
        float distance,
        out Vector3 position,
        out Vector3 tangent)
    {
        position = Vector3.zero;
        tangent = Vector3.forward;

        float distanceBeforeSegment = 0f;

        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Transform start = boundaryPoints[i];
            Transform end =
                boundaryPoints[(i + 1) % boundaryPoints.Length];

            float segmentLength = Vector3.Distance(
                start.position, end.position
            );

            if (segmentLength <= 0.001f)
                continue;

            if (distance <= distanceBeforeSegment + segmentLength)
            {
                float distanceOnSegment =
                    distance - distanceBeforeSegment;

                float t = distanceOnSegment / segmentLength;

                position = Vector3.Lerp(
                    start.position, end.position, t
                );

                tangent = (
                    end.position - start.position
                ).normalized;

                return true;
            }

            distanceBeforeSegment += segmentLength;
        }

        return false;
    }

    private void UpdateArrowColors()
    {
        Color statusColor = strongholdState.GetStatusColor();

        foreach (Renderer item in arrowRenderers)
        {
            if (item == null)
                continue;

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            item.GetPropertyBlock(block);

            block.SetColor("_BaseColor", statusColor);
            block.SetColor("_Color", statusColor);
            block.SetColor("_EmissionColor", statusColor * 2f);

            item.SetPropertyBlock(block);
        }
    }

    // Draw the mapped perimeter in the Scene view for easier editing.
    private void OnDrawGizmos()
    {
        if (boundaryPoints == null ||
            boundaryPoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Transform start = boundaryPoints[i];
            Transform end =
                boundaryPoints[(i + 1) % boundaryPoints.Length];

            if (start == null || end == null)
                continue;

            Gizmos.DrawLine(start.position, end.position);
            Gizmos.DrawSphere(start.position, 0.3f);
        }
    }
    
}
