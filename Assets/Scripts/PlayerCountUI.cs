using UnityEngine;
using TMPro;

public class PlayerCountUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth[] players;
    [SerializeField] private TMP_Text aliveCountText;

    private void Update()
    {
        int aliveCount = 0;

        foreach (PlayerHealth player in players)
        {
            if (player != null && !player.IsDead)
            {
                aliveCount++;
            }
        }

        aliveCountText.text = "PLAYERS ALIVE: " + aliveCount + " / " + players.Length;
    }
}