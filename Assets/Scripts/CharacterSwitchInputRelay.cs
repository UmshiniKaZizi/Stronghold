
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitchInputRelay : MonoBehaviour
{
    [SerializeField] private CharacterSwap characterSwap;

    private void Awake()
    {
        if (characterSwap == null)
            characterSwap = FindFirstObjectByType<CharacterSwap>();

        if (characterSwap == null)
        {
            Debug.LogError(
                "CharacterSwitchInputRelay: No CharacterSwap found.", this);
        }
    }

    public void OnPreviousCharacter(InputValue value)
    {
        if (value.Get<float>() < 0.5f)
            return;

        if (characterSwap == null)
            return;

        Debug.Log("INPUT RELAY: PreviousCharacter received.");
        characterSwap.RequestPreviousCharacter();
    }

    public void OnNextCharacter(InputValue value)
    {
        if (value.Get<float>() < 0.5f)
            return;

        if (characterSwap == null)
            return;

        Debug.Log("INPUT RELAY: NextCharacter received.");
        characterSwap.RequestNextCharacter();
    }
}
