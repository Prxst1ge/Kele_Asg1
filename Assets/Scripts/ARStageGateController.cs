/*
 * Author: Javier
 * Date: 13 December 2025
 * Description:
 * Controls access to the Biscuit AR stage by checking whether the
 * Pineapple Paste stage has been completed. The script reads ingredient
 * progress from the database and enables or disables the Biscuit button
 * accordingly, displaying a lock overlay and message when requirements
 * are not met.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls access to the Biscuit AR stage by checking whether
/// the Pineapple Paste stage requirements have been fulfilled.
/// This script enables or disables the Biscuit button based on
/// ingredient progress stored in the database.
/// </summary>
public class ARStageGateController : MonoBehaviour
{
    /// <summary>
    /// Reference to the DatabaseManager used to retrieve ingredient counts
    /// from Firebase.
    /// </summary>
    [Header("Database")]
    [SerializeField] private DatabaseManager db;

    /// <summary>
    /// The button that allows the user to enter the Biscuit AR stage.
    /// This button will be locked until requirements are met.
    /// </summary>
    [Header("Gate Button")]
    [SerializeField] private Button biscuitButton;

    /// <summary>
    /// Optional UI overlay shown when the Biscuit stage is locked
    /// (e.g. grey panel or lock icon).
    /// </summary>
    [Header("Optional lock UI")]
    [SerializeField] private GameObject biscuitLockOverlay;

    /// <summary>
    /// Optional text displayed on the lock overlay to explain
    /// why the Biscuit stage is locked.
    /// </summary>
    [SerializeField] private TMP_Text biscuitLockText;

    /// <summary>
    /// Database key for the main recipe card.
    /// </summary>
    [Header("Requirements (match your DB keys)")]
    [SerializeField] private string mainCard = "PineappleTart";

    /// <summary>
    /// Database key for the Pineapple Paste component.
    /// </summary>
    [SerializeField] private string component = "PineapplePaste";

    /// <summary>
    /// Database key for the pineapple ingredient.
    /// </summary>
    [SerializeField] private string pineappleKey = "Pineapple_Ingredient";

    /// <summary>
    /// Database key for the sugar ingredient.
    /// </summary>
    [SerializeField] private string sugarKey = "Sugar_Ingredient";

    /// <summary>
    /// Database key for the lemon ingredient.
    /// </summary>
    [SerializeField] private string lemonKey = "Lemon_Ingredient";

    /// <summary>
    /// Required number of pineapples to unlock the Biscuit stage.
    /// </summary>
    [SerializeField] private int requiredPineapple = 2;

    /// <summary>
    /// Required number of sugar units to unlock the Biscuit stage.
    /// </summary>
    [SerializeField] private int requiredSugar = 1;

    /// <summary>
    /// Required number of lemons to unlock the Biscuit stage.
    /// </summary>
    [SerializeField] private int requiredLemon = 1;

    /// <summary>
    /// Unity lifecycle method.
    /// Locks the Biscuit stage by default, loads user collection data,
    /// and starts a refresh routine to account for asynchronous database loading.
    /// </summary>
    void Start()
    {
        // lock first (safe default)
        SetBiscuitUnlocked(false, "Complete Pineapple Paste first");

        if (db != null)
            db.LoadUserCollection();

        // Firebase load is async, so refresh a few times
        StartCoroutine(RefreshGateRoutine());
    }

    /// <summary>
    /// Coroutine that repeatedly checks the database over time
    /// to determine whether the Biscuit stage should be unlocked.
    /// This compensates for asynchronous Firebase data loading.
    /// </summary>
    IEnumerator RefreshGateRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        RefreshGate();

        yield return new WaitForSeconds(0.75f);
        RefreshGate();

        yield return new WaitForSeconds(1.25f);
        RefreshGate();
    }

    /// <summary>
    /// Retrieves ingredient counts from the database and evaluates
    /// whether the Biscuit stage requirements have been met.
    /// </summary>
    void RefreshGate()
    {
        if (db == null) return;

        // Requires DatabaseManager.GetIngredientCount(component, ingredient)
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

    /// <summary>
    /// Enables or disables access to the Biscuit stage and updates
    /// the associated lock UI elements.
    /// </summary>
    /// <param name="unlocked">Whether the Biscuit stage should be accessible.</param>
    /// <param name="msg">Message displayed on the lock overlay when locked.</param>
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
