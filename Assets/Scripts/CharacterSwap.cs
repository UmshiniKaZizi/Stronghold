using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterSwap : MonoBehaviour
{
    [Header("Current Character")]
    public Transform character;

    [Header("Characters")]
    [SerializeField] private List<Transform> possibleCharacters;

    [Header("Stationary Characters")]
    [Tooltip("Add the indexes of characters that cannot move.")]
    [SerializeField] private List<int> stationaryCharacters;

    [Header("Current Character Index")]
    [SerializeField] private int whichCharacter = 0;

    [Header("UI")]
    [SerializeField] private CharacterWheelUI characterWheelUI;

    [Header("Input")]
    [Tooltip("Input action used to switch to the previous character.")]
    [SerializeField] private InputActionReference previousCharacterAction;

    [Tooltip("Input action used to switch to the next character.")]
    [SerializeField] private InputActionReference nextCharacterAction;


    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        if (possibleCharacters == null ||
            possibleCharacters.Count == 0)
        {
            Debug.LogWarning(
                "No characters have been assigned."
            );

            return;
        }

        // Start with first living character
        whichCharacter = FindNextAliveCharacter(
            0,
            1
        );

        if (whichCharacter == -1)
        {
            Debug.LogWarning(
                "All characters are dead."
            );

            return;
        }

        Swap();
    }


    // ==========================================
    // ENABLE INPUT
    // ==========================================

    private void OnEnable()
    {
        if (previousCharacterAction != null)
        {
            previousCharacterAction.action.Enable();
            previousCharacterAction.action.performed +=
                OnPreviousCharacter;
        }

        if (nextCharacterAction != null)
        {
            nextCharacterAction.action.Enable();
            nextCharacterAction.action.performed +=
                OnNextCharacter;
        }
    }


    // ==========================================
    // DISABLE INPUT
    // ==========================================

    private void OnDisable()
    {
        if (previousCharacterAction != null)
        {
            previousCharacterAction.action.performed -=
                OnPreviousCharacter;

            previousCharacterAction.action.Disable();
        }

        if (nextCharacterAction != null)
        {
            nextCharacterAction.action.performed -=
                OnNextCharacter;

            nextCharacterAction.action.Disable();
        }
    }


    // ==========================================
    // PREVIOUS CHARACTER
    // ==========================================

    private void OnPreviousCharacter(
        InputAction.CallbackContext context)
    {
        int nextCharacter =
            FindNextAliveCharacter(
                whichCharacter - 1,
                -1
            );

        if (nextCharacter != -1)
        {
            whichCharacter = nextCharacter;

            Swap();
        }
        else
        {
            Debug.Log(
                "No living character available."
            );
        }
    }


    // ==========================================
    // NEXT CHARACTER
    // ==========================================

    private void OnNextCharacter(
        InputAction.CallbackContext context)
    {
        int nextCharacter =
            FindNextAliveCharacter(
                whichCharacter + 1,
                1
            );

        if (nextCharacter != -1)
        {
            whichCharacter = nextCharacter;

            Swap();
        }
        else
        {
            Debug.Log(
                "No living character available."
            );
        }
    }


    // ==========================================
    // FIND NEXT ALIVE CHARACTER
    // ==========================================

    private int FindNextAliveCharacter(
        int startIndex,
        int direction)
    {
        int characterCount =
            possibleCharacters.Count;

        for (int i = 0;
             i < characterCount;
             i++)
        {
            int index =
                (startIndex +
                 direction * i) %
                characterCount;

            // Handle negative indexes
            if (index < 0)
            {
                index += characterCount;
            }

            Transform currentCharacter =
                possibleCharacters[index];

            if (currentCharacter == null)
                continue;

            PlayerHealth health =
                currentCharacter.GetComponent<PlayerHealth>();

            // Character is alive
            if (health == null ||
                !health.IsDead)
            {
                return index;
            }
        }

        return -1;
    }


    // ==========================================
    // SWAP CHARACTER
    // ==========================================

    public void Swap()
    {
        if (possibleCharacters == null ||
            possibleCharacters.Count == 0)
        {
            return;
        }

        // ==========================================
        // SET CURRENT CHARACTER
        // ==========================================

        character =
            possibleCharacters[whichCharacter];


        // ==========================================
        // CHECK IF DEAD
        // ==========================================

        PlayerHealth selectedHealth =
            character.GetComponent<PlayerHealth>();

        if (selectedHealth != null &&
            selectedHealth.IsDead)
        {
            Debug.Log(
                character.name +
                " is DEAD and cannot be selected."
            );

            int nextCharacter =
                FindNextAliveCharacter(
                    whichCharacter + 1,
                    1
                );

            if (nextCharacter == -1)
            {
                Debug.Log(
                    "ALL CHARACTERS ARE DEAD."
                );

                return;
            }

            whichCharacter =
                nextCharacter;

            character =
                possibleCharacters[whichCharacter];
        }


        // ==========================================
        // LOOP THROUGH CHARACTERS
        // ==========================================

        for (int i = 0;
             i < possibleCharacters.Count;
             i++)
        {
            Transform currentCharacter =
                possibleCharacters[i];

            if (currentCharacter == null)
                continue;

            bool isActiveCharacter =
                currentCharacter == character;


            // ==========================================
            // HEALTH
            // ==========================================

            PlayerHealth health =
                currentCharacter.GetComponent<PlayerHealth>();

            bool isDead =
                health != null &&
                health.IsDead;


            // ==========================================
            // MOVEMENT
            // ==========================================

            PlayerMovement movement =
                currentCharacter.GetComponent<PlayerMovement>();

            if (movement != null)
            {
                bool isStationary =
                    stationaryCharacters.Contains(i);

                bool canMove =
                    isActiveCharacter &&
                    !isStationary &&
                    !isDead;

                movement.SetMovementEnabled(
                    canMove
                );
            }


            // ==========================================
            // PLAYER LOOK
            // ==========================================

            PlayerLook look =
                currentCharacter.GetComponent<PlayerLook>();

            if (look != null)
            {
                look.enabled =
                    isActiveCharacter &&
                    !isDead;
            }


            // ==========================================
            // CAMERA
            // ==========================================

            Camera playerCamera =
                currentCharacter.GetComponentInChildren<Camera>();

            if (playerCamera != null)
            {
                playerCamera.enabled =
                    isActiveCharacter &&
                    !isDead;
            }


            // ==========================================
            // SHOOTING
            // ==========================================

            PlayerShooting shooting =
                currentCharacter.GetComponent<PlayerShooting>();

            if (shooting != null)
            {
                shooting.SetControlled(
                    isActiveCharacter &&
                    !isDead
                );
            }
        }


        // ==========================================
        // CHARACTER UI
        // ==========================================

        if (characterWheelUI != null)
        {
            characterWheelUI.UpdateCharacterSelection(
                whichCharacter
            );
        }


        // ==========================================
        // DEBUG
        // ==========================================

        Debug.Log(
            "Active Character: " +
            character.name
        );
    }
}