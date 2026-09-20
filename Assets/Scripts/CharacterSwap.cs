using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSwap : MonoBehaviour
{
    public Transform character;

    [Header("Characters")]
    [SerializeField] private List<Transform> possibleCharacters;

    [Header("Stationary Characters")]
    [Tooltip("Add the indexes of characters that cannot move.")]
    [SerializeField] private List<int> stationaryCharacters;

    [Header("Current Character")]
    [SerializeField] private int whichCharacter = 0;

    [Header("UI")]
    [SerializeField] private CharacterWheelUI characterWheelUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (possibleCharacters == null || possibleCharacters.Count == 0)
        {
            Debug.LogWarning("No characters have been assigned.");
            return;
        }

        // Start with the first character.
        whichCharacter = 0;

        //if (character == null && possibleCharacters.Count >= 1)
        //{
        //    character = possibleCharacters[0];
        //}
        Swap();
    }

    // Update is called once per frame
    void Update()
    {
        //Previous character
        if(Input.GetKeyDown(KeyCode.Q))
        {
            whichCharacter--;

            if (whichCharacter < 0)
            {
                whichCharacter = possibleCharacters.Count - 1;
            }

            Swap();
           // if (whichCharacter == 0)
           // {
           //     whichCharacter = possibleCharacters.Count - 1;
           // }
           // else
           // {
                //whichCharacter -= 1;
            //    whichCharacter --;
            //}
            //Swap();
        }
        
        //Next Character
        if (Input.GetKeyDown(KeyCode.E))
        {
            whichCharacter++;

            if (whichCharacter >= possibleCharacters.Count)
            {
                whichCharacter = 0;
            }

            Swap();

           // if (whichCharacter == possibleCharacters.Count -1)
           // {
           //     whichCharacter = 0;
         //   }
          //  else
          //  {
                //whichCharacter += 1;
            //    whichCharacter++;
           // }
            //Swap();
        }
        
    }

    public void Swap()
    {
      character = possibleCharacters[whichCharacter];

      for (int i = 0; i < possibleCharacters.Count; i++)
      {
           Transform currentCharacter = possibleCharacters[i];
           bool isActiveCharacter =
                currentCharacter == character;
           // stationaryCharacters.Contains(i);
           // possibleCharacters[i] == character;
           
           PlayerMovement movement =
           currentCharacter.GetComponent<PlayerMovement>();

            if (movement != null)
            {
                bool isStationary =
                    stationaryCharacters.Contains(i);

                bool canMove =
                   isActiveCharacter && !isStationary;

                // Character can only move if:
                // 1. They are currently selected
                // 2. They are NOT stationary
                movement.SetMovementEnabled(canMove);

              //  movement.enabled = isActiveCharacter && !isStationary;
            }

            // ==========================================
            // LOOK / CAMERA CONTROL
            // ==========================================

            PlayerLook look =
                currentCharacter.GetComponent<PlayerLook>();

            if (look != null)
            {
                look.enabled = isActiveCharacter;
            }



            // Enable/disable movement
          //  possibleCharacters[i].GetComponent<PlayerMovement>().enabled = isActiveCharacter;

        // Enable/disable camera
        Camera playerCamera = 
                //possibleCharacters[i].GetComponentInChildren<Camera>();
            currentCharacter.GetComponentInChildren<Camera>();

            if (playerCamera != null)
            {
               playerCamera.enabled = isActiveCharacter;
            }

            if (characterWheelUI != null)
            {
                characterWheelUI.UpdateCharacterSelection(whichCharacter);
            }
        }
    }
}
