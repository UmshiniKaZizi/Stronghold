using UnityEngine;
using TMPro;

public class ArtilleryBombUI : MonoBehaviour
{
    [Header("Artillery References")]
    [SerializeField] private PlayerShooting artilleryShooting;
    [SerializeField] private ArtilleryBombInventory bombInventory;

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;

    [Header("Center Bomb Message")]
    [SerializeField] private GameObject bombAvailableMessage;

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

        if (bombAvailableMessage != null)
        {
            bombAvailableMessage.SetActive(false);
        }
    }

    private void Update()
    {
        if (artilleryShooting == null ||
            bombInventory == null)
        {
            return;
        }

        // Hide everything during onboarding
        // or after Game Over.
        if (GameManager.Instance == null ||
            !GameManager.Instance.HasGameStarted ||
            GameManager.Instance.IsGameOver)
        {
            if (hudPanel != null)
                hudPanel.SetActive(false);

            if (bombAvailableMessage != null)
                bombAvailableMessage.SetActive(false);

            return;
        }

        bool artilleryIsActive =
            artilleryShooting.IsControlled();

        // ==========================================
        // NORMAL ARTILLERY HUD
        // ==========================================

        if (hudPanel != null)
        {
            hudPanel.SetActive(artilleryIsActive);
        }

        // ==========================================
        // CENTER "BOMB AVAILABLE" MESSAGE
        // ==========================================

        bool shouldShowBombMessage =
            artilleryIsActive &&
            bombInventory.HasBomb &&
            !artilleryShooting.IsInBombMode();

        if (bombAvailableMessage != null)
        {
            bombAvailableMessage.SetActive(
                shouldShowBombMessage
            );
        }

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

        if (artilleryShooting.IsInBombMode())
        {
            switchPromptText.text =
                "[F] RETURN TO NORMAL AMMO";

            return;
        }

        if (bombInventory.HasBomb)
        {
            switchPromptText.text =
                "[F] SWITCH TO BOMBS";

            return;
        }

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