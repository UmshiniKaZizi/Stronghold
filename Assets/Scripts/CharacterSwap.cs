using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Switches between characters using a circular doubly linked list.
/// Character GameObjects are not activated/deactivated by this class.
/// </summary>
public class CharacterSwap : MonoBehaviour
{
    private sealed class CharacterNode
    {
        public Transform Character;
        public int OriginalIndex;
        public CharacterNode Previous;
        public CharacterNode Next;
        public bool IsRemoved;

        public CharacterNode(Transform character, int originalIndex)
        {
            Character = character;
            OriginalIndex = originalIndex;
        }
    }

    [Header("Character Setup")]
    [SerializeField] private Transform character;
    [SerializeField] private List<Transform> possibleCharacters = new List<Transform>();

    [Header("Stationary Characters")]
    [Tooltip("Indices in Possible Characters that cannot move. These remain the original Inspector indices.")]
    [SerializeField] private List<int> stationaryCharacters = new List<int>();

    [Header("Current Character")]
    [Tooltip("Starting index in Possible Characters.")]
    [SerializeField] private int whichCharacter;

    [Header("Character Wheel UI")]
    [SerializeField] private CharacterWheelUI characterWheelUI;

    private readonly Dictionary<Transform, CharacterNode> nodeLookup =
        new Dictionary<Transform, CharacterNode>();

    private CharacterNode firstNode;
    private CharacterNode currentNode;
    private int livingNodeCount;
    private bool isInitialized;

    public int CurrentCharacterIndex => currentNode != null ? currentNode.OriginalIndex : -1;
    public Transform CurrentCharacter => currentNode != null ? currentNode.Character : null;

    private void Awake()
    {
        if (possibleCharacters == null)
            possibleCharacters = new List<Transform>();

        Debug.Log(
            $"CharacterSwap Awake | GameObject: {gameObject.name} | " +
            $"Active: {gameObject.activeInHierarchy} | Component Enabled: {enabled}");
    }

    private void OnEnable()
    {
        Debug.Log("CharacterSwap OnEnable | CharacterSwitchInputRelay handles switch input.");
    }

    // Initialization is intentionally controlled by GameManager.StartGame().
    // Do not build the list in Start(), because player health must be initialized first.


    private void OnDisable()
    {
        Debug.LogError(
            "CHARACTER SWAP DISABLED!" +
            " | Active Self: " + gameObject.activeSelf +
            " | Active In Hierarchy: " + gameObject.activeInHierarchy +
            " | Component Enabled: " + enabled +
            " | Parent: " + (transform.parent != null ? transform.parent.name : "No Parent") +
            " | Parent Active: " + (transform.parent != null && transform.parent.gameObject.activeInHierarchy) +
            " | Game Over: " + (GameManager.Instance != null && GameManager.Instance.IsGameOver));

    }

    private void OnDestroy()
    {
        // Character-switch input is handled by CharacterSwitchInputRelay.
    }

    public bool InitializeCharacters()
    {
        Debug.Log("CharacterSwap: Initializing circular linked list after health initialization.");

        BuildCircularList();

        if (livingNodeCount == 0)
        {
            isInitialized = false;
            Debug.LogError("CharacterSwap: Initialization failed; no living characters were found.");
            return false;
        }

        CharacterNode startingNode = FindNodeByOriginalIndex(whichCharacter);
        if (startingNode == null)
            startingNode = firstNode;

        currentNode = startingNode;
        whichCharacter = currentNode.OriginalIndex;
        isInitialized = true;

        ApplyCurrentCharacter();

        Debug.Log(
            $"CharacterSwap initialized | Active: {character.name} | " +
            $"Original index: {whichCharacter} | Living nodes: {livingNodeCount}");
        return true;
    }

    private void BuildCircularList()
    {
        nodeLookup.Clear();
        firstNode = null;
        currentNode = null;
        livingNodeCount = 0;
        isInitialized = false;

        if (possibleCharacters == null)
            return;

        CharacterNode lastNode = null;

        for (int i = 0; i < possibleCharacters.Count; i++)
        {
            Transform candidate = possibleCharacters[i];

            if (candidate == null)
            {
                Debug.LogWarning($"CharacterSwap: Missing character reference at Inspector index {i}.");
                continue;
            }

            PlayerHealth health = candidate.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead)
            {
                Debug.LogWarning($"CharacterSwap: {candidate.name} is already dead; not adding it to the linked list.");
                continue;
            }

            if (nodeLookup.ContainsKey(candidate))
            {
                Debug.LogWarning($"CharacterSwap: Duplicate character reference '{candidate.name}' at index {i}; ignoring duplicate.");
                continue;
            }

            CharacterNode node = new CharacterNode(candidate, i);
            nodeLookup.Add(candidate, node);

            if (firstNode == null)
            {
                firstNode = node;
                lastNode = node;
            }
            else
            {
                node.Previous = lastNode;
                lastNode.Next = node;
                lastNode = node;
            }

            livingNodeCount++;
        }

        if (firstNode != null)
        {
            firstNode.Previous = lastNode;
            lastNode.Next = firstNode;
        }

        Debug.Log($"CharacterSwap: Circular linked list built with {livingNodeCount} living character(s).");
    }

    // Called by CharacterSwitchInputRelay through PlayerInput / Send Messages.
    public void RequestPreviousCharacter()
    {
        LogInputReceived("PREVIOUS");
        if (!isInitialized)
        {
            Debug.LogWarning("CharacterSwap: Previous input ignored; linked list is not initialized.");
            return;
        }

        TrySwitchCharacter(-1);
    }

    // Called by CharacterSwitchInputRelay through PlayerInput / Send Messages.
    public void RequestNextCharacter()
    {
        LogInputReceived("NEXT");
        if (!isInitialized)
        {
            Debug.LogWarning("CharacterSwap: Next input ignored; linked list is not initialized.");
            return;
        }

        TrySwitchCharacter(1);
    }

    private void LogInputReceived(string inputName)
    {
        Debug.Log(
            $"[INPUT DEBUG] {inputName} RECEIVED | Time: {Time.unscaledTime} | " +
            $"Object Active: {gameObject.activeInHierarchy} | Component Enabled: {enabled} | " +
            $"Initialized: {isInitialized} | Current: " +
            (CurrentCharacter != null ? CurrentCharacter.name : "NULL") + " | Game Over: " +
            (GameManager.Instance != null && GameManager.Instance.IsGameOver));
    }

    public void RefreshSwitchingInput()
    {
        Debug.Log("CharacterSwap: CharacterSwitchInputRelay handles switch input.");
    }

    public void TrySwitchCharacter(int direction)
    {
        if (GameManager.Instance != null && (!GameManager.Instance.HasGameStarted || GameManager.Instance.IsGameOver))
            return;

        if (!isInitialized || currentNode == null || livingNodeCount <= 1)
        {
            Debug.LogWarning("CharacterSwap: No other living character is available to switch to.");
            return;
        }

        if (direction == 0)
        {
            Debug.LogWarning("CharacterSwap: Direction must be -1 or 1.");
            return;
        }

        int stepDirection = direction < 0 ? -1 : 1;
        CharacterNode candidate = currentNode;

        for (int steps = 0; steps < livingNodeCount; steps++)
        {
            candidate = stepDirection > 0 ? candidate.Next : candidate.Previous;

            if (candidate == null || candidate.IsRemoved || candidate.Character == null)
                continue;

            PlayerHealth health = candidate.Character.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead)
            {
                RemoveCharacterNode(candidate, false);
                if (livingNodeCount <= 1)
                    break;

                candidate = currentNode;
                continue;
            }

            if (candidate == currentNode)
                break;

            currentNode = candidate;
            whichCharacter = currentNode.OriginalIndex;
            ApplyCurrentCharacter();

            Debug.Log(
                $"CharacterSwap: Switched to {character.name} | Original index: {whichCharacter} | " +
                $"Living nodes: {livingNodeCount}");
            return;
        }

        Debug.LogWarning("CharacterSwap: No other living character could be selected.");
    }

    private CharacterNode FindNodeByOriginalIndex(int index)
    {
        if (firstNode == null)
            return null;

        CharacterNode node = firstNode;
        for (int i = 0; i < livingNodeCount; i++)
        {
            if (node != null && !node.IsRemoved && node.OriginalIndex == index)
                return node;

            node = node != null ? node.Next : null;
        }

        return null;
    }

    public void OnCharacterDied(PlayerHealth deadCharacter)
    {
        if (deadCharacter == null)
        {
            Debug.LogWarning("CharacterSwap: Received a death notification with no PlayerHealth.");
            return;
        }

        Transform deadTransform = deadCharacter.transform;
        if (!nodeLookup.TryGetValue(deadTransform, out CharacterNode deadNode) || deadNode.IsRemoved)
        {
            Debug.LogWarning($"CharacterSwap: Dead character '{deadCharacter.name}' is not in the active linked list.");
            return;
        }

        bool wasCurrentCharacter = deadNode == currentNode;
        Debug.Log(
            $"CharacterSwap: Death reported for {deadCharacter.name} | Was current: {wasCurrentCharacter} | " +
            $"Original index: {deadNode.OriginalIndex} | Living nodes before removal: {livingNodeCount}");

        CharacterNode nextCandidate = deadNode.Next;
        if (nextCandidate == deadNode)
            nextCandidate = null;

        RemoveCharacterNode(deadNode, false);

        if (!wasCurrentCharacter)
        {
            Debug.Log($"CharacterSwap: Removed inactive character '{deadCharacter.name}' from the linked list.");
            return;
        }

        if (livingNodeCount <= 0 || nextCandidate == null)
        {
            currentNode = null;
            character = null;
            whichCharacter = -1;
            isInitialized = false;
            Debug.LogWarning("CharacterSwap: No surviving characters remain.");
            return;
        }

        // Find the next node that is still alive after removing the dead node.
        CharacterNode candidate = nextCandidate;
        int checks = livingNodeCount;

        for (int i = 0; i < checks; i++)
        {
            if (candidate != null && !candidate.IsRemoved && candidate.Character != null)
            {
                PlayerHealth health = candidate.Character.GetComponent<PlayerHealth>();
                if (health == null || !health.IsDead)
                {
                    currentNode = candidate;
                    whichCharacter = currentNode.OriginalIndex;
                    isInitialized = true;
                    ApplyCurrentCharacter();

                    Debug.Log($"CharacterSwap: Automatically switched to {character.name} after death.");
                    return;
                }
            }

            candidate = candidate != null ? candidate.Next : null;
        }

        currentNode = null;
        character = null;
        whichCharacter = -1;
        isInitialized = false;
        Debug.LogWarning("CharacterSwap: No surviving characters remain.");
    }

    /// <summary>
    /// Removes a character from the circular list. This does not destroy or deactivate its GameObject.
    /// </summary>
    public void RemoveCharacter(Transform characterToRemove)
    {
        if (characterToRemove == null)
            return;

        if (!nodeLookup.TryGetValue(characterToRemove, out CharacterNode node) || node.IsRemoved)
        {
            Debug.LogWarning($"CharacterSwap: Cannot remove '{characterToRemove.name}'; it is not in the linked list.");
            return;
        }

        OnCharacterDied(characterToRemove.GetComponent<PlayerHealth>());
    }

    private void RemoveCharacterNode(CharacterNode node, bool updateCurrent)
    {
        if (node == null || node.IsRemoved)
            return;

        bool wasCurrent = node == currentNode;

        if (livingNodeCount == 1)
        {
            firstNode = null;
            if (wasCurrent)
                currentNode = null;
        }
        else
        {
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;

            if (firstNode == node)
                firstNode = node.Next;

            if (wasCurrent && updateCurrent)
                currentNode = node.Next;
        }

        node.IsRemoved = true;
        nodeLookup.Remove(node.Character);
        livingNodeCount--;

        if (livingNodeCount > 0 && firstNode != null)
        {
            firstNode.Previous = FindLastNode(firstNode);
            firstNode.Previous.Next = firstNode;
        }

        Debug.Log(
            $"CharacterSwap: Removed node for '{(node.Character != null ? node.Character.name : "destroyed character")}'. " +
            $"Remaining nodes: {livingNodeCount}");
    }

    private CharacterNode FindLastNode(CharacterNode start)
    {
        if (start == null)
            return null;

        CharacterNode node = start;
        int safety = 0;

        while (node.Next != null && node.Next != start && safety++ <= livingNodeCount)
            node = node.Next;

        return node;
    }

    private void ApplyCurrentCharacter()
    {
        if (currentNode == null || currentNode.Character == null)
        {
            character = null;
            Debug.LogError("CharacterSwap: Cannot apply selection; current linked-list node is missing.");
            return;
        }

        character = currentNode.Character;
        whichCharacter = currentNode.OriginalIndex;

        // Iterate the original Inspector list so every character's control state is updated,
        // including characters that have been removed from the selectable linked list.
        for (int i = 0; i < possibleCharacters.Count; i++)
        {
            Transform candidate = possibleCharacters[i];
            if (candidate == null)
                continue;

            bool isControlled = candidate == character;
            PlayerHealth health = candidate.GetComponent<PlayerHealth>();
            bool isDead = health != null && health.IsDead;

            PlayerMovement movement = candidate.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                bool canMove = isControlled && !isDead && !stationaryCharacters.Contains(i);
                movement.SetMovementEnabled(canMove);
            }

            PlayerLook look = candidate.GetComponent<PlayerLook>();
            if (look != null)
                look.enabled = isControlled && !isDead;

            Camera[] cameras = candidate.GetComponentsInChildren<Camera>(true);
            foreach (Camera cam in cameras)
                cam.enabled = isControlled && !isDead;

            AudioListener[] listeners = candidate.GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener listener in listeners)
                listener.enabled = isControlled && !isDead;

            SniperScopeController scope =
                candidate.GetComponentInChildren<SniperScopeController>(true);
            if (scope != null)
            {
                scope.enabled = true;
                scope.SetControlled(isControlled && !isDead);
            }

            PlayerShooting shooting =
                candidate.GetComponentInChildren<PlayerShooting>(true);
            if (shooting != null)
                shooting.SetControlled(isControlled && !isDead);
        }

        if (characterWheelUI != null)
        {
            characterWheelUI.UpdateCharacterSelection(whichCharacter);
        }

        Debug.Log($"CharacterSwap: Active character is {character.name} | Original index: {whichCharacter}");
    }

    public void RequestSwitchFrom(Transform sourceCharacter, int direction)
    {
        if (sourceCharacter == null || currentNode == null)
            return;

        if (currentNode.Character != sourceCharacter)
        {
            Debug.Log($"CharacterSwap: Ignoring switch request from non-controlled character '{sourceCharacter.name}'.");
            return;
        }

        TrySwitchCharacter(direction);
    }
}
