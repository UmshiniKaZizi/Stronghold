using UnityEngine;
using TMPro;

public class ArtilleryBombUI : MonoBehaviour
{
    [Header("Artillery References")]
    [SerializeField] private PlayerShooting artilleryShooting;
    [SerializeField] private ArtilleryBombInventory bombInventory;

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text bombCountText;
    [SerializeField] private TMP_Text killProgressText;
    [SerializeField] private TMP_Text switchPromptText;

    private void Start()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Make sure references exist.
        if (artilleryShooting == null ||
            bombInventory == null ||
            hudPanel == null)
        {
            return;
        }

        // Hide during onboarding or Game Over.
        if (GameManager.Instance == null ||
            !GameManager.Instance.HasGameStarted ||
            GameManager.Instance.IsGameOver)
        {
            hudPanel.SetActive(false);
            return;
        }

        // Only show when the Artillery Gunman
        // is the character currently controlled.
        bool artilleryIsActive =
            artilleryShooting.IsControlled();

        hudPanel.SetActive(artilleryIsActive);

        if (!artilleryIsActive)
            return;

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        // ==========================================
        // BOMB COUNT
        // ==========================================

        if (bombCountText != null)
        {
            bombCountText.text =
                "BOMBS: " +
                bombInventory.CurrentBombs +
                " / " +
                bombInventory.MaxBombs;
        }

        // ==========================================
        // KILL PROGRESS
        // ==========================================

        if (killProgressText != null)
        {
            if (bombInventory.IsFull)
            {
                killProgressText.text =
                    "BOMB INVENTORY FULL";
            }
            else
            {
                killProgressText.text =
                    "KILLS: " +
                    bombInventory.KillProgress +
                    " / " +
                    bombInventory.KillsRequired;
            }
        }

        // ==========================================
        // F PROMPT
        // ==========================================

        if (switchPromptText == null)
            return;

        // Bomb mode is currently active.
        if (artilleryShooting.IsInBombMode())
        {
            switchPromptText.text =
                "[F] RETURN TO NORMAL AMMO";

            return;
        }

        // At least one bomb is ready.
        if (bombInventory.HasBomb)
        {
            switchPromptText.text =
                "[F] SWITCH TO BOMBS";

            return;
        }

        // No bombs available.
        int killsRemaining =
            bombInventory.KillsRequired -
            bombInventory.KillProgress;

        killsRemaining =
            Mathf.Max(0, killsRemaining);

        switchPromptText.text =
            "EARN " +
            killsRemaining +
            " MORE KILLS FOR A BOMB";
    }
}