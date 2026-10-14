using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Start Menu")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject menuCamera;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
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

    private void Start()
    {
        // Show start screen
        startMenuPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        // Show overview camera
        menuCamera.SetActive(true);

        // Unlock mouse for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // PAUSE GAMEPLAY
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        Debug.Log("START BUTTON PRESSED");

        // Hide start screen
        startMenuPanel.SetActive(false);

        // Turn off overview camera
        menuCamera.SetActive(false);

        // Resume game
        Time.timeScale = 1f;

        // Lock mouse for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER");

        // Overview camera
        menuCamera.SetActive(true);

        // Show Game Over screen
        gameOverPanel.SetActive(true);

        // Unlock mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Freeze game
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();

        Debug.Log("EXIT GAME");
    }
}