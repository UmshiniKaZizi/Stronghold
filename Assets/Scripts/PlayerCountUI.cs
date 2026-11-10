using UnityEngine;
using TMPro;

public class PlayerCountUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth[] players;
    [SerializeField] private TMP_Text aliveCountText;

    private void Start()
    {
        if (aliveCountText != null)
        {
            aliveCountText.enabled = false;
        }
    }

    private void Update()
    {
        if (aliveCountText == null)
            return;

        // Keep the UI hidden during onboarding.
        if (GameManager.Instance == null ||
            !GameManager.Instance.HasGameStarted)
        {
            aliveCountText.enabled = false;
            return;
        }

        // Game has started, so show the UI.
        aliveCountText.enabled = true;

        int aliveCount = 0;

        foreach (PlayerHealth player in players)
        {
            if (player != null &&
                !player.IsDead)
            {
                aliveCount++;
            }
        }

        aliveCountText.text =
            "PLAYERS ALIVE: " +
            aliveCount +
            " / " +
            players.Length;
    }
}