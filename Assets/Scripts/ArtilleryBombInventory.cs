using UnityEngine;

public class ArtilleryBombInventory : MonoBehaviour
{
    [Header("Bomb Rewards")]
    [SerializeField] private int killsRequired = 10;
    [SerializeField] private int maxBombs = 2;
    [SerializeField] private int startingBombs = 0;

    private int currentBombs;
    private int killProgress;

    // ==========================================
    // PUBLIC INFORMATION FOR OTHER SCRIPTS / UI
    // ==========================================

    public int CurrentBombs => currentBombs;

    public int KillProgress => killProgress;

    public int MaxBombs => maxBombs;

    public int KillsRequired => killsRequired;

    public bool HasBomb => currentBombs > 0;

    public bool IsFull => currentBombs >= maxBombs;


    private void Awake()
    {
        // Make sure Inspector values are valid.
        killsRequired = Mathf.Max(1, killsRequired);
        maxBombs = Mathf.Max(1, maxBombs);

        currentBombs =
            Mathf.Clamp(
                startingBombs,
                0,
                maxBombs
            );

        killProgress = 0;

        Debug.Log(
            $"[ArtilleryInventory] INITIALIZED | " +
            $"Bombs: {currentBombs}/{maxBombs} | " +
            $"Progress: {killProgress}/{killsRequired}",
            this
        );
    }


    // ==========================================
    // REGISTER ARTILLERY KILL
    // ==========================================

    public void RegisterKill()
    {
        Debug.Log(
            $"[ArtilleryInventory] RegisterKill() RECEIVED | " +
            $"Bombs: {currentBombs}/{maxBombs} | " +
            $"Progress: {killProgress}/{killsRequired}",
            this
        );

        // If we already have the maximum number
        // of bombs, kills do not add more progress.
        if (currentBombs >= maxBombs)
        {
            Debug.Log(
                "[ArtilleryInventory] INVENTORY FULL | " +
                "Kill progress paused.",
                this
            );

            return;
        }

        killProgress++;

        Debug.Log(
            $"[ArtilleryInventory] KILL COUNTED | " +
            $"Progress: {killProgress}/{killsRequired} | " +
            $"Bombs: {currentBombs}/{maxBombs}",
            this
        );

        // Enough kills earned = give one bomb.
        if (killProgress >= killsRequired)
        {
            currentBombs++;

            // Reset progress for the next bomb.
            killProgress = 0;

            // Make sure we never go above maximum bombs.
            currentBombs =
                Mathf.Clamp(
                    currentBombs,
                    0,
                    maxBombs
                );

            Debug.Log(
                $"[ArtilleryInventory] BOMB AWARDED! | " +
                $"Bombs: {currentBombs}/{maxBombs} | " +
                $"Progress: {killProgress}/{killsRequired}",
                this
            );
        }
    }


    // ==========================================
    // USE ONE BOMB
    // ==========================================

    public bool ConsumeBomb()
    {
        Debug.Log(
            $"[ArtilleryInventory] ConsumeBomb() REQUESTED | " +
            $"Current bombs: {currentBombs}/{maxBombs}",
            this
        );

        if (currentBombs <= 0)
        {
            Debug.LogWarning(
                "[ArtilleryInventory] CANNOT FIRE | " +
                "No bombs available.",
                this
            );

            return false;
        }

        currentBombs--;

        Debug.Log(
            $"[ArtilleryInventory] BOMB CONSUMED | " +
            $"Remaining: {currentBombs}/{maxBombs}",
            this
        );

        return true;
    }
}