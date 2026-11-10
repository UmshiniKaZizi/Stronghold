using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerMarker
    {
        public string characterName;
        public Transform character;
        public RectTransform marker;

        //public bool isActive;
    }

    [Header("Character Switching")]
    [SerializeField] private CharacterSwap characterSwap;

    [Header("Minimap References")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform markerLayer;

    [Header("Player Markers")]
    [SerializeField] private PlayerMarker[] playerMarkers;


    [Header("Character Signature Colours")]

    [SerializeField]
    private Color assaultColour =
    new Color(0.15f, 0.90f, 0.42f);

    [SerializeField]
    private Color artilleryColour =
        new Color(0.66f, 0.33f, 0.97f);

    [SerializeField]
    private Color sniperColour =
        Color.white;


    [Header("Inactive Marker Colours")]
    [SerializeField] private float inactiveBrightness = 0.55f;

    

    [Header("Raycasting")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastHeight = 15f;
    [SerializeField] private float raycastDistance = 40f;

    [Header("Enemy Markers")]
    public RectTransform enemyMarkerPrefab;

    //public RectTransform mapRect;
    public Transform detectionOrigin;

    //[SerializeField] private float enemyDetectionRange = 35f;

    //[SerializeField] private LayerMask obstacleLayer;


    private Dictionary<EnemyMinimapTarget, RectTransform>
        enemyMarkers = new Dictionary<EnemyMinimapTarget, RectTransform>();

    //RectTransform newMarker =
    //Instantiate(enemyMarkerPrefab, markerLayer);

    void LateUpdate()
    {
        UpdatePlayerMarkers();
        UpdateEnemyMarkers();
    }

    void UpdatePlayerMarkers()
    {
        if (minimapCamera == null || markerLayer == null)
            return;

        // Get the currently controlled character
        // directly from your existing CharacterSwap script.

        Transform activeCharacter = characterSwap != null
            ? characterSwap.CurrentCharacter
            : null;

        if (playerMarkers == null)
            return;

        foreach (PlayerMarker player in playerMarkers)
        {
            if (player.character == null || player.marker == null)
                continue;


            // Hide characters that have died.
            PlayerHealth health =
                player.character.GetComponent<PlayerHealth>();

            if (health != null && health.IsDead)
            {
                player.marker.gameObject.SetActive(false);
                continue;
            }

            // Get the character's ground position.
            Vector3 groundPosition =
                GetGroundPosition(player.character.position);

            // // Find the character's position on the ground.
            // Vector3 worldPosition = GetGroundPosition(
            //    player.character.position
            // );

            // Convert world position to minimap coordinates.
            Vector3 viewportPosition =
                minimapCamera.WorldToViewportPoint(groundPosition);

            // Convert world position into minimap coordinates.
            //Vector3 viewportPosition =
              //  minimapCamera.WorldToViewportPoint(worldPosition);

            // Hide markers outside the minimap camera's view.
            bool visible =
                viewportPosition.z > 0 &&
                viewportPosition.x >= 0 &&
                viewportPosition.x <= 1 &&
                viewportPosition.y >= 0 &&
                viewportPosition.y <= 1;

            player.marker.gameObject.SetActive(visible);

            if (!visible)
                continue;

            // Convert viewport coordinates into UI coordinates.
            float x = (viewportPosition.x - 0.5f)
                      * markerLayer.rect.width;

            float y = (viewportPosition.y - 0.5f)
                      * markerLayer.rect.height;

            player.marker.anchoredPosition = new Vector2(x, y);

            // Identify the currently controlled character.
            bool isActive =
                player.character == activeCharacter;

            // Highlight the currently controlled character.
            // Determine the character's signature colour.
            Color signatureColour = Color.gray;

            switch (player.characterName.ToLower())
            {
                case "assault":
                case "assault soldier":
                    signatureColour = assaultColour;
                    break;

                case "artillery":
                case "artillery gunner":
                    signatureColour = artilleryColour;
                    break;

                case "sniper":
                    signatureColour = sniperColour;
                    break;
            }

            // Active characters use their full signature colour.
            // Inactive characters use a darker version.
            Image markerImage =
                player.marker.GetComponent<Image>();

            if (markerImage != null)
            {
                markerImage.color = isActive
                    ? signatureColour
                    : new Color(
                        signatureColour.r * inactiveBrightness,
                        signatureColour.g * inactiveBrightness,
                        signatureColour.b * inactiveBrightness,
                        signatureColour.a
                    );
            }

         

            // Rotate the active character's directional arrow.
            if (isActive)
            {
                Vector3 forwardPosition =
                    minimapCamera.WorldToViewportPoint(
                        groundPosition + player.character.forward
                    );

                Vector2 direction = new Vector2(
                   forwardPosition.x - viewportPosition.x,
                   forwardPosition.y - viewportPosition.y
               );

                if (direction.sqrMagnitude > 0.0001f)
                {
                    float angle =
                        Mathf.Atan2(direction.y, direction.x)
                        * Mathf.Rad2Deg - 90f;

                    player.marker.localRotation =
                        Quaternion.Euler(0, 0, angle);
                }
            }
            else
            {
                player.marker.localRotation = Quaternion.identity;
            }
        }
    }

    private bool IsCharacterThreatened(
    EnemyMinimapTarget enemy)
    {
        foreach (PlayerMarker player in playerMarkers)
        {
            if (player.character == null)
                continue;

            // Ignore dead characters.
            PlayerHealth health =
                player.character.GetComponent<PlayerHealth>();

            if (health != null && health.IsDead)
                continue;

            // Check distance between enemy and character.
            float distance = Vector3.Distance(
                enemy.transform.position,
                player.character.position
            );

            // Enemy is close enough to threaten this character.
            if (distance <= 12f)
                return true;
        }

        return false;
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        Vector3 origin =
            position + Vector3.up * raycastHeight;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return position;
    }

    private void UpdateEnemyMarkers()
    {
        if (minimapCamera == null ||
            markerLayer == null ||
            enemyMarkerPrefab == null)
            return;

        // Remove markers belonging to destroyed enemies.
        List<EnemyMinimapTarget> removedEnemies =
            new List<EnemyMinimapTarget>();

        foreach (var entry in enemyMarkers)
        {
            if (entry.Key == null)
            {

                if (entry.Value != null)
                    Destroy(entry.Value.gameObject);

                removedEnemies.Add(entry.Key);
            }
        }

        foreach (var enemy in removedEnemies)
        {
            enemyMarkers.Remove(enemy);
        }

        // Take a snapshot so registration changes cannot
        // modify the collection while it is being iterated.
        List<EnemyMinimapTarget> activeEnemies =
            new List<EnemyMinimapTarget>(
                EnemyMinimapTarget.ActiveEnemies
            );

        // Update every currently active enemy.
        foreach (EnemyMinimapTarget enemy
            in activeEnemies)
        {
            if (enemy == null)
                continue;

            if (!enemyMarkers.ContainsKey(enemy))
            {
                RectTransform newMarker =
                    Instantiate(enemyMarkerPrefab, markerLayer);

                enemyMarkers.Add(enemy, newMarker);
            }

            RectTransform marker = enemyMarkers[enemy];

            // Check whether any playable character is threatened.
            bool isThreatened = IsCharacterThreatened(enemy);

            Image enemyImage = marker.GetComponent<Image>();

            if (enemyImage != null)
            {
                enemyImage.color = isThreatened
                    ? new Color(1f, 0.55f, 0f)
                    : Color.red;
            }

            //Find the enemy's ground position
            Vector3 groundPosition =
                GetGroundPosition(enemy.transform.position);

            //Convert world position into minimap coordinates
            Vector3 viewportPosition =
                minimapCamera.WorldToViewportPoint(groundPosition);

            bool onMap =
                viewportPosition.z > 0 &&
                viewportPosition.x >= 0 &&
                viewportPosition.x <= 1 &&
                viewportPosition.y >= 0 &&
                viewportPosition.y <= 1;

            // Check whether the active character can detect this enemy.
           // bool detected = IsEnemyDetected(enemy);

            marker.gameObject.SetActive(onMap);

            if (!onMap)
                continue;

            //Convert viewport coordinates to UI coordinates
            float x = (viewportPosition.x - 0.5f)
                      * markerLayer.rect.width;

            float y = (viewportPosition.y - 0.5f)
                      * markerLayer.rect.height;

            marker.anchoredPosition = new Vector2(x, y);
        }
    }

   // private bool IsEnemyDetected(EnemyMinimapTarget enemy)
    //{
    ///    if (characterSwap == null ||
     //      characterSwap.CurrentCharacter == null)
      //      return false;

        // Automatically use the currently controlled character.
      //  Transform activeCharacter =
       //     characterSwap.CurrentCharacter;

        // Start the detection ray around character eye height.
        //Vector3 origin =
         //   activeCharacter.position + Vector3.up * 1.5f;

       // Vector3 target =
          //  enemy.transform.position + Vector3.up;

        //Vector3 direction = target - origin;

        // Check enemy detection range.
        //if (direction.magnitude > enemyDetectionRange)
      //      return false;


        // Raycast against environmental obstacles.
      //  if (Physics.Raycast(
        //    origin,
        //    direction.normalized,
           // direction.magnitude,
         //   obstacleLayer,
         //   QueryTriggerInteraction.Ignore))
       // {
        //    return false;
        //}

       // return true;
    //}
}






 


   

