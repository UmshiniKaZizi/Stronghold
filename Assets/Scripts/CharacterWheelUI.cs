using UnityEngine;
using UnityEngine.UI;

public class CharacterWheelUI : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private Image assaultSlot;
    [SerializeField] private Image artillerySlot;
    [SerializeField] private Image sniperSlot;

    [Header("Selection")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    public void UpdateCharacterSelection(int selectedCharacter)
    {
        // Reset all character slots
        assaultSlot.color = normalColor;
        artillerySlot.color = normalColor;
        sniperSlot.color = normalColor;

        // Highlight the currently selected character
        switch (selectedCharacter)
        {
            case 0:
                assaultSlot.color = selectedColor;
                break;

            case 1:
                artillerySlot.color = selectedColor;
                break;

            case 2:
                sniperSlot.color = selectedColor;
                break;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
