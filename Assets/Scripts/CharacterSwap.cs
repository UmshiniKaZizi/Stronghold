using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSwap : MonoBehaviour
{
    public Transform character;
    public List<Transform> possibleCharacters;
    public int whichCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (character == null && possibleCharacters.Count >= 1)
        {
            character = possibleCharacters[0];
        }
        Swap();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (whichCharacter == 0)
            {
                whichCharacter = possibleCharacters.Count - 1;
            }
            else
            {
                whichCharacter -= 1;
            }
            Swap();
        }
        

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (whichCharacter == possibleCharacters.Count -1)
            {
                whichCharacter = 0;
            }
            else
            {
                whichCharacter += 1;
            }
        }
        Swap();
    }

    public void Swap()
{
    character = possibleCharacters[whichCharacter];

    for (int i = 0; i < possibleCharacters.Count; i++)
    {
        bool isActiveCharacter = possibleCharacters[i] == character;

        // Enable/disable movement
        possibleCharacters[i].GetComponent<PlayerMovement>().enabled = isActiveCharacter;

        // Enable/disable camera
        Camera playerCamera = possibleCharacters[i].GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerCamera.enabled = isActiveCharacter;
        }
    }
}
}
