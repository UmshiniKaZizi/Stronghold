
using UnityEngine;

public class ArtilleryBombInventory : MonoBehaviour
{
    [Header("Bomb Rewards")]
    [SerializeField] private int killsRequired = 10;
    [SerializeField] private int maxBombs = 2;
    [SerializeField] private int startingBombs = 0;

    private int currentBombs;
    private int killProgress;

    public int CurrentBombs => currentBombs;
    public int KillProgress => killProgress;
    public int MaxBombs => maxBombs;
    public bool HasBomb => currentBombs > 0;

    private void Awake()
    {
        currentBombs = Mathf.Clamp(startingBombs, 0, maxBombs);
        killProgress = 0;

        Debug.Log(
            $"[ArtilleryInventory] INITIALIZED | " +
            $"Bombs: {currentBombs}/{maxBombs} | " +
            $"Progress: {killProgress}/{killsRequired}",
            this
        );
    }

    public void RegisterKill()
    {
        Debug.Log(
            $"[ArtilleryInventory] RegisterKill() RECEIVED | " +
            $"Bombs: {currentBombs}/{maxBombs} | " +
            $"Progress: {killProgress}/{killsRequired}",
            this
        );

        // Ignore kills entirely when inventory is full.
        if (currentBombs >= maxBombs)
        {
            Debug.LogWarning(
                "[ArtilleryInventory] INVENTORY FULL | Kill ignored.",
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

        if (killProgress >= killsRequired)
        {
            currentBombs++;
            killProgress = 0;

            Debug.Log(
                $"[ArtilleryInventory] BOMB AWARDED! | " +
                $"Bombs: {currentBombs}/{maxBombs} | " +
                $"Progress reset: {killProgress}/{killsRequired}",
                this
            );
        }
    }

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
                "[ArtilleryInventory] CANNOT FIRE | No bombs available.",
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
