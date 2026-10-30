using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Onboarding")]
    [SerializeField] private GameObject onboardingRoot;
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

        // Show the entire onboarding sequence.
        if (onboardingRoot != null)
        {
            onboardingRoot.SetActive(true);
        }

        // Hide Game Over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Show overview/menu camera.
        if (menuCamera != null)
        {
            menuCamera.SetActive(true);
        }

        // Cursor must be available during onboarding.
        SetCursorState(false);

        // Keep gameplay paused while onboarding is happening.
        Time.timeScale = 0f;

        Debug.Log("GameManager: Waiting for onboarding to finish.");
    }

    public void StartGame()
    {
        if (gameStarted || gameOver)
            return;

        Debug.Log("START BUTTON PRESSED");

        // Find all player health components,
        // including inactive characters.
        PlayerHealth[] allPlayers =
            FindObjectsByType<PlayerHealth>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // STEP 1:
        // Initialise character health.
        foreach (PlayerHealth player in allPlayers)
        {
            if (player != null)
            {
                player.InitializeHealth();
            }
        }

        Debug.Log(
            "GameManager: Initialized health for " +
            allPlayers.Length + " player(s)."
        );

        // STEP 2:
        // Find CharacterSwap if not already assigned.
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

        // STEP 3:
        // Build the character switching list
        // and select the starting character.
        if (!characterSwap.InitializeCharacters())
        {
            Debug.LogError(
                "GameManager: Character initialization failed. " +
                "Gameplay cannot start."
            );

            return;
        }

        // STEP 4:
        // Gameplay has officially started.
        gameStarted = true;

        // STEP 5:
        // Hide the entire onboarding sequence.
        if (onboardingRoot != null)
        {
            onboardingRoot.SetActive(false);
        }

        // Turn off menu/overview camera.
        if (menuCamera != null)
        {
            menuCamera.SetActive(false);
        }

        // STEP 6:
        // Resume the game.
        Time.timeScale = 1f;

        // Lock cursor for gameplay.
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

        // Show overview camera again.
        if (menuCamera != null)
        {
            menuCamera.SetActive(true);
        }

        // Show Game Over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Restore cursor.
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
        {
            Instance = null;
        }
    }
}