using UnityEngine;
using UnityEngine.UI;  // For UI Text display
using UnityEngine.Events;

public class ShipRepair : MonoBehaviour
{
    [Header("Repair Settings")]
    public int totalPartsNeeded = 5;  // Total parts required to fully repair the ship
    public int currentParts = 0;      // Current number of collected parts
    public UnityEvent onPartCollected;    // Triggered on collection: e.g., UI/sound effects
    public UnityEvent onRepairComplete;   // Triggered on full repair: e.g., load win scene + cutscene

    [Header("UI References")]
    public Text partsCounterText;  // Drag HUD Text here: Displays "Parts: X/5"

    void Start()
    {
        UpdateUI();
    }

    public void AddPart(int amount)
    {
        currentParts += amount;
        currentParts = Mathf.Clamp(currentParts, 0, totalPartsNeeded);
        onPartCollected?.Invoke();
        UpdateUI();

        if (currentParts >= totalPartsNeeded)
        {
            onRepairComplete?.Invoke();  // e.g. Load win scene + victory cutscene
            Debug.Log("Ship repair complete! Preparing for liftoff.");  
        }
    }

    private void UpdateUI()
    {
        if (partsCounterText != null)
        {
            partsCounterText.text = $"Parts: {currentParts}/{totalPartsNeeded}";  
            // Optional Polish: Add a quick scale animation for feedback (requires DOTween)
            // partsCounterText.transform.localScale = Vector3.one * 1.2f;
            // DOTween.To(() => partsCounterText.transform.localScale, x => partsCounterText.transform.localScale = x, Vector3.one, 0.3f);
        }
    }
}
