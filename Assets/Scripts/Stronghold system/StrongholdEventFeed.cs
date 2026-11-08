using TMPro;
using UnityEngine;
using System.Collections;
using System;

public enum StrongholdEventType
{
    General,
    Damage,
    ObjectDestroyed,
    RepairProgress,
    RepairCompleted,
    Breach,
    ForceFieldRestored,
    Critical,
    StrongholdDestroyed
}

public class StrongholdEventFeed : MonoBehaviour
{
    [Header("Notification Label")]
    [SerializeField] private TextMeshProUGUI messageLabel;

    [SerializeField] private string strongholdName = "Stronghold";

    [Header("Notification Durations (Seconds)")]
    [SerializeField, Min(0.2f)] private float damageDuration = 3f;
    [SerializeField, Min(0.2f)] private float objectDestroyedDuration = 5f;
    [SerializeField, Min(0.2f)] private float repairProgressDuration = 2.5f;
    [SerializeField, Min(0.2f)] private float repairCompletedDuration = 5f;
    [SerializeField, Min(0.2f)] private float breachDuration = 8f;
    [SerializeField, Min(0.2f)] private float forceFieldRestoredDuration = 4f;
    [SerializeField, Min(0.2f)] private float criticalDuration = 6f;
    [SerializeField, Min(0.2f)] private float generalDuration = 3f;
    [SerializeField, Min(0.2f)] private float duration = 2.5f;

    [Header("Persistent Notifications")]
    [Tooltip("Zero means the message stays visible until another message replaces it or ClearMessage is called.")]
    [SerializeField, Min(0f)] private float strongholdDestroyedDuration = 0f;

    private Coroutine clearRoutine;

    private void Awake()
    {
        if (messageLabel != null)
            messageLabel.gameObject.SetActive(false);
    }

    // Assign the name of the Stronghold that owns this feed.
    public void SetStrongholdName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
            strongholdName = newName;
    }


    // Existing calls to Show(string) continue to work.
    // The message category and duration are detected automatically.
    public void Show(string message)
    {
        Show(DetectEventType(message), message);

       // if (messageLabel == null) return;
       // messageLabel.text = message;
        //messageLabel.gameObject.SetActive(true);
       // if (clearRoutine != null) StopCoroutine(clearRoutine);
       // clearRoutine = StartCoroutine(ClearAfterDelay());
    }

    // Allows other scripts to specify the event category directly.
    public void Show(StrongholdEventType eventType, string message)
    {
        if (messageLabel == null || string.IsNullOrWhiteSpace(message))
            return;

        if (clearRoutine != null)
        {
            StopCoroutine(clearRoutine);
            clearRoutine = null;
        }

        string formattedMessage = FormatMessage(eventType, message);

        messageLabel.text = formattedMessage;
        messageLabel.gameObject.SetActive(true);

        float duration = GetDuration(eventType);

        // A zero duration means the notification stays visible.
        if (duration > 0f)
            clearRoutine = StartCoroutine(ClearAfterDelay(duration));
    }



    public void ShowEnemyHit(
        string enemyName, 
        float damage, 
        float remaining, 
        float maximum)
    {
        string message =
            enemyName + " hit (-" + Mathf.RoundToInt(damage) + ")  " +
            Mathf.CeilToInt(remaining) + "/" +
            Mathf.CeilToInt(maximum) + " HP";

        Show(StrongholdEventType.Damage, message);

       // Show(enemyName + " hit (-" + Mathf.RoundToInt(damage) + ")  " +
           //  Mathf.CeilToInt(remaining) + "/" + Mathf.CeilToInt(maximum) + " HP");
    }

    private string FormatMessage(StrongholdEventType eventType, string message)
    {
        string category = GetCategoryLabel(eventType);

        string cleanMessage = message.Trim();

        // Avoid repeating the Stronghold name if it is already present.
        if (!string.IsNullOrWhiteSpace(strongholdName) &&
            !cleanMessage.StartsWith(
                strongholdName,
                StringComparison.OrdinalIgnoreCase))
        {
            cleanMessage = strongholdName + ": " + cleanMessage;
        }

        return "[" + category + "] " + cleanMessage;
    }

    private StrongholdEventType DetectEventType(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return StrongholdEventType.General;

        string lower = message.ToLowerInvariant();

        if (lower.Contains("stronghold destroyed") ||
            lower.Contains("stronghold lost"))
            return StrongholdEventType.StrongholdDestroyed;

        if (lower.Contains("perimeter breached") ||
            lower.Contains("force field breached"))
            return StrongholdEventType.Breach;

        if (lower.Contains("critical"))
            return StrongholdEventType.Critical;

        if (lower.Contains("repair progress"))
            return StrongholdEventType.RepairProgress;

        if (lower.Contains("repaired") ||
            lower.Contains("object restored") ||
            lower.Contains("fully restored"))
            return StrongholdEventType.RepairCompleted;

        if (lower.Contains("force field restored") ||
            lower.Contains("perimeter restored"))
            return StrongholdEventType.ForceFieldRestored;

        if (lower.Contains("object destroyed"))
            return StrongholdEventType.ObjectDestroyed;

        if (lower.Contains("damaged") ||
            lower.Contains("object hit") ||
            lower.Contains("hit (-"))
            return StrongholdEventType.Damage;

        return StrongholdEventType.General;
    }

    private string GetCategoryLabel(StrongholdEventType eventType)
    {
        switch (eventType)
        {
            case StrongholdEventType.Damage:
                return "DAMAGE";

            case StrongholdEventType.ObjectDestroyed:
                return "OBJECT DESTROYED";

            case StrongholdEventType.RepairProgress:
                return "REPAIR PROGRESS";

            case StrongholdEventType.RepairCompleted:
                return "REPAIR COMPLETE";

            case StrongholdEventType.Breach:
                return "BREACH";

            case StrongholdEventType.ForceFieldRestored:
                return "FORCE FIELD RESTORED";

            case StrongholdEventType.Critical:
                return "CRITICAL";

            case StrongholdEventType.StrongholdDestroyed:
                return "STRONGHOLD LOST";

            default:
                return "UPDATE";
        }
    }

    private float GetDuration(StrongholdEventType eventType)
    {
        switch (eventType)
        {
            case StrongholdEventType.Damage:
                return damageDuration;

            case StrongholdEventType.ObjectDestroyed:
                return objectDestroyedDuration;

            case StrongholdEventType.RepairProgress:
                return repairProgressDuration;

            case StrongholdEventType.RepairCompleted:
                return repairCompletedDuration;

            case StrongholdEventType.Breach:
                return breachDuration;

            case StrongholdEventType.ForceFieldRestored:
                return forceFieldRestoredDuration;

            case StrongholdEventType.Critical:
                return criticalDuration;

            case StrongholdEventType.StrongholdDestroyed:
                return strongholdDestroyedDuration;

            default:
                return generalDuration;
        }
    }


    private IEnumerator ClearAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (messageLabel != null) 
            messageLabel.gameObject.SetActive(false);

        clearRoutine = null;
    }

    public void ClearMessage()
    {
        if (clearRoutine != null)
        {
            StopCoroutine(clearRoutine);
            clearRoutine = null;
        }

        if (messageLabel != null)
            messageLabel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (clearRoutine != null)
        {
            StopCoroutine(clearRoutine);
            clearRoutine = null;
        }
    }
}
