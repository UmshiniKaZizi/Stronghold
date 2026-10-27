
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

    [Header("Startup Systems")]
    [SerializeField] private CharacterSwap characterSwap;

    private int deadPlayers;
    private bool gameOver;
    private bool gameStarted;

    public bool IsGameOver => gameOver;
    public bool HasGameStarted => gameStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (characterSwap == null)
        {
            characterSwap = FindFirstObjectByType<CharacterSwap>();
        }
    }

    private void Start()
    {
        gameStarted = false;
        gameOver = false;
        deadPlayers = 0;

        // Display the start menu.
        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Enable the overview camera.
        if (menuCamera != null)
            menuCamera.SetActive(true);

        // Show and unlock the cursor for menu interaction.
        SetCursorState(false);

        // Pause gameplay until Start is pressed.
        Time.timeScale = 0f;

        Debug.Log("GameManager: Waiting for Start button.");
    }

    public void StartGame()
    {
        if (gameStarted || gameOver)
            return;

        Debug.Log("START BUTTON PRESSED");

        // Find all player health components, including inactive characters.
        PlayerHealth[] allPlayers =
            FindObjectsByType<PlayerHealth>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // STEP 1: Initialize all character health.
        foreach (PlayerHealth player in allPlayers)
        {
            if (player != null)
                player.InitializeHealth();
        }

        Debug.Log(
            "GameManager: Initialized health for " +
            allPlayers.Length + " player(s)."
        );

        // STEP 2: Find the character-switch manager.
        if (characterSwap == null)
        {
            characterSwap =
                FindFirstObjectByType<CharacterSwap>();
        }

        if (characterSwap == null)
        {
            Debug.LogError(
                "GameManager: CharacterSwap was not found. " +
                "Gameplay cannot start."
            );
            return;
        }

        // STEP 3: Build the circular linked list and select
        // the starting character.
        if (!characterSwap.InitializeCharacters())
        {
            Debug.LogError(
                "GameManager: Character initialization failed. " +
                "Gameplay cannot start."
            );
            return;
        }

        // STEP 4: Mark gameplay as started.
        gameStarted = true;

        // STEP 5: Hide the menu and overview camera.
        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (menuCamera != null)
            menuCamera.SetActive(false);

        // STEP 6: Resume gameplay.
        Time.timeScale = 1f;

        // Hide and lock the cursor for gameplay.
        SetCursorState(true);

        Debug.Log(
            "GAME STARTED | Health initialized | " +
            "Character switching initialized."
        );
    }

    private void SetCursorState(bool gameplay)
    {
        if (gameplay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void PlayerDied()
    {
        if (gameOver || !gameStarted)
            return;

        deadPlayers++;

        Debug.Log(
            "PLAYER DIED | " +
            deadPlayers + "/" + deathsRequiredForGameOver
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
        gameStarted = false;

        Debug.Log("GAME OVER");

        // Show the overview camera.
        if (menuCamera != null)
            menuCamera.SetActive(true);

        // Show the Game Over screen.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Restore the cursor for menu interaction.
        SetCursorState(false);

        // Freeze gameplay.
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SetCursorState(false);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        SetCursorState(false);

        Debug.Log("EXIT GAME");

        Application.Quit();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
