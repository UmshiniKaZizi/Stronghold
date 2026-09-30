using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over")]
    [SerializeField] private int deathsRequiredForGameOver = 2;

    private int deadPlayers = 0;
    private bool gameOver = false;

    public bool IsGameOver => gameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayerDied()
    {
        if (gameOver)
            return;

        deadPlayers++;

        Debug.Log(
            "PLAYER DIED | " +
            deadPlayers +
            "/" +
            deathsRequiredForGameOver
        );

        if (deadPlayers >= deathsRequiredForGameOver)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        gameOver = true;

        Debug.Log("GAME OVER - TWO PLAYERS HAVE DIED!");

        // Stop gameplay
        Time.timeScale = 0f;

        // Add Game Over UI here later
    }
}