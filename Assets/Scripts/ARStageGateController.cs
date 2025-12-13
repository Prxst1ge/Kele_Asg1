using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARStageGateController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private DatabaseManager db;

    [Header("Gate Button")]
    [SerializeField] private Button biscuitButton;

    [Header("Optional lock UI")]
    [SerializeField] private GameObject biscuitLockOverlay;   // grey panel / lock icon
    [SerializeField] private TMP_Text biscuitLockText;        // optional text on overlay

    [Header("Requirements (match your DB keys)")]
    [SerializeField] private string mainCard = "PineappleTart";
    [SerializeField] private string component = "PineapplePaste";
    [SerializeField] private string pineappleKey = "Pineapple_Ingredient";
    [SerializeField] private string sugarKey = "Sugar_Ingredient";
    [SerializeField] private string lemonKey = "Lemon_Ingredient";

    [SerializeField] private int requiredPineapple = 2;
    [SerializeField] private int requiredSugar = 1;
    [SerializeField] private int requiredLemon = 1;

    void Start()
    {
        // lock first (safe default)
        SetBiscuitUnlocked(false, "Complete Pineapple Paste first");

        if (db != null)
            db.LoadUserCollection();

        // Firebase load is async, so refresh a few times
        StartCoroutine(RefreshGateRoutine());
    }

    IEnumerator RefreshGateRoutine()
    {
        // try a few times (simple + reliable)
        yield return new WaitForSeconds(0.25f);
        RefreshGate();
        yield return new WaitForSeconds(0.75f);
        RefreshGate();
        yield return new WaitForSeconds(1.25f);
        RefreshGate();
    }

    void RefreshGate()
    {
        if (db == null) return;

        // ✅ This requires DatabaseManager to have GetIngredientCount(component, ingredient)
        int pineapple = db.GetIngredientCount(component, pineappleKey);
        int sugar     = db.GetIngredientCount(component, sugarKey);
        int lemon     = db.GetIngredientCount(component, lemonKey);

        bool unlocked =
            pineapple >= requiredPineapple &&
            sugar     >= requiredSugar &&
            lemon     >= requiredLemon;

        SetBiscuitUnlocked(unlocked, unlocked ? "" : "Complete Pineapple Paste first");
        Debug.Log($"[Gate] PineapplePaste -> P:{pineapple} S:{sugar} L:{lemon} | BiscuitUnlocked={unlocked}");
    }

    void SetBiscuitUnlocked(bool unlocked, string msg)
    {
        if (biscuitButton != null)
            biscuitButton.interactable = unlocked;

        if (biscuitLockOverlay != null)
            biscuitLockOverlay.SetActive(!unlocked);

        if (biscuitLockText != null)
            biscuitLockText.text = msg;
    }
}
