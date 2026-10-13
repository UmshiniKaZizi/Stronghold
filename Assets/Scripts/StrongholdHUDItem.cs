using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StrongholdHUDItem : MonoBehaviour
{
    [Header("Stronghold Label")]
    [SerializeField] private TextMeshProUGUI strongholdLabel;

    [Header("Status Text")]
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Audio Feedback")]
    [SerializeField] private StrongholdAudio hudAudio;

    [Header("Visual Indicators")]
    [SerializeField] private Image statusAccent;
    [SerializeField] private Image backgroundImage;

    [Header("Colours")]
    [SerializeField]
    private Color secureColor =
        new Color(0.13f, 0.85f, 0.42f);

    [SerializeField]
    private Color threatenedColor =
        new Color(0.98f, 0.78f, 0.12f);

    [SerializeField]
    private Color criticalColor =
        new Color(1f, 0.35f, 0.08f);

    [SerializeField]
    private Color destroyedColor =
        new Color(0.95f, 0.18f, 0.22f);

    // Set the letter shown in this box.
    public void SetLabel(string label)
    {
        if (strongholdLabel != null)
            strongholdLabel.text = label;
    }

    // Update the box to match the Stronghold's current state.
    public void SetStatus(StrongholdStatus status)
    {
        Color stateColor;
        string stateText;
       

        switch (status)
        {
            case StrongholdStatus.Secure:
                stateColor = secureColor;
                stateText = "SECURE";
                break;

            case StrongholdStatus.Threatened:
                stateColor = threatenedColor;
                stateText = "THREATENED";
                break;

            case StrongholdStatus.Critical:
                stateColor = criticalColor;
                stateText = "CRITICAL";
                break;

            case StrongholdStatus.Destroyed:
                stateColor = destroyedColor;
                stateText = "DESTROYED";
                break;

            default:
                stateColor = Color.white;
                stateText = "UNKNOWN";
                break;
        }

        if (statusLabel != null)
        {
            statusLabel.text = stateText;
            statusLabel.color = stateColor;
            strongholdLabel.color = stateColor;
        }

        if (statusAccent != null)
            statusAccent.color = stateColor;

        if (backgroundImage != null)
        {
            // A subtle tint reinforces the state without
            // making the entire box difficult to read.
            backgroundImage.color = new Color(
                stateColor.r,
                stateColor.g,
                stateColor.b,
                0.10f
            );
        }

        if (hudAudio != null)
        {
           hudAudio.UpdateAlert(status);
        }
    }
}
